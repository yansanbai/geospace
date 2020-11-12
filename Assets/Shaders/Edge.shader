Shader "GeoSpace/Edge"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" { }
        _Color ("Color", Color) = (1, 1, 1, 1)
        
        _LineWidth ("Line Width", Range(0, 1)) = 1

        _DashTile ("Dash Tile", Range(1, 20)) = 10
        _DashGap ("Dash Gap", Range(-1, 1)) = 0
        
        _DepthBias ("Depth Bias", Float) = 0.003
        _DepthOffset ("Depth Offset", Float) = 1

        _CutOff ("Cut Off", Range(0, 1)) = 0.5

        [Toggle] _Highlight ("Highlight", Float) = 0
        _ColorH ("Highlight Color", Color) = (0.1, 0.1, 0.1, 1)
    }
    
    SubShader
    {
        Tags { "RenderType" = "Line" "Queue" = "Geometry" }
        
        Lighting Off
        ZWrite On
        // Cull Off
        // ZTest Always
        // Blend SrcAlpha OneMinusSrcAlpha
        
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
                float3 normal: NORMAL;
                float2 uv: TEXCOORD0;
                float4 color: COLOR0;
            };
            
            struct v2f
            {
                float4 pos: SV_POSITION;
                float2 uvfront: TEXCOORD0;
                float2 uvback: TEXCOORD1;
                float4 scrPos: TEXCOORD2;
                float4 normalDepth: TEXCOORD3;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            fixed4 _ColorH;
            float _LineWidth;
            float _DashTile;
            float _DashGap;
            float _DepthBias;
            float _DepthOffset;
            fixed _CutOff;
            fixed _Highlight;
            
            sampler2D _DepthTexture;
            
            v2f vert(appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);
                float3 vNormal = v.color.xyz * 2 - 1;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz + vNormal * _DepthBias;
                float4 viewPos = mul(UNITY_MATRIX_V, float4(worldPos, 1.0));
                float4 offsetPos = mul(UNITY_MATRIX_P, viewPos);
                o.scrPos = ComputeScreenPos(offsetPos);
                o.uvfront = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvback = v.uv;
                o.normalDepth.xyz = v.normal;
                o.normalDepth.w = - (UnityObjectToViewPos(v.vertex).z * _ProjectionParams.w);
                o.normalDepth.w = - (viewPos.z * _ProjectionParams.w);

                float3 viewPos1 = UnityObjectToViewPos(v.vertex);
                viewPos1.z += _DepthOffset;
                o.pos = UnityViewToClipPos(viewPos1);

                return o;
            }
            
            fixed4 frag(v2f i): SV_Target
            {
                float3 normalValues;
                float depthValue;
                
                float depth = i.normalDepth.w;
                
                DecodeDepthNormal(tex2D(_DepthTexture, i.scrPos.xy), depthValue, normalValues);
                
                float front = step(depth, depthValue);
                float widthAlpha = step(abs(i.uvfront.y - 0.5), _LineWidth / 2);
                
                float dash = sin(i.uvback.x * 2 * PI * _DashTile);
                float backAlpha = step(_DashGap, dash);
                
                fixed frontAlpha = tex2D(_MainTex, i.uvfront).r;

                fixed4 color = _Color;
                fixed alpha = front * frontAlpha + (1 - front) * backAlpha;
                alpha *= widthAlpha;
                color.a = alpha;

                clip(color.a - _CutOff);

                color.rgb += (fixed3(1, 1, 1) - color.rgb) * _ColorH.rgb * step(0.5f, _Highlight);

                return color;
            }
            
            ENDCG
            
        }
    }
}
