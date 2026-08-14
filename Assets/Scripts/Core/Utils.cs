using UnityEngine;

namespace Core
{
    public static class Utils
    {
        public static float XGridSize = 5f;
        public static float YGridSize = 10f;
        public static int CalculateXGrid(float x)
        {
            return Mathf.RoundToInt(x / XGridSize);
        }
        
        public static int CalculateYGrid(float y)
        {
            return Mathf.RoundToInt(y / YGridSize);
        }
    }
}