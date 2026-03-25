using UnityEngine;
using System.Collections.Generic;

namespace Model
{
    public class EntityModel
    {
        public List<Entity>[][] entities { get; set; }

        public Entity GetInteractableEntity(int surfaceID, int xGrid, Vector2 position, float range)
        {
            if (entities == null) return null;
            if (surfaceID < 0 || surfaceID >= entities.Length) return null;
            
            var surfaceEntities = entities[surfaceID];
            if (surfaceEntities == null || xGrid < 0 || xGrid >= surfaceEntities.Length) return null;

            var entityList = surfaceEntities[xGrid];
            if (entityList == null || entityList.Count == 0) return null;

            Entity closestEntity = null;
            float closestDistanceSqr = range * range;

            foreach (var entity in entityList)
            {
                if (!entity.isInteractable) continue;

                float distanceSqr = (entity.position - position).sqrMagnitude;

                if (distanceSqr < closestDistanceSqr)
                {
                    closestEntity = entity;
                    closestDistanceSqr = distanceSqr;
                }
            }

            return closestEntity;
        }
    }
    
    public class Entity
    {
        public int surfaceID { get; set; }
        public int xGrid { get; set; }
        public Vector2 position { get; set; }
        public string dialogueNodeName { get; set; }
        public bool isInteractable { get; set; } = true;
    }
}