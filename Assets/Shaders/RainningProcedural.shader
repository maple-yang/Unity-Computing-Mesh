Shader "Unlit/RainningProcedural"
{
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			Cull off
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#include "UnityCG.cginc"

			struct RainPanel
			{
    			float3 normal;
    			float3 binormal;
    			float3 position;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			float4 _MainTex_ST;
			StructuredBuffer<RainPanel> instancingBuffer;
			static const float2 vertices[4] = {
				float2(0.02, 0.1),
				float2(0.02, -0.1),
				float2(-0.02, -0.1),
				float2(-0.02, 0.1)
			};
			v2f vert (uint instancingID : SV_INSTANCEID, uint vertexID : SV_VertexID)
			{
				RainPanel p = instancingBuffer[instancingID];
			//	float2 vertOffset = vertices[instancingID];
				float3 worldPos = p.position;// + float3(0, 1, 0) * vertOffset.y + p.binormal * vertOffset.x;
				v2f o;
				o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1));
				return o;
			}
			
			float4 frag (v2f i) : SV_TARGET3
			{
				return 1;
			}
			ENDCG
		}
	}
}
