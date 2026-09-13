using Core;
using UnityEngine;

namespace Presenter
{
    public interface ICameraSource
    {
        public CoordSystemId Coord { get; }
        public Vector3 Position { get; }
        public Quaternion LocalRotation { get; }
        public Vector2 Size { get; } // Unit単位 描画範囲の幅
    }
}