// Compiled shader for all platforms, uncompressed size: 0.6KB

Shader "Mobile/Particles/mParticle" {
	Properties{
	 _MainTex("Particle Texture", 2D) = "white" {}
	_Color("Main Color", Color) = (1,1,1,1)
	}
		SubShader{
		 Tags { "QUEUE" = "Transparent" "IGNOREPROJECTOR" = "true" "RenderType" = "Transparent" }
		 Pass {
		  Tags { "QUEUE" = "Transparent" "IGNOREPROJECTOR" = "true" "RenderType" = "Transparent" }
		  Lighting On

				Material{
				Diffuse[_Color]
				Ambient[_Color]
				}

		  ZWrite Off
		  Cull Off

		  Blend SrcAlpha OneMinusSrcAlpha


		  SetTexture[_MainTex] { combine texture * primary }
		}
	}
}