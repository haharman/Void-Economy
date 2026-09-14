using NUnit.Framework;
using Model;
using R3;

public class WalletModelTests
{
    private WalletModel _sut;

    [SetUp]
    public void Setup()
    {
        _sut = new WalletModel();
    }

    [Test]
    public void 初期状態_残高は0()
    {
        Assert.AreEqual(0, _sut.Balance.CurrentValue);
    }

    [Test]
    public void Deposit_預け入れる_残高が増える()
    {
        _sut.Deposit(100);
        Assert.AreEqual(100, _sut.Balance.CurrentValue);
    }

    [Test]
    public void TryPay_残高が十分_引き出せてtrueを返す()
    {
        _sut.Deposit(100);
        bool result = _sut.TryPay(60);
        Assert.IsTrue(result);
        Assert.AreEqual(40, _sut.Balance.CurrentValue);
    }

    [Test]
    public void TryPay_残高がちょうど同額_引き出せてtrueを返す()
    {
        _sut.Deposit(100);
        bool result = _sut.TryPay(100);
        Assert.IsTrue(result);
        Assert.AreEqual(0, _sut.Balance.CurrentValue);
    }

    [Test]
    public void TryPay_残高が不足_変化せずfalseを返す()
    {
        _sut.Deposit(50);
        bool result = _sut.TryPay(100);
        Assert.IsFalse(result);
        Assert.AreEqual(50, _sut.Balance.CurrentValue);
    }

    [Test]
    public void SetBalance_ロード時の初期化_値が直接反映される()
    {
        _sut.SetBalance(999);
        Assert.AreEqual(999, _sut.Balance.CurrentValue);
    }

    [Test]
    public void Balance_変化を購読できる()
    {
        int observed = -1;
        using var subscription = _sut.Balance.Subscribe(v => observed = v);

        _sut.Deposit(30);

        Assert.AreEqual(30, observed);
    }
}
