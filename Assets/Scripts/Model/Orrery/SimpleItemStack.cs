using System;
using Core;

namespace Model
{
    public class SimpleItemStack
    {
        public ItemId ItemId => _definition.Id;
        private ItemDefinition _definition;
        public int Count { get; set; }
        public float TotalMass => _definition.Mass * Count;
        public float TotalVolume => _definition.Volume * Count;

        public SimpleItemStack(ItemDefinition definition, int count)
        {
            
        }
    }
}