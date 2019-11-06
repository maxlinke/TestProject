
Shader "Custom/DeferredVsForward" {

    Properties {
        _ForwardColor ("Forward Color", color) = (0.8, 0.2, 0.2, 1)
        _DeferredColor ("Deferred Color", color) = (0.2, 0.2, 0.8, 1)
    }

    SubShader {

        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf DeferredVsForward

        // static const fixed4 _ForwardColor = fixed4(0.8, 0.2, 0.2, 1);
        // static const fixed4 _DeferredColor = fixed4(0.2, 0.2, 0.8, 1);

        fixed4 _ForwardColor;
        fixed4 _DeferredColor;

        struct Input {
            float4 color : COLOR;
        };

        half4 LightingDeferredVsForward (SurfaceOutput s, half3 viewDir, UnityGI gi) {
            half4 c;
            c.rgb = s.Albedo * _ForwardColor;
            c.a = s.Alpha;
            return c;
        }

        half4 LightingDeferredVsForward_Deferred (SurfaceOutput s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2) {
            UnityStandardData data;
            data.diffuseColor   = 0;
            data.occlusion      = 1;
            data.specularColor  = 0;
            data.smoothness     = 0;
            data.normalWorld    = s.Normal;

            UnityStandardDataToGbuffer(data, outGBuffer0, outGBuffer1, outGBuffer2);

            half4 emission = half4(s.Albedo * _DeferredColor, 1);

            // #ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
            //     emission.rgb += s.Albedo * gi.indirect.diffuse;
            // #endif

            return emission;
        }

        inline void LightingDeferredVsForward_GI (SurfaceOutput s, UnityGIInput data, inout UnityGI gi) {
            gi = UnityGlobalIllumination (data, 1.0, s.Normal);
        }

        void surf (Input IN, inout SurfaceOutput o) {
            o.Albedo = fixed3(1,1,1);
            o.Alpha = 1;
        }

        ENDCG
    }
    FallBack "Diffuse"
}
