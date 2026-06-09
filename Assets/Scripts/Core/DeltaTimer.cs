using UnityEngine;

namespace Core
{
    public struct DeltaTimer
    {
        private float _lastTimestamp;
        
        public bool isFirstCall => _lastTimestamp == 0;
        
        /// <summary>
        /// 前回の呼び出しからの経過時間を取得
        /// </summary>
        /// <returns>deltaTime</returns>
        public float GetDelta()
        {
            if (isFirstCall)
            {
                Debug.LogError("[DeltaTimer] GetDelta リセットされていません");
                return 0f;
            }
            float currentTime = Time.time;
            float delta = currentTime - _lastTimestamp;
            _lastTimestamp = currentTime;
            return delta; 
        }

        /// <summary>
        /// 初期化
        /// </summary>
        public void Reset() => _lastTimestamp = Time.time;
    }
}