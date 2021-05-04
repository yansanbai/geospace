Shader "TEXDraw/Gradient/Full"
{
	Properties
	{
		_Grad0("Grad 0", Color) = (1, 1, 1, 1)
		_Grad1("Grad 1", Color) = (1, 1, 1, 0.5)
		_GradRot("Grad Rotation", Range(0, 6.29)) = 0
		_GradShift("Grad Shift", Range(-0.5, 1)) = 0
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
		Tags 
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"TexMaterialType"="RequireUV2"
			"TexMaterialAlts"="X:8,10,18"
		}
		Lighting Off 
		Cull Off 
		ZTest [unity_GUIZTestMode]
		ZWrite Off 
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]
	
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp] 
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}
		CGINCLUDE
			#include "UnityCG.cginc"
		
		half4 _Grad0;
		half4 _Grad1;
		half _GradRot;
		half _GradShift;
			
		half4 lerp4(half4 a, half4 b, half t)
		{
			if(t < 0)
				t = 1+t;
			if(t > 1)
				t = 2-t;
			return (b - a) * t + a;
		}
		
		half4 grad(half2 uv2)
		{
			return lerp4(_Grad0, _Grad1, (uv2.y * cos(_GradRot) + uv2.x * sin(_GradRot))*(_GradShift+1));
		}

				
		ENDCG
		
		Pass
		{
			Name "SecondaryPass"
			CGPROGRAM
			#define TEX_4_2
			#define TEX_X
			#include "TEXDrawIncludes.cginc"
            #pragma vertex vert
			#pragma fragment frag
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = getTexPoint(i.uv, i.uv1.x);
				return mask(mix(i.color, col) * grad(i.uv2), i.world);
			}

			ENDCG
		}
		Pass
		{
			Name "ThirdPass"
			CGPROGRAM
			#define TEX_4_3
			#include "TEXDrawIncludes.cginc"
            #pragma vertex vert
			#pragma fragment frag
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = getTexPoint(i.uv, i.uv1.x);
				return mask(mix(i.color, col) * grad(i.uv2), i.world);
			}

			ENDCG
		}
		Pass
		{
			Name "FourthPass"
			CGPROGRAM
			#define TEX_4_4
			#include "TEXDrawIncludes.cginc"
            #pragma vertex vert
			#pragma fragment frag
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = getTexPoint(i.uv, i.uv1.x);
				return mask(mix(i.color, col) * grad(i.uv2), i.world);
			}

			ENDCG
		}
		Pass
		{
			Name "PrimaryPass"
			CGPROGRAM
            #define TEX_4_1
			#include "TEXDrawIncludes.cginc"
            #pragma vertex vert
			#pragma fragment frag
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = getTexPoint(i.uv, i.uv1.x);
				return mask(mix(i.color, col) * grad(i.uv2), i.world);
			}
			ENDCG
		}
	}
}
