Shader "GeoSpace/Face"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" { }
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Alpha ("Alpha", Range(0, 1)) = 0.5

        _CutOff ("Cut Off", Range(0, 1)) = 0.5

        [Toggle] _Highlight ("Highlight", Float) = 0
        _ColorH ("Highlight Color", Color) = (0.1, 0.1, 0.1, 1)
    }
    
    SubShader
    {
        
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        
        // Lighting Off
        // ZTest Always
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            // Cull Front
            
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
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _ColorH;
            fixed _Alpha;
            fixed _CutOff;
            fixed _Highlight;
            
            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
            
            fixed4 frag(v2f i): SV_Target
            {
                fixed tex = tex2D(_MainTex, i.uv).r;
                fixed4 color = _Color;

                clip(color.a - _CutOff);

                color.a *= tex * _Alpha;

                color.rgb += (fixed3(1, 1, 1) - color.rgb * color.a) * _ColorH.rgb * step(0.5f, _Highlight);

                return color;
            }
            ENDCG
            
        }
    }
}