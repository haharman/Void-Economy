using System.Collections.Generic;
using R3;

namespace Model
{
    // InventoryModelのみが実装するインターフェース
    public interface IInventorySource
    {
        GetResult TryGet(ItemId id, int count, out ItemStack item);
        SetResult TrySet(ItemStack stack, out ItemStack remain);
        GetResult Get(ItemId id, int count, out ItemStack item);
        SetResult Set(ItemStack stack, out ItemStack remain);
        Observable<Unit> Updated { get; }
        List<ItemStack> ItemList(SortType sortType, bool ascending);
        float TotalMass { get; }
        float TotalVolume { get; }
        float MassLimit { get; }
        float VolumeLimit { get; }
        float MassUtilization { get; }
        float VolumeUtilization { get; }
    }
}
