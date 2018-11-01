Shader "Custom/LightmapEmissionTesting/surf_opaque_uvdisplay" {

	Properties {
		_Blue ("Blue", Range(0,1)) = 1.0
	}

	SubShader {

		Tags { "RenderType" = "Opaque" }

		CGPROGRAM
		#pragma surface surf Lambert vertex:vert
		#pragma target 3.0

		fixed _Blue;

		struct Input {
			float2 coords;
		};

		void vert (inout appdata_full v, out Input o) {
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.coords = v.texcoord;
		}

		void surf (Input IN, inout SurfaceOutput o) {
			fixed4 c = fixed4(1,1,1,1);
			c.rg = IN.coords;
			c.b = _Blue;
			o.Albedo = fixed4(0,0,0,0);
			o.Emission = c;
			o.Alpha = c.a;
		}

		ENDCG
	}
	FallBack "Diffuse"
}
