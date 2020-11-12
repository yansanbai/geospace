
Shader "GeoSpace/NavAxis"
{
    Properties
    {
        _ColorX ("X Color", Color) = (1, 0, 0, 1)
        _ColorShadowX ("X Shadow Color", Color) = (0.5, 0, 0, 1)
        _ColorY ("Y Color", Color) = (0, 1, 0, 1)
        _ColorShadowY ("Y Shadow Color", Color) = (0, 0.5, 0, 1)
        _ColorZ ("Z Color", Color) = (0, 0, 1, 1)
        _ColorShadowZ ("Z Shadow Color", Color) = (0, 0, 0.5, 1)
        _ColorW ("W Color", Color) = (0.8, 0.8, 0.8, 1)
        _ColorShadowW ("W Shadow Color", Color) = (0.5, 0.5, 0.5, 1)
        
        [Toggle]_Highlight ("Highlight", Float) = 0
        _HighlightColor ("Highlight Color", Color) = (0.5, 0.5, 0.5, 1)
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
                float4 uv: TEXCOORD0;
                float4 color: COLOR;
            };
            
            struct v2f
            {
                float4 pos: SV_POSITION;
                float4 color: COLOR;
                float3 worldNormal: TEXCOORD0;
                float3 worldPos: TEXCOORD1;
                float3 planeNormal: TEXCOORD2;
                float4 uv: TEXCOORD3;
            };
            
            float4 _ColorX, _ColorY, _ColorZ, _ColorW;
            float4 _ColorShadowX, _ColorShadowY, _ColorShadowZ, _ColorShadowW;
            float _Highlight;
            float4 _HighlightColor;
            
            v2f vert(appdata v)
            {
                v2f o;
                
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.planeNormal = mul(unity_ObjectToWorld, float3(0, 1, 0)).xyz;
                o.uv = v.uv;
                o.color = v.color;
                
                return o;
            }
            
            float4 frag(v2f i): SV_Target
            {
                float3 worldNormal = normalize(i.worldNormal);
                float3 worldLight = normalize(_WorldSpaceLightPos0.xyz);
                float3 worldView = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);
                
                float3 planeNormal = normalize(i.planeNormal);
                float distance = -dot(planeNormal, worldView);
                float3 fakeLight = normalize(worldView + planeNormal * distance);
                
                float isFake = step(i.color.r, 0.5);
                float ndlFake = saturate(dot(worldNormal, fakeLight));
                float ndlNormal = saturate(dot(worldNormal, worldLight));
                float ndl = ndlFake;
                
                // float3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * _Color.rgb;
                float3 colorX = i.uv.x * lerp(_ColorShadowX.rgb, _ColorX.rgb, ndl);
                float3 colorY = i.uv.y * lerp(_ColorShadowY.rgb, _ColorY.rgb, ndl);
                float3 colorZ = i.uv.z * lerp(_ColorShadowZ.rgb, _ColorZ.rgb, ndl);
                float3 colorW = i.uv.w * lerp(_ColorShadowW.rgb, _ColorW.rgb, ndl);
                
                float3 diffuse = colorX + colorY + colorZ + colorW;
                diffuse += _HighlightColor * _Highlight;
                
                float4 color;
                
                color.rgb = diffuse;
                color.a = 1;
                
                return color;
            }
            ENDCG
            
        }
    }
}