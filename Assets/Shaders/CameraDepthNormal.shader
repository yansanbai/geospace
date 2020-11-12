Shader "Depth/CameraDepthNormal"
{
    SubShader
    {
        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            sampler2D _CameraDepthNormalsTexture;
            fixed _showNormalColors = 0;

            struct a2v
            {
                float4 pos: POSITION;
                half2 uv: TEXCOORD0;
            };

            struct v2f
            {
                float4 pos: SV_POSITION;
                half2 uv: TEXCOORD0;
                float4 scrPos: TEXCOORD1;
            };


            v2f vert(a2v i)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(i.pos);
                o.scrPos = ComputeScreenPos(o.pos);
                o.uv = i.uv;
                return o;
            }

            half4 frag(v2f i): COLOR
            {
                float3 normalValue;
                float depthValue;

                DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture, i.scrPos.xy), depthValue, normalValue);

                if (_showNormalColors == 1)
                {
                    float4 normal = float4(normalValue * 0.5 + 0.5, 1);
                    return normal;
                }
                else
                {
                    float4 depth = float4(depthValue, depthValue, depthValue, 1);
                    return depth;
                }
            }
            
            ENDCG
            
        }
    }
    FallBack "Diffuse"
}