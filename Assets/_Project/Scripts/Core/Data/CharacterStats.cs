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
        public int Int { get; set; }
        public int Res { get; set; }
        public int Agi { get; set; }
        public int Dex { get; set; }
        public int Luk { get; set; }
        public int Lv { get; set; }
        public int Exp { get; set; }
        public int Sp { get; set; }
        public ElementType Element { get; set; }
        public ElementType WeakElement { get; set; }
        public ElementType ResistElement { get; set; }
        public ElementType NullElement { get; set; }

        public CharacterStats(
            string name,
            int maxHp,
            int maxMp,
            int atk,
            int def,
            int agi,
            ElementType element = ElementType.None,
            int intStat = 0,
            int res = 0,
            int dex = 0,
            int luk = 100,
            int lv = 1)
        {
            Name = name;
            MaxHp = maxHp;
            CurrentHp = maxHp;
            MaxMp = maxMp;
            CurrentMp = maxMp;
            Atk = atk;
            Def = def;
            Int = intStat;
            Res = res;
            Agi = agi;
            Dex = dex;
            Luk = luk;
            Lv = lv;
            Exp = 0;
            Element = element;
            WeakElement = ElementType.None;
            ResistElement = ElementType.None;
            NullElement = ElementType.None;
        }
    }
}
