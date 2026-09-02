using System;
using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class ParallaxLoopView : MonoBehaviour
    {
        [Tooltip("惑星の半径(Unit)")] public float planetRadius;
        
        [Space, Tooltip("ループするレイヤーを手動設定")]public List<LoopLayer> loopLayerList = new();

        [Space, Tooltip("ループしないオブジェクトをこのオブジェクトの子として配置"), ListLabel("Name")] public List<NonLoopLayer> nonLoopLayerList = new();
        
        [Header("Sprite Width Calculator")] 
        [Tooltip("繰り返し回数"), Min(1)] public int repeatCount;
        [Tooltip("スピード")][Range(-1f, 1f)] public float speed;

        [Space, Tooltip("スプライト幅(px) 高さは180px基準"), InspectorReadOnly]
        public int calculatedSpriteWidth;
        
        [Button] private void GenerateLoopLayers()
        {
            // 親オブジェクトの破棄
            if (_loopLayerParentObj != null)
            {
                #if UNITY_EDITOR
                DestroyImmediate(_loopLayerParentObj);
                _loopLayerParentObj = null;
                #else
                Destroy(_loopLayerParentObj);
                _loopLayerParentObj = null;
                #endif
            }
            
            // 親オブジェクトの生成
            _loopLayerParentObj = new GameObject("Loop Layers");
            _loopLayerParentObj.transform.SetParent(transform);
            _loopLayerParentObj.transform.localPosition = Vector3.zero;

            // カメラとその横幅を取得
            if (Camera == null)
                Camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            float cameraWidth = Camera == null ? Camera.orthographicSize * 2f * Camera.aspect : 10f;
            float parallaxWidth = planetRadius * 2f * Mathf.PI;
            
            foreach (var layer in loopLayerList)
            {
                if (layer.sprite == null)
                {
                    Debug.LogError($"LoopLayer {layer.name} に、Spriteを設定してください");
                    continue;
                }

                if (layer.loopCount < 1)
                {
                    Debug.LogError($"LoopLayer {layer.name} のloopCountに、1以上の値を設定してください");
                    continue;
                }

                // spriteWidth, speedの算出
                layer.spriteWidth = layer.sprite.rect.width / layer.sprite.pixelsPerUnit;
                layer.speed = 1f - layer.spriteWidth * layer.loopCount / parallaxWidth;
                
                // ゲームオブジェクト作成
                layer.gameObject = new GameObject(layer.name);
                layer.gameObject.transform.SetParent(_loopLayerParentObj.transform);
                
                // 描画
                layer.spriteRenderer = layer.gameObject.AddComponent<SpriteRenderer>();
                layer.spriteRenderer.sprite = layer.sprite;
                if (layer.loopCount > 1)
                {
                    layer.spriteRenderer.drawMode = SpriteDrawMode.Tiled;
                    layer.spriteRenderer.tileMode = SpriteTileMode.Continuous;
                    float totalWidth = layer.spriteWidth * layer.loopCount;
                    layer.spriteRenderer.size = new Vector2(totalWidth, layer.spriteRenderer.size.y);
                }
                
                // 前後関係
                layer.spriteRenderer.sortingLayerName = "Parallax";
                layer.spriteRenderer.sortingOrder = 0;
                layer.gameObject.transform.localPosition = new Vector3(layer.offsetX, layer.offsetY, layer.speed);
            }
        }
        
        [Button] private void ImportNonLoopLayers()
        {
            
        }

        [Button]
        private void CalculateSpriteWidth()
        {
            float parallaxWidth = planetRadius * 2f * Mathf.PI;
            calculatedSpriteWidth = Mathf.RoundToInt((1f - speed) * parallaxWidth / (float)repeatCount * 10f);
        }

        public Camera Camera;
        
        private float _planetCircumference;
        public void Initialize(Camera camera)
        {
            Camera = camera;
        }
        
        [Serializable]
        public class LoopLayer
        {
            public String name;
            public Sprite sprite;
            [Min(1)] public int loopCount = 1;
            
            [Tooltip("0.1刻みに丸められます")] public float offsetX;
            [Tooltip("0.1刻みに丸められます")] public float offsetY;

            [InspectorReadOnly] public float spriteWidth;
            [InspectorReadOnly] public float speed;
            [InspectorReadOnly] public Vector3 position;
            [InspectorReadOnly] public GameObject gameObject;
            [HideInInspector] public SpriteRenderer spriteRenderer;
        }

        [Serializable]
        public class NonLoopLayer
        {
            public Transform transform;
            [Range(-1f, 1f)] public float speed;
            [InspectorReadOnly] public Vector3 basePosition;
        }

        private GameObject _loopLayerParentObj;

        public void UpdateLoopLayers(float cameraX, float cameraWidth)
        {
            if (loopLayerList == null || loopLayerList.Count == 0) return;
            if (Camera == null)
            {
                Debug.LogError("Camera をセットしてください");
                return;
            }
            float parallaxWidth = planetRadius * 2f * Mathf.PI;
            float rightCameraEdge = cameraX - cameraWidth / 2f;
            float leftCameraEdge = cameraX + cameraWidth / 2f;
            
            foreach (var layer in loopLayerList)
            {
                
                float cameraOffsetX = (parallaxWidth + (cameraX - layer.offsetX) % parallaxWidth) % parallaxWidth;
                float spriteWidth = layer.spriteWidth * layer.loopCount;
                // 等差数列 cameraOffsetX + spriteWidth * n の中で、leftCameraEdge 未満になる最大の項
                float diff = (leftCameraEdge - cameraOffsetX) / spriteWidth;
                int n = Mathf.CeilToInt(diff) - 1;
                float leftSpriteEdge = cameraOffsetX + spriteWidth * n;
                
                // spriteWidth*n > cameraWidth を満たす最小の n
                int tileCount = Mathf.FloorToInt(cameraWidth / spriteWidth) + 1;
                
                layer.gameObject.transform.localPosition = new Vector3(leftSpriteEdge, layer.offsetY, layer.speed);
                layer.spriteRenderer.size = new Vector2(spriteWidth, layer.spriteRenderer.size.y);
            }
        }

        
    }
}