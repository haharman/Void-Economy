using System;
using UnityEngine;

namespace Core
{
    public enum ItemType
    {
        Simple,
        Unique,
    }

    /// <summary>
    /// 単一のアイテムの設定を行う。レシピの設定などをインスペクターからドラッグ&ドロップで行えるという利点も。
    /// </summary>
    public class ItemDefinition : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [SerializeField] private float mass;
        [SerializeField] private float volume;

        public ItemId Id => new ItemId(id);
        public string DisplayName => displayName;
        public float Mass => mass;
        public float Volume => volume;
    }

    [CreateAssetMenu(menuName = "ItemDefinition", fileName = "NewItemDefinition")]
    public class SimpleItemDefinition : ItemDefinition
    {
        
    }

    [CreateAssetMenu(menuName = "ItemDefinition", fileName = "NewItemDefinition")]
    public class UniqueItemDefinition : ItemDefinition
    {
        
    }

    public readonly struct ItemId : IEquatable<ItemId>
    {
        public string Value { get; }

        public ItemId(string value)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("ItemId cannot be null or empty.", nameof(value));
            Value = value;
        }

        public bool Equals(ItemId other) => Value == other.Value;
        public override bool Equals(object obj) => obj is ItemId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value;

        public static bool operator ==(ItemId left, ItemId right) => left.Equals(right);
        public static bool operator !=(ItemId left, ItemId right) => !left.Equals(right);
    }
    
    public readonly struct ItemInstanceId : IEquatable<ItemInstanceId>
    {
        public Guid Value { get; }

        public ItemInstanceId(Guid value) => Value = value;

        public static ItemInstanceId New() => new ItemInstanceId(Guid.NewGuid());

        public bool Equals(ItemInstanceId other) => Value.Equals(other.Value);
        public override bool Equals(object obj) => obj is ItemInstanceId other && Equals(other);
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();

        public static bool operator ==(ItemInstanceId left, ItemInstanceId right) => left.Equals(right);
        public static bool operator !=(ItemInstanceId left, ItemInstanceId right) => !left.Equals(right);
    }
}