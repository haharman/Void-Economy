using System;
using Core;

namespace Model
{
    /// <summary>
    /// UniqueItemは結構継承される
    /// </summary>
    public class UniqueItemInstance
    {
        public ItemId ItemId;
        public Guid InstanceId { get; }
        public ItemDefinition Definition { get; }
        public int Count { get; set; } // stackableな場合のみ意味を持つ

        public UniqueItemInstance(ItemDefinition definition, int count = 1)
        {
            InstanceId = Guid.NewGuid();
            Definition = definition;
            Count = count;
        }
    }
}