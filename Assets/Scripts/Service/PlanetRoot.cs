using System;
using System.Collections.Generic;
using System.Threading;
using Core;
using Model;
using UnityEngine;
using View;
using Presenter;
using UnityEngine.SceneManagement;

namespace Service
{
    public class PlanetRoot : MonoBehaviour
    {
        public CoordSystemId PlanetId { private get; set; }
        // Service
        private IUpdateService _orreryUpdateService;
        private UpdateService _planetUpdateService;
        private ISaveDataService _saveDataService;
        private ICameraSource _cameraSource;

        // Model
        private EntityModel _entityModel;
        
        // View
        [SerializeField] private ParallaxLoopView parallaxView;
        [SerializeField] private CoordView coordView;
        [SerializeField] private PlanetView planetView;
        // Presenter
        private ParallaxPresenter _parallaxPresenter; // 一時停止中
        private PlanetPresenter _planetPresenter;

        private readonly CancellationTokenSource _cts = new();
        
        public void Bootstrap(CoordSystemId planetId, IPlanetModel planetModel,
            IJetpackSource jetpackSource, ISaveDataService saveDataService, ICameraSource cameraSource)
        {
            PlanetId = planetId;
            _cameraSource = cameraSource;
            var ct = _cts.Token;
            Debug.Log("[SurfaceRoot] Init");
            _parallaxPresenter = new ParallaxPresenter(parallaxView, _cameraSource, planetId);
            _planetPresenter = new PlanetPresenter(planetId, planetView, coordView, jetpackSource, 
                planetModel, ct);
            _planetUpdateService = new UpdateService();
            _saveDataService = saveDataService; // まだ使わないが、ロードするならこの関数内で登録する必要がある
        }
        
        // Bootstrap と Initialize の間に必ずLoadが入る

        public void Initialize(IUpdateService orreryUpdateService)
        {
            _planetPresenter.Initialize();
            
            _orreryUpdateService = orreryUpdateService;
            _planetUpdateService.Register(_planetPresenter);
            _planetUpdateService.Register(_parallaxPresenter);
            _orreryUpdateService.Register(_planetUpdateService);
        }
        
        public void Terminate()
        {
            _cts.Cancel();
            
            _orreryUpdateService.Unregister(_planetUpdateService);
            _planetUpdateService.Unregister(_planetPresenter);
            _planetUpdateService.Unregister(_parallaxPresenter);
        }

        #region 自動アタッチ
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (parallaxView == null)
            {
                parallaxView = GetComponentInChildren<ParallaxLoopView>(true);
                if (parallaxView == null)
                    Debug.LogWarning($"{name}: ParallaxView が子階層に見つかりません", this);
            }

            if (coordView == null)
            {
                coordView = GetComponentInChildren<CoordView>(true);
                if (coordView == null)
                    Debug.LogWarning($"{name}: SurfaceView が子階層に見つかりません", this);
            }

            if (planetView == null)
            {
                planetView = GetComponentInChildren<PlanetView>(true);
                if (planetView == null)
                    Debug.LogWarning($"{name}: PlanetView が子階層に見つかりません", this);
            }
        }
        #endif
        #endregion 自動アタッチ
        
    }
}