
Shader "TEXDraw/Lit Bump Map/Full"
{
	Properties
	{
		
		_Specular("Specular", Color) = (0,0,0,1)
		_MainBump ("Diffuse Bump Map", 2D) = "bump" {}
		_Shininess ("Shininess", Range(0.01, 1.0)) = 0.2
		[Space]
		[MiniThumbTexture] _Font0("Font 0", 2D) = "white" {}
		[MiniThumbTexture] _Font1("Font 1", 2D) = "white" {}
		[MiniThumbTexture] _Font2("Font 2", 2D) = "white" {}
		[MiniThumbTexture] _Font3("Font 3", 2D) = "white" {}
		[MiniThumbTexture] _Font4("Font 4", 2D) = "white" {}
		[MiniThumbTexture] _Font5("Font 5", 2D) = "white" {}
		[MiniThumbTexture] _Font6("Font 6", 2D) = "white" {}
		[MiniThumbTexture] _Font7("Font 7", 2D) = "white" {}
		[Space]
		[MiniThumbTexture] _Font8("Font 8", 2D) = "white" {}
		[MiniThumbTexture] _Font9("Font 9", 2D) = "white" {}
		[MiniThumbTexture] _FontA("Font A", 2D) = "white" {}
		[MiniThumbTexture] _FontB("Font B", 2D) = "white" {}
		[MiniThumbTexture] _FontC("Font C", 2D) = "white" {}
		[MiniThumbTexture] _FontD("Font D", 2D) = "white" {}
		[MiniThumbTexture] _FontE("Font E", 2D) = "white" {}
		[MiniThumbTexture] _FontF("Font F", 2D) = "white" {}
		[Space]
		[MiniThumbTexture] _Font10("Font 10", 2D) = "white" {}
		[MiniThumbTexture] _Font11("Font 11", 2D) = "white" {}
		[MiniThumbTexture] _Font12("Font 12", 2D) = "white" {}
		[MiniThumbTexture] _Font13("Font 13", 2D) = "white" {}
		[MiniThumbTexture] _Font14("Font 14", 2D) = "white" {}
		[MiniThumbTexture] _Font15("Font 15", 2D) = "white" {}
		[MiniThumbTexture] _Font16("Font 16", 2D) = "white" {}
		[MiniThumbTexture] _Font17("Font 17", 2D) = "white" {}
		[Space]
		[MiniThumbTexture] _Font18("Font 18", 2D) = "white" {}
		[MiniThumbTexture] _Font19("Font 19", 2D) = "white" {}
		[MiniThumbTexture] _Font1A("Font 1A", 2D) = "white" {}
		[MiniThumbTexture] _Font1B("Font 1B", 2D) = "white" {}
		[MiniThumbTexture] _Font1C("Font 1C", 2D) = "white" {}
		[MiniThumbTexture] _Font1D("Font 1D", 2D) = "white" {}
		//[MiniThumbTexture] _Font1E("Font 1E", 2D) = "white" {}
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15
	}
	
	SubShader
	{
		LOD 250

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType"="Plane"
			"TexMaterialType"="RequireUV2"
		}

		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}
		
		Cull Off
		//ZWrite Off
		ZTest [unity_GUIZTestMode]
		Offset -1, -1
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaTest Greater 0
		
		
		CGINCLUDE
		half4 _Color;
		half4 _Specular;
		half4 _MainBump_ST;
		
		sampler2D _MainBump;

		half _Shininess;

			half4 LightingPPL (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
			{

			#ifndef USING_DIRECTIONAL_LIGHT
				lightDir = normalize(lightDir);
			#endif

				half diffuseFactor = max(0.0, dot(s.Normal, lightDir));
				
				#if SHADER_TARGET >= 30
				half shininess = s.Gloss * 250.0 + 4.0;

				// Phong shading model
				half reflectiveFactor = max(0.0, dot(-viewDir, reflect(lightDir, s.Normal)));
				// Blinn-Phong shading model
				//half reflectiveFactor = max(0.0, dot(s.Normal, normalize(lightDir + viewDir)));
				
				half specularFactor = pow(reflectiveFactor, shininess) * s.Specular;

				half4 c;
				c.rgb = (s.Albedo * diffuseFactor + _Specular.rgb * specularFactor) * _LightColor0.rgb;
				c.rgb *= atten;
				c.a = s.Alpha;
				#else
				//FOR 2.0: Turned off specularity since math instruction limit
				half4 c;
				c.rgb = (s.Albedo * diffuseFactor) * _LightColor0.rgb;
				c.rgb *= atten;
				c.a = s.Alpha;
				#endif
				clip (c.a - 0.01);
				return c;
			}


		
		void vert_t(inout appdata_full v)
		{
			v.texcoord2 = v.tangent;
		}
		struct Input
		{
			half2 uv_Font0;
			half2 uv2_Font1;
			half2 uv3_Font2;
			fixed4 color : COLOR;
		};

		ENDCG

		//Second Pass
		CGPROGRAM
		#define TEX_5_2
		#include "TEXDrawIncludes.cginc"
		#pragma surface surf PPL vertex:vert_t alpha novertexlights noshadow nolightmap nofog
		
		void surf (Input IN, inout SurfaceOutput o)
		{			
			fixed4 col2 = getTexPoint(IN.uv_Font0, IN.uv2_Font1);
			fixed4 col = mix(IN.color, col2);
			o.Albedo = col.rgb;
			o.Alpha = col.a;
			o.Normal = UnpackNormal(tex2D(_MainBump, TRANSFORM_TEX(IN.uv3_Font2, _MainBump)));
			o.Specular = _Specular.a;
			o.Gloss = _Shininess;
		} 
		ENDCG
		//Third Pass
		CGPROGRAM
		#define TEX_5_3
		#include "TEXDrawIncludes.cginc"
		#pragma surface surf PPL vertex:vert_t alpha novertexlights noshadow nolightmap nofog
		
		void surf (Input IN, inout SurfaceOutput o)
		{			
			fixed4 col2 = getTexPoint(IN.uv_Font0, IN.uv2_Font1);
			fixed4 col = mix(IN.color, col2);
			o.Albedo = col.rgb;
			o.Alpha = col.a;
			o.Normal = UnpackNormal(tex2D(_MainBump, TRANSFORM_TEX(IN.uv3_Font2, _MainBump)));
			o.Specular = _Specular.a;
			o.Gloss = _Shininess;
		} 
		ENDCG
		//Fourth Pass
		CGPROGRAM
		#define TEX_5_4
		#include "TEXDrawIncludes.cginc"
		#pragma surface surf PPL vertex:vert_t alpha novertexlights noshadow nolightmap nofog
		
		void surf (Input IN, inout SurfaceOutput o)
		{			
			fixed4 col2 = getTexPoint(IN.uv_Font0, IN.uv2_Font1);
			fixed4 col = mix(IN.color, col2);
			o.Albedo = col.rgb;
			o.Alpha = col.a;
			o.Normal = UnpackNormal(tex2D(_MainBump, TRANSFORM_TEX(IN.uv3_Font2, _MainBump)));
			o.Specular = _Specular.a;
			o.Gloss = _Shininess;
		} 
		ENDCG
		//Fifth Pass
		CGPROGRAM
		#define TEX_5_5
		#include "TEXDrawIncludes.cginc"
		#pragma surface surf PPL vertex:vert_t alpha novertexlights noshadow nolightmap nofog
		
		void surf (Input IN, inout SurfaceOutput o)
		{			
			fixed4 col2 = getTexPoint(IN.uv_Font0, IN.uv2_Font1);
			fixed4 col = mix(IN.color, col2);
			o.Albedo = col.rgb;
			o.Alpha = col.a;
			o.Normal = UnpackNormal(tex2D(_MainBump, TRANSFORM_TEX(IN.uv3_Font2, _MainBump)));
			o.Specular = _Specular.a;
			o.Gloss = _Shininess;
		} 
		ENDCG
		//First Pass
		CGPROGRAM
		#define TEX_5_1
		#include "TEXDrawIncludes.cginc"
		#pragma surface surf PPL vertex:vert_t alpha novertexlights noshadow nolightmap nofog
		
		void surf (Input IN, inout SurfaceOutput o)
		{			
			fixed4 col2 = getTexPoint(IN.uv_Font0, IN.uv2_Font1);
			fixed4 col = mix(IN.color, col2);
			o.Albedo = col.rgb;
			o.Alpha = col.a;
			o.Normal = UnpackNormal(tex2D(_MainBump, TRANSFORM_TEX(IN.uv3_Font2, _MainBump)));
			o.Specular = _Specular.a;
			o.Gloss = _Shininess;
		} 
		ENDCG
	}
	Fallback "TEXDraw/Lit Transparent/4 Pass"
}

