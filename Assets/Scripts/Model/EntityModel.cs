using UnityEngine;
using System.Collections.Generic;

namespace Model
{
    public class EntityModel
    {
        private YarnModel _yarnModel;
        private List<Entity> _entities;
        
        public EntityModel(YarnModel yarnModel)
        {
            _yarnModel = yarnModel;
        }
        
        public List<Entity> Init(List<EntityConfig> entityConfigs)
        {
            _entities = new List<Entity>();
            foreach (var config in entityConfigs)
            {
                _entities.Add(new Entity(config, _yarnModel));
            }

            Debug.Log("[EntityModel] Init with: " + _entities.Count + "entities");
            return _entities;
        }
        
        public Entity GetInteractableEntity(int xGrid, Vector2 position, float range)
        {
            if (_entities == null || _entities.Count == 0) return null;
            
            var closestEntity = (Entity)null;
            float closestDistanceSqr = range * range;
            
            foreach (var entity in _entities)
            {
                if (!entity.isInteractable) continue;
                if (Mathf.Abs(entity.xGrid - xGrid) > 1) continue;
                //Debug.Log("[EntityModel] GetInteractiveEntity xGrid一致");
                float distanceSqr = (entity.position.Value - position).sqrMagnitude;
                // Debug.Log("[EntityModel] GetInteractiveEntity distanceSqr: " + distanceSqr + " rangeSqr: " + closestDistanceSqr);
                if (distanceSqr < closestDistanceSqr)
                {
                    closestEntity = entity;
                    closestDistanceSqr = distanceSqr;
                }
            }
            return closestEntity;
        }
    }
}