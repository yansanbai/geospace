Shader "GeoSpace/Caret"
{
    Properties
    {
        _Color ("Text Color", Color) = (1, 1, 1, 1)
        _Rate ("Rate", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        
        Tags { "RenderType" = "Sign" "Queue" = "Transparent" }
        
        Lighting Off
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
            
            fixed4 _Color;
            float _Rate;
            
            
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
                color.a = step(0, sin(_Time.y * 5) + _Rate);
                return color;
            }
            ENDCG
            
        }
    }
}