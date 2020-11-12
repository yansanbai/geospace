Shader "GeoSpace/[Legacy]Line"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        
        _LineWidth ("Line Width", Range(0, 1)) = 1
        
        _DashTile ("Dash Tile", Range(1, 20)) = 10
        _DashGap ("Dash Gap", Range(-1, 1)) = 0
        
        _DepthBias ("Depth Bias", Float) = 0.003
    }
    
    SubShader
    {
        Tags { "RenderType" = "Line" "Queue" = "Transparent" }
        
        // ZWrite Off
        // Cull Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        
        LOD 100
        
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
                float2 uv: TEXCOORD0;
                float4 scrPos: TEXCOORD1;
                float4 normalDepth: TEXCOORD2;
            };
            
            float4 _Color;
            float _LineWidth;
            float _DashTile;
            float _DashGap;
            float _DepthBias;
            
            sampler2D _DepthTexture;
            // uniform sampler2D_float _CameraDepthTexture;
            
            v2f vert(appdata v)
            {
                v2f o;
                
                // 1. Origin 
                // o.pos = UnityObjectToClipPos(v.vertex);
                // o.scrPos = ComputeScreenPos(o.pos);
                // o.uv = v.uv;
                // o.normalDepth.xyz = v.normal;
                // o.normalDepth.w = - (mul(UNITY_MATRIX_MV, v.vertex).z * _ProjectionParams.w);
                
                
                // 2. Edge Normal 
                o.pos = UnityObjectToClipPos(v.vertex);
                float3 vNormal = v.color.xyz * 2 - 1;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz + vNormal * _DepthBias;
                float4 viewPos = mul(UNITY_MATRIX_V, float4(worldPos, 1.0));
                float4 offsetPos = mul(UNITY_MATRIX_P, viewPos);
                o.scrPos = ComputeScreenPos(offsetPos);
                o.uv = v.uv;
                o.normalDepth.xyz = v.normal;
                o.normalDepth.w = - (mul(UNITY_MATRIX_MV, v.vertex).z * _ProjectionParams.w);
                o.normalDepth.w = - (viewPos.z * _ProjectionParams.w);
                
                // 3. Line Mesh Normal
                // Blend OneMinusDstColor Oneo.pos = UnityObjectToClipPos(v.vertex);
                // float4 offsetVertex = float4(v.vertex + v.normal * _DepthBias, 1);
                // float4 offsetPos = UnityObjectToClipPos(offsetVertex);
                // o.scrPos = ComputeScreenPos(o.pos);
                // o.uv = v.uv;
                // o.normalDepth.xyz = v.normal;
                // o.normalDepth.w = - (mul(UNITY_MATRIX_MV, offsetVertex).z * _ProjectionParams.w);
                
                return o;
            }
            
            float4 frag(v2f i): SV_Target
            {
                float3 normalValues;
                float depthValue;
                
                float depth = i.normalDepth.w;
                
                DecodeDepthNormal(tex2D(_DepthTexture, i.scrPos.xy), depthValue, normalValues);
                // float depthValue = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.scrPos.xy));
                // depthValue = Linear01Depth(depthValue);
                
                float front = 1 * step(depth, depthValue);// + _DepthBias);
                
                float widthAlpha = step(abs(i.uv.y - 0.5), _LineWidth / 2);
                float dash = sin(i.uv.x * 2 * PI * _DashTile);
                float dashAlpha = step(_DashGap, dash);
                float alpha = widthAlpha * saturate(front + dashAlpha);
                
                float4 color = float4(_Color.rgb, alpha);
                
                return color;
            }
            
            ENDCG
        }
    }
}
