using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class ItemRegistry : IItemRegistrySource
    {
        private readonly Dictionary<ItemId, ItemDefinitionSo> _definitions = new();

        public ItemRegistry(ItemDatabaseSo database)
        {
            foreach (var definition in database.Definitions)
            {
                var id = definition.Id;
                if (_definitions.ContainsKey(id))
                {
                    Debug.LogError($"[ItemRegistry] ItemIdが重複しています id={id}");
                    continue;
                }

                _definitions[id] = definition;
            }
        }

        public bool TryGetDefinition(ItemId id, out ItemDefinitionSo definition)
        {
            return _definitions.TryGetValue(id, out definition);
        }
    }
}
