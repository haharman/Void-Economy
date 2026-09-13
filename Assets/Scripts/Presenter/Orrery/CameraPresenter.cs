using System.Collections.Generic;
using System.Threading;
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
        private CinemachineView _cinemachineView;
        

        public CoordSystemId Coord { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion LocalRotation { get; private set; }
        public Vector2 Size { get; private set; }
        
        public CameraPresenter(Camera camera, Dictionary<CoordSystemId, Transform> coordTransformDictionary, IJetpackSource jetpackSource, CinemachineView cinemachineView, CancellationToken cancellationToken)
        {
            _camera = camera;
            _coordTransformDictionary = coordTransformDictionary;
            _jetpackSource = jetpackSource;
            _cinemachineView = cinemachineView;
            
            _cinemachineView.Initialize(_jetpackSource, cancellationToken);
        }

        public void OnUpdate(float deltaTime)
        {
            Coord = _jetpackSource.CoordPos.CurrentValue.CoordSystem;
            float cameraHeight = _camera.orthographicSize * 2f;
            Size = new Vector2(cameraHeight * _camera.aspect, cameraHeight);
            if(_coordTransformDictionary.TryGetValue(Coord, out Transform transform))
            {
                Position = transform.InverseTransformPoint(_camera.transform.position);
                LocalRotation = Quaternion.Inverse(transform.rotation) * _camera.transform.rotation;
            }
        }
    }
}