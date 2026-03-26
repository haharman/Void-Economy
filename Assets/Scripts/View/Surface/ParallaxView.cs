using UnityEngine;

namespace View
{
    public class ParallaxView : MonoBehaviour
    {
        [System.Serializable]
        public class ParallaxLayer
        {
            [Tooltip("ループ用に横に隙間なく2枚並べたSprite")]
            public Transform[] sprites;
            
            [Tooltip("1.0 = プレイヤーと同じ平面, 0.5 = カメラの半分の速度(中景), 0.0 = 通常の配置(近景)")]
            public float parallaxFactor;
            
            [Header("Horizontal Boundaries")]
            public float leftBoundaryX;
            public bool isLeftInfinite = true;

            public float rightBoundaryX;
            public bool isRightInfinite = true;

            [HideInInspector] public float spriteWidth;
        }

        [SerializeField] private ParallaxLayer[] layers;
        [SerializeField] private Transform cameraTransform;
        
        private float _previousCameraX;
        private bool _isInitialized = false;

        private void Update()
        {
            float cameraXPosition = cameraTransform.position.x;
            if (!_isInitialized)
            {
                Initialize(cameraXPosition);
                return;
            }
            
            float deltaX = cameraXPosition - _previousCameraX;

            foreach (var layer in layers)
            {
                if (layer.sprites == null || layer.sprites.Length < 2) continue;

                // スプライトの半幅（中心から端までの距離）
                float halfWidth = layer.spriteWidth / 2f;

                // 1. パララックス効果による移動
                float moveAmount = deltaX * layer.parallaxFactor;

                foreach (var sprite in layer.sprites)
                {
                    sprite.position += new Vector3(moveAmount, 0, 0);
                }

                // 2. 画面外に出たスプライトのループ（ワープ）処理
                foreach (Transform currentSprite in layer.sprites)
                {
                    // カメラとスプライト中心の距離
                    float distFromCamera = cameraXPosition - currentSprite.position.x;

                    // カメラが右に進み、スプライトが左の画面外に完全に出た場合
                    if (distFromCamera > layer.spriteWidth)
                    {
                        float warpAmount = layer.spriteWidth * layer.sprites.Length;
                        float targetX = currentSprite.position.x + warpAmount;
                        float targetRightEdge = targetX + halfWidth;
                        bool canWarp = (layer.isRightInfinite || cameraXPosition < layer.rightBoundaryX);
                        if (canWarp)
                            currentSprite.position += new Vector3(warpAmount, 0, 0);
                    }
                    // カメラが左に進み、スプライトが右の画面外に完全に出た場合
                    else if (distFromCamera < -layer.spriteWidth)
                    {
                        float warpAmount = layer.spriteWidth * layer.sprites.Length;
                        float targetX = currentSprite.position.x - warpAmount;
                        float targetLeftEdge = targetX - halfWidth;
                        bool canWarp = (layer.isLeftInfinite || cameraXPosition > layer.leftBoundaryX);
                        if (canWarp)
                            currentSprite.position -= new Vector3(warpAmount, 0, 0);
                    }
                }
            }

            // 今回のカメラ座標を保存
            _previousCameraX = cameraXPosition;
        }

        private void Initialize(float initialCameraX)
        {
            foreach (var layer in layers)
            {
                if (layer.sprites != null && layer.sprites.Length >= 2)
                {
                    SpriteRenderer sr = layer.sprites[0].GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        layer.spriteWidth = sr.bounds.size.x;
                    }
                    else
                    {
                        Debug.LogError("SpriteRendererが見つかりません。ワープ計算ができません。");
                    }
                }
            }
            _previousCameraX = initialCameraX;
            _isInitialized = true;
        }
    }
}