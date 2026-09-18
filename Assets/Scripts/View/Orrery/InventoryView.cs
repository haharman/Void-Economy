using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Model;
using UnityEngine;
using R3;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

namespace View
{
    public class InventoryView : MonoBehaviour
    {
        #region フィールド
        
        // 定数
        [SerializeField] private int maxViewportItemCount; // ViewportHeightにおさまる最大アイテム件数
        [SerializeField] private int maxItemCount; // アイテムの最大件数（スタックするのでアイテムの種類数と等価）
        [SerializeField] private float selectBarDuration;
        
        // 参照
        [SerializeField] private RectTransform windowRect;
        [SerializeField] private RectTransform viewportRect;
        [SerializeField] private RectTransform listParentRect;
        [SerializeField] private InventoryItemView itemPrefab;
        [SerializeField] private GameObject noItemMessageObject;
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text itemNameText;
        [SerializeField] private TMP_Text itemFlavorText;
        [SerializeField] private RectTransform selectBarRect;
        [SerializeField] private TMP_Text massUtilizationText;
        [SerializeField] private TMP_Text volumeUtilizationText;
        [SerializeField] private RectTransform massUtilizationBarRect;
        [SerializeField] private RectTransform volumeUtilizationBarRect;
        private IInventorySource _inventory;
        
        // private
        private SortType _currentSortType;
        private InventoryItemView[] _items;
        private List<ItemStack> _itemList;
        private float _itemHeight;
        
        #endregion
        #region メソッド
        
        // 最初にインベントリロード後に呼ばれる
        public void Initialize(IInventorySource inventorySource, CancellationToken cancellationToken)
        {
            _inventory = inventorySource;
            _inventory.Updated.Subscribe(_ => UpdateInventory()).RegisterTo(cancellationToken);
            _items = new InventoryItemView[maxItemCount];
            for (int i = 0; i < maxItemCount; i++)
            {
                var itemView = Instantiate(itemPrefab, listParentRect);
                itemView.index = i;
                _items[i] = itemView;
                itemView.button.onClick.AddListener(() => OnItemSelected(itemView.index));
            }
            
            // 最後に現在のインベントリを反映
            UpdateInventory();
        }
        
        // OpenInventory, CloseInventoryは、このゲームオブジェクト自体をSetActibe=falseするのでここで実装すべきじゃないと思う。
        // ウィンドウを開閉するボタンを実装するはずなのでそこ？
        
        private void Update()
        {
            UpdateSelectedItem();
        }
        
        // 要件が固まっていない
        private void UpdateSelectedItem()
        {
            // スクロール位置に応じて選択を変更する
        }

        public void UpdateInventory()
        {
            //var currentItemId = 一致しているアイテム位置にスクロール、選択する用に保持しておく 
            _itemList = _inventory.ItemList(_currentSortType, true);
            var itemCount = _itemList.Count;
            for (int i = 0; i < maxItemCount; i++)
            {
                _items[i].gameObject.SetActive(i < itemCount);
            }
            // アイテムがなかったら、メッセージオブジェクトを表示して終了
            noItemMessageObject.SetActive(itemCount == 0);
            selectBarRect.gameObject.SetActive(itemCount != 0);
            if (itemCount == 0) return;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(windowRect);
            var viewportHeight = viewportRect.rect.height;
            _itemHeight = viewportHeight / Mathf.Min(maxViewportItemCount, itemCount);
            selectBarRect.sizeDelta = new Vector2(selectBarRect.sizeDelta.x, _itemHeight);
            foreach (var itemView in _items)
            {
                itemView.UpdateHeight(_itemHeight);
            }
            
            // リストの合計高さを計算
            var totalHeight = _itemHeight * itemCount;
            for (int i = 0; i < _itemList.Count; i++)
            {
                _items[i].Bind(_itemList[i]);
            }
            OnItemSelected(0);
        }
        
        // アイテムが選択されたとき
        public void OnItemSelected(int index)
        {
            Debug.Log($"アイテムが選択された: {index}");
            itemNameText.text = _itemList[index].Definition.DisplayName;
            itemFlavorText.text = _itemList[index].Definition.FlavorText;
            itemImage.sprite = _itemList[index].Definition.Image;
            
            // 選択位置を示すUI
            selectBarRect.DOKill();
            selectBarRect.DOAnchorPosY(-1f * _itemHeight * (index + 0.5f), selectBarDuration)
                .SetEase(Ease.OutSine)
                .SetLink(gameObject);
            
            // TopBarのMass,Volume表示
            massUtilizationText.text = _inventory.TotalMass.ToString("G3", CultureInfo.InvariantCulture);
            volumeUtilizationText.text = _inventory.TotalVolume.ToString("G3", CultureInfo.InvariantCulture);
            massUtilizationBarRect.anchorMax = new Vector2(_inventory.MassUtilization, 1);
            volumeUtilizationBarRect.anchorMax = new Vector2(_inventory.VolumeUtilization, 1);
            Debug.Log($"Mass: {_inventory.MassUtilization}, Volume: {_inventory.VolumeUtilization}");
        }
        
        #endregion
    }
}