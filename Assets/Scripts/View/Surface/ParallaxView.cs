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
            
            // プレイヤーが右に1.0進むとき、各レイヤーはparallaxFactor左に動く
            
            // 2.0: プレイヤーの2倍の速度で動く（近景）
            // 1.0: プレイヤーと同じ平面
            // 0.5: 遠景（ゆっくり動く）
            // 0.0: 超遠景（動かない）
            [Tooltip("1.0 = プレイヤーと同じ平面, 0.5 = カメラの半分の速度(中景), 0.0 = 通常の配置(近景)")]
            public float parallaxFactor; 

            [HideInInspector] public float spriteWidth;
        }

        [SerializeField] private ParallaxLayer[] layers;
        
        private float _previousCameraX;
        private bool _isInitialized = false;

        // Presenterからカメラ（またはプレイヤー）のX座標を渡して毎フレーム呼ぶ
        public void UpdateBackground(float cameraXPosition)
        {
            // 初回呼び出し時に初期化を行う（初回から大きくワープするのを防ぐため）
            if (!_isInitialized)
            {
                Initialize(cameraXPosition);
                return;
            }

            // 前回からのカメラの移動量を計算
            float deltaX = cameraXPosition - _previousCameraX;

            foreach (var layer in layers)
            {
                if (layer.sprites == null || layer.sprites.Length < 2) continue;

                // 1. パララックス効果による移動
                // Factorが1ならカメラと同じだけ動き(画面に固定されて見える)、0なら動かない
                float moveAmount = deltaX * layer.parallaxFactor;

                foreach (var sprite in layer.sprites)
                {
                    sprite.position += new Vector3(moveAmount, 0, 0);
                }

                // 2. 画面外に出たスプライトのループ（ワープ）処理
                for (int i = 0; i < layer.sprites.Length; i++)
                {
                    Transform currentSprite = layer.sprites[i];
                    
                    // カメラの現在地と、スプライトの中心との距離
                    float distFromCamera = cameraXPosition - currentSprite.position.x;

                    // カメラが右に進み、スプライトが左の画面外に完全に出た場合
                    if (distFromCamera > layer.spriteWidth)
                    {
                        // 2枚分の幅だけ右に瞬間移動（ワープ）させる
                        currentSprite.position += new Vector3(layer.spriteWidth * layer.sprites.Length, 0, 0);
                    }
                    // カメラが左に進み、スプライトが右の画面外に完全に出た場合
                    else if (distFromCamera < -layer.spriteWidth)
                    {
                        // 2枚分の幅だけ左に瞬間移動させる
                        currentSprite.position -= new Vector3(layer.spriteWidth * layer.sprites.Length, 0, 0);
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
                    // SpriteRendererから実際の画像の横幅を自動計算する
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