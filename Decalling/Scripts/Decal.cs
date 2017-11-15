using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Framework.DeferredDecalling;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class Decal : MonoBehaviour
{
    public const string DEFERRED_SHADER = "Decalling/Deferred Decal";
    public const string UNLIT_SHADER = "Decalling/Unlit";

    private static readonly Material[] EMPTY_MATS = new Material[0];
        
    private static Shader m_deferredShader;
    private static Shader m_unlitShader;
        
    public enum DecalType
    {
        Deferred,
        Unlit,
        Invalid,
    }

    [SerializeField]
    private Material m_material;
    public Material Material
    {
        get { return m_material; }
        set
        {
            if (m_material != value)
            {
                m_material = value;
                OnMaterialUpdate();
            }
        }
    }

    [SerializeField] [Range(0, 5)]
    private float m_intensity = 1.0f;
    public float Intensity
    {
        get { return m_intensity; }
        set { m_intensity = Mathf.Max(value, 0); }
    }

    [SerializeField] [Range(0, 10)]
    private float m_emissionIntensity = 1.0f;
    public float EmissionIntensity
    {
        get { return m_emissionIntensity; }
        set { m_emissionIntensity = Mathf.Max(value, 0); }
    }

    [SerializeField]
    [Tooltip("Set a GameObject here to only draw this Decal on decendants of the object.")]
    private GameObject m_limitTo = null;
    public GameObject LimitTo
    {
        get { return m_limitTo; }
        set
        {
            if (m_limitTo != value)
            {
                DecalRegistrar.Instance.RemoveLimitTo(m_limitTo);
                m_limitTo = value;
                DecalRegistrar.Instance.AddLimitTo(m_limitTo);
            }
        }
    }

    [SerializeField]
    [Tooltip("Enable to draw the Albedo / Emission pass of the Decal.")]
    private bool m_drawAlbedo = true;
    public bool DrawAlbedo
    {
        get { return m_drawAlbedo; }
        set { m_drawAlbedo = value; }
    }

    [SerializeField]
    [Tooltip("Use an interpolated light probe for this decal for indirect light. This breaks instancing for the decal and thus comes with a performance impact, so use with caution.")]
    private bool m_useLightProbes = false;
    public bool UseLightProbes
    {
        get { return m_useLightProbes; }
        set { m_useLightProbes = value; }
    }

    [SerializeField]
    [Tooltip("Enable to draw the Normal / SpecGloss pass of the Decal.")]
    private bool m_drawNormalAndGloss = true;
    public bool DrawNormalAndGloss
    {
        get { return m_drawNormalAndGloss; }
        set { m_drawNormalAndGloss = value; }
    }

    [SerializeField]
    [Tooltip("Enable perfect Normal / SpecGloss blending between decals. Costly and has no effect when decals don't overlap, so use with caution.")]
    private bool m_highQualityBlending = false;
    public bool HighQualityBlending
    {
        get { return m_highQualityBlending; }
        set { m_highQualityBlending = value; }
    }
        
    private MeshRenderer m_renderer;
    public MeshRenderer Renderer
    {
        get { return m_renderer; }
    }

    private DecalType m_decalType;
    public DecalType Type
    {
        get { return m_decalType; }
    }

    private List<Decal> m_register;
    public List<Decal> Register
    {
        get { return m_register; }
        set { m_register = value; }
    }
        
    private void Awake()
    {
        Init();
    }

    private void Start()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Init();
        }

        foreach (Camera cam in SceneView.GetAllSceneCameras())
        {
            DecalRenderer renderer = cam.GetComponent<DecalRenderer>();
            if (renderer == null)
            {
                renderer = cam.gameObject.AddComponent<DecalRenderer>();
            }
        }
#endif

        DecalRegistrar.Instance.RegisterDecal(this);
    }

    private void OnDestroy()
    {
        DecalRegistrar.Instance.UnregisterDecal(this);
    }

    private void Init()
    {
        if (m_deferredShader == null)
        {
            m_deferredShader = Shader.Find(DEFERRED_SHADER);
        }
        if (m_unlitShader == null)
        {
            m_unlitShader = Shader.Find(UNLIT_SHADER);
        }

        GetComponent<MeshFilter>().mesh = CubeBuilder.Cube;

        m_renderer = GetComponent<MeshRenderer>();
        m_renderer.shadowCastingMode = ShadowCastingMode.Off;
        m_renderer.receiveShadows = false;
        m_renderer.materials = EMPTY_MATS;
        m_renderer.lightProbeUsage = LightProbeUsage.BlendProbes;
        m_renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

        if (m_material == null)
        {
            Debug.LogError("Material null on decal: " + gameObject.name);
            enabled = false;
        }

        OnMaterialUpdate();
    }

    public void OnMaterialUpdate()
    {
        if (m_material == null)
        {
            m_decalType = DecalType.Invalid;
        }
        else if (m_material.shader == m_deferredShader)
        {
            m_decalType = DecalType.Deferred;
        }
        else if (m_material.shader == m_unlitShader)
        {
            m_decalType = DecalType.Unlit;
        }
        else
        {
            m_decalType = DecalType.Invalid;
        }
    }

    private void OnDrawGizmos()
    {
        DrawGizmo(false);
    }

    private void OnDrawGizmosSelected()
    {
        DrawGizmo(true);
    }

    private void DrawGizmo(bool selected)
    {
        Gizmos.matrix = transform.localToWorldMatrix;

        if (!selected)
        {
            // Draw an invisible cube to allow selection
            Gizmos.color = new Color(0, 0, 0, 0);
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }

        Gizmos.color = (selected ? 0.65f : 0.2f) * Color.white;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
    }
}