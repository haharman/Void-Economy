using UnityEngine;
using System.Collections.Generic;

namespace Model
{
    public class EntityModel
    {
        private List<Entity> _entities;
        private IDialogueRunner _dialogueRunner;
        
        public EntityModel(IDialogueRunner dialogueRunner)
        {
            _dialogueRunner = dialogueRunner;
        }
        
        public List<Entity> Init(List<EntityConfig> entityConfigs)
        {
            _entities = new List<Entity>();
            foreach (var config in entityConfigs)
            {
                _entities.Add(new Entity(config, _dialogueRunner));
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
                if (!entity.IsInteractable) continue;
                if (Mathf.Abs(entity.XGrid - xGrid) > 1) continue;
                //Debug.Log("[EntityModel] GetInteractiveEntity xGrid一致");
                float distanceSqr = (entity.Position.Value - position).sqrMagnitude;
                //Debug.Log("[EntityModel] GetInteractiveEntity distanceSqr: " + distanceSqr + " rangeSqr: " + closestDistanceSqr);
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