// Claudeによって生成されたテストシェーダー
// Dustは現状維持、Fogを調整中。

Shader "Custom/Surface/Atmosphere"
{
    Properties
    {
        // 元画像
        _MainTex        ("Texture",        2D)    = "white" {}
        // 霧の色
        _FogColor       ("Fog Color",      Color) = (0.75, 0.82, 0.9, 1)
        // 霧の開始距離
        _FogNear        ("Fog Near",       Float) = 6.0
        // 霧の終了距離
        _FogFar         ("Fog Far",        Float) = 40.0
        // 霧の量
        _HeightFogAmt   ("Height Fog",     Float) = 0.6
        // 霧の高さ
        _HeightFogY     ("Height Fog Y",   Float) = 1.5
        _DustColor      ("Dust Color",     Color) = (0.92, 0.86, 0.7, 1)
        _DustGridSize   ("Dust Grid",      Float) = 36.0
        _DustDensity    ("Dust Density",   Range(0,1)) = 0.88
        _DustSpeed      ("Dust Speed",     Float) = 0.25
        _DustIntensity  ("Dust Intensity", Float) = 0.55
    }
    SubShader
    {
        // Ztest Always: 手前のオブジェクトに隠されないように、常にZtest（深度の検証）が成功
        // Cull Off: 裏面削除なし
        // ZWrite Off: 深度バッファーに書き込まない
        ZTest Always Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // _BlitTexture は Blit.hlsl が TEXTURE2D_X として宣言済み
            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            half4  _FogColor;
            float  _FogNear, _FogFar, _HeightFogAmt, _HeightFogY;
            half4  _DustColor;
            float  _DustGridSize, _DustDensity, _DustSpeed, _DustIntensity;

            // ────────────────────────────────────────────
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float DustLayer(float2 uv, float time, float scale)
            {
                float2 grid  = uv * scale;
                float2 cell  = floor(grid);
                float2 local = frac(grid);

                float2 seed = cell;
                float  rnd  = hash(seed + 7.3);
                if (rnd < _DustDensity) return 0;

                float  speed  = _DustSpeed * (0.4 + hash(seed) * 0.6);
                float2 origin = float2(hash(seed + 1.1), hash(seed + 2.3));
                float2 pos    = origin + float2(
                    sin(time * speed       + origin.x * 6.28) * 0.28,
                    cos(time * speed * 0.6 + origin.y * 6.28) * 0.28);
                pos = frac(pos);

                float d = length(local - pos);
                float r = 0.055 / (scale / _DustGridSize);
                return step(d, r) * (rnd - _DustDensity) / max(1.0 - _DustDensity, 0.001);
            }

            // ────────────────────────────────────────────
            half4 frag(Varyings input) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D_X_LOD(_BlitTexture, sampler_LinearClamp, input.texcoord.xy, 0);

                float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, input.texcoord);

                // ════════════════════════════════════════
                // ★ Fix #1 & #2: 直交投影対応の視線距離
                // ════════════════════════════════════════
                // LinearEyeDepth() の内部式: 1 / (z * _ZBufferParams.z + _ZBufferParams.w)
                // これは透視投影専用。直交投影では深度が [near, far] の線形マッピングになるため
                // まったく異なる計算が必要。
                //
                // また UNITY_REVERSED_Z（Metal / DX12 / Vulkan）では
                // バッファの near=1.0, far=0.0 と反転しているため正規化が必要。
                #if defined(UNITY_REVERSED_Z)
                    float depth01 = 1.0 - rawDepth;   // → near=0, far=1 に統一
                #else
                    float depth01 = rawDepth;
                #endif

                float perspEye = LinearEyeDepth(rawDepth, _ZBufferParams);
                // _ProjectionParams: x=±1, y=near, z=far, w=1/far
                float orthoEye = lerp(_ProjectionParams.y, _ProjectionParams.z, depth01);

                // unity_OrthoParams.w: 1.0=直交投影, 0.0=透視投影
                // lerp で一つのコードパスに統合（条件分岐なし）
                float eyeDist = lerp(perspEye, orthoEye, unity_OrthoParams.w);

                // ════════════════════════════════════════
                // ★ Fix #2: ワールドY 座標の復元（Unity 2D 向けに刷新）
                // ════════════════════════════════════════
                // 旧コードの問題:
                //   NDC の Z 範囲が API ごとに異なる（DX:[0,1] / GL:[-1,1]）ため
                //   _InvViewProjMatrix に渡す rawDepth がプラットフォームで意味が変わる。
                //
                // 修正後:
                //   直交投影（Unity 2D 標準）では投影行列を使わずとも
                //   UV → ワールドY は以下の式で正確に求まる。
                //   unity_OrthoParams.y = Camera.orthographicSize（半高さ）
                float worldY = _WorldSpaceCameraPos.y
                             + (input.texcoord.y - 0.5) * unity_OrthoParams.y * 2.0;

                // ════════════════════════════════════════
                // 霧係数
                // ════════════════════════════════════════
                float distFog = saturate((eyeDist - _FogNear) / max(_FogFar - _FogNear, 0.001));
                distFog = distFog * distFog;

                float heightFog = saturate((_HeightFogY - worldY) / 2.0) * _HeightFogAmt;
                float fogTotal  = saturate(distFog + heightFog * (1.0 - distFog));
                col.rgb = lerp(col.rgb, _FogColor.rgb, fogTotal * _FogColor.a);

                // ════════════════════════════════════════
                // 埃
                // ════════════════════════════════════════
                // ★ Fix #5: _FogNear=0 のときの 0 除算を max でガード
                float dustVis = (1.0 - distFog * 0.7)
                              * saturate((eyeDist - _FogNear * 0.2) / max(_FogNear * 0.8, 0.001));

                float dust = 0;
                dust += DustLayer(input.texcoord, _Time.y, _DustGridSize);
                dust += DustLayer(input.texcoord, _Time.y, _DustGridSize * 0.65) * 0.5;
                dust  = saturate(dust) * dustVis * _DustIntensity;

                half3 dustBlend = lerp(_DustColor.rgb, col.rgb * _DustColor.rgb * 1.2, 0.4);
                col.rgb = lerp(col.rgb, dustBlend, dust);

                return col;
            }
            ENDHLSL
        }
    }
}