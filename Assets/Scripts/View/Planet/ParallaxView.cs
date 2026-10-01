using System;
using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class ParallaxView : MonoBehaviour
    {
        [Tooltip("ループするレイヤーを手動設定")]public List<LoopLayer> loopLayerList = new();

        [Space, Tooltip("ループしないオブジェクトをこのオブジェクトの子として配置"), ListLabel("Name")] public List<NonLoopLayer> nonLoopLayerList = new();
        
        [Header("Sprite Width Calculator")] 
        [Tooltip("Width Calculator専用。実行時の半径は PlanetModel から注入されるため、この値は実行時には使われない")] public float calculatorPlanetRadius;
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

                // spriteWidth, totalWidthの算出 (speedは半径が注入される SetPlanetRadius で算出)
                layer.spriteWidth = layer.sprite.rect.width / layer.sprite.pixelsPerUnit;
                layer.totalWidth = layer.spriteWidth * layer.loopCount;
                
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
                // zは実行時に UpdateLoopLayers が speed を設定する
                layer.gameObject.transform.localPosition = new Vector3(layer.offsetX, layer.offsetY, 0f);
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
            float parallaxWidth = calculatorPlanetRadius * 2f * Mathf.PI;
            calculatedSpriteWidth = Mathf.RoundToInt((1f - speed) * parallaxWidth / (float)repeatCount * 10f);
        }

        public Camera Camera;

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
            
            [Tooltip("レイヤーをSurface上でこの量だけ横にずらす(Unit)")] public float offsetX;
            [Tooltip("レイヤーのY位置(Unit)")] public float offsetY;

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

        private float _planetRadius;
        private bool _warnedRadiusNotSet;

        // PlanetRoot.Bootstrap が PlanetModel.Radius を注入する
        public void SetPlanetRadius(float planetRadius)
        {
            if (planetRadius <= 0f)
            {
                Debug.LogError($"[ParallaxView] planetRadius に0以下の値が渡されました: {planetRadius}", this);
                return;
            }
            _planetRadius = planetRadius;

            float parallaxWidth = 2f * Mathf.PI * _planetRadius;
            if (loopLayerList == null) return;
            foreach (var layer in loopLayerList)
                layer.speed = 1f - layer.totalWidth / parallaxWidth;
        }

        // SetPlanetRadius が未呼出の間は更新できない(警告は一度だけ)
        private bool HasPlanetRadius()
        {
            if (_planetRadius > 0f) return true;
            if (!_warnedRadiusNotSet)
            {
                Debug.LogWarning("[ParallaxView] SetPlanetRadius が未呼出のため、更新をスキップします", this);
                _warnedRadiusNotSet = true;
            }
            return false;
        }

        public void UpdateLoopLayers(float cameraRawX, float cameraWidth)
        {
            if (loopLayerList == null || loopLayerList.Count == 0) return;
            if (!HasPlanetRadius()) return;

            float leftCameraEdge = cameraRawX - cameraWidth / 2f;
            float rightCameraEdge = cameraRawX + cameraWidth / 2f;
            
            foreach (var layer in loopLayerList)
            {
                // GenerateLoopLayers 未実行 / Sprite未設定の要素は更新できない(毎フレーム例外にしない)
                if (layer.spriteWidth <= 0f || layer.gameObject == null || layer.spriteRenderer == null) continue;

                // レイヤー位置(Surface上のX)。周長Wで畳まない。
                // speed = 1 - totalWidth/W なので、カメラが周長Wだけ進むと、カメラ相対のタイル位相は
                // totalWidth (= spriteWidth*loopCount ≡ 0 mod spriteWidth) だけずれる = 周回の継ぎ目でも連続になる。
                // W で畳むと W mod spriteWidth だけ位相が飛ぶため、畳んではいけない。
                float layerX = cameraRawX * layer.speed + layer.offsetX;

                // カメラの左端を考慮したレイヤーの左端
                // 等差数列 layerX + spriteWidth * n の中で、leftCameraEdge 未満になる最大の項
                float diff = (leftCameraEdge - layerX) / layer.spriteWidth;
                int n = Mathf.CeilToInt(diff) - 1;
                float leftLayerEdge = layerX + layer.spriteWidth * n;

                // カメラの右端を考慮したレイヤーの繰り返し回数
                // leftLayerEdge + spriteWidth*tileCount > rightCameraEdge を満たす最小の tileCount
                int tileCount = Mathf.FloorToInt((rightCameraEdge - leftLayerEdge) / layer.spriteWidth) + 1;
                
                layer.gameObject.transform.localPosition = new Vector3(leftLayerEdge, layer.offsetY, layer.speed);
                layer.spriteRenderer.size = new Vector2(layer.spriteWidth * tileCount, layer.spriteRenderer.size.y);
            }
        }

        // 非ループレイヤーは speed=0 (地表に置く物)のみ想定。Surface座標のまま、カメラに最も近い周回のイメージに置く
        // TODO(未決定): Entityを含む場合、EntityModelの位置(固定)と描画位置(最近傍イメージ)が継ぎ目付近でずれる
        public void UpdateNonLoopLayers(float cameraRawX)
        {
            if (nonLoopLayerList == null || nonLoopLayerList.Count == 0) return;
            if (!HasPlanetRadius()) return;
            float parallaxWidth = 2f * Mathf.PI * _planetRadius;

            foreach (var layer in nonLoopLayerList)
            {
                // 子を削除したまま ImportNonLoopLayers し忘れた要素は飛ばす
                if (layer.transform == null) continue;

                // レイヤー位置 区間[0,parallaxWidth)
                float layerX = (parallaxWidth + (layer.basePosition.x) % parallaxWidth) % parallaxWidth;
                
                // (layerX + parallaxWidth*n) -  cameraX の絶対値が最小となるX
                float spriteX = cameraRawX + Mathf.Repeat(layerX - cameraRawX + parallaxWidth * 0.5f, parallaxWidth) - parallaxWidth * 0.5f;

                layer.transform.localPosition = new Vector3(spriteX, layer.basePosition.y, layer.basePosition.z);
            }
        }
    }
}
