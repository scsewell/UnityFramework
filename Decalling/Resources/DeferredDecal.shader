Shader "Decalling/Deferred Decal"
{
	Properties
	{
		_MaskTex("Mask", 2D) = "white" {}
		[PerRendererData] _MaskMultiplier("Mask (Multiplier)", Float) = 1.0
		_MaskNormals("Mask Normals", Float) = 1.0
		[PerRendererData] _LimitTo("Limit To", Float) = 0

		_Albedo_On("Enable Albedo", Float) = 0
		_MainTex("Albedo", 2D) = "white" {}
		[HDR] _Color("Albedo (Multiplier)", Color) = (1,1,1,1)

		_Emission_On("Enable Emission", Float) = 0
		_EmissionTex("Emission", 2D) = "white" {}
		[HDR] _EmissionColor("Emission Color", Color) = (0,0,0,0)

		_Normal_On("Enable Normals", Float) = 0
		[Normal] _NormalTex ("Normal", 2D) = "bump" {}
		_NormalMultiplier ("Normal (Multiplier)", Float) = 1.0

		_Spec_Smooth_On ("Enable Spec Smooth", Float) = 0
		_SpecularTex ("Specular", 2D) = "white" {}
		_SpecularMultiplier ("Specular (Multiplier)", Color) = (0.2, 0.2, 0.2, 1.0)

		_SmoothnessTex ("Smoothness", 2D) = "white" {}
		_SmoothnessMultiplier ("Smoothness (Multiplier)", Range(0.0, 1.0)) = 0.5

		_DecalBlendMode("Blend Mode", Float) = 0
		_DecalSrcBlend("SrcBlend", Float) = 1.0
		_DecalDstBlend("DstBlend", Float) = 10.0
		_NormalBlendMode("Normal Blend Mode", Float) = 0

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

		// Pass 0: Albedo and emission / lighting
		Pass
		{
			Blend [_DecalSrcBlend] [_DecalDstBlend]

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ EMISSION_ON
			#pragma multi_compile __ UNITY_HDR_ON
			#pragma multi_compile __ SMOOTH_EDGE
			#pragma multi_compile_instancing
			#include "DecallingCommon.cginc"

			void frag(v2f i, out float4 outAlbedo : SV_Target0, out float4 outEmission : SV_Target1)
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

				outAlbedo = float4(color.rgb * color.a, color.a);

				color *= float4(ShadeSH9(float4(gbuffer_normal, 1.0f)), 1.0f);

#if EMISSION_ON
				float4 emission = tex2D(_EmissionTex, texUV) * _EmissionColor;
				color.rgb += emission.rgb * emission.a * UNITY_ACCESS_INSTANCED_PROP(_Emission_arr, _Emission);
#endif

#ifndef UNITY_HDR_ON
				// Handle logarithmic encoding in Gamma space
				color.rgb = exp2(-color.rgb);
#endif

				outEmission = float4(color.rgb * color.a, color.a);
			}
			ENDCG
		}

		// Pass 1: Normals and specular / smoothness
		Pass
		{
			Blend Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile __ NORMALS_ON MASKED_NORMALS_ON
			#pragma multi_compile __ SPEC_SMOOTH_ON
			#pragma multi_compile __ SMOOTH_EDGE
			#pragma multi_compile_instancing
			#include "DecallingCommon.cginc"

			void frag(v2f i, out float4 outSpecSmoothness : SV_Target0, out float4 outNormal : SV_Target1)
			{
				DEFERRED_FRAG_HEADER

				float3 gbuffer_normal = tex2D(_CameraGBufferTexture2Copy, uv) * 2.0f - 1.0f;

#if SMOOTH_EDGE
				FACING_CLIP_SMOOTH
#else
				FACING_CLIP
#endif

#if NORMALS_ON || MASKED_NORMALS_ON
				float3 decalBitangent;
				if (_NormalBlendMode == 0)
				{
					// Reorient decal
					i.decalNormal = gbuffer_normal;
					decalBitangent = cross(i.decalNormal, i.decalTangent);
					float3 oldDecalTangent = i.decalTangent;
					i.decalTangent = cross(i.decalNormal, decalBitangent);
					
					if (dot(oldDecalTangent, i.decalTangent))
					{
						i.decalTangent *= -1;
					}
				}
				else
				{
					decalBitangent = cross(i.decalNormal, i.decalTangent);
				}

				// Get normal from normal map
				float3 normal = UnpackNormal(tex2D(_NormalTex, texUV));
				normal.xy *= _NormalMultiplier;
				normal = mul(normal, half3x3(i.decalTangent, decalBitangent, i.decalNormal));

				// Simple alpha blending of normals
				// TODO: Any more advanced blending feasible in world-space and worthwhile?
				//   http://blog.selfshadow.com/publications/blending-in-detail/
#if MASKED_NORMALS_ON
				float normalMask = mask;
#elif SMOOTH_EDGE
				float normalMask = UNITY_ACCESS_INSTANCED_PROP(_MaskMultiplier_arr, _MaskMultiplier) * facing;
#else
				float normalMask = UNITY_ACCESS_INSTANCED_PROP(_MaskMultiplier_arr, _MaskMultiplier);
#endif
				normal = (1.0f - normalMask) * gbuffer_normal + normalMask * normal;
				normal = normalize(normal);

				// Write normal
				outNormal = float4(normal * 0.5f + 0.5f, 1);
#endif

#if SPEC_SMOOTH_ON
				// Get specular / smoothness from GBuffer
				float4 specSmoothness = tex2D(_CameraGBufferTexture1Copy, uv);

				// Get new values from textures
				float4 specularVal = tex2D(_SpecularTex, texUV);
				float4 smoothnessVal = tex2D(_SmoothnessTex, texUV);

				// Write, interpolate between old and new values
				outSpecSmoothness = float4(lerp(specSmoothness.xyz, smoothnessVal.rgb * _SpecularMultiplier.rgb, mask * specularVal.a * _SpecularMultiplier.a),
										   lerp(specSmoothness.a, smoothnessVal.r * _SmoothnessMultiplier, mask * smoothnessVal.a * _SpecularMultiplier.a));
#endif
				}
			ENDCG
		}
	}
	Fallback Off
}