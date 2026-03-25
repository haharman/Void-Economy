using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Model.Physics
{
    [BurstCompile]
    public struct PhysicsUpdateJob : IJobParallelFor
    {
        // グローバルな定数パラメータ
        [ReadOnly] public float DeltaTime;
        [ReadOnly] public float GravityPower;
        [ReadOnly] public float SofteningFactor;

        // 惑星のデータ（全船で共通の読み取り専用）
        [ReadOnly] public NativeArray<PlanetData> Planets;

        // 船ごとのデータ（インデックスで紐づく）
        [ReadOnly] public NativeArray<ShipConfig> Configs;
        [ReadOnly] public NativeArray<ShipInput> Inputs;

        // 読み書き両方行う最新の状態
        public NativeArray<ShipState> States;

        public void Execute(int index)
        {
            // 現在のデータをコピーして取得
            var state = States[index];
            var config = Configs[index];
            var input = Inputs[index];

            // 1. 各種加速度の計算
            Vector2 thrustAcceleration = Vector2.zero;
            Vector2 swingByAcceleration = Vector2.zero;

            if (input.IsThrusting)
                thrustAcceleration = state.Forward * config.ThrustPower;

            if (input.IsBoosting)
                swingByAcceleration = CalculateBoost(state.Position, state.VelocityForward);

            // 2. 速度と座標の更新
            state.Acceleration = thrustAcceleration + swingByAcceleration + CalculateGravity(state.Position);
            state.Velocity += state.Acceleration * DeltaTime;
            state.Velocity = Vector2.ClampMagnitude(state.Velocity, config.MaxVelocity);

            // 3. スケールの更新と移動
            state.Scale = CalculateScale(state.Position);
            state.Position += state.Scale * DeltaTime * state.Velocity;

            if (state.Velocity.magnitude > 0.01f)
                state.VelocityForward = state.Velocity.normalized;

            // 変更した状態を配列に戻す（必須）
            States[index] = state;
        }

        // --- 以下、元のメソッドをJob内に移植 ---

        private Vector2 CalculateGravity(Vector2 position)
        {
            Vector2 gravity = Vector2.zero;
            for (int i = 0; i < Planets.Length; i++)
            {
                Vector2 direction = Planets[i].Position - position;
                float distance = direction.magnitude;
                
                if (distance < Planets[i].GravityFadeOutRadius)
                {
                    float strength = Planets[i].Mass / (Mathf.Pow(distance, GravityPower) + SofteningFactor);
                    Vector2 g = direction.normalized * strength;
                    
                    if (distance > Planets[i].GravityRadius)
                    {
                        g *= Mathf.InverseLerp(Planets[i].GravityFadeOutRadius, Planets[i].GravityRadius, distance);
                    }
                    gravity += g;
                }
            }
            return gravity;
        }

        private Vector2 CalculateBoost(Vector2 position, Vector2 velocityForward)
        {
            Vector2 boost = Vector2.zero;
            for (int i = 0; i < Planets.Length; i++)
            {
                Vector2 direction = Planets[i].Position - position;
                float distance = direction.magnitude;
                
                if (distance < Planets[i].GravityRadius)
                {
                    boost += velocityForward * Planets[i].Mass / (distance + SofteningFactor);
                }
            }
            return boost;
        }

        private float CalculateScale(Vector2 position)
        {
            float scale = 1f;
            for (int i = 0; i < Planets.Length; i++)
            {
                Vector2 direction = Planets[i].Position - position;
                float distance = direction.magnitude;
                
                if (distance < Planets[i].AtmosphereRadius)
                {
                    scale = Mathf.Lerp(1f, 0.05f, Mathf.InverseLerp(Planets[i].AtmosphereRadius, Planets[i].Radius, distance));
                }
            }
            return scale;
        }
    }
}