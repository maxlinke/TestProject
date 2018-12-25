Shader "Custom/NormalDebug" {

	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_DisplayNormals ("Display Normals", Range(0,1)) = 0.0
	}

	SubShader {

		Tags { "RenderType"="Opaque" }

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert

		sampler2D _MainTex;
		fixed4 _Color;
		float _DisplayNormals;

		struct Input {
			float2 uv_MainTex;
			float3 worldNormal;
		};

		void vert (inout appdata_base v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.worldNormal = UnityObjectToWorldNormal(v.normal).xyz;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 tex = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = tex.rgb * (1.0 - _DisplayNormals);
			o.Emission = IN.worldNormal * _DisplayNormals;
			o.Alpha = 1.0;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
