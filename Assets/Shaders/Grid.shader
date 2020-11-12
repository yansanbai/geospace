Shader "GeoSpace/Grid"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Size ("Size", Float) = 1
        _LineWidth ("Line Width", Range(0, 1)) = 1
        
        _DashTile ("Dash Tile", Range(1, 20)) = 10
        _DashGap ("Dash Gap", Range(-1, 1)) = 0
    }
    
    SubShader
    {
        Tags { "RenderType" = "Grid" "Queue" = "Transparent" }
        
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            #define PI 3.14159
            
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
            
            fixed4 _Color;
            float _Size;
            float _LineWidth;
            float _DashTile;
            float _DashGap;
            fixed _CutOff;
            
            v2f vert(appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                return o;
            }
            
            fixed4 frag(v2f i): SV_Target
            {
                fixed4 color = _Color;
                float widthAlpha = step(abs(i.uv.y - 0.5), _LineWidth / 2);
                
                float dash = sin(i.uv.x * 2 * PI * _DashTile * _Size);
                float dashAlpha = step(_DashGap, dash);

                color.a = widthAlpha * dashAlpha;

                clip(color.a - _CutOff);

                return color;
            }
            
            ENDCG
            
        }
    }
}
