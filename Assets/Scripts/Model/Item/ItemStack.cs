namespace Model
{
    public struct ItemStack
    {
        public ItemId ItemId { get; private set; }
        public int Count { get; private set; }
        public float Quality { get; private set; }
        public ItemDefinition Definition { get; private set; }

        public float TotalMass => Definition != null ? Definition.Mass * Count : 0f;
        public float TotalVolume => Definition != null ? Definition.Volume * Count : 0f;

        public static ItemStack EmptyItemStack => new ItemStack(ItemId.Empty, 0, 0f, null);

        public ItemStack(ItemId itemId, int count, float quality, ItemDefinition definition)
        {
            ItemId = itemId;
            Count = count;
            Quality = quality;
            Definition = definition;
        }

        public void Marge(ItemStack additional)
        {
            var totalCount = Count + additional.Count;
            Quality = totalCount > 0
                ? (Quality * Count + additional.Quality * additional.Count) / totalCount
                : 0f;
            Count = totalCount;
        }

        public ItemStack Split(int count)
        {
            Count -= count;
            return new ItemStack(ItemId, count, Quality, Definition);
        }
    }
}
