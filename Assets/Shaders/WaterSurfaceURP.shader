Shader "Custom/WaterSurfaceURP"
{
    Properties
    {
        [Header(Base Textures And Color)]
        _MainTex ("水面ベーステクスチャ (Water Base Tex)", 2D) = "white" {}
        _NoiseTex ("歪み用ノイズ (Distortion Noise, Optional)", 2D) = "gray" {}
        _Color ("水の色 (Water Tint Color)", Color) = (1,1,1,1)

        [Header(Required_Ambient Wave from Source2)]
        _WaveSpeed ("波の速度 (Wave Speed)", Range(0, 3)) = 0.3
        _WaveAmplitude ("振幅 (Wave Amplitude)", Range(0, 4)) = 1.0
        _DistortionStrength ("揺らぎの強さ (Ambient Distortion Strength)", Range(0, 2)) = 0.5

        [Header(Required_Ripple from Source1, externally triggered)]
        _RippleStrength ("波紋の強さ (Ripple Strength)", Range(0, 3)) = 1.2
        _Damping ("減衰 (Damping / Decay over time)", Range(0, 5)) = 1.2

        [Header(Optional Fine_Tuning)]
        _WaveFrequency ("波の周波数 (Ambient Wave Frequency)", Range(0.5, 20)) = 6.0
        _RippleSpeed ("波紋の伝播速度 (Ripple Propagation Speed)", Range(0.05, 2)) = 0.4
        _RippleFrequency ("波紋の細かさ (Ripple Frequency)", Range(5, 80)) = 30.0
        _RippleDistanceFalloff ("波紋の距離減衰 (Ripple Distance Falloff)", Range(0, 10)) = 2.0

        [Header(Optional Sun Glint Realism)]
        [Toggle] _EnableGlint ("陽光ハイライトを有効化 (Enable Sun Glint)", Float) = 1
        _GlintDirection ("光の方向 (Glint Light Direction)", Vector) = (-0.3, 1.0, 0.3, 0)
        _GlintPower ("ハイライトの鋭さ (Glint Sharpness)", Range(1, 200)) = 60
        _GlintIntensity ("ハイライトの強さ (Glint Intensity)", Range(0, 5)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "Sprite Unlit"
            Tags { "LightMode" = "UniversalForwardOnly" }
            Cull Off
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define PI 3.14159265358979
            #define WATER_ANGLE_DIV 7.0
            #define WATER_STEPS 8
            #define MAX_RIPPLES 8

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                float4 _NoiseTex_ST;
                float4 _Color;

                float _WaveSpeed;
                float _WaveAmplitude;
                float _DistortionStrength;
                float _WaveFrequency;

                float _RippleStrength;
                float _Damping;
                float _RippleSpeed;
                float _RippleFrequency;
                float _RippleDistanceFalloff;

                float _EnableGlint;
                float4 _GlintDirection;
                float _GlintPower;
                float _GlintIntensity;

                float4 _RippleData[MAX_RIPPLES];
                int _RippleCount;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color       : COLOR;
                float2 uv          : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.color = IN.color;
                OUT.uv = IN.uv;
                return OUT;
            }

            float WaterField(float2 uv, float time)
            {
                float deltaTheta = 2.0 * PI / WATER_ANGLE_DIV;
                float sum = 0.0;
                float drift = time * _WaveSpeed * 1.5;

                for (int i = 0; i < WATER_STEPS; i++)
                {
                    float theta = deltaTheta * (float)i;
                    float2 adj = uv;
                    adj.x += cos(theta) * time * _WaveSpeed + drift;
                    adj.y -= sin(theta) * time * _WaveSpeed - drift;
                    sum += cos((adj.x * cos(theta) - adj.y * sin(theta)) * _WaveFrequency) * _WaveAmplitude;
                }
                return cos(sum);
            }

            float2 ComputeAmbientGradient(float2 uv, float time, float aspect, out float height)
            {
                const float eps = 0.01;
                float h0 = WaterField(uv, time);
                float hx = WaterField(uv + float2(eps, 0.0), time);
                float hy = WaterField(uv + float2(0.0, eps * aspect), time);
                height = h0;
                return float2((h0 - hx) / eps, (h0 - hy) / eps);
            }

            float2 ComputeRippleGradient(float2 uv, float time, float aspect, out float height)
            {
                float2 grad = float2(0.0, 0.0);
                float h = 0.0;
                int count = min(_RippleCount, MAX_RIPPLES);

                for (int i = 0; i < count; i++)
                {
                    float4 r = _RippleData[i];
                    float2 origin = r.xy;
                    float startTime = r.z;
                    float age = time - startTime;
                    if (age < 0.0) continue;

                    float2 d = uv - origin;
                    d.x *= aspect;
                    float dist = length(d);
                    float distSafe = max(dist, 1e-4);

                    float waveR = _RippleSpeed * age;
                    float distFalloff = exp(-_RippleDistanceFalloff * dist);
                    float timeFalloff = exp(-_Damping * age);
                    float envelope = saturate(distFalloff * timeFalloff);

                    float phase = (dist - waveR) * _RippleFrequency;
                    float hi = sin(phase) * envelope;
                    h += hi;

                    const float epsilon = 0.001;
                    float phase2 = ((dist + epsilon) - waveR) * _RippleFrequency;
                    float envelope2 = saturate(exp(-_RippleDistanceFalloff * (dist + epsilon)) * timeFalloff);
                    float hForward = sin(phase2) * envelope2;
                    float dHdr = (hForward - hi) / epsilon;

                    float2 dirN = d / distSafe;
                    grad += dirN * dHdr;
                }

                height = h;
                return grad;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float time = _Time.y;

                float aspect = (_MainTex_TexelSize.w > 0.0)
                    ? _MainTex_TexelSize.z / _MainTex_TexelSize.w
                    : 1.0;

                float ambientHeight;
                float2 ambientGrad = ComputeAmbientGradient(uv, time, aspect, ambientHeight);

                float rippleHeight;
                float2 rippleGrad = ComputeRippleGradient(uv, time, aspect, rippleHeight);

                float2 totalGrad = ambientGrad * _DistortionStrength + rippleGrad * _RippleStrength;
                float gradLen = length(totalGrad);
                const float maxGrad = 0.6;
                if (gradLen > maxGrad)
                {
                    totalGrad *= maxGrad / gradLen;
                }

                const float uvDistortAmount = 0.08;
                float2 distortedUV = saturate(uv + totalGrad * uvDistortAmount);

                float2 noiseUV = uv * 3.0 + time * _WaveSpeed * 0.05;
                float2 noiseSample = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).rg;
                distortedUV = saturate(distortedUV + (noiseSample - 0.5) * 0.02 * _DistortionStrength);

                float4 baseTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, distortedUV);
                float3 waterColor = baseTex.rgb * _Color.rgb * IN.color.rgb;

                float3 lightDirRaw = _GlintDirection.xyz;
                float3 lightDir = (dot(lightDirRaw, lightDirRaw) > 1e-6)
                    ? normalize(lightDirRaw)
                    : float3(0.0, 1.0, 0.0);

                const float pseudoNormalFlatness = 0.35;
                float3 normal = normalize(float3(-totalGrad.x, pseudoNormalFlatness, -totalGrad.y));
                float ndotl = max(0.0, dot(normal, lightDir));
                float glint = pow(ndotl, _GlintPower) * _GlintIntensity * _EnableGlint;

                waterColor += glint;

                return float4(waterColor, baseTex.a * _Color.a * IN.color.a);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
