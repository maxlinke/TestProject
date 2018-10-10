Shader "Custom/NormalExtrudeShader"{

	Properties{
		_Extrusion ("Extrusion", float) = 1.0
	}

	SubShader{

		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog			
			#include "UnityCG.cginc"

			float _Extrusion;

			struct appdata{
				float4 vertex : POSITION;
				float3 normal: NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : TEXCOORD1;
				UNITY_FOG_COORDS(2)
			};
			
			v2f vert (appdata v){
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex + (_Extrusion * v.normal));
				o.uv = v.uv;
				o.normal = v.normal;
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target{
				fixed4 col = fixed4(i.normal.xyz, 1);
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}

			ENDCG

		}
	}
}
