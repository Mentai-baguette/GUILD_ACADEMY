using System;

namespace GuildAcademy.Core.Data
{
    public class ItemData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemType Type { get; set; }
        public ItemEffectType Effect { get; set; }
        public int Value { get; set; }
        public int MaxStack { get; set; }
        public int BuyPrice { get; set; }
        public int SellPrice { get; set; }
        public bool IsUsableInBattle { get; set; }
        public bool IsUsableInField { get; set; }

        public ItemData(string name, string description, ItemType type,
            ItemEffectType effect = ItemEffectType.None, int value = 0,
            int maxStack = 99, int buyPrice = 0, int sellPrice = 0,
            bool isUsableInBattle = true, bool isUsableInField = true)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? "";
            Type = type;
            Effect = effect;
            Value = Math.Max(0, value);
            MaxStack = maxStack > 0 ? maxStack : 1;
            BuyPrice = Math.Max(0, buyPrice);
            SellPrice = Math.Max(0, sellPrice);
            IsUsableInBattle = isUsableInBattle;
            IsUsableInField = isUsableInField;
        }
    }
}
