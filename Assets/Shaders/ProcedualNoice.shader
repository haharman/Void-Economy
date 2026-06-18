Shader "Learning/ProceduralNoise"
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
        _Octaves ("Octaves (層数)", Range(1, 10)) = 9
        _Persistence ("Persistence (詳細度)", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
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
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            // ============================================================
            // === Perlin Noise 実装 ===
            // ============================================================
            // 簡易版 Perlin Noise（学習用）
            // 実際にはAsset Storeから Noise関数を使うことを推奨
            
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
            // 【説明】
            // グリッド内のランダム値を補間する
            // 入力: 2D座標
            // 出力: -1.0 ～ 1.0 のランダムな値
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
                
                // 【重要】滑らかな補間（Smoothstep）
                // これがPerlin Noiseをランダムノイズと区別する
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
            // 【説明】複数の Noise を重ねることで複雑な形を作る
            // 層が多いほど細かくなる
            float fbm(float2 p, int octaves, float persistence)
            {
                float amplitude = 1.0;      // 各層の強度
                float frequency = 1.0;     // 各層のスケール
                float result = 0.0;
                float maxAmplitude = 0.0;
                
                // 複数層のNoiseを加算
                for (int i = 0; i < octaves; i++)
                {
                    // i番目の層を計算
                    result += noise(p * frequency) * amplitude;
                    maxAmplitude += amplitude;
                    
                    // 次の層に向けてスケール調整
                    frequency *= 2.0;          // 周波数を2倍（細かくなる）
                    amplitude *= persistence;  // 振幅を減らす
                }
                
                // 正規化
                return result / maxAmplitude;
            }
            
            // ============================================================
            // === Fragment Shader ===
            // ============================================================
            fixed4 frag(v2f i) : SV_Target
            {
                // === 座標準備 ===
                float2 uv = i.worldPos.xy;
                
                // === ステップ1: 基本的なNoiseを取得 ===
                // （ここから複雑になっていく）
                
                // ステップ1-A: 静止したNoise
                // float noise1 = noise(uv * _NoiseScale);
                
                // ステップ1-B: アニメーションするNoise
                // float noise1 = noise(uv * _NoiseScale + _Time.y * _NoiseSpeed);
                
                // === ステップ2: ドリフト効果（x方向移動） ===
                // ドリフト速度を計算
                float drift = i.worldPos.x + _Time.y * _DriftSpeed;
                
                // ドリフト方向のゆらぎ（sin波で上下動）
                float driftWave = sin(drift * 0.5) * _DriftAmount;
                
                // === ステップ3: 複数層を重ねる（FBM） ===
                float2 p = uv * _NoiseScale + float2(drift, _Time.y * _NoiseSpeed);
                float n_fbm = fbm(p, _Octaves, _Persistence);
                
                // === ステップ4: 形状調整 ===
                // ノイズ値を適切な範囲に圧縮（透明度の計算用）
                
                // 【方法A】smoothstep（エッジを強調）
                float alpha_a = smoothstep(0.2, 0.8, n_fbm + driftWave);
                
                // 【方法B】絶対値（より複雑な形）
                float alpha_b = abs(n_fbm + driftWave) * 0.8;
                
                // 【方法C】sine波（より柔らかい）
                float alpha_c = sin((n_fbm + driftWave) * 3.14159) * 0.5 + 0.5;
                
                // どれを使うか選択（学習用）
                float alpha = alpha_a;
                
                // === ステップ5: 密度調整 ===
                alpha *= _FogDensity;
                
                // === 最終出力 ===
                return fixed4(_FogColor.rgb, alpha);
            }
            ENDCG
        }
    }
}
