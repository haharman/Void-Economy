using UnityEngine;
using Model;

namespace View
{
    public class SpacePlayerView : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;

        public Vector2 GetLocation()
        {
            return new Vector2(playerTransform.position.x, playerTransform.position.y);
        }

        public void UpdateTransform(Vector2 location, Vector2 rotation, float scale)
        {
            playerTransform.position = new Vector3(location.x, location.y, playerTransform.position.z);
            float angle = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg - 90f;
            playerTransform.rotation = Quaternion.Euler(0, 0, angle);
            playerTransform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
