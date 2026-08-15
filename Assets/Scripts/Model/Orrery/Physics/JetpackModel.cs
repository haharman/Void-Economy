using Core;
using UnityEngine;
using R3;

namespace Model
{
    /// <summary>
    /// プレイヤーの物理状態を保持するModel。
    /// PhysicsModel：物理演算ツール
    /// OrreryModel：星系の状態・座標系の定義
    ///
    /// 座標系の「いつ切り替えるか」はここが決め、「どう変換するか」は OrreryModel に委譲する。
    /// </summary>
    public class JetpackModel : IJetpackSource
    {
        // IJetpackSource
        public ReadOnlyReactiveProperty<CoordPos> CoordPos => _coordPos;
        private readonly ReactiveProperty<CoordPos> _coordPos = new();

        public Observable<CoordSystemId> LoadSurfaceRequested => _loadSurfaceRequested;
        private readonly Subject<CoordSystemId> _loadSurfaceRequested = new();

        public Observable<CoordSystemId> UnloadSurfaceRequested => _unloadSurfaceRequested;
        private readonly Subject<CoordSystemId> _unloadSurfaceRequested = new();
        
        // 定数
        private const float InteractionRange = 2.5f;
        private const float MaxVelocity = 200f;
        private const float OrreryRadius = 3000f;

        // 定数(PhysicsModelに移譲予定)
        // NOTE: 旧実装は速度・加速度に deltaTime を余分に掛けていたため、
        //       60fps 相当の実効値へ換算してある（旧 WalkingVelocity=700, ThrustAcceleration=15）。
        //       フレームレート非依存になった代わりに体感が変わる可能性があるので要再調整。
        private const float WalkingSpeed = 50.7f;        // units / sec
        private const float ThrustAcceleration = 40f; // units / sec^2

        public const float AirDrag = 0.1f;
        public const float BounceCutoff = 1f;
        public const float Bounciness = 0.25f;

        // フィールド
        private readonly EntityModel _entityModel;
        private readonly OrreryModel _orreryModel;

        private IPlanetModel _closestPlanet;
        private IPlanetModel ClosestPlanet => (_closestPlanet != null) ? _closestPlanet : _orreryModel.GetPlanetModel(_coordPos.Value.CoordSystem);

        private float KarmanRadius => KarmanAltitude + ClosestPlanet.Radius - 1f;

        private float KarmanAltitude => ClosestPlanet.Radius * 1.5f;
        private float SpaceLoadAltitude => ClosestPlanet.Radius * 1f;
        private float SurfaceUnloadAltitude => KarmanAltitude; // 保留。プレイヤーも消えちゃうので高度を最大値に変更中。
        private float SurfaceLoadAltitude => KarmanAltitude;//ClosestPlanet.Radius * 1f;
        private float ParallaxAltitude => ClosestPlanet.ParallaxHeight;
        private float LiftoffAltitude => 2f;
        private float TouchdownAltitude => 0.1f;
        private float GroundAltitude => 0f;

        private Vector2 _previousPosition;
        
        

        // Input
        public Vector2 MoveInput { get; set; }

        // セーブされないプロパティ
        public int XGrid { get; set; }
        public int YGrid { get; set; }
        public Entity CurrentTarget { get; private set; }


        public JetpackModel(EntityModel entityModel, OrreryModel orreryModel)
        {
            _entityModel = entityModel;
            _orreryModel = orreryModel;
        }

        public void Tick(float deltaTime)
        {
            CoordSystemId coord = _coordPos.Value.CoordSystem;
            Vector2 p = _coordPos.Value.Position;
            Vector2 v = _coordPos.Value.Velocity;
            Vector2 f = _coordPos.Value.Forward;

            Vector2 moveInput = new Vector2( MoveInput.x * f.y + MoveInput.y * f.x, -MoveInput.x * f.x + MoveInput.y * f.y );

            // 加速度（deltaTime は掛けない）
            Vector2 a = Vector2.zero;

            if (coord == CoordSystemId.Global)
            {
                a += _orreryModel.GetGravityAccelerationAt(p);
                a += ThrustAcceleration * moveInput;
                v += a * deltaTime;
                v = Vector2.ClampMagnitude(v, MaxVelocity);
                p += v * deltaTime;
                p = Vector2.ClampMagnitude(p, OrreryRadius);
            }
            else
            {
                if (p.y > TouchdownAltitude)
                {
                    // 設地していないなら重力をかける
                    float falloff = ClosestPlanet.Radius / (ClosestPlanet.Radius + Mathf.Max(p.y, 0f));
                    a.y -= ClosestPlanet.Gravity * falloff * falloff;
                    if(p.y > LiftoffAltitude)
                    {
                        a.x = moveInput.x * ThrustAcceleration;
                    }
                }
                else
                {
                    // 設置しているなら完全に接地させる
                    p.y = GroundAltitude;
                    // 横移動は歩行
                    v.x = moveInput.x * WalkingSpeed;
                    f = Vector2.up;
                }
                a.y += moveInput.y * ThrustAcceleration;
                
                v += a * deltaTime;
                
                // 空気抵抗
                v -= v * (AirDrag * Mathf.Clamp01(1f - p.y / KarmanAltitude)* deltaTime);
                
                // 最大速度制限
                v = Vector2.ClampMagnitude(v, MaxVelocity);
                
                p += v * deltaTime;
                
                // めり込み制限
                p.y = Mathf.Max(p.y, GroundAltitude);

                // 一周したら x を [0, 2πR) に折り返す。
                // ToSurfaceFrame が返す x と同じ値域に保ち、周回ごとに座標がずれていくのを防ぐ。
                p.x = Mathf.Repeat(p.x, 2f * Mathf.PI * ClosestPlanet.Radius);
            }

            // ---------------- 座標系の遷移判定 ----------------

            // Enter Karman : Space → Surface
            if (coord == CoordSystemId.Global)
            {
                var (id, closestPlanet, distanceToClosestPlanet) = _orreryModel.GetClosestPlanet(p);
                if (closestPlanet != null)
                {
                    _closestPlanet = closestPlanet;
                    if (distanceToClosestPlanet < KarmanRadius)
                    {
                        _coordPos.Value = _orreryModel.ToSurfaceCoord(id, p, v, f);
                        _previousPosition = _coordPos.Value.Position;
                        Debug.Log("[JetpackModel] Surface座標へ変更");
                    }
                    else
                    {
                        _coordPos.Value = new CoordPos(coord, p, v, f);
                        _previousPosition = p;
                    }
                }
                else Debug.LogError("[JetpackModel] Closest planet not found");
            }
            else
            {
                // UnloadSurfaceRequest
                if (_previousPosition.y <= SurfaceUnloadAltitude && p.y > SurfaceUnloadAltitude)
                {
                    _unloadSurfaceRequested.OnNext(coord);
                }
                
                // LoadSurfaceRequest
                if (_previousPosition.y >= SurfaceLoadAltitude && p.y < SurfaceLoadAltitude)
                {
                    _loadSurfaceRequested.OnNext(coord);
                }
                
                // Exit Karman : Surface → Space
                // Surface座標系の y は「高度」なので、比較対象は KarmanRadius ではなく KarmanAltitude
                if (p.y > KarmanAltitude)
                {
                    _coordPos.Value = _orreryModel.ToGlobalCoord(coord, p, v, f);
                    _previousPosition = _coordPos.Value.Position;
                    Debug.Log("[JetpackModel] Space座標へ変更");
                }
                else
                {
                    _coordPos.Value = new CoordPos(coord, p, v, f);
                    _previousPosition = p;
                }
            }

            // ---------------- コミット ----------------

            // Grid 判定
            XGrid = Utils.CalculateXGrid(p.x);
            YGrid = Utils.CalculateYGrid(p.y);

            // CurrentTarget 判定
            CurrentTarget = _entityModel.GetInteractableEntity(XGrid, p, InteractionRange);

            Debug.Log(_coordPos.Value.CoordSystem);
        }

        public void Interact()
        {
            if (CurrentTarget != null)
            {
                CurrentTarget.OnInteract();
                Debug.Log("[PlayerModel] Interact with: " + CurrentTarget.DialogueNodeName);
            }
            else
                Debug.Log("[PlayerModel] Interact 対象なし");
        }

        public void ReadFrom(CoordPos coordPos)
        {
            _coordPos.Value = coordPos;
            _previousPosition = coordPos.Position;
        }
    }
}
