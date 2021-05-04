
Shader "TEXDraw/Lit Transparent/Full"
{
	Properties
	{
		
		_Specular("Specular", Color) = (0,0,0,1)
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
		[MiniThumbTexture] _Font1E("Font 1E", 2D) = "white" {}
		
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
			"TexMaterialType"="Surface"
			"TexMaterialAlts"="X:10"
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
		
		half4 LightingPPL (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
		{
				half3 nNormal = normalize(s.Normal);
				half shininess = s.Gloss * 250.0 + 4.0;

			#ifndef USING_DIRECTIONAL_LIGHT
				lightDir = normalize(lightDir);
			#endif

				// Phong shading model
				half reflectiveFactor = max(0.0, dot(-viewDir, reflect(lightDir, nNormal)));

				// Blinn-Phong shading model
				//half reflectiveFactor = max(0.0, dot(nNormal, normalize(lightDir + viewDir)));
				
				half diffuseFactor = max(0.0, dot(nNormal, lightDir));
				half specularFactor = pow(reflectiveFactor, shininess) * s.Specular;

				half4 c;
				c.rgb = (s.Albedo * diffuseFactor + _Specular.rgb * specularFactor) * _LightColor0.rgb;
				c.rgb *= atten;
				c.a = s.Alpha;
				clip (c.a - 0.01);
				return c;
		}

		struct appdata_t
		{
			float4 vertex : POSITION;
			float2 texcoord : TEXCOORD0;
			float2 texcoord1 : TEXCOORD1;
			float2 texcoord2 : TEXCOORD2;
			fixed4 color : COLOR;
			float3 normal : NORMAL;
		};

		void vert_t(inout appdata_t v)
		{
			//v.texcoord2 = v.tangent;
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
		#define TEX_4_2
		#include "TEXDrawIncludes.cginc"
		#pragma surface surf PPL vertex:vert_t alpha novertexlights noshadow nolightmap nofog
		
		void surf (Input IN, inout SurfaceOutput o)
		{			
			fixed4 col2 = getTexPoint(IN.uv_Font0, IN.uv2_Font1.x);
			fixed4 col = mix(IN.color, col2);
			o.Albedo = col.rgb;
			o.Alpha = col.a;
		} 

		ENDCG
		//Third Pass
		CGPROGRAM
		#define TEX_4_3
		#include "TEXDrawIncludes.cginc"
		#pragma surface surf PPL vertex:vert_t alpha novertexlights noshadow nolightmap nofog
		void surf (Input IN, inout SurfaceOutput o)
		{			
			fixed4 col2 = getTexPoint(IN.uv_Font0, IN.uv2_Font1.x);
			fixed4 col = mix(IN.color, col2);
			o.Albedo = col.rgb;
			o.Alpha = col.a;
		} 
		ENDCG
		//Fourth Pass
		CGPROGRAM
		#define TEX_4_4
		#include "TEXDrawIncludes.cginc"
		#pragma surface surf PPL vertex:vert_t alpha novertexlights noshadow nolightmap nofog
		
		void surf (Input IN, inout SurfaceOutput o)
		{			
			fixed4 col2 = getTexPoint(IN.uv_Font0, IN.uv2_Font1.x);
			fixed4 col = mix(IN.color, col2);
			o.Albedo = col.rgb;
			o.Alpha = col.a;
		} 
		ENDCG
		//First Pass
		CGPROGRAM
		#define TEX_4_1
		#include "TEXDrawIncludes.cginc"
		#pragma surface surf PPL vertex:vert_t alpha novertexlights noshadow nolightmap nofog
		void surf (Input IN, inout SurfaceOutput o)
		{			
			fixed4 col2 = getTexPoint(IN.uv_Font0, IN.uv2_Font1.x);
			fixed4 col = mix(IN.color, col2);
			o.Albedo = col.rgb;
			o.Alpha = col.a;
		} 
		ENDCG
	}
	Fallback "TEXDraw/Default/4 Pass"
}

