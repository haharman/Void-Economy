using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class ParallaxLoopView : MonoBehaviour
    {
        private float cameraWidth;
        private float planetCircumference;
        public void Initialize()
        {
            
        }
        
        public class LoopLayer
        {
            public SpriteRenderer Sr;
            public int N;
            public float Phase;
            public float OffsetY;
            private float _width;
        }
        
        [SerializeField] private List<LoopLayer> loopLayers;

        private void Rebuild()
        {
            foreach (var layer in loopLayers)
            {
                float totalWidth = layer.Sr.size.x * layer.N;
                if(totalWidth < cameraWidth) Debug.LogError("[LoopParallaxView] spriteの幅 × n がカメラ幅より小さいです");
                if(totalWidth > planetCircumference) Debug.LogError("[LoopParallaxView] spriteの幅 × n が惑星の円周より大きいです");
                
                float velocity = totalWidth / planetCircumference;
                float depth = -velocity; // 大きいほうが奥
                
                layer.Sr.sortingLayerName = "Parallax";   
                layer.Sr.sortingOrder = 0;
                
                layer.Sr.transform.localPosition = new Vector3(layer.Sr.transform.localPosition.x, layer.OffsetY, depth);
                
            }
        }

        private void OnUpdate()
        {
            
        }
        
    }
}