using System;
using UnityEngine;
using UnityEngine.Localization;

namespace Model
{
    public enum ItemType
    {
        Stackable,
        Unique,
    }

    /// <summary>
    /// 単一のアイテムの設定を行う。レシピの設定などをインスペクターからドラッグ&ドロップで行えるという利点も。
    /// </summary>
    /// 用途に応じたインターフェースで見れる値を制限する予定
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private LocalizedString displayName;
        [SerializeField] private LocalizedString flavorText;
        [SerializeField] private Sprite icon;
        [SerializeField] private float mass;
        [SerializeField] private float volume;

        public ItemId Id => new ItemId(id);
        public string DisplayName => displayName.GetLocalizedString();
        public string FlavorText => flavorText.GetLocalizedString();
        public float Mass => mass;
        public float Volume => volume;
    }

    public readonly struct ItemId : IEquatable<ItemId>
    {
        public static readonly ItemId Empty = default;

        public string Value { get; }

        public ItemId(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("ItemId cannot be null or empty.", nameof(value));
            Value = value;
        }

        public bool Equals(ItemId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ItemId other && Equals(other);
        public override int GetHashCode() => Value?.GetHashCode() ?? 0;
        public override string ToString() => Value;

        public static bool operator ==(ItemId left, ItemId right) => left.Equals(right);
        public static bool operator !=(ItemId left, ItemId right) => !left.Equals(right);
    }
}