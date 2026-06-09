using Core;
using Model.Physics;
using UnityEngine;
using R3;
using UnityEditor;


namespace Model
{
    /// <summary>
    /// プレイヤーの状態を保持するModel。
    /// </summary>
    public class PlayerModel
    {
        // 定数
        private const float InteractionRange = 1.5f;
        private const float VelocityFactor = 3f;
        
        // フィールド
        private GlobalStateModel _globalStateModel;
        private EntityModel _entityModel;
        private IPhysicsSource _physicsSource;
        
        // プロパティ
        public Vector2 moveInput { get; set; }
        public int xGrid { get; set; }
        public int yGrid { get; set; }
        public ReactiveProperty<Vector2> position { get; } = new(Vector2.zero);
        public ReactiveProperty<Vector2> velocity { get; } = new(Vector2.zero);
        public Entity currentTarget { get; private set; }
        
        
        public PlayerModel(GlobalStateModel globalStateModel, EntityModel entityModel, IPhysicsSource physicsSource)
        {
            _globalStateModel = globalStateModel;
            _entityModel = entityModel;
            _physicsSource = physicsSource;
        }
        
        public void OnUpdate(float deltaTime)
        {
            velocity.Value = new Vector2(moveInput.x * VelocityFactor, 0f);
            position.Value += velocity.Value * deltaTime;
            xGrid = Utils.CalculateXGrid(position.Value.x);
        }

        public void Fly(Vector2 moveInput)
        {
            position.Value = _physicsSource.position;
            xGrid = Utils.CalculateXGrid(position.Value.x);
            yGrid = (int)position.Value.y / 10;
        }

        public void Interact()
        {
            currentTarget = _entityModel.GetInteractableEntity(xGrid, position.Value, InteractionRange);
            if (currentTarget != null)
            {
                currentTarget.OnInteract();
                Debug.Log("[PlayerModel] Interact with: " + currentTarget.dialogueNodeName);
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