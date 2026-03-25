using Core;
using Model.Physics;
using UnityEngine;
using R3;


namespace Model
{
    /// <summary>
    /// プレイヤーの状態を保持するModel。
    /// </summary>
    public class PlayerModel
    {
        private GlobalStateModel _globalStateModel;
        private EntityModel _entityModel;
        private IPhysicsSource _physicsSource;
        
        #region Input
        public Vector2 moveInput { get; set; }
        public bool interactFlag { get; set; }
        public bool openMenuFlag { get; set; }
        public bool forceExitFlag { get; set; }
        #endregion
        
        #region 状態
        public Surface currentSurface { get; set; }
        public int xGrid { get; set; }
        public int yGrid { get; set; }
        public ReactiveProperty<Vector2> position { get; } = new(Vector2.zero);
        public ReactiveProperty<Vector2> velocity { get; } = new(Vector2.zero);
        public Entity currentTarget { get; private set; }
        public float interactionRange { get; } = 1.5f;
        const float VelocityFactor = 1f;
        #endregion
        
        public PlayerModel(GlobalStateModel globalStateModel, EntityModel entityModel, IPhysicsSource physicsSource)
        {
            _globalStateModel = globalStateModel;
            _entityModel = entityModel;
            _physicsSource = physicsSource;
        }
        
        // --- メソッド ---
        public void SetTarget(Entity entity)
        {
            currentTarget = entity;
        }

        public void ClearTarget()
        {
            currentTarget = null;
        }
        
        public void ClearFlags()
        {
            interactFlag = false;
            openMenuFlag = false;
            forceExitFlag = false;
        }

        public void OnUpdate(float deltaTime)
        {
            if (_globalStateModel.currentSceneState == GlobalStateModel.SceneState.Surface)
            {
                velocity.Value = new Vector2(moveInput.x * VelocityFactor, 0f);
                position.Value += velocity.Value * deltaTime;
                xGrid = (int)position.Value.x / 10;
                currentTarget = _entityModel.GetInteractableEntity((int)currentSurface, xGrid, position.Value, interactionRange);
            }
            else if(_globalStateModel.currentSceneState == GlobalStateModel.SceneState.Space)
            {
                position.Value = _physicsSource.position;
                xGrid = (int)position.Value.x / 10;
                yGrid = (int)position.Value.y / 10;
            }
            
        }
    }
}