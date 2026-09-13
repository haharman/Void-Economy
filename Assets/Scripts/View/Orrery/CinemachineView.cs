using System.Threading;
using Model;
using R3;
using Unity.Cinemachine;
using UnityEngine;

namespace View
{
    public class CinemachineView : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;
        
        public void Initialize(IJetpackSource jetpackSource, CancellationToken cancellationToken)
        {
            Debug.Log("[CinemachineView] Initializing CinemachineView");
            jetpackSource.PlayerWarped.Subscribe(HandlePlayerWarped).RegisterTo(cancellationToken);
        }
        
        private void HandlePlayerWarped(Vector2 delta)
        {
            Debug.Log("[CinemachineView] Player warped");
            CinemachineCore.OnTargetObjectWarped(playerTransform, delta);
        }
    }
}