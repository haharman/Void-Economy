using Core;
using UnityEngine;
using R3;

namespace Model
{
    public class Entity
    {
        private IDialogueRunner _dialogueRunner;
        
        public EntityType EntityType { get; set; }
        public ReactiveProperty<Vector2> Position { get; }
        public int XGrid { get; set; }
        public bool IsInteractable { get; set; }
        public string DialogueNodeName { get; set; }
        public Vector2 InteractionGuidePosition { get; } // 頭上の「E: 話しかける」ナビゲーション(Interaction Prompt)の位置

        public Entity(EntityConfig config, IDialogueRunner dialogueRunner)
        {
            _dialogueRunner = dialogueRunner;
            Position = new ReactiveProperty<Vector2>(config.Position);
            XGrid = Utils.CalculateXGrid(Position.Value.x);
            IsInteractable = config.IsInteractable;
            DialogueNodeName = config.DialogueNodeName;
            InteractionGuidePosition = new Vector2(Position.Value.x, Position.Value.y + config.InteractionGuideOffsetY);
        }
        
        public void OnInteract()
        {
            if (!IsInteractable) return;
            Debug.Log($"[Entity] Interacted: {DialogueNodeName}");
            _dialogueRunner.StartDialogue(DialogueNodeName);
        }
    }
}