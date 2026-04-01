using UnityEngine;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Data
{
    [CreateAssetMenu(fileName = "NewSkill", menuName = "GuildAcademy/Skill Data")]
    public class SkillDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string skillName;
        [TextArea] public string description;

        [Header("Stats")]
        public int power;
        public int mpCost;
        public ElementType element;
        public SkillTargetType targetType;
        public bool isHealing;

        [Header("Break")]
        public int breakValue;

        public SkillData ToSkillData()
        {
            return new SkillData
            {
                Name = skillName,
                Description = description,
                Power = power,
                MpCost = mpCost,
                Element = element,
                TargetType = targetType,
                IsHealing = isHealing,
                BreakValue = breakValue
            };
        }
    }
}
