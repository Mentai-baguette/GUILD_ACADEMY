using System.Collections.Generic;

namespace GuildAcademy.Core.Data
{
    public class BattleSetupData
    {
        public static BattleSetupData Current { get; set; }

        public List<CharacterStats> Party { get; set; }
        public List<CharacterStats> Enemies { get; set; }
        public List<SkillData> EnemySkills { get; set; }
        public string ReturnSceneName { get; set; }
        public bool IsBossBattle { get; set; }
        public bool CanFlee { get; set; } = true;
        public string BattleBgmId { get; set; }

        public BattleSetupData(List<CharacterStats> party, List<CharacterStats> enemies,
            string returnSceneName, List<SkillData> enemySkills = null)
        {
            Party = party ?? throw new System.ArgumentNullException(nameof(party));
            Enemies = enemies ?? throw new System.ArgumentNullException(nameof(enemies));
            ReturnSceneName = returnSceneName ?? throw new System.ArgumentNullException(nameof(returnSceneName));
            EnemySkills = enemySkills ?? new List<SkillData>();
        }
    }
}
