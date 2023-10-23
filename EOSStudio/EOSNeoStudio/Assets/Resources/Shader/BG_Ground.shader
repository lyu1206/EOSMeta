Shader "DHC/BG_Ground" {
	Properties {
		_MainTex ("Top Tex", 2D) = "white" {}
		_SpecPow("Specular Pow", Range(10,200)) = 100
		_Scale ("tiling", Range(0, 100)) = 1
		_NoiseTex ("NoiseTex", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		#pragma surface surf GroundLight //noshadow

		sampler2D _MainTex;
		sampler2D _NoiseTex;
		fixed _Scale;
		float _SpecPow;

		struct Input {
			float3 worldPos;
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) {
			_Scale *= 0.01;
			fixed2 TopUV = float2(IN.worldPos.x, IN.worldPos.z);
			fixed4 Toptex = tex2D (_MainTex, TopUV*_Scale);
			fixed4 Noisetex = tex2D(_NoiseTex, IN.uv_MainTex);
	
			o.Albedo = Toptex.rgb * Noisetex.rgb; 
			o.Gloss = Toptex.a ; // Toptex의 Alpha를 Specular 로 쓴다 
			o.Alpha = Toptex.a;
			}

		 float4 LightingGroundLight (SurfaceOutput s, float3 lightDir, float3 viewDir, float atten){
	   		 float4 final;

			 float3 DiffColor;
	   		 float3 ndotl = saturate(dot(s.Normal, lightDir));
	   		 DiffColor.rgb = ndotl* s.Albedo* _LightColor0.rgb* atten;

			 float3 SpecColor;
			 float3 H = normalize(lightDir + viewDir);
			 float spec = saturate(dot(H, s.Normal));
			 spec = pow(spec, _SpecPow);
			 SpecColor = spec * s.Gloss;

			 final.rgb = DiffColor.rgb + SpecColor.rgb;
			 final.a = s.Alpha;
			 return final;
		   	}	
		ENDCG
	}
	FallBack "Diffuse"
}