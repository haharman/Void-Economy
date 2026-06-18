Shader "Learning/ProceduralNoise_Sprite"
{
    Properties
    {
        // === SpriteRenderer 必須 ===
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        
        // === 霧パラメータ ===
        _FogColor ("Fog Color", Color) = (1, 1, 1, 1)
        _FogDensity ("Fog Density", Range(0, 2)) = 0.6
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 2.0
        _NoiseSpeed ("Animation Speed", Range(0, 5)) = 1.0
        _DriftSpeed ("Drift Speed", Range(0, 10)) = 2.0
        _DriftAmount ("Drift Amount", Range(0, 1)) = 0.3
        _Octaves ("Octaves", Range(1, 4)) = 3
        _Persistence ("Persistence", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "RenderPipeline"="UniversalPipeline"
        }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            HLSLPROGRAM
            #pragma prefer_hlsl2021
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
                sampler2D _MainTex;
                float4 _MainTex_ST;
                float4 _Color;
                float4 _FogColor;
                float _FogDensity;
                float _NoiseScale;
                float _NoiseSpeed;
                float _DriftSpeed;
                float _DriftAmount;
                int _Octaves;
                float _Persistence;
            CBUFFER_END
            
            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                float3 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0)).xyz;
                o.pos = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                o.worldPos = worldPos;
                return o;
            }
            
            float hash(float n)
            {
                return frac(sin(n) * 43758.5453123);
            }
            
            float hash(float2 p)
            {
                float h = dot(p, float2(127.1, 311.7));
                return frac(sin(h) * 43758.5453123);
            }
            
            float noise(float2 p)
            {
                float2 pi = floor(p);
                float2 pf = frac(p);
                
                float n00 = hash(pi + float2(0.0, 0.0));
                float n10 = hash(pi + float2(1.0, 0.0));
                float n01 = hash(pi + float2(0.0, 1.0));
                float n11 = hash(pi + float2(1.0, 1.0));
                
                float u = smoothstep(0.0, 1.0, pf.x);
                float v = smoothstep(0.0, 1.0, pf.y);
                
                float nx0 = lerp(n00, n10, u);
                float nx1 = lerp(n01, n11, u);
                float nxy = lerp(nx0, nx1, v);
                
                return nxy * 2.0 - 1.0;
            }
            
            float fbm(float2 p, int octaves, float persistence)
            {
                float amplitude = 1.0;
                float frequency = 1.0;
                float result = 0.0;
                float maxAmplitude = 0.0;
                
                for (int i = 0; i < 4; i++)
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
            
            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.worldPos.xy;
                float drift = i.worldPos.x + _Time.y * _DriftSpeed;
                float driftWave = sin(drift * 0.5) * _DriftAmount;
                
                float2 p = uv * _NoiseScale + float2(drift, _Time.y * _NoiseSpeed);
                float n_fbm = fbm(p, _Octaves, _Persistence);
                
                float alpha = smoothstep(0.2, 0.8, n_fbm + driftWave);
                alpha *= _FogDensity;
                
                // _MainTex をサンプル（SpriteRenderer 互換性）
                float4 texColor = tex2D(_MainTex, float2(0, 0));
                
                float4 result = float4(_FogColor.rgb, alpha) * _Color;
                return result;
            }
            ENDHLSL
        }
    }
}
