Shader "GeoSpace/Axis"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _LineWidth ("Line Width", Range(0, 1)) = 1
    }
    
    SubShader
    {
        Tags { "RenderType" = "Axis" "Queue" = "Geometry" }
        
        Lighting Off
        ZWrite Off
        
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
            
            fixed4 _Color;
            float _LineWidth;
            
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
                float alpha = step(abs(i.uv.y - 0.5), _LineWidth / 2);
                color.a = alpha;

                return color;
            }
            
            ENDCG
            
        }
    }
}
