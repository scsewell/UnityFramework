using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Framework.DeferredDecalling
{
    [ExecuteInEditMode]
    public class DecalRenderer : MonoBehaviour
    {
        private const string ID_SHADER = "Hidden/DrawID";

        private const string BUFFER_NAME = "DeferredDecals-";
        private const string BUFFER_ID_NAME = BUFFER_NAME + "Limit IDs";
        private const string BUFFER_ALBEDO_NAME = BUFFER_NAME + "AlbedoEmission";
        private const string BUFFER_NORM_SPEC_NAME = BUFFER_NAME + "NormSpecSmooth";
        private const string BUFFER_UNLIT_NAME = BUFFER_NAME + "Unlit";

        private static readonly RenderTargetIdentifier[] ALBEDO_TARGETS = new RenderTargetIdentifier[] {
            BuiltinRenderTextureType.GBuffer0,
            BuiltinRenderTextureType.GBuffer3,
        };

        private static readonly RenderTargetIdentifier[] ALBEDO_TARGETS_HDR = new RenderTargetIdentifier[] {
            BuiltinRenderTextureType.GBuffer0,
            BuiltinRenderTextureType.CameraTarget,
        };

        private static readonly RenderTargetIdentifier[] NORMAL_TARGETS = new RenderTargetIdentifier[] {
            BuiltinRenderTextureType.GBuffer1,
            BuiltinRenderTextureType.GBuffer2,
        };

        private const CameraEvent LIMIT_ID_EVENT = CameraEvent.AfterGBuffer;
        private const CameraEvent DEFERRED_EVENT = CameraEvent.BeforeReflections;
        private const CameraEvent UNLIT_EVENT = CameraEvent.BeforeImageEffectsOpaque;

        private CommandBuffer m_bufferAlbedo = null;
        private CommandBuffer m_bufferNormSpec = null;
        private CommandBuffer m_bufferUnlit = null;
        private CommandBuffer m_bufferLimitTo = null;
        
        private Camera m_camera;
        private Material m_drawIDMat;

        private DecalBlock m_propBlock;
        private InstancedDecalBlock m_instanceBlock1;
        private InstancedDecalBlock m_instanceBlock2;
        
        private Plane[] m_frustumPlanes = new Plane[6];

        private bool m_useInstancing = true;


        private void Awake()
        {
            Init();
        }

#if UNITY_EDITOR
        private void OnEnable()
        {
            Init();
        }
#endif

        private void Init()
        {
            m_camera = GetComponent<Camera>();

            m_drawIDMat = new Material(Shader.Find(ID_SHADER));

            m_propBlock = new DecalBlock();
            m_instanceBlock1 = new InstancedDecalBlock();
            m_instanceBlock2 = new InstancedDecalBlock();

            m_useInstancing = SystemInfo.supportsInstancing;
        }

        private void OnDisable()
        {
            if (m_bufferLimitTo != null)
            {
                m_camera.RemoveCommandBuffer(LIMIT_ID_EVENT, m_bufferLimitTo);
                m_bufferLimitTo = null;
            }

            if (m_bufferAlbedo != null)
            {
                m_camera.RemoveCommandBuffer(DEFERRED_EVENT, m_bufferAlbedo);
                m_bufferAlbedo = null;
            }

            if (m_bufferNormSpec != null)
            {
                m_camera.RemoveCommandBuffer(DEFERRED_EVENT, m_bufferNormSpec);
                m_bufferNormSpec = null;
            }

            if (m_bufferUnlit != null)
            {
                m_camera.RemoveCommandBuffer(UNLIT_EVENT, m_bufferUnlit);
                m_bufferUnlit = null;
            }
        }

        private void OnPreRender()
        {
            PrepareBuffers();

            GeometryUtils.CalculateFrustumPlanes(m_frustumPlanes, m_camera);
            
            DrawGameObjectIDs();
            DrawDeferred();
            DrawUnlitDecals();
        }

        private void PrepareBuffers()
        {
            CreateBuffer(ref m_bufferLimitTo, m_camera, BUFFER_ID_NAME, LIMIT_ID_EVENT);
            CreateBuffer(ref m_bufferAlbedo, m_camera, BUFFER_ALBEDO_NAME, DEFERRED_EVENT);
            CreateBuffer(ref m_bufferNormSpec, m_camera, BUFFER_NORM_SPEC_NAME, DEFERRED_EVENT);
            CreateBuffer(ref m_bufferUnlit, m_camera, BUFFER_UNLIT_NAME, UNLIT_EVENT);

            m_bufferLimitTo.Clear();
            m_bufferAlbedo.Clear();
            m_bufferNormSpec.Clear();
            m_bufferUnlit.Clear();
        }

        private void DrawGameObjectIDs()
        {
            int idTex = Shader.PropertyToID("_GameObjectIDTex");
            m_bufferLimitTo.GetTemporaryRT(idTex, -1, -1, 0, FilterMode.Point, RenderTextureFormat.RFloat);
            m_bufferLimitTo.SetRenderTarget(idTex, BuiltinRenderTextureType.CameraTarget);
            m_bufferLimitTo.ClearRenderTarget(false, true, Color.white);

            if (m_drawIDMat == null)
            {
                return;
            }
            
            foreach (KeyValuePair<GameObject, DecalRegistrar.LimitToInfo> limitTo in DecalRegistrar.Instance.LimitTo)
            {
                GameObject limitToObject = limitTo.Key;

                if (limitToObject != null)
                {
                    m_bufferLimitTo.SetGlobalFloat("_ID", limitTo.Key.GetInstanceID());

                    foreach (Renderer renderer in limitTo.Value.Renderers)
                    {
                        if (renderer != null && GeometryUtility.TestPlanesAABB(m_frustumPlanes, renderer.bounds))
                        {
                            m_bufferLimitTo.DrawRenderer(renderer, m_drawIDMat);
                        }
                    }
                }
            }
        }

        private void DrawDeferred()
        {
            int copy1id = Shader.PropertyToID("_CameraGBufferTexture1Copy");
            int copy2id = Shader.PropertyToID("_CameraGBufferTexture2Copy");

            bool hasInitAlbedo = false;
            bool hasInitNrmSpec = false;

            foreach (KeyValuePair<Material, List<Decal>> matToDecal in DecalRegistrar.Instance.DeferredDecals)
            {
                Material material = matToDecal.Key;
                List<Decal> decals = matToDecal.Value;
                
                bool renderAlbedo = material.IsKeywordEnabled("ALBEDO_ON");
                bool renderNormals = material.IsKeywordEnabled("NORMALS_ON") || material.IsKeywordEnabled("MASKED_NORMALS_ON");
                bool renderSpecSmooth = material.IsKeywordEnabled("SPEC_SMOOTH_ON");

                bool instance = m_useInstancing && material.enableInstancing;

                for (int i = 0; i < decals.Count; i++)
                {
                    Decal decal = decals[i];

                    if (decal.enabled && decal.Intensity > 0 && GeometryUtility.TestPlanesAABB(m_frustumPlanes, decal.Renderer.bounds))
                    {
                        if (decal.DrawAlbedo && renderAlbedo)
                        {
                            if (instance && !decal.UseLightProbes)
                            {
                                if (m_instanceBlock1.AddInstanceValues(decal))
                                {
                                    InitAlbedoBuffer(ref hasInitAlbedo);
                                    m_instanceBlock1.DrawInstances(m_bufferAlbedo, material, 0);
                                }
                            }
                            else
                            {
                                InitAlbedoBuffer(ref hasInitAlbedo);
                                m_propBlock.Draw(m_bufferAlbedo, material, 0, decal, true);
                            }
                        }

                        if (decal.DrawNormalAndGloss && (renderNormals || renderSpecSmooth))
                        {
                            if (decal.HighQualityBlending)
                            {
                                InitNrmSpecBuffer(ref hasInitNrmSpec, renderNormals, copy2id, renderSpecSmooth, copy1id);
                                m_propBlock.Draw(m_bufferNormSpec, material, 1, decal, false);
                            }
                            else
                            {
                                if (instance)
                                {
                                    if (m_instanceBlock2.AddInstanceValues(decal))
                                    {
                                        InitNrmSpecBuffer(ref hasInitNrmSpec, false, copy2id, false, copy1id);
                                        m_instanceBlock2.DrawInstances(m_bufferNormSpec, material, 1);
                                    }
                                }
                                else
                                {
                                    InitNrmSpecBuffer(ref hasInitNrmSpec, false, copy2id, false, copy1id);
                                    m_propBlock.Draw(m_bufferNormSpec, material, 1, decal, false);
                                }
                            }
                        }
                    }
                }

                if (instance)
                {
                    if (m_instanceBlock1.InstanceCount > 0)
                    {
                        InitAlbedoBuffer(ref hasInitAlbedo);
                        m_instanceBlock1.DrawInstances(m_bufferAlbedo, material, 0);
                    }
                    if (m_instanceBlock2.InstanceCount > 0)
                    {
                        InitNrmSpecBuffer(ref hasInitNrmSpec, false, copy2id, false, copy1id);
                        m_instanceBlock2.DrawInstances(m_bufferNormSpec, material, 1);
                    }
                }
            }
        }

        private void InitAlbedoBuffer(ref bool hasInit)
        {
            if (!hasInit)
            {
                RenderTargetIdentifier[] albedoTargets = m_camera.allowHDR ? ALBEDO_TARGETS_HDR : ALBEDO_TARGETS;
                m_bufferAlbedo.SetRenderTarget(albedoTargets, BuiltinRenderTextureType.CameraTarget);
                hasInit = true;
            }
        }

        private void InitNrmSpecBuffer(ref bool hasInit, bool updateNormals, int normalID, bool updateSpecSmooth, int specSmoothID)
        {
            if (!hasInit)
            {
                m_bufferNormSpec.GetTemporaryRT(specSmoothID, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
                m_bufferNormSpec.GetTemporaryRT(normalID, -1, -1, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            }

            if (!hasInit || updateSpecSmooth)
            {
                m_bufferNormSpec.Blit(BuiltinRenderTextureType.GBuffer1, specSmoothID);
            }
            if (!hasInit || updateNormals)
            {
                m_bufferNormSpec.Blit(BuiltinRenderTextureType.GBuffer2, normalID);
            }
            if (!hasInit || updateNormals || updateSpecSmooth)
            {
                m_bufferNormSpec.SetRenderTarget(NORMAL_TARGETS, BuiltinRenderTextureType.CameraTarget);
            }
            hasInit = true;
        }

        private void DrawUnlitDecals()
        {
            bool hasInit = false;

            foreach (KeyValuePair<Material, List<Decal>> matToDecal in DecalRegistrar.Instance.UnlitDecals)
            {
                Material material = matToDecal.Key;
                List<Decal> decals = matToDecal.Value;

                bool instance = m_useInstancing && material.enableInstancing;

                for (int i = 0; i < decals.Count; i++)
                {
                    Decal decal = decals[i];

                    if (decal.enabled && decal.Intensity > 0 && GeometryUtility.TestPlanesAABB(m_frustumPlanes, decal.Renderer.bounds))
                    {
                        if (instance)
                        {
                            if (m_instanceBlock1.AddInstanceValues(decal))
                            {
                                InitUnlitBuffer(ref hasInit);
                                m_instanceBlock1.DrawInstances(m_bufferUnlit, material, 0);
                            }
                        }
                        else
                        {
                            InitUnlitBuffer(ref hasInit);
                            m_propBlock.Draw(m_bufferUnlit, material, 0, decal, false);
                        }
                    }
                }

                if (instance && m_instanceBlock1.InstanceCount > 0)
                {
                    InitUnlitBuffer(ref hasInit);
                    m_instanceBlock1.DrawInstances(m_bufferUnlit, material, 0);
                }
            }
        }
        
        private void InitUnlitBuffer(ref bool hasInit)
        {
            if (!hasInit)
            {
                m_bufferUnlit.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
                hasInit = true;
            }
        }

        private void CreateBuffer(ref CommandBuffer buffer, Camera cam, string name, CameraEvent evt)
        {
            if (buffer == null)
            {
                foreach (CommandBuffer existingBuffer in cam.GetCommandBuffers(evt))
                {
                    if (existingBuffer.name == name)
                    {
                        buffer = existingBuffer;
                        break;
                    }
                }
                
                if (buffer == null)
                {
                    buffer = new CommandBuffer();
                    buffer.name = name;
                    cam.AddCommandBuffer(evt, buffer);
                }
            }
        }
    }
}
