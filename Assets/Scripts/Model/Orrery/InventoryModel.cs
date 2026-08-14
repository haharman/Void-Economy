using System;
using System.Collections.Generic;
using Core;

// 関連ファイル：ItemDefinisionSO.cs

namespace Model
{
    
    public interface IInventoryService
    {
        void Init();
        void RemoveItem();
    }

    public class InventoryModel : IInventoryService
    {
        private List<ItemInstanceId> uniqueItems;
        public void Init()
        {
            
        }

        public void RemoveItem()
        {

        }
    }
    
    public readonly struct TradeOffer
    {
        public readonly IReadOnlyList<ItemInstanceId> InstanceIds; // ユニーク品
        public readonly IReadOnlyDictionary<string, int> StackableAmounts; // スタック品
    }
}