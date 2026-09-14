using R3;

namespace Model
{
    public class WalletModel : IWalletSource
    {
        public ReadOnlyReactiveProperty<int> Balance => _balance;
        private readonly ReactiveProperty<int> _balance = new();

        public bool TryPay(int amount)
        {
            if (amount > _balance.Value) return false;
            _balance.Value -= amount;
            return true;
        }

        public void Deposit(int amount)
        {
            _balance.Value += amount;
        }

        // インターフェースでは公開しない。WalletPresenterのみがロード時の初期化に使う
        public void SetBalance(int amount)
        {
            _balance.Value = amount;
        }
    }
}