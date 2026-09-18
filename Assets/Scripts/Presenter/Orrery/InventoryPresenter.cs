using System.Threading;
using Core;
using Model;
using View;

namespace Presenter
{
    public class InventoryPresenter : ISaveDataReader, ISaveDataWriter
    {
        private readonly InventoryModel _model;
        private readonly InventoryView _view;

        public InventoryPresenter(InventoryModel model, InventoryView view)
        {
            _model = model;
            _view = view;
        }

        public void Initialize(CancellationToken cancellationToken)
        {
            _view.Initialize(_model, cancellationToken);
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
