using UnityEngine;
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
        public int spd;

        [Header("Element")]
        public ElementType element;
        public ElementType weakElement;
        public ElementType resistElement;
        public ElementType nullElement;

        public CharacterStats ToCharacterStats()
        {
            var stats = new CharacterStats(characterName, maxHp, maxMp, atk, def, spd, element);
            stats.WeakElement = weakElement;
            stats.ResistElement = resistElement;
            stats.NullElement = nullElement;
            return stats;
        }
    }
}
