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
    public class SpacePlayerModel
    {
        // 定数
        private const float InteractionRange = 2.5f;
        private const float VelocityFactor = 3.5f;
        private static readonly Vector2 startPosition = new Vector2(0f, -9f);
        // フィールド
        private EntityModel _entityModel;
        private IPhysicsSource _physicsSource;
        
        // プロパティ
        public Vector2 moveInput { get; set; }
        public int xGrid { get; set; }
        public int yGrid { get; set; }
        public ReactiveProperty<Vector2> position { get; } = new ReactiveProperty<Vector2>(startPosition);
        public ReactiveProperty<Vector2> velocity { get; } = new(Vector2.zero);
        public Entity currentTarget { get; private set; }
        
        
        public SpacePlayerModel(EntityModel entityModel, IPhysicsSource physicsSource)
        {
            _entityModel = entityModel;
            _physicsSource = physicsSource;
            
        }
        
        public void OnUpdate(float deltaTime)
        {
            velocity.Value = new Vector2(moveInput.x * VelocityFactor, 0f);
            position.Value += velocity.Value * deltaTime;
            xGrid = Utils.CalculateXGrid(position.Value.x);
            currentTarget = _entityModel.GetInteractableEntity(xGrid, position.Value, InteractionRange);
        }

        public void Fly(Vector2 moveInput)
        {
            position.Value = _physicsSource.position;
            xGrid = Utils.CalculateXGrid(position.Value.x);
            yGrid = (int)position.Value.y / 10;
        }

        public void Interact()
        {
            if (currentTarget != null)
            {
                currentTarget.OnInteract();
                Debug.Log("[PlayerModel] Interact with: " + currentTarget.DialogueNodeName);
            }
            else
                Debug.Log("[PlayerModel] Interact 対象なし");
        }
        
        public void OpenMenu()
        {
        }

        public void ForceExit()
        {
        }
    }
}