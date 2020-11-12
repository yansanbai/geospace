Shader "GeoSpace/Sign"
{
    Properties
    {
        _MainTex ("Font Texture", 2D) = "white" { }
        _Color ("Text Color", Color) = (1, 1, 1, 1)
    }
    
    SubShader
    {
        
        Tags { "RenderType" = "Sign" "Queue" = "Transparent" }
        
        Lighting Off
        ZWrite On
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
                float4 color: COLOR0;
            };
            
            struct v2f
            {
                float4 pos: SV_POSITION;
                float2 uv: TEXCOORD0;
                float4 color: TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            
            v2f vert(appdata v)
            {
                v2f o;
                // o.pos = UnityObjectToClipPos(v.vertex);
                float2 vertex = v.vertex.xy;
                o.pos = mul(UNITY_MATRIX_P, mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0)) + float4(vertex, 0.0, 0.0));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            float4 frag(v2f i): SV_Target
            {
                float4 color = _Color;
                color.a *= tex2D(_MainTex, i.uv).a;
                return color;
            }
            
            ENDCG
            
        }
    }
}