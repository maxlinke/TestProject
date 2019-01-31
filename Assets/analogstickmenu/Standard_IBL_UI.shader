Shader "Custom/UI/Standard_IBL" {

	Properties {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [NoScaleOffset] _MetallicGlossMap ("Metallic + Smoothness (A)", 2D) = "white" {}
        [NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" {}
        [NoScaleOffset] _EnvironmentMap ("Environment", CUBE) = "" {}
        _DiffuseMipLevel ("Diffuse Mip Level", Range(0.0, 20.0)) = 10.0

        [HideInInpsector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInpsector] _Stencil ("Stencil ID", Float) = 0
        [HideInInpsector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInpsector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInpsector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInpsector] _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader {

        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass {
            Name "Default"

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #include "UnityPBSLighting.cginc"

            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP

            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float4 normal   : NORMAL;
                float4 tangent  : TANGENT;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float3 worldNormal	 : TEXCOORD2;
                float3 tangentSpace0 : TEXCOORD3;
                float3 tangentSpace1 : TEXCOORD4;
                float3 tangentSpace2 : TEXCOORD5;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _MetallicGlossMap;
            sampler2D _BumpMap;
            samplerCUBE _EnvironmentMap;
            float _DiffuseMipLevel;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            v2f vert (appdata_t v) {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color;
				float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
				float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				float3 worldBitangent = cross(worldNormal, worldTangent) * tangentSign;
				OUT.worldPosition = mul(unity_ObjectToWorld, v.vertex);
				OUT.worldNormal = worldNormal;
				OUT.tangentSpace0 = float3(worldTangent.x, worldBitangent.x, worldNormal.x);
				OUT.tangentSpace1 = float3(worldTangent.y, worldBitangent.y, worldNormal.y);
				OUT.tangentSpace2 = float3(worldTangent.z, worldBitangent.z, worldNormal.z);
                return OUT;
            }

            UnityGI getGI (half3 normal, half3 viewDir, half smoothness) {
                UnityGI output;
                ResetUnityGI(output);

                UnityLight light;
                light.color = fixed3(0,0,0);
                light.dir = float3(0,1,0);

                half3 reflectedViewDir = reflect(-viewDir, normal);
                half roughness = 1.0 - (smoothness * smoothness);
                UnityIndirect indirect;
                indirect.diffuse = texCUBElod (_EnvironmentMap, half4(normal, _DiffuseMipLevel));
                indirect.specular = texCUBElod (_EnvironmentMap, half4(reflectedViewDir, roughness * _DiffuseMipLevel));

                output.light = light;
                output.indirect = indirect;
                return output;
            }

            fixed4 frag (v2f IN) : SV_Target {
            	float3 texNormal = UnpackNormal(tex2D(_BumpMap, IN.texcoord));
            	float3 worldNormal;
            	worldNormal.x = dot(IN.tangentSpace0, texNormal);
            	worldNormal.y = dot(IN.tangentSpace1, texNormal);
            	worldNormal.z = dot(IN.tangentSpace2, texNormal);

            	float3 worldViewDir = normalize(UnityWorldSpaceViewDir(IN.worldPosition));
            	float3 worldReflection = reflect(worldNormal, -worldViewDir);

                half4 tex = tex2D(_MainTex, IN.texcoord);
                half4 metal = tex2D(_MetallicGlossMap, IN.texcoord);

                half metallic = metal.r;
                half smoothness = metal.a;

                half3 albedo = (tex + _TextureSampleAdd) * IN.color * _Color;

                half3 specularTint;
	            half oneMinusReflectivity;
	            albedo = DiffuseAndSpecularFromMetallic(albedo, metallic, specularTint, oneMinusReflectivity);

                UnityGI gi = getGI(worldNormal, worldViewDir, smoothness);

                half4 color = UNITY_BRDF_PBS(
                    albedo,
                    specularTint,
                    oneMinusReflectivity,
                    smoothness,
                    worldNormal,
                    worldViewDir,
                    gi.light,
                    gi.indirect
                );
                color.a = tex.a;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }

        ENDCG

        }
    }
}