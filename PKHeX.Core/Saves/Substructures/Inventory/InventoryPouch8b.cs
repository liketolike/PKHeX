using System;
using System.Diagnostics;
using System.Linq;

namespace PKHeX.Core
{
    public sealed class InventoryPouch8b : InventoryPouch
    {
        private const int SIZE_ITEM = 0x10;

        private InventoryItem[] OriginalItems = Array.Empty<InventoryItem>();
        public bool SetNew { get; set; } = false;

        public InventoryPouch8b(InventoryType type, ushort[] legal, int maxCount, int offset) : base(type, legal, maxCount, offset) { }

        public override void GetPouch(byte[] data)
        {
            Items = new InventoryItem[LegalItems.Length];
            int ctr = 0;
            foreach (var index in LegalItems)
            {
                var ofs = GetItemOffset(index, Offset);
                var count = BitConverter.ToInt32(data, ofs);
                if (count == 0)
                    continue;

                bool isNew = BitConverter.ToInt32(data, ofs + 4) == 0;
                bool isFavorite = BitConverter.ToInt32(data, ofs + 0x8) == 1;
                // ushort sortOrder = BitConverter.ToUInt16(data, ofs + 0xE);
                Items[ctr++] = new InventoryItem { Index = index, Count = count, New = isNew, FreeSpace = isFavorite };
            }

            while (ctr != LegalItems.Length)
                Items[ctr++] = new InventoryItem();
            OriginalItems = Items.Select(i => i.Clone()).ToArray();
        }

        public override void SetPouch(byte[] data)
        {
            foreach (var item in Items)
            {
                var index = (ushort)item.Index;
                var isInLegal = Array.IndexOf(LegalItems, index);
                if (isInLegal == -1)
                {
                    Debug.WriteLine($"Invalid Item ID returned within this pouch: {index}");
                    continue;
                }

                if (SetNew && item.Index != 0)
                    item.New |= OriginalItems.All(z => z.Index != item.Index);

                var ofs = GetItemOffset(index, Offset);
                WriteItem(item, data, ofs);
            }
        }

        public static int GetItemOffset(ushort index, int baseOffset) => baseOffset + (SIZE_ITEM * index);

        public static void WriteItem(InventoryItem item, byte[] data, int ofs)
        {
            BitConverter.GetBytes((uint)item.Count).CopyTo(data, ofs);
            BitConverter.GetBytes(item.New ? 0u : 1u).CopyTo(data, ofs + 4);
            BitConverter.GetBytes(item.FreeSpace ? 1u : 0u).CopyTo(data, ofs + 8);
            if (item.Count == 0)
                BitConverter.GetBytes((ushort)0xFFFF).CopyTo(data, ofs + 0xE);
        }
    }
}
