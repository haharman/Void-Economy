using System;

namespace Model
{
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