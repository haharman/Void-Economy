using System.Collections.Generic;
using Model;
using UnityEngine;
using Core;

namespace View
{
    public class SurfaceView : MonoBehaviour
    {
        [SerializeField] private List<EntityView> entitiyList;
        public Surface surface;

        public List<EntityConfig> GetEntityConfigList()
        {
            List<EntityConfig> configList = new List<EntityConfig>();
            foreach (var entityView in entitiyList)
            {
                configList.Add(entityView.GetConfig());
            }
            return configList;
        }
        
        public List<EntityView> GetEntityViewList()
        {
            return entitiyList;
        }
    }
}