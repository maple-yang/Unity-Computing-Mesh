Shader "Hidden/ShadowMask"
{
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			float4x4 _InvVP;
			float3 _ShadowCamPos;
			float4 _ShadowCamDirection;
			float _ShadowCamFarClip;
			float4x4 _ShadowMapVP;
			Texture2D _CameraDepthTexture; SamplerState sampler_CameraDepthTexture;
			Texture2D _DirShadowMap; SamplerState sampler_DirShadowMap;
			static const int2 offsets[8] = 
			{
				int2(1,0),
				int2(-1,0),
				int2(0,1),
				int2(0, -1),
				int2(-1,-1),
				int2(-1,1),
				int2(1,-1),
				int2(1,1)
			};
			float4 frag (v2f i) : SV_Target
			{
				float depth = _CameraDepthTexture.Sample(sampler_CameraDepthTexture, i.uv);
				float4 worldPos = mul(_InvVP, float4(i.uv * 2 - 1, depth, 1));
				
				float4 shadowPos = mul(_ShadowMapVP, worldPos);
				float2 shadowUV = shadowPos.xy / shadowPos.w;
				shadowUV = shadowUV * 0.5 + 0.5;
				worldPos /= worldPos.w;
				float dist = dot(_ShadowCamDirection.xyz, worldPos.xyz - _ShadowCamPos);
				dist /= _ShadowCamFarClip;
				float value = dist < _DirShadowMap.Sample(sampler_DirShadowMap, shadowUV).g;
				return value;
			}
			ENDCG
		}
	}
}
