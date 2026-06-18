using UnityEngine;
using Core;
using Model;   
namespace View
{
    public class EntityView : MonoBehaviour
    {
        [SerializeField] private bool isInteractable = true;
        [SerializeField] private string dialogueNodeName;
        [SerializeField] private Transform entityTransform;
        [SerializeField] private float interactionGuideOffsetY;

        public EntityConfig GetConfig()
        {
            return new EntityConfig
            {
                IsInteractable = isInteractable,
                DialogueNodeName = dialogueNodeName,
                Position = (Vector2)entityTransform.position,
                InteractionGuideOffsetY = interactionGuideOffsetY
            };
        }
        
        public void OnUpdatePosition(Vector2 position)
        {
            entityTransform.position = position;
        }
    }
}