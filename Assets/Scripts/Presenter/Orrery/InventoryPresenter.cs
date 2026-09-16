using Core;
using Model;

namespace Presenter
{
    public class InventoryPresenter : ISaveDataReader, ISaveDataWriter
    {
        private readonly InventoryModel _model;

        public InventoryPresenter(InventoryModel model)
        {
            _model = model;
        }

        public void ClearItems()
        {
            _model.ClearItems();
        }

        public void ReadFrom(SaveData data)
        {
            _model.Load(data.inventory);
        }

        public void WriteTo(SaveData data)
        {
            data.inventory = _model.Save();
        }
    }
}
