Shader "GeoSpace/Arrow"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    
    SubShader
    {
        Tags { "RenderType" = "Arrow" "Queue" = "Geometry" }
        
        Lighting Off
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            
            struct appdata
            {
                float4 vertex: POSITION;
            };
            
            struct v2f
            {
                float4 pos: SV_POSITION;
            };
            
            fixed4 _Color;
            
            
            v2f vert(appdata v)
            {
                v2f o;

                o.pos = UnityObjectToClipPos(v.vertex);
                
                return o;
            }
            
            fixed4 frag(v2f i): SV_Target
            {
                fixed4 color = _Color;

                return color;
            }
            
            ENDCG
            
        }
    }
}
