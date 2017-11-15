using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Framework.DeferredDecalling
{
    public class DecalShaderGUI : ShaderGUI
    {
        private MaterialEditor m_editor;
        private MaterialProperty[] m_properties;
        private Material m_target;

        private enum DecalBlendMode
        {
            Default,
            Additive,
            Multiply
        }

        private enum NormalBlendMode
        {
            Modulate,
            Overwrite
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            m_target = materialEditor.target as Material;
            m_editor = materialEditor;
            m_properties = properties;

            DrawTextureSingleLine("_MaskTex", "_MaskMultiplier");
            EditorGUILayout.Space();

            if (KeywordToggle("_Albedo_On", "ALBEDO_ON", "Albedo / Emission"))
            {
                EditorGUI.indentLevel += 2;
                DecalBlendMode blendMode = (DecalBlendMode)m_target.GetFloat("_DecalBlendMode");
                blendMode = (DecalBlendMode)EditorGUILayout.EnumPopup(new GUIContent("Blend Mode"), blendMode);
                ApplyBlendMode(blendMode);
                EditorGUI.indentLevel -= 2;
                DrawTextureSingleLine("_MainTex", "_Color");
                EditorGUILayout.Space();

                if (KeywordToggle("_Emission_On", "EMISSION_ON", "Emission"))
                {
                    DrawTextureSingleLine("_EmissionTex", "_EmissionColor");
                    EditorGUILayout.Space();
                }
            }
            else
            {
                SetKeyword("EMISSION_ON", false);
            }
            
            if (KeywordToggle("_Normal_On", "NORMALS_ON", "Normals") && m_target.HasProperty("_Normal_On"))
            {
                DrawTextureSingleLine("_NormalTex", "_NormalMultiplier");

                EditorGUI.indentLevel += 2;
                NormalBlendMode normalBlendMode = (NormalBlendMode)m_target.GetFloat("_NormalBlendMode");
                normalBlendMode = (NormalBlendMode)EditorGUILayout.EnumPopup(new GUIContent("Blend Mode"), normalBlendMode);
                m_target.SetFloat("_NormalBlendMode", (float)normalBlendMode);
                bool maskNormals = (m_target.GetInt("_MaskNormals") != 0);
                maskNormals = EditorGUILayout.Toggle(new GUIContent("Mask Normals"), maskNormals);
                m_target.SetFloat("_MaskNormals", (maskNormals ? 1 : 0));
                SetKeyword("MASKED_NORMALS_ON", maskNormals);
                SetKeyword("NORMALS_ON", !maskNormals);
                EditorGUI.indentLevel -= 2;
                EditorGUILayout.Space();
            }
            else
            {
                SetKeyword("MASKED_NORMALS_ON", false);
            }

            if (KeywordToggle("_Spec_Smooth_On", "SPEC_SMOOTH_ON", "Spec Gloss"))
            {
                DrawTextureSingleLine("_SpecularTex", "_SpecularMultiplier");
                DrawTextureSingleLine("_SmoothnessTex", "_SmoothnessMultiplier");
                EditorGUILayout.Space();
            }

            m_editor.TextureScaleOffsetProperty(FindProperty("_MainTex", m_properties));

            EditorGUILayout.Space();
            
            if (KeywordToggle("_SmoothEdge_On", "SMOOTH_EDGE", "Smooth Edge"))
            {
                MaterialProperty maxAngleProp = FindProperty("_SmoothMaxAngle", m_properties);
                float maxAngle = EditorGUILayout.Slider(new GUIContent("Max Angle"), maxAngleProp.floatValue * Mathf.Rad2Deg, 0, 180);
                maxAngleProp.floatValue = maxAngle * Mathf.Deg2Rad;

                MaterialProperty smoothAngleProp = FindProperty("_SmoothAngle", m_properties);
                float smoothAngle = EditorGUILayout.Slider(new GUIContent("Smooth Angle"), smoothAngleProp.floatValue * Mathf.Rad2Deg, 0, 180);
                smoothAngleProp.floatValue = smoothAngle * Mathf.Deg2Rad;
            }
            else
            {
                MaterialProperty maxAngleProp = FindProperty("_MaxAngle", m_properties);
                float maxAngle = EditorGUILayout.Slider(new GUIContent("Max Angle"), Mathf.Acos(maxAngleProp.floatValue) * Mathf.Rad2Deg, 0, 180);
                maxAngleProp.floatValue = Mathf.Cos(maxAngle * Mathf.Deg2Rad);
            }
            EditorGUILayout.Space();

            m_target.enableInstancing = true;
        }

        private void ApplyBlendMode(DecalBlendMode source)
        {
            m_target.SetFloat("_DecalBlendMode", (float)source);
            switch (source)
            {
                case DecalBlendMode.Default:
                    m_target.SetFloat("_DecalSrcBlend", (float)BlendMode.One);
                    m_target.SetFloat("_DecalDstBlend", (float)BlendMode.OneMinusSrcAlpha);
                    break;
                case DecalBlendMode.Additive:
                    m_target.SetFloat("_DecalSrcBlend", (float)BlendMode.One);
                    m_target.SetFloat("_DecalDstBlend", (float)BlendMode.One);
                    break;
                case DecalBlendMode.Multiply:
                    m_target.SetFloat("_DecalSrcBlend", (float)BlendMode.DstColor);
                    m_target.SetFloat("_DecalDstBlend", (float)BlendMode.OneMinusSrcAlpha);
                    break;
                default:
                    m_target.SetFloat("_DecalSrcBlend", (float)BlendMode.Zero);
                    m_target.SetFloat("_DecalDstBlend", (float)BlendMode.Zero);
                    Debug.LogError("Unsupported decal blend mode: " + source);
                    break;
            }
        }

        private void DrawTextureSingleLine(string baseName, string extraName)
        {
            MaterialProperty texture = FindProperty(baseName, m_properties, false);
            MaterialProperty extra = FindProperty(extraName, m_properties, false);
            if (texture != null)
            {
                if (extra != null && extra.flags != MaterialProperty.PropFlags.PerRendererData)
                {
                    m_editor.TexturePropertySingleLine(new GUIContent(texture.displayName), texture, extra);
                }
                else
                {
                    m_editor.TexturePropertySingleLine(new GUIContent(texture.displayName), texture);
                }
            }
        }

        private bool KeywordToggle(string propertyName, string keyword, string displayValue)
        {
            bool isOn = true;
            if (m_target.HasProperty(propertyName))
            {
                MaterialProperty specSmoothOn = FindProperty(propertyName, m_properties);
                isOn = EditorGUILayout.Toggle(new GUIContent(displayValue), specSmoothOn.floatValue == 1);
                specSmoothOn.floatValue = isOn ? 1 : 0;
                SetKeyword(keyword, isOn);

                if (!isOn)
                {
                    EditorGUILayout.Space();
                }
            }
            return isOn;
        }

        private void SetKeyword(string keyword, bool isSet)
        {
            if (isSet)
            {
                m_target.EnableKeyword(keyword);
            }
            else
            {
                m_target.DisableKeyword(keyword);
            }
        }
    }
}
