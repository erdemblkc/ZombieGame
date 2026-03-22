Shader "Custom/RadialBlur"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off ZTest Always Blend Off Cull Off

        Pass
        {
            Name "RadialBlur"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // SprintBlurEffect.cs tarafından her frame set edilir
            float _RadialBlurStrength;

            #define SAMPLES 12

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv  = input.texcoord;
                float2 dir = uv - float2(0.5, 0.5);  // merkezden uzaklık vektörü

                half4 color = 0;

                // Merkeze doğru SAMPLES adet sample al ve ortalamasını al
                [unroll]
                for (int i = 0; i < SAMPLES; i++)
                {
                    float t       = (float)i / (SAMPLES - 1);
                    float2 offset = dir * _RadialBlurStrength * t;
                    color += SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv - offset);
                }

                return color / SAMPLES;
            }
            ENDHLSL
        }
    }
}
