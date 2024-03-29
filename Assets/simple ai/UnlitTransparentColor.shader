﻿Shader "Unlit/Transparent Color" {

	Properties {
		_Color ("Color", Color) = (1,1,1,1)
	}

	SubShader {

		ZWrite Off
		ZTest LEqual
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha

		Tags { "Queue"="Transparent" }
		LOD 100

		Pass {

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			fixed4 _Color;

			struct appdata {
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				UNITY_FOG_COORDS(1)
			};
			
			v2f vert (appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target {
				fixed4 col = _Color;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}

			ENDCG
		}
	}
}
