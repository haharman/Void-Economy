using UnityEngine;
using Unity.Cinemachine;

namespace View
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera vcam;
        [SerializeField] private float scale = 5f;

        void LateUpdate()
        {
            float baseScale = vcam.Follow.localScale.x;
            vcam.Lens.OrthographicSize = baseScale * scale;
        }
    }
}