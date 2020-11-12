Shader "Depth/CameraDepth"
{
    SubShader
    {
        Pass
        {
            CGPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            uniform sampler2D_float _CameraDepthTexture;

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
                o.pos = UnityObjectToClipPos(i.pos);;
                o.scrPos = ComputeScreenPos(o.pos);
                o.uv = i.uv;
                return o;
            }

            fixed4 frag(v2f i): COLOR
            {
                // float depth = UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture, i.uv));
                float depth = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos));
                // float depth = SAMPLE_DEPTH_TEXTURE_LOD(_CameraDepthTexture, UNITY_PROJ_COORD(float4(i.uv, 0, 0)));
                
                float linear01Depth = Linear01Depth(depth);
                // float linear01Depth = depth;
                return fixed4(linear01Depth, linear01Depth, linear01Depth, 1);
            }
            ENDCG
            
        }
    }
}