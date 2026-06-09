using UnityEngine;
using Model;

namespace View
{
    public class SurfacePlayerView : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        private const float WalkingVelocityThreshold = 0.1f;
        private static readonly int WalkingHash = Animator.StringToHash("Walking");
        
        private float _defaultScaleX;

        public void Initialize()
        {
            _defaultScaleX = playerTransform.localScale.x;
        }
        
        public void OnUpdateVelocity(float xVelocity)
        {
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

        public void OnUpdatePosition(Vector2 position)
        {
            //Debug.Log("[SurfacePlayerView] OnUpdatePosition: " + position);
            // 座標
            playerTransform.position = new Vector3(position.x, position.y, playerTransform.position.z);

        }
    }
}
