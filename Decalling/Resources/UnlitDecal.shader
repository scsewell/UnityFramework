Shader "Decalling/Unlit"
{
	Properties
	{
		_MaskTex("Mask", 2D) = "white" {}
		[PerRendererData] _MaskMultiplier("Mask (Multiplier)", Float) = 1.0

		_MainTex("Albedo", 2D) = "white" {}
		[HDR] _Color("Albedo (Multiplier)", Color) = (1,1,1,1)

		_DecalBlendMode("Blend Mode", Float) = 0
		_DecalSrcBlend("SrcBlend", Float) = 1.0
		_DecalDstBlend("DstBlend", Float) = 10.0
			
		_MaxAngle("Max Amgle", Float) = 0.5			// cos(60 * (PI/180))
		_SmoothEdge_On("Smooth Edge", Float) = 1
		_SmoothMaxAngle("Max Amgle", Float) = 1.047	// 60 * (PI/180)
		_SmoothAngle("Smooth Angle", Float) = 0.175	// 10 * (PI/180)
	}

	CustomEditor "Framework.DeferredDecalling.DecalShaderGUI"

	SubShader
	{
		Cull Front
		ZTest GEqual
		ZWrite Off
	
		// Pass 0: Unlit decal
		Pass
		{
			Blend [_DecalSrcBlend] [_DecalDstBlend]

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ SMOOTH_EDGE
			#pragma multi_compile_instancing
			#include "DecallingCommon.cginc"

			float4 frag(v2f i) : SV_Target0
			{
				DEFERRED_FRAG_HEADER

				float3 gbuffer_normal = tex2D(_CameraGBufferTexture2, uv) * 2.0f - 1.0f;

#if SMOOTH_EDGE
				FACING_CLIP_SMOOTH
#else
				FACING_CLIP
#endif

				float4 color = tex2D(_MainTex, texUV) * _Color;
				color.a *= mask;

				return float4(color.rgb * color.a, color.a);
			}
			ENDCG
		}
	}
	Fallback Off
}
