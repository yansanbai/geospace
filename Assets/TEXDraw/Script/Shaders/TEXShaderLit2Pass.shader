
Shader "TEXDraw/Lit Transparent/x10 Samples"
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
			"TexMaterialType"="Standard"
			"TexMaterialAlts"="@TEXDraw/Lit Transparent"
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
		Lighting Off
		ZWrite Off
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
			fixed4 color : COLOR;
			float3 normal : NORMAL;
		};

		struct Input
		{
			half2 uv_Font0;
			half2 uv2_Font1;
			fixed4 color : COLOR;
		};

		ENDCG

		//Second Pass
		CGPROGRAM
		#pragma surface surf PPL alpha novertexlights noshadow novertexlights nolightmap nofog
		#include "UnityCG.cginc"
		
		sampler2D _Font8;
		sampler2D _Font9;
		sampler2D _FontA;
		sampler2D _FontB;
		sampler2D _FontC;
		sampler2D _FontD;
		sampler2D _FontE;
		sampler2D _FontF;
		
		fixed4 getTexPoint(half2 uv, half index)
		{
			fixed4 alpha;
			if(index == 8)
			alpha = tex2D(_Font8, uv);
			else if(index == 9)
			alpha = tex2D(_Font9, uv);
			else if(index == 10)
			alpha = tex2D(_FontA, uv);
			else if(index == 11)
			alpha = tex2D(_FontB, uv);
			else if(index == 12)
			alpha = tex2D(_FontC, uv);
			else if(index == 13)
			alpha = tex2D(_FontD, uv);
			else if(index == 14)
			alpha = tex2D(_FontE, uv);
			else if(index == 15)
			alpha = tex2D(_FontF, uv);
			else
			alpha = fixed4(0, 0, 0, 0);
			return alpha;
		}
		
		void surf (Input IN, inout SurfaceOutput o)
		{			
			half x, y, z;
			x = floor(IN.uv2_Font1.x*8+0.5h);
			y = floor(IN.uv2_Font1.y*4+0.5h);
			z = (y * 8) + x;
			fixed4 col2 = getTexPoint(IN.uv_Font0, z);
			fixed4 col = fixed4(max(IN.color, col2).rgb, IN.color.a * col2.a);
			o.Albedo = col.rgb;
			o.Alpha = col.a;
		} 

		ENDCG
		//First Pass
		CGPROGRAM
		#pragma surface surf PPL alpha novertexlights noshadow novertexlights nolightmap nofog
		#include "UnityCG.cginc"
		
		sampler2D _Font0;
		sampler2D _Font1;
		sampler2D _Font2;
		sampler2D _Font3;
		sampler2D _Font4;
		sampler2D _Font5;
		sampler2D _Font6;
		sampler2D _Font7;
		
		fixed4 getTexPoint(half2 uv, half index)
		{
			fixed4 alpha;
			if(index == 0)
			alpha = tex2D(_Font0, uv);
			else if(index == 1)
			alpha = tex2D(_Font1, uv);
			else if(index == 2)
			alpha = tex2D(_Font2, uv);
			else if(index == 3)
			alpha = tex2D(_Font3, uv);
			else if(index == 4)
			alpha = tex2D(_Font4, uv);
			else if(index == 5)
			alpha = tex2D(_Font5, uv);
			else if(index == 6)
			alpha = tex2D(_Font6, uv);
			else if(index == 7)
			alpha = tex2D(_Font7, uv);
			else if(index == 31)
			alpha = fixed4(1, 1, 1, 1);
			else
			alpha = fixed4(0, 0, 0, 0);
			return alpha;
		}
		
		void surf (Input IN, inout SurfaceOutput o)
		{			
			half x, y, z;
			x = floor(IN.uv2_Font1.x*8+0.5h);
			y = floor(IN.uv2_Font1.y*4+0.5h);
			z = (y * 8) + x;
			fixed4 col2 = getTexPoint(IN.uv_Font0, z);
			fixed4 col = fixed4(max(IN.color, col2).rgb, IN.color.a * col2.a);
		
			o.Albedo = col.rgb;
			o.Alpha = col.a;
		} 
		ENDCG
	}
	Fallback "TEXDraw/Default/Full"
}

