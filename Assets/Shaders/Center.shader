Shader "GeoSpace/Center"
{
    Properties
    {
        _Color ("Color", Color) = (1, 0, 0, 1)
        _ShadowColor ("Shadow Color", Color) = (0.5, 0, 0, 1)
    }
    
    SubShader
    {
        
        Tags { "LightMode" = "ForwardBase" "Queue" = "Geometry" "RenderType" = "Opaque" }
        
        Lighting Off
        
        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            
            struct appdata
            {
                float4 vertex: POSITION;
                float3 normal: NORMAL;
            };
            
            struct v2f
            {
                float4 pos: SV_POSITION;
                float3 worldNormal: TEXCOORD0;
                float3 worldLight: TEXCOORD1;
            };
            
            float4 _Color, _ShadowColor;
            
            v2f vert(appdata v)
            {
                v2f o;
                
                o.pos = UnityObjectToClipPos(v.vertex);
                
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                
                float3 viewUp = mul((float3x3)unity_CameraToWorld, float3(0, 1, 0));
                float3 viewForward = mul((float3x3)unity_CameraToWorld, float3(0, 0, -1));
                float3 viewRight = mul((float3x3)unity_CameraToWorld, float3(1, 0, 0));
                
                o.worldLight = normalize(viewUp + viewForward + viewRight);
                
                return o;
            }
            
            float4 frag(v2f i): SV_Target
            {
                float3 worldNormal = normalize(i.worldNormal);
                float3 worldLight = normalize(i.worldLight);
                
                float ndl = saturate(dot(worldNormal, worldLight));
                float3 diffuse = lerp(_ShadowColor.rgb, _Color.rgb, ndl);
                
                float4 color;
                
                color.rgb = diffuse;
                color.a = 1;
                
                return color;
            }
            ENDCG
            
        }
    }
}