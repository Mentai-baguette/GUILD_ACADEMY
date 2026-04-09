using UnityEngine;
using UnityEngine.Serialization;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Data
{
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "GuildAcademy/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string enemyName;
        [TextArea] public string description;
        public bool isBoss;
        public BattlePhase bossPhase;

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

        [Header("Break")]
        public int breakGaugeMax;
        public int breakWeakHitValue;

        [Header("Skills")]
        public SkillDataSO[] skills = new SkillDataSO[0];

        [Header("Rewards")]
        public int expReward;
        public int goldReward;

        public CharacterStats ToCharacterStats()
        {
            var stats = new CharacterStats(
                enemyName, maxHp, maxMp, atk, def, agi, element,
                intStat, res, dex, luk, lv: level);
            stats.WeakElement = weakElement;
            stats.ResistElement = resistElement;
            stats.NullElement = nullElement;
            return stats;
        }
    }
}
