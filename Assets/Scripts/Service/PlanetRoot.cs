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
        [SerializeField] private ParallaxView parallaxView;
        [SerializeField] private CoordView coordView;
        [SerializeField] private PlanetView planetView;
        // Presenter
        private ParallaxPresenter _parallaxPresenter; // parallaxView 未設定の惑星では null
        private PlanetPresenter _planetPresenter;

        // Terminate で Cancel+Dispose するため readonly にしない（Bootstrap のたびに作り直す）
        private CancellationTokenSource _cts;
        
        public void Bootstrap(CoordSystemId planetId, IPlanetModel planetModel,
            IJetpackSource jetpackSource, ISaveDataService saveDataService, ICameraSource cameraSource)
        {
            PlanetId = planetId;
            _cameraSource = cameraSource;
            DisposeCts();
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;
            Debug.Log("[SurfaceRoot] Init");
            if (parallaxView != null)
            {
                // 半径の出所は PlanetModel.Radius に一本化（ParallaxView への手入力は廃止）
                parallaxView.SetPlanetRadius(planetModel.Radius);
                _parallaxPresenter = new ParallaxPresenter(parallaxView, _cameraSource, planetId);
            }
            else
            {
                _parallaxPresenter = null;
                Debug.LogWarning($"[PlanetRoot] {planetId}: ParallaxView が未設定のためParallaxを無効化します", this);
            }
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
            if (_parallaxPresenter != null) _planetUpdateService.Register(_parallaxPresenter);
            _orreryUpdateService.Register(_planetUpdateService);
        }
        
        public void Terminate()
        {
            DisposeCts();

            // 子を先に外してから親を外す（親が外れた後は子の削除が処理されないため）
            if (_planetUpdateService != null)
            {
                if (_planetPresenter != null) _planetUpdateService.Unregister(_planetPresenter);
                if (_parallaxPresenter != null) _planetUpdateService.Unregister(_parallaxPresenter);
            }
            
            // Initialize 前に Terminate されても NRE にしない
            if (_orreryUpdateService != null && _planetUpdateService != null)
                _orreryUpdateService.Unregister(_planetUpdateService);
            _orreryUpdateService = null;
        }

        private void DisposeCts()
        {
            if (_cts == null) return;
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }

        #region 自動アタッチ
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (parallaxView == null)
            {
                parallaxView = GetComponentInChildren<ParallaxView>(true);
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