using UnityEngine;

namespace Model
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "NewItemDatabase")]
    public class ItemDatabaseSo : ScriptableObject
    {
        [SerializeField] private ItemDefinitionSo[] definitions;

        public ItemDefinitionSo[] Definitions => definitions;
    }
}
