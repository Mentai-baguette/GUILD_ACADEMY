using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Branch
{
    public class EndingContext
    {
        public FlagSystem Flags { get; set; }
        public TrustSystem Trust { get; set; }
        public BattlePhase Phase { get; set; }
        public BattleResult Result { get; set; }
        public bool ShionRescued { get; set; }
        public bool CarlosDefeated { get; set; }
        public int ErosionPercent { get; set; }
        public int ShionTrust { get; set; }
        public bool GreyveEventCleared { get; set; }
        public int SetsunaTrust { get; set; }
    }
}
