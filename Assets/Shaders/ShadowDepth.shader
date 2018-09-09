Shader "Hidden/ShadowDepth"
{
	SubShader
	{
		Tags {"RenderType" = "Opaque"}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			float4 _ShadowCamDirection;
			float _ShadowCamFarClip;
			float4x4 _ShadowMapVP;
			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD1;
				float3 normal : TEXCOORD2;
			};

			v2f vert (appdata v)
			{
				v2f o;
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.vertex = mul(UNITY_MATRIX_VP, worldPos);
				o.worldPos = worldPos.xyz;
				o.normal = mul((float3x3)unity_ObjectToWorld, v.normal);
				return o;
			}
			float3 _ShadowCamPos;
			float4 frag (v2f i) : SV_Target
			{
				float dotValue = dot(_ShadowCamDirection.xyz, normalize(i.normal));
				float dist = dot(_ShadowCamDirection.xyz, i.worldPos - _ShadowCamPos) + _ShadowCamDirection.w;
				dist /= _ShadowCamFarClip;
				return dist;
			}
			ENDCG
		}
	}
}
