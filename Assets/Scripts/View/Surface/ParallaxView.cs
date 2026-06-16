using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace View
{
    // [ExecuteAlways] // EditorModeでもライフサイクル関数が呼び出される
    public class ParallaxView : MonoBehaviour
    {
        public enum LayerType
        {
            Pivot,
            Range
        }

        public enum Pivot
        {
            Center,
            Min,
            Max
        }
        [System.Serializable]
        public class ParallaxLayer
        {
            [Header("Common")]
            public string name;
            public Sprite sprite; 
            public LayerType type;
            public int repeatCount = 1;
            public int order = 1;
            
            [Header("Pivot")]
            public Pivot pivot;
            public float pivotX;
            [Tooltip("奥行き 0.0 = Playerと同じ平面, -1.0 = 1Unit手前, 1.0 = 1Unit奥")] public float depthFromPlayer;
            [Tooltip("有効にすると、repeatCount は無視されます")] public bool loop;
            
            [Header("Range")]
            public float minX;
            public float maxX;
            
            // ===== 自動で算出される非表示プロパティ =====
            [Tooltip("中央とカメラが重なる座標")] [HideInInspector] public int sortingOrder;
        }

        // ParallaxLayerに紐づくコンポーネントをまとめる構造体
        [System.Serializable]
        public struct LayerComponents
        {
            public SpriteRenderer sr;
            public RectTransform rt;
            public LayerComponents(SpriteRenderer sr, RectTransform rt)
            {
                this.sr = sr;
                this.rt = rt;
            }
        }
        // Class,ComponentsをListに格納するための構造体
        [System.Serializable]
        public struct LayerEntry
        {
            public ParallaxLayer Layer;
            // public List<LayerComponents> ComponentsList;
            public LayerEntry(ParallaxLayer layer)
            {
                Layer = layer;
                //ComponentsList = new List<LayerComponents>();
            }
        }
        
        private Dictionary<ParallaxLayer, List<LayerComponents>> _layers;
        
        [SerializeField] private List<ParallaxLayer> layerList;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float rangeBuffer = 0f;
        
        private float ScreenWidth => Camera.main.ViewportToWorldPoint(new Vector3(1, 0, 0)).x - Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;

        void Awake()
        {
            if(layerList == null || layerList.Count == 0)
            {
                Debug.LogWarning("[ParallaxView] LayerListが設定されていません");
                return;
            }
            RebuildLayers();
        }
        
        // インスペクターで値が書き換わった時呼ばれる
        void OnValidate()
        {
            //RebuildLayers();
        }
        
        private void Update()
        {
            float cameraX = cameraTransform.position.x;
            UpdateLayers(cameraX);
        }
        

        /// <summary>
        /// ゲームオブジェクトを再生成し、Parallaxシーンを組み立てる
        /// </summary>
        private void RebuildLayers()
        {
            foreach (Transform c in transform)
            {
                #if UNITY_EDITOR
                    if (Application.isPlaying)
                        Destroy(c.gameObject);
                    else
                        DestroyImmediate(c.gameObject);
                #else
                    Destroy(c.gameObject);
                #endif
            }
            _layers = layerList.ToDictionary(layer => layer, layer => new List<LayerComponents>());
            if (_layers == null) return;
            foreach (var layer in _layers.Keys)
            {
                foreach (var components in _layers[layer])
                {
                    Destroy(components.sr.gameObject);
                }
                
                _layers[layer].Clear();
                int componentsCount = 0;
                if (layer.loop)
                    componentsCount = Mathf.CeilToInt(ScreenWidth / (layer.sprite.rect.width / layer.sprite.pixelsPerUnit)) + 2;
                else
                    componentsCount = layer.repeatCount;
                for(int i = 0; i < componentsCount; i++)
                {
                    // オブジェクトを生成
                    var obj = new GameObject($"{layer.name}_Loop_{i}");
                    
                    // ParallaxViewオブジェクトの子として配置
                    obj.transform.SetParent(this.transform);
                        
                    // SpriteRendererの設定
                    var sr = obj.AddComponent<SpriteRenderer>();
                    sr.sprite = layer.sprite;
                    sr.sortingOrder = layer.order;
                    sr.sortingLayerName = "Parallax";   
                        
                    // RectTransformの設定
                    var rt = obj.AddComponent<RectTransform>();
                        
                    // コンポネント参照を格納
                    _layers[layer].Add(new LayerComponents(sr, rt));
                }
            }
        }
        
        /// <summary>
        /// Parallax処理を行う。カメラの移動量に応じて、各レイヤーの位置を更新する。
        /// </summary>
        private void UpdateLayers(float cameraX)
        {
            if(_layers == null) return;
            foreach (var (layer, cList) in _layers)
            {
                float spriteW = layer.sprite.rect.width / layer.sprite.pixelsPerUnit;
                float spritesW = layer.type == LayerType.Pivot && layer.loop ? spriteW : spriteW * layer.repeatCount;
                float rangeMin = cameraX - ScreenWidth/2f - rangeBuffer - spriteW / 2f;
                float rangeMax = cameraX + ScreenWidth/2f + rangeBuffer + spriteW / 2f;
                switch (layer.type)
                {
                    case LayerType.Pivot:
                        // ScreenWidth
                        // cameraX
                        // pivotをCenterに統一
                        float pivot = layer.pivot switch
                        {
                            Pivot.Center => layer.pivotX,
                            Pivot.Min => layer.pivotX + spritesW / 2f,
                            Pivot.Max => layer.pivotX - spritesW / 2f,
                            _ => layer.pivotX
                        };
                        float parallaxOffset = (cameraX - layer.pivotX) * (layer.depthFromPlayer);
                        Debug.Log(parallaxOffset);
                        float origin = pivot + parallaxOffset;
                        if (layer.loop)
                        {
                            int startN = Mathf.CeilToInt((rangeMin - origin) / spriteW);
                            int endN = Mathf.FloorToInt((rangeMax - origin) / spriteW);
                            int i = 0;
                            for (int n = startN; n <= endN; n++)
                            {
                                float spriteX = origin + n * spriteW;
                                cList[i].sr.gameObject.SetActive(true);
                                cList[i].rt.localPosition = new Vector3(spriteX, cList[i].rt.localPosition.y, cList[i].rt.localPosition.z);
                                i++;
                            }
                            for(int j = i; j < cList.Count; j++)
                            {
                                cList[j].sr.gameObject.SetActive(false);
                            }
                        }
                        else
                        {
                            //Debug.Log($"[ParallaxView] Pivot Layer: {layer.name}, Origin: {origin}, Range: ({rangeMin}, {rangeMax})");
                            // 範囲内なら描画する
                            for (int i = 0; i < layer.repeatCount; i++)
                            {
                                float spriteX = origin - spritesW/2 + spriteW * i;
                                if(spriteX > rangeMin && spriteX < rangeMax)
                                {
                                    cList[i].sr.gameObject.SetActive(true);
                                    cList[i].rt.localPosition = new Vector3(spriteX, cList[i].rt.localPosition.y, cList[i].rt.localPosition.z);
                                }
                                else
                                {
                                    cList[i].sr.gameObject.SetActive(false);
                                }
                            }
                            

                        }
                        break;
                    case LayerType.Range:
                        float x = cameraX - layer.minX; // minXを基準としたカメラの位置
                        float d = layer.maxX - layer.minX; // Cameraの移動距離
                        // (CameraがminXからmaxXまで移動したとき、spriteが移動する距離) * (Cameraの移動割合)
                        float spritesMinX = layer.minX + (d - spritesW) * (x / d);
                        for (int i = 0; i < layer.repeatCount; i++)
                        {
                            float spriteX = spritesMinX + spriteW * (i + 0.5f);
                            if (spriteX + spriteW / 2 > rangeMin && spriteX - spriteW / 2 < rangeMax)
                            {
                                cList[i].sr.gameObject.SetActive(true);
                                cList[i].rt.localPosition = new Vector3(spriteX, cList[i].rt.localPosition.y,
                                    cList[i].rt.localPosition.z);
                            }
                            else
                            {
                                cList[i].sr.gameObject.SetActive(false);
                            }
                        }
                        break;
                    
                }
            }
        }
    }
}