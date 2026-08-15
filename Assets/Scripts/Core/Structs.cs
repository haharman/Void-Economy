using UnityEngine;

namespace Core
{
    public readonly struct CoordPos
    {
        public readonly CoordSystemId CoordSystem;
        public readonly Vector2 Position;
        public readonly Vector2 Velocity;
        public readonly Vector2 Forward;

        public CoordPos(CoordSystemId coordSystem, Vector2 position, Vector2 velocity, Vector2 forward)
        {
            CoordSystem = coordSystem;
            Position = position;
            Velocity = velocity;
            Forward = forward;
        }
    }
}