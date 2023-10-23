Shader "DHC/BG_GroundWall" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_SpecPow ("Specular Pow", Range(10,200)) = 100
		_Scale("tiling", Range(0, 100)) = 1

	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		#pragma surface surf GroundLight //shadow

		sampler2D _MainTex;
	//	sampler2D _SpecTex; 
		float _SpecPow; 
		fixed _Scale;

		struct Input {      
			float2 uv_MainTex ;  //: TEXCOORD0;
		//	float2 uv_SpecTex; 
		};

		void surf (Input IN, inout SurfaceOutput o) {
			//_Scale *= 0.01;
			fixed4 tex = tex2D (_MainTex, IN.uv_MainTex*_Scale);
		  	o.Albedo = tex.rgb;
			o.Gloss = tex.a; // mainTex의 Alpha로 사용한다 
			o.Alpha = tex.a;
      }

		 float4 LightingGroundLight (SurfaceOutput s, float3 lightDir, float3 viewDir, float atten){
	   		 float4 final;

			 float3 DiffColor; 
	   		 float3 ndotl = saturate(dot(s.Normal, lightDir));
	   		 DiffColor = ndotl* s.Albedo* _LightColor0.rgb* atten;

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