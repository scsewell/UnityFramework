using UnityEngine;
using UnityEngine.Rendering;

namespace Framework.DeferredDecalling
{
    public class DecalBlock : AdvancedMatPropBlock
    {
        public void Draw(CommandBuffer buffer, Material mat, int pass, Decal decal, bool setAmbientLight)
        {
            Block.Clear();
            Block.SetFloat("_MaskMultiplier", decal.Intensity);
            Block.SetFloat("_Emission", decal.EmissionIntensity);
            Block.SetFloat("_LimitTo", (decal.LimitTo != null) ? decal.LimitTo.GetInstanceID() : float.NaN);

            if (setAmbientLight)
            {
                SphericalHarmonicsL2 probe;
                if (decal.UseLightProbes)
                {
                    LightProbes.GetInterpolatedProbe(decal.transform.position, decal.Renderer, out probe);
                }
                else
                {
                    probe = RenderSettings.ambientProbe;
                }
                SetAmbientLight(probe);
            }
            
            buffer.DrawMesh(CubeBuilder.Cube, decal.transform.localToWorldMatrix, mat, 0, pass, Block);
        }
    }
}
