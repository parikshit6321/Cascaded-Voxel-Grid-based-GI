Shader "Hidden/ColorShader"
{
	Properties
	{
	}
	SubShader
	{
		// Color writing pass
		Pass
		{
			CGPROGRAM
		
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			// Structure representing the input to the vertex shader
			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			// Structure representing the input to the fragment shader
			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			// Vertex shader for the color writing pass
			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				o.color = v.color;
				return o;
			}

			// Fragment shader for the color writing pass
			float4 frag(v2f i) : SV_Target
			{
				return i.color;
			}

			ENDCG
		}
	}
}