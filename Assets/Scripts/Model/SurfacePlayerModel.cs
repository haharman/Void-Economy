using System.Numerics;
using Core;
using Model.Physics;
using UnityEngine;
using R3;
using UnityEditor;
using Vector2 = UnityEngine.Vector2;


namespace Model
{
    /// <summary>
    /// プレイヤーの状態を保持するModel。
    /// </summary>
    public class SurfacePlayerModel
    {
        // 定数
        private const float InteractionRange = 2.5f;
        private const float VelocityFactor = 3.5f;
        private static readonly Vector2 StartPosition = new Vector2(0f, -9f); // ロードが実装されれば不要
        
        // フィールド
        private GlobalStateModel _globalStateModel;
        private EntityModel _entityModel;
        private IPhysicsSource _physicsSource;
        
        // セーブされるプロパティ
        public bool IsSurface;
        public ReactiveProperty<Vector2> Position { get; } = new ReactiveProperty<Vector2>(StartPosition);
        public Vector2 Forward = new Vector2(0f, 0f);
        public ReactiveProperty<Vector2> Velocity { get; } = new(Vector2.zero);
        
        // セーブされないプロパティ
        public Vector2 MoveInput { get; set; }
        public int XGrid { get; set; }
        public int YGrid { get; set; }
        public Entity CurrentTarget { get; private set; }
        
        
        public SurfacePlayerModel(EntityModel entityModel)
        {
            _entityModel = entityModel;
        }
        
        public void Tick(float deltaTime)
        {
            Velocity.Value = new Vector2(MoveInput.x * VelocityFactor, 0f);
            Position.Value += Velocity.Value * deltaTime;
            XGrid = Utils.CalculateXGrid(Position.Value.x);
            CurrentTarget = _entityModel.GetInteractableEntity(XGrid, Position.Value, InteractionRange);
        }
        public class PlayerSaveData
        {
            public bool isSurface;
            public Vector2 position; // 座標
            public Vector2 forward; // 向き
            public Vector2 velocity; // 速度
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
    }
}