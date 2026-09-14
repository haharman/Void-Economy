using System.Threading;
using Core;
using Model;
using R3;
using View;

namespace Presenter
{
    public class WalletPresenter : ISaveDataReader, ISaveDataWriter
    {
        private const float ChangeAnimationDuration = 0.3f;

        private readonly WalletModel _model;
        private readonly WalletView _view;

        public WalletPresenter(WalletModel walletModel, WalletView walletView, CancellationToken cancellationToken)
        {
            _model = walletModel;
            _view = walletView;

            _model.Balance
                .Subscribe(balance => _view.Set(balance, ChangeAnimationDuration))
                .RegisterTo(cancellationToken);
        }

        public void ReadFrom(SaveData data)
        {
            _model.SetBalance(data.economy.balance);
            _view.SetForce(_model.Balance.CurrentValue);
        }

        public void WriteTo(SaveData data)
        {
            data.economy = new EconomySaveData { balance = _model.Balance.CurrentValue };
        }
    }
}
