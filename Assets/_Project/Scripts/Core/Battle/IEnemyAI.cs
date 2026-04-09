using System.Collections.Generic;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Battle
{
    public interface IEnemyAI
    {
        BattleCommand DecideAction(EnemyAIContext context);
    }

    public class EnemyAIContext
    {
        public CharacterStats Actor { get; set; }
        public List<CharacterStats> Party { get; set; }
        public List<CharacterStats> Enemies { get; set; }
        public BreakSystem BreakSystem { get; set; }
        public List<SkillData> AvailableSkills { get; set; }
        public IRandom Random { get; set; }
    }
}
