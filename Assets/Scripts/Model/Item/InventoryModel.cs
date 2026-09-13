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
        Success, // 成功。すべてのアイテムを収納した
        InvalidId, // 失敗。無効なID
        FullOfMass,
        FullOfVolume,
        InsufficientMass,
        InsufficientVolume,
    }

    public enum SortType
    {
        Name,
        Count,
        TotalMass,
        TotalVolume,
        MassPerVolume,
    }
    public interface IInventorySource
    {
        public GetResult TryGet(ItemId id, int count, out ItemStack item);
        public bool TrySet(ItemId id, int count, out SetResult set);
        public int GetCount(ItemId id);
        public List<ItemStack> ItemList(SortType sortType, bool ascending);
    }
    
    public interface IInventoryService {
        public void ClearItems();
    }

    public class InventoryModel : IInventoryService, IInventorySource
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