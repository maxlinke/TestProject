Shader "Custom/Experimental/X_DeformationSnow" {

	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		[NoScaleOffset] _TopTex ("Top Texture", 2D) = "white" {}
		_TopTexTiling ("Top Texture Tiling", Float) = 1.0
		[NoScaleOffset] _BottomTex ("Bottom Texture", 2D) = "grey" {}
		_BottomTexTiling ("Bottom Texture Tiling", Float) = 1.0
		[PerRendererData] _DeformationTex ("Deformation Control Texture", 2D) = "black" {}
		[PerRendererData] _DeformationAmount ("Deformation Amount", Float) = 1.0
		_DebugShow ("Show Deformation Texture (Debug)", Range(0,1)) = 0
	}

	SubShader {

		Tags { "RenderType"="Opaque" }

		CGPROGRAM
		#pragma surface surf Lambert addshadow vertex:vert

		fixed4 _Color;
		sampler2D _TopTex;
		sampler2D _BottomTex;
		sampler2D _DeformationTex;
		float _DeformationAmount;
		float _TopTexTiling;
		float _BottomTexTiling;
		float _DebugShow;

		struct Input {
			float level;
			float4 coords;
			float2 defaultUV;
		};

		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.defaultUV = v.texcoord.xy;
			o.level = tex2Dlod(_DeformationTex, v.texcoord).r;
			v.vertex.y += o.level * _DeformationAmount;
			float2 wp = mul(unity_ObjectToWorld, v.vertex).xz;
			o.coords = float4(0,0,0,0);
			o.coords.xy += wp * _TopTexTiling;
			o.coords.zw += wp * _BottomTexTiling;
		}		

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 top = tex2D(_TopTex, IN.coords.xy);
			fixed4 bottom = tex2D(_BottomTex, IN.coords.zw);
			fixed4 c = lerp(bottom, top, IN.level) * _Color;
			o.Albedo = lerp(c.rgb, fixed3(0,0,0), _DebugShow);
			fixed4 deform = tex2D(_DeformationTex, IN.defaultUV);
			o.Emission = lerp(fixed3(0,0,0), deform.rgb, _DebugShow);
			o.Alpha = c.a;
		}

		ENDCG
	}

	FallBack "Diffuse"
}
