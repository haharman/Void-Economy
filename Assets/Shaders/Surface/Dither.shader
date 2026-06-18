// Claudeによって生成されたテストシェーダー
Shader "Custom/Surface/Dither"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _PaletteSize ("Palette Size", Float) = 16.0
    }
    SubShader
    {
        ZTest Always Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // _BlitTexture は Blit.hlsl が TEXTURE2D_X として宣言済み
            float _PaletteSize;

            // Bayer matrix 4x4
            static const float bayerMatrix[16] = {
                0.0 / 16.0,  8.0 / 16.0,  2.0 / 16.0, 10.0 / 16.0,
                12.0 / 16.0, 4.0 / 16.0, 14.0 / 16.0, 6.0 / 16.0,
                3.0 / 16.0, 11.0 / 16.0, 1.0 / 16.0, 9.0 / 16.0,
                15.0 / 16.0, 7.0 / 16.0, 13.0 / 16.0, 5.0 / 16.0
            };

            half4 frag(Varyings input) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, input.texcoord.xy, 0);

                // Bayer ディザ
                float2 screenPos = input.texcoord * _ScreenParams.xy;
                int x = int(screenPos.x) % 4;
                int y = int(screenPos.y) % 4;
                float threshold = bayerMatrix[y * 4 + x];

                // パレット量子化
                col.rgb = floor(col.rgb * _PaletteSize + threshold) / _PaletteSize;

                return col;
            }
            ENDHLSL
        }
    }
}
