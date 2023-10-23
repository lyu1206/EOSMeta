Shader "DHC/BG_Objects" {
	Properties {
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
	//	_SpecTex ("Specular Texture", 2D) = "white" {}
		_SpecPow ("Specular Pow", Range(10,200)) = 100
		//[HideInInspector]
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		
		CGPROGRAM
		#pragma surface surf GroundLight //shadow

		sampler2D _MainTex;
	//	sampler2D _SpecTex; 
		float _SpecPow; 

		struct Input {      
			float2 uv_MainTex ;  
		//	float2 uv_SpecTex; 
		};

		void surf (Input IN, inout SurfaceOutput o) {
        fixed4 tex = tex2D (_MainTex, IN.uv_MainTex);
	//	float4 sp = tex2D(_MainTex, IN.uv_SpecTex);
		  	o.Albedo = tex.rgb;
			o.Gloss = tex.a; // mainTex의 Alpha로 사용한다 
			o.Alpha = tex.a;
      }

		 float4 LightingGroundLight (SurfaceOutput s, float3 lightDir, float3 viewDir, float atten){
	   		 float4 final;

			 float3 DiffColor; 
	   		 float3 ndotl = saturate(dot(s.Normal, lightDir)*0.5+0.5);
	   		 DiffColor = ndotl* s.Albedo* _LightColor0.rgb* atten;

			 float3 SpecColor; 
			 float3 H = normalize(lightDir + viewDir);
			 float spec = saturate(dot(H, s.Normal)*0.5+0.7);
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