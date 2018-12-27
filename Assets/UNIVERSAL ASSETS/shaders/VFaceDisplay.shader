Shader "Custom/VFaceDisplay"{

	Properties{
		_ColFront ("Front Color", Color) = (0.7, 0.7, 1.0, 1.0)
		_ColBack ("Back Color", Color) = (1.0, 0.7, 0.7, 1.0)
	}

	SubShader{

		Tags { "RenderType"="Opaque" }

		Pass{

			Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			fixed4 _ColFront;
			fixed4 _ColBack;

			struct appdata {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				return o;
			}
			
			fixed4 frag (v2f i, fixed facing : VFACE) : SV_Target {
				return ((facing > 0) ? _ColFront : _ColBack);
			}

			ENDCG
		}
	}
}
