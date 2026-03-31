namespace GuildAcademy.Core.Data
{
    public class CharacterStats
    {
        public string Name { get; set; }
        public int MaxHp { get; set; }
        public int CurrentHp { get; set; }
        public int MaxMp { get; set; }
        public int CurrentMp { get; set; }
        public int Atk { get; set; }
        public int Def { get; set; }
        public int Spd { get; set; }
        public ElementType Element { get; set; }
        public ElementType WeakElement { get; set; }
        public ElementType ResistElement { get; set; }
        public ElementType NullElement { get; set; }

        public CharacterStats(string name, int maxHp, int maxMp, int atk, int def, int spd, ElementType element = ElementType.None)
        {
            Name = name;
            MaxHp = maxHp;
            CurrentHp = maxHp;
            MaxMp = maxMp;
            CurrentMp = maxMp;
            Atk = atk;
            Def = def;
            Spd = spd;
            Element = element;
            WeakElement = ElementType.None;
            ResistElement = ElementType.None;
            NullElement = ElementType.None;
        }
    }
}
