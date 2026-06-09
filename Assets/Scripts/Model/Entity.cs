using Core;
using UnityEngine;
using R3;

namespace Model
{
    public class Entity
    {
        private YarnModel _yarnModel;
        
        public EntityType entityType { get; set; }
        public ReactiveProperty<Vector2> position { get; }
        public int xGrid { get; set; }
        public bool isInteractable { get; set; }
        public string dialogueNodeName { get; set; }
        
        public Entity(EntityConfig config, YarnModel yarnModel)
        {
            _yarnModel = yarnModel;
            
            position = new ReactiveProperty<Vector2>(config.Position);
            xGrid = Utils.CalculateXGrid(position.Value.x);
            isInteractable = config.IsInteractable;
            dialogueNodeName = config.DialogueNodeName;
        }
        
        public void OnInteract()
        {
            if (!isInteractable) return;
            Debug.Log($"[Entity] Interacted: {dialogueNodeName}");
            _yarnModel.StartDialogue(dialogueNodeName);
        }
    }
}