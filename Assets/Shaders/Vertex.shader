Shader "GeoSpace/Vertex"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" { }
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Size ("Size", Range(0, 1)) = 0.8

        _DepthOffset ("Depth Offset", Float) = 1
        _CutOff ("Cut Off", Range(0, 1)) = 0.5

        [Toggle] _Highlight ("Highlight", Float) = 0
        _ColorH ("Highlight Color", Color) = (0.1, 0.1, 0.1, 1)

        [Toggle] _Active ("Active", Float) = 0
        _ColorA ("Active Color", Color) = (1, 0.839, 0.463, 1)
        _SizeA ("Active Size", Range(0, 1)) = 1
        _ActiveOffsetX ("Active Offset X", Range(-1, 1)) = 0
        _ActiveOffsetY ("Active Offset Y", Range(-1, 1)) = 0
    }
    
    SubShader
    {
        
        Tags { "RenderType" = "Vertex" "Queue" = "Geometry" }
        
        Lighting Off
        // Cull Off
        // ZTest Always
        ZWrite On
        // Blend SrcAlpha OneMinusSrcAlpha
        
        // Active
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
            
            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _ColorH;
            fixed4 _ColorA;
            float _Size;
            float _SizeA;
            float _DepthOffset;
            fixed _CutOff;
            fixed _Highlight;
            fixed _Active;
            float _ActiveOffsetX;
            float _ActiveOffsetY;
            
            v2f vert(appdata v)
            {
                v2f o;
                // o.pos = UnityObjectToClipPos(v.vertex * _Size);
                float3 viewPos = UnityObjectToViewPos(v.vertex * _SizeA);
                viewPos.x += _ActiveOffsetX;
                viewPos.y += _ActiveOffsetY;
                viewPos.z += _DepthOffset;
                o.pos = UnityViewToClipPos(viewPos);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag(v2f i): SV_Target
            {
                fixed tex = tex2D(_MainTex, i.uv).r;
                fixed4 color = _ColorA;
                color.a *= tex;
                color.a *= _Active;

                clip(color.a - _CutOff);

                return color;
            }
            ENDCG
            
        }
        
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
            
            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _ColorH;
            float _Size;
            float _DepthOffset;
            fixed _CutOff;
            fixed _Highlight;
            
            v2f vert(appdata v)
            {
                v2f o;
                // o.pos = UnityObjectToClipPos(v.vertex * _Size);
                float3 viewPos = UnityObjectToViewPos(v.vertex * _Size);
                viewPos.z += _DepthOffset +0.01;
                o.pos = UnityViewToClipPos(viewPos);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag(v2f i): SV_Target
            {
                fixed tex = tex2D(_MainTex, i.uv).r;
                fixed4 color = _Color;
                color.a *= tex;

                clip(color.a - _CutOff);

                color.rgb += (fixed3(1, 1, 1) - color.rgb) * _ColorH.rgb * step(0.5f, _Highlight);

                return color;
            }
            ENDCG
            
        }
    }
}