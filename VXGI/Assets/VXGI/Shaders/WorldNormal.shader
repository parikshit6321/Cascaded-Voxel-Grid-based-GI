Shader "Hidden/WorldNormal"
{
	Properties
	{
	}
	SubShader
	{
		// World position writing pass
		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0

			#include "UnityCG.cginc"

			uniform int _WorldVolumeBoundary;

			// Structure representing the input to the vertex shader
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
			};

			// Structure representing the input to the fragment shader
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float3 worldNormal : NORMAL;
			};

			// Vertex shader for the world normal writing pass
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				o.worldNormal = mul(_Object2World, v.normal);
				return o;
			}

			// Fragment shader for the world normal writing pass
			float4 frag(v2f i) : SV_Target
			{
				float3 normal = (i.worldNormal * 0.5) + 0.5;
				return float4(normal, 1.0);
			}

			ENDCG
		}
	}
}