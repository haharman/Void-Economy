using UnityEngine;
using UnityEngine.Localization;

namespace Model
{
    /// <summary>
    /// 単一のアイテムの設定を行う。レシピの設定などをインスペクターからドラッグ&ドロップで行えるという利点も。
    /// </summary>
    /// 用途に応じたインターフェースで見れる値を制限する予定
    [CreateAssetMenu(fileName = "ItemDefinition", menuName = "NewItemDefinition")]
    public class ItemDefinitionSo : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private LocalizedString displayName;
        [SerializeField] private LocalizedString flavorText;
        [SerializeField] private Sprite icon;
        [SerializeField] private float mass;
        [SerializeField] private float volume;
        [SerializeField] private Sprite image;

        public ItemId Id => new ItemId(id);
        public string DisplayName => displayName.GetLocalizedString();
        public string FlavorText => flavorText.GetLocalizedString();
        public Sprite Icon => icon;
        public float Mass => mass;
        public float Volume => volume;
        public Sprite Image => image;
    }
}