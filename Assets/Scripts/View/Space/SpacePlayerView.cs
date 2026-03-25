using UnityEngine;
using Model;

namespace View
{
    public class SpacePlayerView : MonoBehaviour
    {
        [SerializeField] private Transform transform;

        public Vector2 GetLocation()
        {
            return new Vector2(transform.position.x, transform.position.y);
        }

        public void UpdateTransform(Vector2 location, Vector2 rotation, float scale)
        {
            transform.position = new Vector3(location.x, location.y, transform.position.z);
            float angle = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
