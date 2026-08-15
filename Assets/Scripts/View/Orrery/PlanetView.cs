using UnityEngine;

namespace View
{
    public class PlanetView : MonoBehaviour
    {
        [SerializeField] private PlanetTextureView planetTextureView;

        // 本来ならPlanetTextureView自身に書く処理。現在は大きさの制御だけなのでこちらに書いている
        public void SetTextureScale(float diameter) { planetTextureView.transform.localScale = new Vector3(diameter, diameter, 1f); }

        public void UpdatePosition(Vector2 position)
        {
            transform.position = position;
        }
        
        #region 自動アタッチ
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (planetTextureView == null)
            {
                planetTextureView = GetComponentInChildren<PlanetTextureView>(true);
                if (planetTextureView == null)
                    Debug.LogWarning($"{name}: PlanetTextureView が子階層に見つかりません", this);
            }
        }
        #endif
        #endregion 自動アタッチ
    }
}