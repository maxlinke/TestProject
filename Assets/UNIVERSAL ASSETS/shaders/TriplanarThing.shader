// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/TriplanarThing" {

	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Tiling ("Tiling", Float) = 1.0
		[NoScaleOffset] _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Unlit ("Unlit", Range(0.0, 1.0)) = 0.0
	}

	SubShader {

		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM

		#pragma surface surf Lambert vertex:vert fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;
		fixed4 _Color;
		float _Tiling;
		float _Unlit;

		struct Input {
			float3 worldPos;
			float3 worldNormal;
			float3 coords;
		};

		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			o.worldNormal = UnityObjectToWorldNormal(v.normal).xyz;
			o.coords = o.worldPos * _Tiling;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			half3 blending = abs(IN.worldNormal);
			blending /= dot(blending, 1.0);

			fixed4 cx = tex2D (_MainTex, IN.coords.zy);
			fixed4 cy = tex2D (_MainTex, IN.coords.xz);
			fixed4 cz = tex2D (_MainTex, IN.coords.xy);

			fixed4 c = blending.x * cx + blending.y * cy + blending.z * cz;
			c *= _Color;

			o.Albedo = (1.0 - _Unlit) * c.rgb;
			o.Emission = _Unlit * c.rgb;
			o.Alpha = c.a;
		}

		ENDCG
	}
	FallBack "Diffuse"
}
