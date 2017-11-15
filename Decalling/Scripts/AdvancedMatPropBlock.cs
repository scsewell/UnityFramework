using UnityEngine;
using UnityEngine.Rendering;

namespace Framework.DeferredDecalling
{
    public class AdvancedMatPropBlock
    {
        private MaterialPropertyBlock m_block = new MaterialPropertyBlock();
        public MaterialPropertyBlock Block
        {
            get { return m_block; }
        }

        private Vector4[] m_probeCoeffs = new Vector4[7];
        
        public void SetAmbientLight(SphericalHarmonicsL2 probe)
        {
            for (int i = 0; i < 3; i++)
            {
                m_probeCoeffs[i].x = probe[i, 3];
                m_probeCoeffs[i].y = probe[i, 1];
                m_probeCoeffs[i].z = probe[i, 2];
                m_probeCoeffs[i].w = probe[i, 0] - probe[i, 6];
            }

            for (int i = 0; i < 3; i++)
            {
                m_probeCoeffs[i + 3].x = probe[i, 4];
                m_probeCoeffs[i + 3].y = probe[i, 5];
                m_probeCoeffs[i + 3].z = 3.0f * probe[i, 6];
                m_probeCoeffs[i + 3].w = probe[i, 7];
            }

            m_probeCoeffs[6].x = probe[0, 8];
            m_probeCoeffs[6].y = probe[1, 8];
            m_probeCoeffs[6].z = probe[2, 8];
            m_probeCoeffs[6].w = 1.0f;

            m_block.SetVector("unity_SHAr", m_probeCoeffs[0]);
            m_block.SetVector("unity_SHAg", m_probeCoeffs[1]);
            m_block.SetVector("unity_SHAb", m_probeCoeffs[2]);
            m_block.SetVector("unity_SHBr", m_probeCoeffs[3]);
            m_block.SetVector("unity_SHBg", m_probeCoeffs[4]);
            m_block.SetVector("unity_SHBb", m_probeCoeffs[5]);
            m_block.SetVector("unity_SHC", m_probeCoeffs[6]);
        }
    }
}
