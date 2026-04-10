namespace GuildAcademy.Core.Data
{
    public enum CommandType { Attack, Skill, Item, Defend, Flee, DualArts, Change, Swap, Special }

    public class BattleCommand
    {
        public CharacterStats Attacker { get; set; }
        public CharacterStats Target { get; set; }
        public CommandType Type { get; set; }
        public ElementType Element { get; set; }
        public int SkillPower { get; set; }
        public int MpCost { get; set; }
        public bool IsMagic { get; set; }
    }
}
