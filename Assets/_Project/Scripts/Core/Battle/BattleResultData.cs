using System.Collections.Generic;

namespace GuildAcademy.Core.Battle
{
    public class BattleResultData
    {
        public int TotalEXP { get; set; }
        public int TotalGold { get; set; }
        public List<string> DroppedItems { get; set; } = new List<string>();
        public Dictionary<string, LevelUpInfo> LevelUps { get; set; } = new Dictionary<string, LevelUpInfo>();
        public int TotalSP { get; set; }
    }

    public class LevelUpInfo
    {
        public int OldLevel { get; set; }
        public int NewLevel { get; set; }
        public Dictionary<string, int> StatChanges { get; set; } = new Dictionary<string, int>();
    }
}
