using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

namespace Model.Physics
{
    public class PhysicsEngine : IPhysicsSource
    {
        // IPhysicsSourceの実装
        public Vector2 position => _states[0].Position; // 例: プレイヤ
        public Vector2 velocity => _states[0].Velocity;
        public Vector2 acceleration => _states[0].Acceleration;
        
        [Header("Global Physics Settings")]
        public float GravityPower = 1.5f;
        public float SofteningFactor = 1f;
        public float MassFactor = 3f;

        private const int MAX_SHIPS = 10000; // 例: 1万隻

        // メモリ空間に連続確保される巨大な配列
        private NativeArray<ShipState> _states;
        private NativeArray<ShipConfig> _configs;
        private NativeArray<ShipInput> _inputs;
        private NativeArray<PlanetData> _planets;

        private JobHandle _jobHandle;

        private void Awake()
        {
            // 配列の初期化 (Allocator.Persistentでシーン生存中ずっと保持)
            _states = new NativeArray<ShipState>(MAX_SHIPS, Allocator.Persistent);
            _configs = new NativeArray<ShipConfig>(MAX_SHIPS, Allocator.Persistent);
            _inputs = new NativeArray<ShipInput>(MAX_SHIPS, Allocator.Persistent);
            
            // 惑星データの初期化 (元のstatic配列をNativeArrayに変換)
            InitializePlanets();
        }

        private void InitializePlanets()
        {
            _planets = new NativeArray<PlanetData>(2, Allocator.Persistent);
            _planets[0] = new PlanetData { Mass = 500f * MassFactor, Radius = 25f, AtmosphereRadius = 50f, GravityRadius = 500f, GravityFadeOutRadius = 550f, Position = new Vector2(0f, -100f) };
            _planets[1] = new PlanetData { Mass = 1331f * MassFactor, Radius = 55f, AtmosphereRadius = 110f, GravityRadius = 1000f, GravityFadeOutRadius = 1100f, Position = new Vector2(0f, 400f) };
        }

        private void Update()
        {
            // 1. 前フレームのJobが終わるまで待機
            _jobHandle.Complete();

            // 2. Jobのパラメータ設定
            var job = new PhysicsUpdateJob
            {
                DeltaTime = Time.deltaTime,
                GravityPower = GravityPower,
                SofteningFactor = SofteningFactor,
                Planets = _planets,
                Configs = _configs,
                Inputs = _inputs,
                States = _states
            };

            // 3. 全船の計算をマルチスレッドで開始（バッチサイズ64で分割）
            _jobHandle = job.Schedule(MAX_SHIPS, 64);
        }

        private void LateUpdate()
        {
            // View側が座標を取得しに来る前に計算を完了させておく
            _jobHandle.Complete();
        }

        // --- 外部（Presenter/View）との連携API ---

        /// <summary>ユーザーの入力を受け取る（毎フレームPresenterなどから呼ばれる）</summary>
        public void SetInput(int shipId, Vector2 forward, bool isThrusting, bool isBoosting)
        {
            _jobHandle.Complete(); // 念のため実行中の書き換えを防ぐ
            
            var state = _states[shipId];
            state.Forward = forward;
            _states[shipId] = state;

            _inputs[shipId] = new ShipInput { IsThrusting = isThrusting, IsBoosting = isBoosting };
        }

        /// <summary>Viewが毎フレーム座標を取得する（ポーリング用）</summary>
        public ShipState GetState(int shipId)
        {
            return _states[shipId];
        }

        // --- メモリリーク防止 ---
        private void OnDestroy()
        {
            _jobHandle.Complete();
            if (_states.IsCreated) _states.Dispose();
            if (_configs.IsCreated) _configs.Dispose();
            if (_inputs.IsCreated) _inputs.Dispose();
            if (_planets.IsCreated) _planets.Dispose();
        }

        public void OnUpdate(float deltaTime)
        {
            
        }
    }
}