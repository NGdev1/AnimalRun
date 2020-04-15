Shader "Mobile/mParticles" {
	Properties {
	   _MainTex ("Font Texture", 2D)="white" {}
	   _Cutoff ("Alpha cutoff", Range (0,1)) = 0.5
	   _Color ("Main Color", Color) = (1,.5,.5,1)
	}

	SubShader {

	   Lighting On
	   Material {
            Diffuse [_Color]
            Ambient [_Color]
       }	
	   cull off Blend SrcAlpha OneMinusSrcAlpha AlphaTest Greater[_Cutoff]
	   Pass {
	      SetTexture [_MainTex] {
	        Combine texture * primary DOUBLE, texture * primary
	      }
	   }
	}
}