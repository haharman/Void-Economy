Shader "Learning/ProceduralNoise_URP"
{
    Properties
    {
        // === 見た目調整 ===
        _FogColor ("Fog Color", Color) = (1, 1, 1, 1)
        _FogDensity ("Fog Density", Range(0, 2)) = 0.6
        
        // === Noise調整 ===
        _NoiseScale ("Noise Scale (大きさ)", Range(0.1, 10)) = 2.0
        _NoiseSpeed ("Animation Speed", Range(0, 5)) = 1.0
        
        // === ドリフト（流れ） ===
        _DriftSpeed ("Drift Speed (x移動)", Range(0, 10)) = 2.0
        _DriftAmount ("Drift Amount (ゆらぎ)", Range(0, 1)) = 0.3
        
        // === FBM（複数層の重ね） ===
        _Octaves ("Octaves (層数)", Range(1, 4)) = 3
        _Persistence ("Persistence (詳細度)", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            HLSLPROGRAM
            #pragma prefer_hlsl2021
            #pragma vertex vert
            #pragma fragment frag
            
            // === Include ===
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            // === Properties ===
            float4 _FogColor;
            float _FogDensity;
            float _NoiseScale;
            float _NoiseSpeed;
            float _DriftSpeed;
            float _DriftAmount;
            int _Octaves;
            float _Persistence;
            
            // === 構造体 ===
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };
            
            // === Vertex Shader ===
            v2f vert(appdata v)
            {
                v2f o;
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                o.worldPos = worldPos;
                return o;
            }
            
            // ============================================================
            // === Perlin Noise 実装 ===
            // ============================================================
            
            // ステップ1: ハッシュ関数（乱数生成）
            float hash(float n)
            {
                return frac(sin(n) * 43758.5453123);
            }
            
            // ステップ2: 2D用ハッシュ
            float hash(float2 p)
            {
                float h = dot(p, float2(127.1, 311.7));
                return frac(sin(h) * 43758.5453123);
            }
            
            // ステップ3: Perlin Noise（Value Noise版）
            float noise(float2 p)
            {
                // グリッド座標を取得
                float2 pi = floor(p);
                float2 pf = frac(p);
                
                // 4隅のハッシュ値を計算
                float n00 = hash(pi + float2(0.0, 0.0));
                float n10 = hash(pi + float2(1.0, 0.0));
                float n01 = hash(pi + float2(0.0, 1.0));
                float n11 = hash(pi + float2(1.0, 1.0));
                
                // 滑らかな補間（Smoothstep）
                float u = smoothstep(0.0, 1.0, pf.x);
                float v = smoothstep(0.0, 1.0, pf.y);
                
                // 双線形補間（Bilinear Interpolation）
                float nx0 = lerp(n00, n10, u);
                float nx1 = lerp(n01, n11, u);
                float nxy = lerp(nx0, nx1, v);
                
                // -1.0 ～ 1.0 に正規化
                return nxy * 2.0 - 1.0;
            }
            
            // ============================================================
            // === FBM: Fractal Brownian Motion ===
            // ============================================================
            float fbm(float2 p, int octaves, float persistence)
            {
                float amplitude = 1.0;
                float frequency = 1.0;
                float result = 0.0;
                float maxAmplitude = 0.0;
                
                // 複数層のNoiseを加算
                for (int i = 0; i < 4; i++)  // 最大4層
                {
                    if (i < octaves)
                    {
                        result += noise(p * frequency) * amplitude;
                        maxAmplitude += amplitude;
                        
                        frequency *= 2.0;
                        amplitude *= persistence;
                    }
                }
                
                return result / maxAmplitude;
            }
            
            // ============================================================
            // === Fragment Shader ===
            // ============================================================
            float4 frag(v2f i) : SV_Target
            {
                // === 座標準備 ===
                float2 uv = i.worldPos.xy;
                
                // === ドリフト効果（x方向移動） ===
                float drift = i.worldPos.x + _Time.y * _DriftSpeed;
                float driftWave = sin(drift * 0.5) * _DriftAmount;
                
                // === 複数層を重ねる（FBM） ===
                float2 p = uv * _NoiseScale + float2(drift, _Time.y * _NoiseSpeed);
                float n_fbm = fbm(p, _Octaves, _Persistence);
                
                // === 形状調整 ===
                float alpha = smoothstep(0.2, 0.8, n_fbm + driftWave);
                
                // === 密度調整 ===
                alpha *= _FogDensity;
                
                // === 最終出力 ===
                return float4(_FogColor.rgb, alpha);
            }
            ENDHLSL
        }
    }
}
