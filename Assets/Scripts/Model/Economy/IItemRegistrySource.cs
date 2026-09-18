namespace Model
{
    // ItemRegistryのみが実装するインターフェース
    public interface IItemRegistrySource
    {
        bool TryGetDefinition(ItemId id, out ItemDefinitionSo definition);
    }
}
