Shader "GeoSpace/Geometry"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _ShadowColor ("Shadow Color", Color) = (0, 0, 0, 1)
        _Alpha ("Alpha", Range(0, 1)) = 0.5
        
        [Toggle] _Visible ("Visible", Float) = 0
    }
    
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        LOD 100
        
        CGINCLUDE
        
        #include "UnityCG.cginc"
        #include "Lighting.cginc"
        
        float4 _Color, _ShadowColor;
        float _Alpha, _Visible;
        
        struct appdata
        {
            float4 vertex: POSITION;
            float4 normal: NORMAL;
        };
        
        struct v2f
        {
            float4 pos: SV_POSITION;
            float3 worldNormal: TEXCOORD0;
        };
        
        v2f vert(appdata v)
        {
            v2f o;
            
            o.pos = UnityObjectToClipPos(v.vertex);
            o.worldNormal = UnityObjectToWorldNormal(v.normal);
            
            return o;
        }
        
        float4 frag(v2f i, fixed facing: VFACE): SV_Target
        {
            float3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
            
            float3 worldNormal = normalize(i.worldNormal);
            float3 worldLight = normalize(_WorldSpaceLightPos0.xyz);
            
            if (facing < 0)
                worldNormal = -worldNormal;
            
            float ndl = saturate(dot(worldNormal, worldLight));
            
            float3 diffuse = _LightColor0.rgb * lerp(_ShadowColor.rgb, _Color.rgb, ndl);
            float4 color = float4(ambient + diffuse, _Alpha * _Visible);
            
            return color;
        }
        ENDCG
        
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            Cull Front
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #define _BACF_FACE
            
            ENDCG
            
        }
        
        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            Cull Back
            
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            ENDCG
            
        }
    }
    FallBack "Diffuse"
}
