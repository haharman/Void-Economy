using R3;

namespace Model
{
    public interface IWalletSource
    {
        public ReadOnlyReactiveProperty<int> Balance { get; }
        public bool TryPay(int amount);
        public void Deposit(int amount);
    }
}
