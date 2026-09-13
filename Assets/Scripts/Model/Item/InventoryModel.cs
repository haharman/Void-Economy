using System.Collections.Generic;
using System.Linq;
using R3;
using UnityEngine;

namespace Model
{
    public enum GetResult
    {
        Success,
        NotFound,
        InsufficientCount
    }

    public enum SetResult
    {
        Success,
        InsufficientMass,
        InsufficientVolume,
        FullOfMass,
        FullOfVolume
    }

    public enum SortType
    {
        Name,
        Count,
        TotalMass,
        TotalVolume,
        MassPerVolume,
        VolumePerMass,
    }

    public class InventoryModel : IInventoryService, IInventorySource
    {
        private Dictionary<ItemId, ItemStack> Items;
        private float _massLimit = 100f;
        private float _volumeLimit = 100f;

        public Observable<Unit> Updated => _updated;
        private readonly Subject<Unit> _updated = new();

        public float MassLimit => _massLimit;
        public float VolumeLimit => _volumeLimit;

        private float CurrentMass => Items.Values.Sum(stack => stack.TotalMass);
        private float CurrentVolume => Items.Values.Sum(stack => stack.TotalVolume);

        public InventoryModel()
        {
            Items = new();
        }

        public void ClearItems()
        {
            Items.Clear();
            _updated.OnNext(Unit.Default);
        }

        public void ReadFrom()
        {
        }

        public void WriteTo()
        {
        }

        public GetResult TryGet(ItemId id, int count, out ItemStack item)
        {
            if (!Items.TryGetValue(id, out var stack))
            {
                item = ItemStack.EmptyItemStack;
                return GetResult.NotFound;
            }

            if (stack.Count < count)
            {
                item = ItemStack.EmptyItemStack;
                return GetResult.InsufficientCount;
            }

            item = stack.Split(count);
            SetItem(id, stack);
            _updated.OnNext(Unit.Default);
            return GetResult.Success;
        }

        public GetResult Get(ItemId id, int count, out ItemStack item)
        {
            if (!Items.TryGetValue(id, out var stack))
            {
                item = ItemStack.EmptyItemStack;
                return GetResult.NotFound;
            }

            var takeCount = Mathf.Min(stack.Count, count);
            if (takeCount <= 0)
            {
                item = ItemStack.EmptyItemStack;
                return GetResult.NotFound;
            }

            item = stack.Split(takeCount);
            SetItem(id, stack);
            _updated.OnNext(Unit.Default);
            return takeCount < count ? GetResult.InsufficientCount : GetResult.Success;
        }

        public SetResult TrySet(ItemStack stack, out ItemStack remain)
        {
            var (maxByMass, maxByVolume, isMassBottleneck) = ComputeCapacity(stack);

            if (stack.Count > maxByMass || stack.Count > maxByVolume)
            {
                remain = stack;
                return isMassBottleneck ? SetResult.InsufficientMass : SetResult.InsufficientVolume;
            }

            Merge(stack);
            remain = ItemStack.EmptyItemStack;
            _updated.OnNext(Unit.Default);
            return SetResult.Success;
        }

        public SetResult Set(ItemStack stack, out ItemStack remain)
        {
            var (maxByMass, maxByVolume, isMassBottleneck) = ComputeCapacity(stack);
            var storableCount = Mathf.Clamp(Mathf.Min(stack.Count, maxByMass, maxByVolume), 0, stack.Count);

            if (storableCount <= 0)
            {
                remain = stack;
                return isMassBottleneck ? SetResult.FullOfMass : SetResult.FullOfVolume;
            }

            var shortage = stack.Count - storableCount;
            remain = shortage > 0 ? stack.Split(shortage) : ItemStack.EmptyItemStack;

            Merge(stack);
            _updated.OnNext(Unit.Default);

            return shortage == 0
                ? SetResult.Success
                : isMassBottleneck ? SetResult.InsufficientMass : SetResult.InsufficientVolume;
        }

        public int GetCount(ItemId id)
        {
            return Items.TryGetValue(id, out var stack) ? stack.Count : 0;
        }

        /// <summary>
        /// stack の個数を0から増やしていったとき、Mass と Volume のどちらが先に不足するかを判定する。
        /// </summary>
        private (int maxByMass, int maxByVolume, bool isMassBottleneck) ComputeCapacity(ItemStack stack)
        {
            var massHeadroom = _massLimit - CurrentMass;
            var volumeHeadroom = _volumeLimit - CurrentVolume;

            var perUnitMass = stack.Definition != null ? stack.Definition.Mass : 0f;
            var perUnitVolume = stack.Definition != null ? stack.Definition.Volume : 0f;

            var maxByMass = perUnitMass > 0f ? Mathf.FloorToInt(massHeadroom / perUnitMass) : int.MaxValue;
            var maxByVolume = perUnitVolume > 0f ? Mathf.FloorToInt(volumeHeadroom / perUnitVolume) : int.MaxValue;

            return (maxByMass, maxByVolume, maxByMass <= maxByVolume);
        }

        public List<ItemStack> ItemList(SortType sortType, bool ascending)
        {
            IEnumerable<ItemStack> query = Items.Values;

            query = sortType switch
            {
                SortType.Name => ascending
                    ? query.OrderBy(DisplayNameOf)
                    : query.OrderByDescending(DisplayNameOf),
                SortType.Count => ascending
                    ? query.OrderBy(stack => stack.Count)
                    : query.OrderByDescending(stack => stack.Count),
                SortType.TotalMass => ascending
                    ? query.OrderBy(stack => stack.TotalMass)
                    : query.OrderByDescending(stack => stack.TotalMass),
                SortType.TotalVolume => ascending
                    ? query.OrderBy(stack => stack.TotalVolume)
                    : query.OrderByDescending(stack => stack.TotalVolume),
                SortType.MassPerVolume => ascending
                    ? query.OrderBy(MassPerVolumeOf)
                    : query.OrderByDescending(MassPerVolumeOf),
                SortType.VolumePerMass => ascending
                    ? query.OrderBy(VolumePerMassOf)
                    : query.OrderByDescending(VolumePerMassOf),
                _ => query
            };

            return query.ToList();
        }

        private static string DisplayNameOf(ItemStack stack) => stack.Definition != null ? stack.Definition.DisplayName : string.Empty;
        private static float MassPerVolumeOf(ItemStack stack) => stack.TotalVolume > 0f ? stack.TotalMass / stack.TotalVolume : 0f;
        private static float VolumePerMassOf(ItemStack stack) => stack.TotalMass > 0f ? stack.TotalVolume / stack.TotalMass : 0f;

        private void Merge(ItemStack stack)
        {
            if (Items.TryGetValue(stack.ItemId, out var existing))
            {
                existing.Marge(stack);
                SetItem(stack.ItemId, existing);
            }
            else
            {
                SetItem(stack.ItemId, stack);
            }
        }

        private void SetItem(ItemId id, ItemStack stack)
        {
            if (stack.Count <= 0)
            {
                Items.Remove(id);
                return;
            }

            Items[id] = stack;
            CheckIntegrity(id, stack);
        }

        private void CheckIntegrity(ItemId key, ItemStack value)
        {
            if (key != value.ItemId)
            {
                Debug.LogError($"[InventoryModel] Itemsのキーとアイテムのidが一致していません key={key} value={value.ItemId}");
            }
        }
    }
}
