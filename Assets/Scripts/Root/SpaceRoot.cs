using Core;
using Model.Physics;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Root
{
    public class SpaceRoot : MonoBehaviour, ISceneRoot
    {
        private PhysicsEngine _physicsEngine;
        
        public void Init()
        {
            Debug.Log("[SpaceRoot] Init");
            _physicsEngine = new PhysicsEngine();
        }

        public void OnUpdate(float deltaTime)
        {
            
        }
    }
}