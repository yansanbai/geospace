Shader "GeoSpace/[Legacy]Point"
{
    Properties
    {
        _ColorI ("Color Inner", Color) = (1, 1, 1, 1)
        _RadiusI ("Radius Inner", Range(0, 0.5)) = 0.2

        _ColorO ("Color Outter", Color) = (1, 1, 1, 1)
        _RadiusO ("Radius Outter", Range(0, 0.5)) = 0.5

        [Toggle] _Highlight ("Highlight", Float) = 0
    }
    
    SubShader
    {
        
        Tags { "RenderType" = "Point" "Queue" = "Transparent" }
        
        Lighting Off
        // Cull Off
        ZTest Always
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex: POSITION;
                float2 uv: TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos: SV_POSITION;
                float2 uv: TEXCOORD0;
            };
            
            fixed4 _ColorI, _ColorO;
            fixed _RadiusI, _RadiusO;
            fixed _Highlight;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag(v2f i): SV_Target
            {
                float2 vec = i.uv - float2(0.5, 0.5);
                float length = vec.x * vec.x + vec.y * vec.y;
                fixed inO = step(length, _RadiusO * _RadiusO);
                fixed inI = step(length, _RadiusI * _RadiusI);
                fixed alpha = _Highlight * inO + (1 - _Highlight) * inI;
                fixed4 color = _ColorI * inI + _ColorO * (1 - inI);
                color.a *= alpha;
                return color;
            }
            ENDCG
            
        }
    }
}