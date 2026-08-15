using UnityEngine;

namespace Model
{
    public interface IPlanetModel
    {
        string Name { get; }
        float Radius { get; }
        float Gravity { get; }
        float ParallaxHeight { get; }
        float OrbitalRadius { get; }
        float Phase { get; }
        float OrbitalPeriodSeconds { get; }
        Vector2 Position { get; }
        Vector2 Velocity { get; }

        float Mu { get; }
    }
}
