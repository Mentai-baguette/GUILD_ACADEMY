namespace GuildAcademy.Core.Data
{
    public enum SkillTargetType
    {
        SingleEnemy,
        AllEnemies,
        SingleAlly,
        AllAllies,
        Self
    }

    public class SkillData
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Power { get; set; }
        public int MpCost { get; set; }
        public ElementType Element { get; set; }
        public SkillTargetType TargetType { get; set; }
        public bool IsHealing { get; set; }
        public bool IsMagic { get; set; }
        public int BreakValue { get; set; }
    }
}
