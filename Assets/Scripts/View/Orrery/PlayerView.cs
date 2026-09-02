using System;
using System.Collections.Generic;
using Core;
using UnityEngine;
using Model;

namespace View
{
    public class PlayerView : MonoBehaviour
    {
        
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform globalCoordTransform;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float animationSpeed = 0.65f;
        
        private CoordSystemId _currentCoodinateSystem;
        private const float WalkingVelocityThreshold = 0.1f;
        private static readonly int WalkingHash = Animator.StringToHash("Walking");
        private IReadOnlyDictionary<CoordSystemId, Transform> _planetTransformDictionary;
        private float _defaultScaleX;
        private bool _initialized;

        public void Initialize(IReadOnlyDictionary<CoordSystemId, Transform> planetTransformDictionary)
        {
            _defaultScaleX = playerTransform.localScale.x;
            animator.speed = animationSpeed;
            _planetTransformDictionary = planetTransformDictionary;
            _initialized = true;
        }

        public void OnUpdatePositionVelocity(Vector2 position, Vector2 velocity)
        {
            playerTransform.localPosition = position;
            OnUpdateVelocity(velocity.x);
        }

        public void OnUpdateRotation(Vector2 forward)
        {
            // 向き
            Vector3 up = new Vector3(forward.x, forward.y, 0f);
            playerTransform.localRotation = Quaternion.LookRotation(Vector3.forward, up);
        }

        public void OnUpdateCoordPos(CoordPos coordPos)
        {
            if (!_initialized) return;

            if (coordPos.CoordSystem == CoordSystemId.Global)
            {
                playerTransform.SetParent(globalCoordTransform, worldPositionStays: false);
            }
            else if (_planetTransformDictionary.TryGetValue(coordPos.CoordSystem, out var planetCoordTransform))
            {
                playerTransform.SetParent(planetCoordTransform, worldPositionStays: false);
            }
            playerTransform.localPosition = coordPos.Position;
        }
        
        private void OnUpdateVelocity(float xVelocity)
        {
            if (!_initialized) return;
            
            //Debug.Log("[SurfacePlayerView] OnUpdateVelocity: " + xVelocity);
            // 向き
            if (xVelocity > 0)
                spriteRenderer.flipX = true;
            else if (xVelocity < 0)
                spriteRenderer.flipX = false;
            
            // アニメーション
            if (Mathf.Abs(xVelocity) > WalkingVelocityThreshold)
            {
                animator.SetBool(WalkingHash, true);
            }
            else
            {
                animator.SetBool(WalkingHash, false);
            }
        }
    }
}
