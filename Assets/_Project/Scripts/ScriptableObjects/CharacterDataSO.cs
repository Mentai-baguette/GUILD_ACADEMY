using UnityEngine;
using UnityEngine.Serialization;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Data
{
    [CreateAssetMenu(fileName = "NewCharacter", menuName = "GuildAcademy/Character Data")]
    public class CharacterDataSO : ScriptableObject
    {
        [Header("Identity")]
        public CharacterId id;
        public string characterName;
        [TextArea] public string description;

        [Header("Base Stats")]
        public int maxHp;
        public int maxMp;
        public int atk;
        public int def;
        public int intStat;
        public int res;
        [FormerlySerializedAs("spd")]
        public int agi;
        public int dex;
        public int luk = 100;

        [Header("Level")]
        public int level = 1;

        [Header("Element")]
        public ElementType element;
        public ElementType weakElement;
        public ElementType resistElement;
        public ElementType nullElement;

        public CharacterStats ToCharacterStats()
        {
            var stats = new CharacterStats(
                characterName, maxHp, maxMp, atk, def, agi, element,
                intStat, res, dex, luk, lv: level);
            stats.WeakElement = weakElement;
            stats.ResistElement = resistElement;
            stats.NullElement = nullElement;
            return stats;
        }
    }
}
