Shader "GeoSpace/SignBackground"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _ScaleX ("Scale X", Float) = 1.0
        _ScaleY ("Scale Y", Float) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType" = "SignBackground" "Queue" = "Transparent" }
        
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
            
            float4 _Color;
            float _ScaleX, _ScaleY;
            
            v2f vert(appdata v)
            {
                v2f o;
                // o.pos = UnityObjectToClipPos(v.vertex);
                float2 vertex = v.vertex.xy;
                o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0)) + float4(vertex, 0.0, 0.0) * float4(_ScaleX, _ScaleY, 1.0, 1.0));
                o.uv = v.uv;
                return o;
            }
            
            float4 frag(v2f i): SV_Target
            {
                float4 color = _Color;
                return color;
            }
            ENDCG
            
        }
    }
}