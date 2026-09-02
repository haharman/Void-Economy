using System.Collections.Generic;
using Core;
using Model;
using UnityEngine;
using View;

namespace Presenter
{
    public class CameraPresenter : ICameraSource, IUpdatable
    {
        private Camera _camera;
        private Dictionary<CoordSystemId, Transform> _coordTransformDictionary;
        private IJetpackSource _jetpackSource;

        public CoordSystemId Coord { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion LocalRotation { get; private set; }
        public Vector2 Size { get; private set; }
        
        public CameraPresenter(Camera camera, Dictionary<CoordSystemId, Transform> coordTransformDictionary, IJetpackSource jetpackSource)
        {
            _camera = camera;
            _coordTransformDictionary = coordTransformDictionary;
            _jetpackSource = jetpackSource;
        }

        public void OnUpdate(float deltaTime)
        {
            Coord = _jetpackSource.CoordPos.CurrentValue.CoordSystem;
            if(_coordTransformDictionary.TryGetValue(Coord, out Transform transform))
            {
                Position = transform.InverseTransformPoint(_camera.transform.position);
                LocalRotation = Quaternion.Inverse(transform.rotation) * _camera.transform.rotation;
            }
        }
    }
}