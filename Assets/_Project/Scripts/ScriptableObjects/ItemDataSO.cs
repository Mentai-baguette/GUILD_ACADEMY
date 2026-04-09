using UnityEngine;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Data
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "GuildAcademy/Item Data")]
    public class ItemDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string itemName;
        [TextArea] public string description;
        public ItemType type;

        [Header("Effect")]
        public ItemEffectType effect;
        public int value;

        [Header("Stack & Price")]
        public int maxStack = 99;
        public int buyPrice;
        public int sellPrice;

        [Header("Usability")]
        public bool isUsableInBattle = true;
        public bool isUsableInField = true;

        public ItemData ToItemData()
        {
            return new ItemData(
                itemName, description, type, effect, value,
                maxStack, buyPrice, sellPrice,
                isUsableInBattle, isUsableInField);
        }
    }
}
