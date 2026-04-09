using NUnit.Framework;
using GuildAcademy.Core.Data;
using System;

namespace GuildAcademy.Tests.EditMode.Data
{
    [TestFixture]
    public class ItemDataTests
    {
        [Test]
        public void Constructor_SetsAllProperties()
        {
            var item = new ItemData("ポーション", "HPを100回復する", ItemType.Consumable,
                ItemEffectType.HpRestore, 100, 99, 50, 25);

            Assert.AreEqual("ポーション", item.Name);
            Assert.AreEqual("HPを100回復する", item.Description);
            Assert.AreEqual(ItemType.Consumable, item.Type);
            Assert.AreEqual(ItemEffectType.HpRestore, item.Effect);
            Assert.AreEqual(100, item.Value);
            Assert.AreEqual(99, item.MaxStack);
            Assert.AreEqual(50, item.BuyPrice);
            Assert.AreEqual(25, item.SellPrice);
        }

        [Test]
        public void Constructor_DefaultValues()
        {
            var item = new ItemData("テストアイテム", "説明", ItemType.Material);

            Assert.AreEqual(ItemEffectType.None, item.Effect);
            Assert.AreEqual(0, item.Value);
            Assert.AreEqual(99, item.MaxStack);
            Assert.AreEqual(0, item.BuyPrice);
            Assert.AreEqual(0, item.SellPrice);
            Assert.IsTrue(item.IsUsableInBattle);
            Assert.IsTrue(item.IsUsableInField);
        }

        [Test]
        public void Constructor_NullName_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ItemData(null, "desc", ItemType.Consumable));
        }

        [Test]
        public void Constructor_NullDescription_DefaultsToEmpty()
        {
            var item = new ItemData("アイテム", null, ItemType.KeyItem);
            Assert.AreEqual("", item.Description);
        }

        [Test]
        public void KeyItem_CanBeNonStackable()
        {
            var item = new ItemData("重要アイテム", "ストーリーキーアイテム", ItemType.KeyItem,
                maxStack: 1);
            Assert.AreEqual(1, item.MaxStack);
        }

        [Test]
        public void ConsumableItem_HpRestore_HasCorrectValue()
        {
            var potion = new ItemData("ポーション", "HPを100回復", ItemType.Consumable,
                ItemEffectType.HpRestore, 100, buyPrice: 50, sellPrice: 25);
            Assert.AreEqual(ItemEffectType.HpRestore, potion.Effect);
            Assert.AreEqual(100, potion.Value);
        }

        [Test]
        public void ReviveItem_HasCorrectEffect()
        {
            var phoenix = new ItemData("フェニックスの尾", "戦闘不能を回復", ItemType.Consumable,
                ItemEffectType.Revive, 1, buyPrice: 300, sellPrice: 150);
            Assert.AreEqual(ItemEffectType.Revive, phoenix.Effect);
        }

        [Test]
        public void FieldOnlyItem_NotUsableInBattle()
        {
            var tent = new ItemData("テント", "HP/MP全回復（フィールドのみ）", ItemType.Consumable,
                ItemEffectType.FullRestore, 100, buyPrice: 500, sellPrice: 250,
                isUsableInBattle: false, isUsableInField: true);
            Assert.IsFalse(tent.IsUsableInBattle);
            Assert.IsTrue(tent.IsUsableInField);
        }
    }
}
