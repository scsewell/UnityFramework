using UnityEditor;
using UnityEngine;

namespace Framework.DeferredDecalling
{
    [CustomEditor(typeof(Decal))]
    [CanEditMultipleObjects]
    public class DecalEditor : Editor
    {
        private Decal m_target;

        private SerializedProperty m_material;
        private SerializedProperty m_intensity;
        private SerializedProperty m_emissionIntensity;
        private SerializedProperty m_limitTo;
        private SerializedProperty m_drawAlbedo;
        private SerializedProperty m_useLightProbes;
        private SerializedProperty m_drawNormalAndGloss;
        private SerializedProperty m_highQualityBlending;

        private void OnEnable()
        {
            m_target = target as Decal;

            m_material = serializedObject.FindProperty("m_material");
            m_intensity = serializedObject.FindProperty("m_intensity");
            m_emissionIntensity = serializedObject.FindProperty("m_emissionIntensity");
            m_limitTo = serializedObject.FindProperty("m_limitTo");
            m_drawAlbedo = serializedObject.FindProperty("m_drawAlbedo");
            m_useLightProbes = serializedObject.FindProperty("m_useLightProbes");
            m_drawNormalAndGloss = serializedObject.FindProperty("m_drawNormalAndGloss");
            m_highQualityBlending = serializedObject.FindProperty("m_highQualityBlending");
        }

        public override void OnInspectorGUI()
        {
            Material oldMat = m_target.Material;
            EditorGUILayout.PropertyField(m_material);
            Material mat = (Material)m_material.objectReferenceValue;

            if (m_target.Type == Decal.DecalType.Deferred)
            {
                bool renderAlbedo = mat.IsKeywordEnabled("ALBEDO_ON");
                bool renderEmission = mat.IsKeywordEnabled("EMISSION_ON");
                bool renderNormals = mat.IsKeywordEnabled("NORMALS_ON") || mat.IsKeywordEnabled("MASKED_NORMALS_ON");
                bool renderSpecSmooth = mat.IsKeywordEnabled("SPEC_SMOOTH_ON");

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Deferred Decal", EditorStyles.boldLabel);
                if (Camera.main != null && Camera.main.actualRenderingPath != RenderingPath.DeferredShading)
                {
                    EditorGUILayout.HelpBox("Main camera is not using the Deferred rendering path. " +
                        "Deferred decals will not be drawn. Current path: " + Camera.main.actualRenderingPath, MessageType.Error);
                }
                if (renderAlbedo)
                {
                    EditorGUILayout.PropertyField(m_drawAlbedo);
                    EditorGUI.BeginDisabledGroup(!m_drawAlbedo.boolValue);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_useLightProbes);
                    if (renderEmission)
                    {
                        EditorGUILayout.PropertyField(m_emissionIntensity);
                    }
                    EditorGUI.indentLevel--;
                }
                if (renderNormals || renderSpecSmooth)
                {
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.PropertyField(m_drawNormalAndGloss);
                    EditorGUI.BeginDisabledGroup(!m_drawNormalAndGloss.boolValue);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(m_highQualityBlending);
                    EditorGUI.indentLevel--;
                    EditorGUI.EndDisabledGroup();
                }
                EditorGUILayout.PropertyField(m_intensity);
                EditorGUILayout.PropertyField(m_limitTo);
            }
            else if (m_target.Type == Decal.DecalType.Unlit)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Unlit Decal", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(m_intensity);
                EditorGUILayout.PropertyField(m_limitTo);
            }
            else
            {
                EditorGUILayout.HelpBox("Select a material with a deferred decal shader.", MessageType.Info);
            }

            serializedObject.ApplyModifiedProperties();

            if (oldMat != mat)
            {
                m_target.OnMaterialUpdate();
            }
        }
    }
}
