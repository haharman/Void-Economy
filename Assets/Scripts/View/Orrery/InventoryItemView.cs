using System.Globalization;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class InventoryItemView : MonoBehaviour
    {
        public int index;
        public Button button;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text count;
        [SerializeField] private TMP_Text mass;
        [SerializeField] private TMP_Text volume;
        [SerializeField] private TMP_Text quality;
        private string _massUnit = "<size=16> kg";
        private string _volumeUnit = "<size=16> m³";
        private string _qualityUnit = "<size=16> %";
        public void UpdateHeight(float itemHeight)
        {
            // RectTransformの高さ変更（Stretchなしなのでそのまま高さになる）
            Vector2 size = rectTransform.sizeDelta;
            size.y = itemHeight;
            rectTransform.sizeDelta = size;
            
            // VerticalLayoutに申告する高さを変更
            layoutElement.preferredHeight = itemHeight;
        }

        public void Bind(ItemStack stack)
        {
            icon.sprite = stack.Definition.Icon;
            count.text = stack.Count.ToString();
            mass.text = stack.TotalMass.ToString("G3", CultureInfo.InvariantCulture) + _massUnit;
            volume.text = stack.TotalVolume.ToString("G3", CultureInfo.InvariantCulture) + _volumeUnit;
            quality.text = (stack.Quality*100f).ToString("G3", CultureInfo.InvariantCulture) + _qualityUnit;
        }
    }
}