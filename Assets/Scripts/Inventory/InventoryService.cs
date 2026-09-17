using System.Collections.Generic;

namespace TsOnline
{
    public class ItemStack
    {
        public string itemId;
        public int count;
    }

    /// <summary>8-slot bag. Herb is the only item used in Step 5.</summary>
    public static class InventoryService
    {
        public const int MaxSlots = 8;
        public const string HerbId = "herb";
        public const int HerbHeal = 16;

        public static readonly List<ItemStack> Slots = new List<ItemStack>();

        public static void Clear()
        {
            Slots.Clear();
        }

        public static int CountOf(string itemId)
        {
            int n = 0;
            for (int i = 0; i < Slots.Count; i++)
            {
                if (Slots[i] != null && Slots[i].itemId == itemId)
                    n += Slots[i].count;
            }

            return n;
        }

        public static bool Add(string itemId, int amount)
        {
            if (string.IsNullOrEmpty(itemId) || amount <= 0)
                return false;
            for (int i = 0; i < Slots.Count; i++)
            {
                if (Slots[i] != null && Slots[i].itemId == itemId)
                {
                    Slots[i].count += amount;
                    return true;
                }
            }

            if (Slots.Count >= MaxSlots)
                return false;
            Slots.Add(new ItemStack { itemId = itemId, count = amount });
            return true;
        }

        public static bool TryConsume(string itemId, int amount = 1)
        {
            if (amount <= 0)
                return true;
            for (int i = 0; i < Slots.Count; i++)
            {
                ItemStack stack = Slots[i];
                if (stack == null || stack.itemId != itemId || stack.count < amount)
                    continue;
                stack.count -= amount;
                if (stack.count <= 0)
                    Slots.RemoveAt(i);
                return true;
            }

            return false;
        }

        public static SavedStack[] Export()
        {
            var list = new SavedStack[Slots.Count];
            for (int i = 0; i < Slots.Count; i++)
            {
                list[i] = new SavedStack
                {
                    itemId = Slots[i].itemId,
                    count = Slots[i].count
                };
            }

            return list;
        }

        public static void Apply(SavedStack[] stacks)
        {
            Slots.Clear();
            if (stacks == null)
                return;
            for (int i = 0; i < stacks.Length && Slots.Count < MaxSlots; i++)
            {
                if (stacks[i] == null || string.IsNullOrEmpty(stacks[i].itemId) || stacks[i].count <= 0)
                    continue;
                Slots.Add(new ItemStack { itemId = stacks[i].itemId, count = stacks[i].count });
            }
        }
    }
}
