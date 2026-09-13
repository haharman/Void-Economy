using System;
using Core;

namespace Model
{
    public class ItemStack
    {
        // public
        public ItemId ItemId => _definition.Id;
        public int Count { get; set; }
        public float TotalMass => _definition.Mass * Count;
        public float TotalVolume => _definition.Volume * Count;
        public float Quality { get; set; }
        
        // private
        private ItemDefinition _definition;
        
        public ItemStack(ItemDefinition definition, int count)
        {
            
        }
    }
}