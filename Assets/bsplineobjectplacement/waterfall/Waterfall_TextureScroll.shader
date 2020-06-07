Shader "Custom/Waterfall_TextureScroll" {

    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        _PreTexST ("Pre Texture Offset & Tiling", Vector) = (0, 0, 1, 1)
        _YPow ("UV Y Power", float) = 1.0
        _PostTexST ("Post Texture Offset & Tiling", Vector) = (0, 0, 1, 1)
        _ScrollSpeed ("Scroll Speed", float) = 1.0
        _BendAmount ("Bend Amount", float) = 1.0
    }

    SubShader {

        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert
        #pragma target 3.0

        fixed4 _Color;
        sampler2D _MainTex;
        float4 _PreTexST;
        float _YPow;
        float4 _PostTexST;
        float _ScrollSpeed;
        float _BendAmount;

        struct Input {
            float2 pretransformedUV;
        };

        void vert(inout appdata_full v, out Input o){
			UNITY_INITIALIZE_OUTPUT(Input, o);
            o.pretransformedUV = (v.texcoord.xy * _PreTexST.zw) + _PreTexST.xy;
            float2 uv = v.texcoord.xy;
            // TODO all the vertex offsetting AND NORMAL CALCULATING!!!

            // old code, might be useful
			// o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			// o.worldNormal = UnityObjectToWorldNormal(v.normal);
		}

        void surf (Input IN, inout SurfaceOutput o) {
            float texUVY = pow(IN.pretransformedUV.y, _YPow) + _Time.y * _ScrollSpeed;
            float2 texUV = (float2(IN.pretransformedUV.x, texUVY) * _PostTexST.zw) + _PostTexST.xy;
            fixed4 c = tex2D (_MainTex, texUV) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }

        ENDCG
    }

    FallBack "Diffuse"
}
