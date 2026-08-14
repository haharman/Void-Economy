using System.Numerics;
using System;
using UnityEngine;


namespace Model
{
    public class InteractionGuideModel
    {
        private JetpackModel _jetpackModel;

        private Entity _currentTarget => _jetpackModel.CurrentTarget;
        private Entity _previousTarget;
        private bool _wasTargetNull = false;

        public event Action<UnityEngine.Vector2> OnInteractionGuideEnabled;
        public event Action OnInteractionGuideDisabled;

        public InteractionGuideModel(JetpackModel pM)
        {
            _jetpackModel = pM;
        }

        public void Tick(float deltaTime)
        {
            // ターゲットあり && (前フレームターゲットなし || 前フレームターゲットと現在ターゲットが異なる)
            // 場合にInteractionGuideを有効化
            if(_currentTarget != null && (_wasTargetNull || _previousTarget != _currentTarget))
            {
                Debug.Log($"[InteractionGuideModel] InvokeEnabled");
                OnInteractionGuideEnabled?.Invoke(_currentTarget.InteractionGuidePosition);
                _wasTargetNull = false;
                _previousTarget = _currentTarget;
            }
            // 無効化
            else if(!_wasTargetNull && _jetpackModel.CurrentTarget == null)
            {
                Debug.Log("[InteractionGuideModel] InvokeDisabled");
                OnInteractionGuideDisabled?.Invoke();
                _wasTargetNull = true;
            }
        }
    }
}