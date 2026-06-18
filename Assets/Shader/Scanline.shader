Shader "Custom/Scanline"
{
    Properties
    {
        _LineCount ("Line Count", Float) = 180.0
        _Intensity ("Intensity", Float) = 0.25
        _Speed     ("Speed",     Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert   // Blit.hlsl が提供する頂点シェーダーをそのまま使う
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            //  ↑ Vert関数・Varyings構造体・_BlitTexture がここで定義される

            CBUFFER_START(UnityPerMaterial)
                float _LineCount;
                float _Intensity;
                float _Speed;
            CBUFFER_END

            // Varyings は Blit.hlsl のものを使う（texcoord フィールドを持つ）
            half4 frag(Varyings IN) : SV_Target
            {
                // _MainTex → _BlitTexture  /  IN.uv → IN.texcoord
                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, IN.texcoord);

                float t = _Time.y;

                float sinVal  = sin((IN.texcoord.y + t * _Speed) * _LineCount * 3.14159);
                float scanline = sinVal * sinVal;

                float flicker     = frac(sin(floor(t * 12.0) * 127.1) * 43758.5);
                float flickerMask = step(0.97, flicker);

                float noiseLine = step(0.995, frac(
                    sin(floor(IN.texcoord.y * 80.0 + t * 3.0) * 91.3) * 43758.5
                ));

                float scanDark = 1.0 - _Intensity * (1.0 - scanline);
                scanDark -= flickerMask * 0.08;
                //col.rgb *= scanDark;
                // オーバーレイに変更
                float3 blend = float3(scanDark, scanDark, scanDark);
                float3 dark  = 2.0 * col.rgb * blend;
                float3 light = 1.0 - 2.0 * (1.0 - col.rgb) * (1.0 - blend);
                col.rgb = lerp(dark, light, step(0.5, col.rgb));

                col.rgb += noiseLine * 0.05;

                return col;
            }
            ENDHLSL
        }
    }
}