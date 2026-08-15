using System;
using Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Model
{
    [Serializable]
    public class PlanetModel : IPlanetModel
    {
        [SerializeField, FormerlySerializedAs("name")] private string _name;
        [SerializeField, FormerlySerializedAs("radius")] private float _radius;
        [SerializeField, FormerlySerializedAs("gravity")] private float _gravity;
        [SerializeField, FormerlySerializedAs("parallaxHeight")] private float _parallaxHeight;
        [SerializeField, FormerlySerializedAs("orbitalRadius")] private float _orbitalRadius;
        [SerializeField, FormerlySerializedAs("orbitalPeriodSeconds")] private float _orbitalPeriodSeconds;

        public string Name => _name;
        public float Radius => _radius;
        public float Gravity => _gravity;
        public float ParallaxHeight => _parallaxHeight;
        public float OrbitalRadius => _orbitalRadius;
        public float OrbitalPeriodSeconds => _orbitalPeriodSeconds;

        public float Phase { get; private set; }
        public Vector2 Position { get; private set; }
        public float Mu => Gravity * Radius * Radius;

        /// <summary>
        /// 公転による速度（グローバル座標系）。
        /// Surface座標系との相互変換で、惑星に対する相対速度を得るために使う。
        /// </summary>
        public Vector2 Velocity { get; private set; }

        public void Tick(double orreryCumulativeSeconds)
        {
            // 恒星など公転しない天体はゼロ除算になるのでガード
            if (OrbitalPeriodSeconds <= 0f)
            {
                Phase = 0f;
                Position = OrbitalRadius * new Vector2(1f, 0f);
                Velocity = Vector2.zero;
                return;
            }

            Phase = (float)(orreryCumulativeSeconds / OrbitalPeriodSeconds % 1.0);
            float angle = Phase * 2f * Mathf.PI;

            Position = OrbitalRadius * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // d/dt [ R * (cos(ωt), sin(ωt)) ] = R * ω * (-sin(ωt), cos(ωt))
            float angularVelocity = 2f * Mathf.PI / OrbitalPeriodSeconds;
            Velocity = OrbitalRadius * angularVelocity * new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));
        }
    }
}
