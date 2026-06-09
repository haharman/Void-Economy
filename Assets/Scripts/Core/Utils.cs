using UnityEngine;

namespace Core
{
    public static class Utils
    {
        public static float GridSize = 5f;
        public static int CalculateXGrid(float x)
        {
            return Mathf.RoundToInt(x / GridSize);
        }
    }
}