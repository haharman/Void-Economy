using Model;
using System;
using UnityEngine;
using DG.Tweening;

namespace View
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class InteractionGuideView : MonoBehaviour
    {
        private InteractionGuideModel _model;
        private SpriteRenderer _spriteRenderer;
        [SerializeField] private float offsetY = 1.0f;
        [SerializeField] private float duration = 0.5f;
        
        public void Initialize(InteractionGuideModel model, CoordView coordView)
        {
            _model = model;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            OnDisable();
            OnEnable();
            DisableHandler();
            transform.SetParent(coordView.transform);
        }

        private void OnEnable()
        {
            if (_model == null)
            {
                Debug.LogWarning("[InteractionGuideView] Modelが割り当てられていません");
                return;
            }

            _model.OnInteractionGuideEnabled += EnableHandler;
            _model.OnInteractionGuideDisabled += DisableHandler;
        }

        private void OnDisable()
        {
            if(_model == null) return;
            _model.OnInteractionGuideEnabled -= EnableHandler;
            _model.OnInteractionGuideDisabled -= DisableHandler;
        }
        
        private void EnableHandler(Vector2 position)
        {
            //Debug.Log("[InteractionGuideView] EnableHandler(): " + position);
            Vector3 start = new Vector3(position.x, position.y - offsetY, transform.position.z);
            Vector3 end = new Vector3(position.x, position.y, transform.position.z);
            _spriteRenderer.enabled = true;
            
            transform.DOKill();
            _spriteRenderer.DOKill();
            transform.DOMove(end, duration).From(start).SetEase(Ease.OutQuad);
            _spriteRenderer.DOFade(1f, duration).From(0f).SetEase(Ease.OutQuad);
        }

        private void DisableHandler()
        {
            //Debug.Log("[InteractionGuideView] DisableHandler()");
            Vector3 end = new Vector3(transform.position.x, transform.position.y - offsetY, transform.position.z);
            
            transform.DOKill();
            _spriteRenderer.DOKill();
            transform.DOMove(end, duration).SetEase(Ease.OutQuad);
            _spriteRenderer.DOFade(0f, duration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => _spriteRenderer.enabled = false);
        }


    }
}