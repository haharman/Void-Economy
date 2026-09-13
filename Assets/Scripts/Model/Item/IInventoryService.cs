namespace Model
{
    // InventoryModelのみが実装するインターフェース
    public interface IInventoryService
    {
        void ClearItems();
        void ReadFrom();
        void WriteTo();
    }
}
