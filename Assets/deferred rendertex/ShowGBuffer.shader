Shader "Custom/ShowGBuffer" {

    Properties{
		[HideInInspector] _MainTex ("Texture", 2D) = "white" {}
        _TexFactor ("Main Tex Factor", Range(0, 1)) = 0
        _DiffuseFactor ("Diffuse", Range(0, 1)) = 0
        _OcclusionFactor ("Occlusion", Range(0, 1)) = 0
        _SpecularFactor ("Specular", Range(0, 1)) = 0
        _SmoothnessFactor ("Smoothness", Range(0, 1)) = 0
        _NormalFactor ("Normals", Range(0, 1)) = 0
	}

	SubShader{

		Cull Off
		ZWrite Off
		ZTest Always

		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _MainTex_ST;

            sampler2D _CameraGBufferTexture0;
            sampler2D _CameraGBufferTexture1;
            sampler2D _CameraGBufferTexture2;

            float _TexFactor;
            float _DiffuseFactor;
            float _OcclusionFactor;
            float _SpecularFactor;
            float _SmoothnessFactor;
            float _NormalFactor;
            
			struct appdata{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert (appdata v){
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target{
				fixed4 tex = tex2D(_MainTex, i.uv);

                half4 gbuffer0 = tex2D (_CameraGBufferTexture0, i.uv); // Diffuse RGB, Occlusion A
                half4 gbuffer1 = tex2D (_CameraGBufferTexture1, i.uv); // Specular RGB, Smoothness A
                half4 gbuffer2 = tex2D (_CameraGBufferTexture2, i.uv); // Normal RGB

                fixed4 col;
                col.rgb = 0;
                col.rgb += _TexFactor * tex.rgb;
                col.rgb += _DiffuseFactor * gbuffer0.rgb;
                col.rgb += _OcclusionFactor * gbuffer0.a;
                col.rgb += _SpecularFactor * gbuffer1.rgb;
                col.rgb += _SmoothnessFactor * gbuffer1.a;
                col.rgb += _NormalFactor * gbuffer2.rgb;
                col.a = 1;
				return col;
			}
			ENDCG
		}
	}
}