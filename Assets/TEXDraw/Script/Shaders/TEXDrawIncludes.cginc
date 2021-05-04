
#include "UnityCG.cginc"
#include "UnityUI.cginc"

// Common APPDATA for most TEXDraw needs
struct appdata
{
	float4 vertex : POSITION;
	// Texture or font UV
	float2 uv : TEXCOORD0;
	// (font index, TMP crispness)
	float2 uv1 : TEXCOORD1;
	// Autofill UV
	float2 uv2 : TEXCOORD2;
	// UI color
	float4 color : COLOR;
};

// Common V2F for most TEXDraw needs
struct v2f
{
	float4 vertex : SV_POSITION;
	float2 uv : TEXCOORD0;
	float2 uv1 : TEXCOORD1;
	float2 uv2 : TEXCOORD2;
	float2 world : TEXCOORD3;
	float4 color : COLOR;
};

// Common Vertex program for most TEXDraw needs
v2f vert (appdata v)
{
	v2f o = {
		UnityObjectToClipPos(v.vertex),
		v.uv,
		v.uv1,
		v.uv2,
		v.vertex.xy,
		v.color
	};
	return o;
}

fixed4 mix (fixed4 vert, fixed4 tex)
{
   /*
	*	The reason of why using max:
	* 	Font textures is alpha-only, means it's RGB will be black
	* 	With colors from col, the output color would be the same as i.color 
	*	But this comes problem for sprites: it's RGB value will be overwritten by col.
	*	So, every use of Non-alpha-only sprites must have set the i.color down to black,
	*	which automatically handled by Charbox.cs
	*/
	fixed4 color = fixed4(max(vert, tex).rgb, vert.a * tex.a);
	return color;
}

// Rect mask 2D clipping

float4 _ClipRect;

fixed4 mask(fixed4 color, float2 vertex)
{
	color.a *= UnityGet2DClipping(vertex, _ClipRect);
	return color;
}

#ifdef TEX_4_1

sampler2D _Font0, _Font1, _Font2, _Font3, _Font4, _Font5, _Font6, _Font7;

fixed4 getTexPoint(float2 uv, float index)
{
	fixed4 alpha;
	if(index <= 0.5)		alpha = tex2D(_Font0, uv);
	else if(index <= 1.5)	alpha = tex2D(_Font1, uv);
	else if(index <= 2.5)	alpha = tex2D(_Font2, uv);
	else if(index <= 3.5)	alpha = tex2D(_Font3, uv);
	else if(index <= 4.5)	alpha = tex2D(_Font4, uv);
	else if(index <= 5.5)	alpha = tex2D(_Font5, uv);
	else if(index <= 6.5)	alpha = tex2D(_Font6, uv);
	else if(index <= 7.5)	alpha = tex2D(_Font7, uv);
	else if(index <= 30.5)	alpha = fixed4(0, 0, 0, 0);
	else if(index <= 31.5)	alpha = fixed4(0, 0, 0, 1);
	else					alpha = fixed4(0, 0, 0, 0);
	return alpha;
}
#endif
#ifdef TEX_4_2
sampler2D _Font8, _Font9, _FontA, _FontB, _FontC, _FontD, _FontE, _FontF;

fixed4 getTexPoint(float2 uv, float index)
{
	fixed4 alpha;
	if (index <= 7.5)		alpha = fixed4(0, 0, 0, 0);
	else if(index <= 8.5)	alpha = tex2D(_Font8, uv);
	else if(index <= 9.5)	alpha = tex2D(_Font9, uv);
	else if(index <= 10.5)	alpha = tex2D(_FontA, uv);
	else if(index <= 11.5)	alpha = tex2D(_FontB, uv);
	else if(index <= 12.5)	alpha = tex2D(_FontC, uv);
	else if(index <= 13.5)	alpha = tex2D(_FontD, uv);
	else if(index <= 14.5)	alpha = tex2D(_FontE, uv);
	else if(index <= 15.5)	alpha = tex2D(_FontF, uv);
	else if(index <= 31.5)	alpha = fixed4(0, 0, 0, 0);
	else if(index <= 32.5)	alpha = fixed4(0, 0, 0, 1);
	else					alpha = fixed4(0, 0, 0, 0);
	return alpha;
}
#endif

#ifdef TEX_4_3
sampler2D _Font10, _Font11, _Font12, _Font13, _Font14, _Font15, _Font16, _Font17;

fixed4 getTexPoint(float2 uv, float index)
{
	fixed4 alpha;
	if (index <= 15.5)		alpha = fixed4(0, 0, 0, 0);
	else if(index <= 16.5)	alpha = tex2D(_Font10, uv);
	else if(index <= 17.5)	alpha = tex2D(_Font11, uv);
	else if(index <= 18.5)	alpha = tex2D(_Font12, uv);
	else if(index <= 19.5)	alpha = tex2D(_Font13, uv);
	else if(index <= 20.5)	alpha = tex2D(_Font14, uv);
	else if(index <= 21.5)	alpha = tex2D(_Font15, uv);
	else if(index <= 22.5)	alpha = tex2D(_Font16, uv);
	else if(index <= 23.5)	alpha = tex2D(_Font17, uv);
	else					alpha = fixed4(0, 0, 0, 0);
	return alpha;
}
#endif

#ifdef TEX_4_4
sampler2D _Font18, _Font19, _Font1A, _Font1B, _Font1C, _Font1D, _Font1E;


fixed4 getTexPoint(float2 uv, float index)
{
	fixed4 alpha;
	if (index <= 23.5)		alpha = fixed4(0, 0, 0, 0);
	else if(index <= 24.5)	alpha = tex2D(_Font18, uv);
	else if(index <= 25.5)	alpha = tex2D(_Font19, uv);
	else if(index <= 26.5)	alpha = tex2D(_Font1A, uv);
	else if(index <= 27.5)	alpha = tex2D(_Font1B, uv);
	else if(index <= 28.5)	alpha = tex2D(_Font1C, uv);
	else if(index <= 29.5)	alpha = tex2D(_Font1D, uv);
	else if(index <= 30.5)	alpha = tex2D(_Font1E, uv);
	else					alpha = fixed4(0, 0, 0, 0);
	return alpha;
}
#endif
#ifdef TEX_5_1

sampler2D _Font0, _Font1, _Font2, _Font3, _Font4, _Font5;

fixed4 getTexPoint(float2 uv, float index)
{
	fixed4 alpha;
	if(index <= 0.5)		alpha = tex2D(_Font0, uv);
	else if(index <= 1.5)	alpha = tex2D(_Font1, uv);
	else if(index <= 2.5)	alpha = tex2D(_Font2, uv);
	else if(index <= 3.5)	alpha = tex2D(_Font3, uv);
	else if(index <= 4.5)	alpha = tex2D(_Font4, uv);
	else if(index <= 5.5)	alpha = tex2D(_Font5, uv);
	else if(index <= 30.5)	alpha = fixed4(0, 0, 0, 0);
	else if(index <= 31.5)	alpha = fixed4(0, 0, 0, 1);
	else					alpha = fixed4(0, 0, 0, 0);
	return alpha;
}

#endif
#ifdef TEX_5_2
sampler2D _Font6, _Font7, _Font8, _Font9, _FontA, _FontB;

fixed4 getTexPoint(float2 uv, float index)
{
	fixed4 alpha;
	if(index <= 5.5)		alpha = fixed4(0, 0, 0, 0);
	else if(index <= 6.5)	alpha = tex2D(_Font6, uv);
	else if(index <= 7.5)	alpha = tex2D(_Font7, uv);
	else if(index <= 8.5)	alpha = tex2D(_Font8, uv);
	else if(index <= 9.5)	alpha = tex2D(_Font9, uv);
	else if(index <= 10.5)	alpha = tex2D(_FontA, uv);
	else if(index <= 11.5)	alpha = tex2D(_FontB, uv);
	else if(index <= 31.5)	alpha = fixed4(0, 0, 0, 0);
	else if(index <= 32.5)	alpha = fixed4(0, 0, 0, 1);
	else					alpha = fixed4(0, 0, 0, 0);
	return alpha;
}

#endif

#ifdef TEX_5_3
sampler2D _FontC, _FontD, _FontE, _FontF, _Font10, _Font11;

fixed4 getTexPoint(float2 uv, float index)
{
	fixed4 alpha;
	if(index <= 11.5)			alpha = fixed4(0, 0, 0, 0);
	else if(index <= 12.5)	alpha = tex2D(_FontC, uv);
	else if(index <= 13.5)	alpha = tex2D(_FontD, uv);
	else if(index <= 14.5)	alpha = tex2D(_FontE, uv);
	else if(index <= 15.5)	alpha = tex2D(_FontF, uv);
	else if(index <= 16.5)	alpha = tex2D(_Font10, uv);
	else if(index <= 17.5)	alpha = tex2D(_Font11, uv);
	else					alpha = fixed4(0, 0, 0, 0);
	return alpha;
}

#endif
#ifdef TEX_5_4
sampler2D _Font12, _Font13, _Font14, _Font15, _Font16, _Font17;
fixed4 getTexPoint(float2 uv, float index)
{
	fixed4 alpha;
	if(index <= 17.5)		alpha = fixed4(0, 0, 0, 0);
	else if(index <= 18.5)	alpha = tex2D(_Font12, uv);
	else if(index <= 19.5)	alpha = tex2D(_Font13, uv);
	else if(index <= 20.5)	alpha = tex2D(_Font14, uv);
	else if(index <= 21.5)	alpha = tex2D(_Font15, uv);
	else if(index <= 22.5)	alpha = tex2D(_Font16, uv);
	else if(index <= 23.5)	alpha = tex2D(_Font17, uv);
	else					alpha = fixed4(0, 0, 0, 0);
	return alpha;
}

#endif

#ifdef TEX_5_5
sampler2D _Font18, _Font19, _Font1A, _Font1B, _Font1C, _Font1D;//, _Font1E;

fixed4 getTexPoint(float2 uv, float index)
{
	fixed4 alpha;

	if(index <= 23.5)		alpha = fixed4(0, 0, 0, 0);
	else if(index <= 24.5)	alpha = tex2D(_Font18, uv);
	else if(index <= 25.5)	alpha = tex2D(_Font19, uv);
	else if(index <= 26.5)	alpha = tex2D(_Font1A, uv);
	else if(index <= 27.5)	alpha = tex2D(_Font1B, uv);
	else if(index <= 28.5)	alpha = tex2D(_Font1C, uv);
	else if(index <= 29.5)	alpha = tex2D(_Font1D, uv);
	//else if(index <= 30.5)	alpha = tex2D(_Font1E, uv);
	else					alpha = fixed4(0, 0, 0, 0);
	return alpha;
}

#endif
