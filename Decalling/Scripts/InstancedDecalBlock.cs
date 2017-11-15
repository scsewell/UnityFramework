using UnityEngine;
using UnityEngine.Rendering;

namespace Framework.DeferredDecalling
{
    public class InstancedDecalBlock : AdvancedMatPropBlock
    {
        private const int INSTANCE_BUFFER_SIZE = 500;
        
        private Matrix4x4[] m_matrices      = new Matrix4x4[INSTANCE_BUFFER_SIZE];
        private float[] m_fadeValues        = new float[INSTANCE_BUFFER_SIZE];
        private float[] m_emissionValues    = new float[INSTANCE_BUFFER_SIZE];
        private float[] m_limitToIDValues   = new float[INSTANCE_BUFFER_SIZE];
        
        private int m_instanceCount = 0;
        public int InstanceCount
        {
            get { return m_instanceCount; }
        }

        public bool AddInstanceValues(Decal decal)
        {
            m_matrices[m_instanceCount]         = decal.transform.localToWorldMatrix;
            m_fadeValues[m_instanceCount]       = decal.Intensity;
            m_emissionValues[m_instanceCount]   = decal.EmissionIntensity;
            m_limitToIDValues[m_instanceCount]  = decal.LimitTo ? decal.LimitTo.GetInstanceID() : float.NaN;
            m_instanceCount++;
            return m_instanceCount == INSTANCE_BUFFER_SIZE;
        }

        public void DrawInstances(CommandBuffer buffer, Material mat, int pass)
        {
            Block.Clear();
            Block.SetFloatArray("_MaskMultiplier", m_fadeValues);
            Block.SetFloatArray("_Emission", m_emissionValues);
            Block.SetFloatArray("_LimitTo", m_limitToIDValues);
            SetAmbientLight(RenderSettings.ambientProbe);
            buffer.DrawMeshInstanced(CubeBuilder.Cube, 0, mat, pass, m_matrices, m_instanceCount, Block);
            m_instanceCount = 0;
        }
    }
}
