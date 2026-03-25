using UnityEngine;

namespace Core
{
    using System;

    /// <summary>
    /// プレイヤーの論理座標と所属レイヤーを保持する構造体 (Core層)
    /// </summary>
    public readonly struct SurfacePosition : IEquatable<SurfacePosition>
    {
        public Surface surface { get; }
        public float x { get; }
        public float y { get; }
        public int layerIndex { get; }

        public SurfacePosition(Surface surface, float x, float y, int layerIndex)
        {
            this.surface = surface;
            this.x = x;
            this.y = y;
            this.layerIndex = layerIndex;
        }

        // Rxでの比較パフォーマンスを最適化
        public bool Equals(SurfacePosition other)
        {
            return Mathf.Approximately(x, other.x) && Mathf.Approximately(y, other.y) && layerIndex == other.layerIndex;
        }
        
        public override bool Equals(object obj) => obj is SurfacePosition other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(x, y, layerIndex);
    }
}