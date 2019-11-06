Shader "Custom/MatrixExperiments/FwdBaseLitCustomMatrix" {
    
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }

    SubShader {

        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        ZTest LEqual
        ZWrite On
        Cull Back

        Pass {

            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _Color;

            float4x4 CustomMVPMatrix;
            float4x4 CustomModelMatrix;
            float4x4 CustomNormalMatrix;

            struct appdata {
                float4 vertex : POSITION;
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 worldNormal : TEXCOORD2;
                float3 lightDir : TEXCOORD3;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = mul(CustomMVPMatrix, v.vertex);
                // o.vertex = mul(CustomMVPMatrix, o.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(CustomModelMatrix, v.vertex).xyz;
                o.worldNormal = mul(CustomNormalMatrix, v.normal).xyz;
                o.lightDir = WorldSpaceLightDir(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                fixed diff = 0.5 * saturate(dot(i.lightDir, i.worldNormal)) + 0.5;
                col *= diff;
                return col;
            }
            ENDCG
        }
    }

    FallBack "VertexLit"

}
