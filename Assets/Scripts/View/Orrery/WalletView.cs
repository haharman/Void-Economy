using UnityEngine;
using DG.Tweening;
using TMPro;

namespace View
{
    public class WalletView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI numberText;

        private int _displayedBalance;
        private Tween _tween;

        public void SetForce(int balance)
        {
            _tween?.Kill();
            _displayedBalance = balance;
            numberText.text = balance.ToString();
        }

        public void Set(int balance, float duration)
        {
            // Tweenで反映する
            _tween?.Kill();
            _tween = DOVirtual.Int(_displayedBalance, balance, duration, value =>
                {
                    _displayedBalance = value;
                    numberText.text = value.ToString();
                })
                .SetLink(gameObject);
        }
    }
}