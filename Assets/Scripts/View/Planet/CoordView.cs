using UnityEngine;

namespace View
{
    public class CoordView : MonoBehaviour
    {
        [SerializeField] private Transform surfaceTransform;
        
        public void UpdatePosition(float x, float r)
        {
            float theta = - x / r; // [rad]
            transform.localRotation = Quaternion.Euler(0f, 0f, theta * Mathf.Rad2Deg);
            surfaceTransform.localPosition = new Vector3(-x, r, surfaceTransform.localPosition.z);
        }
    }
}