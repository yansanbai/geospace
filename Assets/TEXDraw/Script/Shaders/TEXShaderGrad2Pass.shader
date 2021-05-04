Shader "TEXDraw/Gradient/x10 Samples"
{
	Properties
	{
		[Header(Using 16 Textures and 2 Batches per Component)]
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
			"TexMaterialAlts"="@TEXDraw/Gradient"
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
		
		UsePass "TEXDraw/Gradient/Full/SECONDARYPASS"
		UsePass "TEXDraw/Gradient/Full/PRIMARYPASS"
	}
}
