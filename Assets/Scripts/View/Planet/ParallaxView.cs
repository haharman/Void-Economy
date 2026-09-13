using System;
using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class ParallaxView : MonoBehaviour
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
                layer.totalWidth = layer.spriteWidth * layer.loopCount;
                layer.speed = 1f - layer.totalWidth/ parallaxWidth;
                
                // ゲームオブジェクト作成
                layer.gameObject = new GameObject(layer.name);
                layer.gameObject.transform.SetParent(_loopLayerParentObj.transform);
                
                // 描画
                layer.spriteRenderer = layer.gameObject.AddComponent<SpriteRenderer>();
                layer.spriteRenderer.sprite = layer.sprite;
                layer.spriteRenderer.drawMode = SpriteDrawMode.Tiled;
                layer.spriteRenderer.tileMode = SpriteTileMode.Continuous;
                layer.spriteRenderer.size = new Vector2(layer.totalWidth, layer.spriteRenderer.size.y);
                
                // 前後関係
                layer.spriteRenderer.sortingLayerName = "Parallax";
                layer.spriteRenderer.sortingOrder = 0;
                layer.gameObject.transform.localPosition = new Vector3(layer.offsetX, layer.offsetY, layer.speed);
            }
        }
        
        [Button] private void ImportNonLoopLayers()
        {
            nonLoopLayerList.Clear();
            foreach (Transform child in transform)
            {
                if (child.gameObject == _loopLayerParentObj) continue;
                if (child.name == "Loop Layers") continue;
                if (child.name == "Player") continue;
                NonLoopLayer layer = new NonLoopLayer();
                layer.name = child.name;
                layer.transform = child;
                layer.basePosition = child.localPosition;
                nonLoopLayerList.Add(layer);
            }
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
            [InspectorReadOnly] public float totalWidth;
            [InspectorReadOnly] public float speed;
            [InspectorReadOnly] public Vector3 position;
            [InspectorReadOnly] public GameObject gameObject;
            [HideInInspector] public SpriteRenderer spriteRenderer;
        }

        [Serializable]
        public class NonLoopLayer
        {
            public String name;
            public Transform transform;
            [InspectorReadOnly] public Vector3 basePosition;
        }

        private static GameObject _loopLayerParentObj;

        public void UpdateLoopLayers(float cameraRawX, float cameraWidth)
        {
            Debug.Log("[ParallaxView] UpdateLoopLayers");
            //Debug.Log($"Camera X: {cameraRawX}, Camera Width: {cameraWidth}");
            if (loopLayerList == null || loopLayerList.Count == 0) return;
            float parallaxWidth = planetRadius * 2f * Mathf.PI;

            float leftCameraEdge = cameraRawX - cameraWidth / 2f;
            float rightCameraEdge = cameraRawX + cameraWidth / 2f;
            
            foreach (var layer in loopLayerList)
            {
                // カメラ位置 区間[0,parallaxWidth)
                float cameraX = (parallaxWidth + (cameraRawX - layer.offsetX) % parallaxWidth) % parallaxWidth;
                
                // レイヤー位置 区間[0,parallaxWidth)
                float layerX = (parallaxWidth + (cameraX * layer.speed) % parallaxWidth) % parallaxWidth;

                //Debug.Log($"leftCameraEdge: {leftCameraEdge}, rightCameraEdge: {rightCameraEdge}, spriteWidth: {layer.spriteWidth}");
                // カメラの左端を考慮したレイヤーの左端
                // 等差数列 layerX + spriteWidth * n の中で、leftCameraEdge 未満になる最大の項
                float diff = (leftCameraEdge - layerX) / layer.spriteWidth;
                int n = Mathf.CeilToInt(diff) - 1;
                float leftLayerEdge = layerX + layer.spriteWidth * n;

                //Debug.Log($"rightCameraEdge{rightCameraEdge}, leftLayerEdge: {leftLayerEdge}, spriteWidth: {layer.spriteWidth}");
                // カメラの右端を考慮したレイヤーの繰り返し回数
                // leftLayerEdge + spriteWidth*tileCount > rightCameraEdge を満たす最小の tileCount
                int tileCount = Mathf.FloorToInt((rightCameraEdge - leftLayerEdge) / layer.spriteWidth) + 1;
                //Debug.Log($"tileCount: {tileCount}");
                
                layer.gameObject.transform.localPosition = new Vector3(leftLayerEdge, layer.offsetY, layer.speed);
                layer.spriteRenderer.size = new Vector2(layer.spriteWidth * tileCount, layer.spriteRenderer.size.y);
                //Debug.Log($"cameraX: {cameraX}, layerX: {layer.spriteWidth}, layerLeft: {leftLayerEdge}, tileCount: {tileCount}, layerRight: {leftLayerEdge+layer.spriteWidth*tileCount}");
            }
        }

        public void UpdateNonLoopLayers(float cameraRawX, float cameraWidth)
        {
            Debug.Log("[ParallaxView] UpdateNonLoopLayers");
            Debug.Log($"Camera X: {cameraRawX}, Camera Width: {cameraWidth}");
            if (nonLoopLayerList == null || nonLoopLayerList.Count == 0) return;
            float parallaxWidth = planetRadius * 2f * Mathf.PI;
            float leftCameraEdge = cameraRawX - cameraWidth / 2f;
            float rightCameraEdge = cameraRawX + cameraWidth / 2f;

            foreach (var layer in nonLoopLayerList)
            { 
                // レイヤー位置 区間[0,parallaxWidth)
                float layerX = (parallaxWidth + (layer.basePosition.x) % parallaxWidth) % parallaxWidth;
                
                // (layerX + parallaxWidth*n) -  cameraX の絶対値が最小となるX
                float spriteX = cameraRawX + Mathf.Repeat(layerX - cameraRawX + parallaxWidth * 0.5f, parallaxWidth) - parallaxWidth * 0.5f;

                layer.transform.localPosition = new Vector3(spriteX, layer.transform.position.y, layer.transform.position.z);

            }

        }

        
    }
}