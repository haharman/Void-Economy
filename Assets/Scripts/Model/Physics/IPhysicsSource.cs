using UnityEngine;

namespace Model.Physics
{
    public interface IPhysicsSource
    {
        public Vector2 position { get; }
        public Vector2 velocity { get; }
        public Vector2 acceleration { get; }
    }
}