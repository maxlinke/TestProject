Shader "Custom/OnlyShadeSH9"{

	Properties{ }

	SubShader{

		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass{

			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata{
				float4 vertex : POSITION;
				float4 normal : NORMAL;
			};

			struct v2f{
				float4 vertex : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
				UNITY_FOG_COORDS(1)
			};
			
			v2f vert (appdata v){
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal).xyz;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target{
				i.worldNormal = normalize(i.worldNormal);
				fixed4 col = fixed4(ShadeSH9(half4(i.worldNormal, 1)), 1);
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
