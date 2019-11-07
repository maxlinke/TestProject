Shader "Custom/MatrixExperiments/MatrixDebug" {

    //use with a simple uv-plane

    Properties {
        _Scale ("Scale of difference", float) = 1.0
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float _Scale;
            float4x4 _DebugMatrix;
            // float4x4 _DebugMatrixA;
            // float4x4 _DebugMatrixB;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = float2(v.uv.x, 1.0 - v.uv.y);  //flipping the uv's so (0,0) is top left, x goes left, y goes down
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                int x = max(min(floor(i.uv.x * 4), 3), 0);
                int y = max(min(floor(i.uv.y * 4), 3), 0);
                float4x4 delta = UNITY_MATRIX_P - _DebugMatrix;
                // float4x4 delta = _DebugMatrixA - _DebugMatrixB;
                delta *= _Scale;
                delta += 0.5;
                fixed4 col = fixed4(1, 1, 1, 1);
                col.rgb = delta[x][y];
                col *= (1 - fwidth(x)) * (1 - fwidth(y));
                return col;
            }
            ENDCG
        }
    }
}
