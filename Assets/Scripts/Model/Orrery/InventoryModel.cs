using System;
using System.Collections.Generic;
using Core;
using UnityEditor;

// 関連ファイル：ItemDefinitionSO.cs

namespace Model
{
    
    public enum GetResult
    {
        Success,
        InvalidId,
        NotFound,
        InsufficientCount
    }

    public enum SetResult
    {
        Success,
        InvalidId,
        FullOfMass,
        FullOfVolume
    }
    public interface IInventorySource
    {
        public GetResult TryGet(ItemId id, int count, out ItemStack item);
        public SetResult TrySet(ItemId id, int count, out ItemStack item);
        public int GetCount(ItemId id);
    }
    
    public interface IInventoryService
    {
        public void ClearItems();
    }

    public class InventoryModel : IInventoryService
    {
        

        private Dictionary<ItemId, ItemStack> Items;

        public InventoryModel()
        {
            Items = new();
        }
        
        public void Init()
        {
            
        }

        public void ClearItems()
        {
            Items.Clear();
        }
        
        
    }
    
    public readonly struct TradeOffer
    {
        public readonly IReadOnlyDictionary<string, int> StackableAmounts; // スタック品
    }
}