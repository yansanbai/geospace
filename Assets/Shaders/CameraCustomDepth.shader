Shader "Depth/CameraCustomDepth"
{
    Properties
    {
        _MainTex ("", 2D) = "white" { }
        _DepthTexture ("_DepthTexture", 2D) = "white" { }
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _DepthTexture;


            struct v2f
            {
                float4 pos: SV_POSITION;
                float4 scrPos: TEXCOORD1;
            };

            v2f vert(appdata_base v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.scrPos = ComputeScreenPos(o.pos);
                return o;
            }

            sampler2D _MainTex;

            half4 frag(v2f i): COLOR
            {

                float3 normalValues;
                float depthValue;

                DecodeDepthNormal(tex2D(_DepthTexture, i.scrPos.xy / i.scrPos.w), depthValue, normalValues);

                float4 depth = float4(depthValue, depthValue, depthValue, 1);
                return depth;
            }
            
            ENDCG
            
        }
    }
    FallBack "Diffuse"
}