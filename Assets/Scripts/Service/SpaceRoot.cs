using Core;
using Model.Physics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Service
{
    public class SpaceRoot : MonoBehaviour
    {
        private PhysicsEngine _physicsEngine;
        
        public void Init()
        {
            Debug.Log("[SpaceRoot] Init");
            _physicsEngine = new PhysicsEngine();
        }
    }
}