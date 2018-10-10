Shader "Custom/ColorGradeShader"{

	Properties{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Gradient ("Gradient", 2D) = "white" {}
	}

	SubShader{

		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			sampler2D _Gradient;

			struct appdata{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 normal : NORMAL;
			};

			struct v2f{
				float2 uv : TEXCOORD0;
				float4 normal : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v){
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.normal = v.normal;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target{
				fixed4 tex = tex2D(_MainTex, i.uv);
				tex.rgb *= _Color.rgb;

				//half lum = 0.299 * tex.r + 0.587 * tex.g + 0.114 * tex.b;
				//return tex2D(_Gradient, half2(lum, 0.5));

				return fixed4(i.uv, 1, 1);
			}
			ENDCG
		}
	}
}
