using UnityEngine;
using System.Runtime.InteropServices;

namespace Model.Physics
{

    // --- 宇宙船のデータ構造 ---

    // 状態
    [StructLayout(LayoutKind.Sequential)]
    public struct ShipState
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Vector2 Acceleration;
        public Vector2 Forward;
        public Vector2 VelocityForward;
        public float Scale;
    }

    // 基本性能
    [StructLayout(LayoutKind.Sequential)]
    public struct ShipConfig
    {
        public float ThrustPower; // (元の _thurstPower)
        public float MaxVelocity;
    }

    // 外部入力
    [StructLayout(LayoutKind.Sequential)]
    public struct ShipInput
    {
        public bool IsThrusting;
        public bool IsBoosting;
    }

    // --- 惑星のデータ構造 ---

    [StructLayout(LayoutKind.Sequential)]
    public struct PlanetData
    {
        public float Mass;
        public float Radius;
        public float AtmosphereRadius;
        public float GravityRadius;
        public float GravityFadeOutRadius;
        public Vector2 Position;
    }
}