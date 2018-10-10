// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/ShadowMapTestShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{

			Tags {"LightMode" = "ForwardBase"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile_fwdbase
			#pragma multi_compile _ VERTEXLIGHT_ON LIGHTMAP_ON
			
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv1:	TEXCOORD1;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 lightmapUV : TEXCOORD1;
				float4 worldPos : TEXCOORD3;
				UNITY_FOG_COORDS(4)
				//UNITY_SHADOW_COORDS(3)
				SHADOW_COORDS(5)
				//LIGHTING_COORDS(5,6)
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			
			v2f vert (appdata v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.lightmapUV = v.uv1 * unity_LightmapST.xy + unity_LightmapST.zw;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				UNITY_TRANSFER_FOG(o,o.pos);
				//UNITY_TRANSFER_SHADOW(o, o.lightmapUV);
				TRANSFER_SHADOW(o);
				//TRANSFER_VERTEX_TO_FRAGMENT(o);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 tex = tex2D(_MainTex, i.uv) * _Color;

				fixed4 c;

				//fixed atten;
				//UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
				//fixed atten = LIGHT_ATTENUATION(i);
				fixed atten = SHADOW_ATTENUATION(i);

				c = atten;

				#ifdef LIGHTMAP_ON
					c.rgb *= DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.lightmapUV));
				#endif

				UNITY_APPLY_FOG(i.fogCoord, c);
				return c;
			}
			ENDCG
		}

		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}
}
