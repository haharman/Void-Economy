using Core;
using R3;
using UnityEngine;

namespace Model
{
    public interface IJetpackSource
    {
        public ReadOnlyReactiveProperty<CoordPos> CoordPos { get; }
        public Observable<CoordSystemId> LoadSurfaceRequested { get; }
        public Observable<CoordSystemId> UnloadSurfaceRequested { get; }
        public Observable<Vector2> PlayerWarped { get; }
    }
}