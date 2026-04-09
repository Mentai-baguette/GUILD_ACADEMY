namespace GuildAcademy.Core.Dungeon
{
    public class DungeonData
    {
        public string DungeonId { get; set; }
        public string Name { get; set; }
        public int TotalFloors { get; set; }
        public int PortalInterval { get; set; } = 5;
        public int BossFloor { get; set; }
        public int MidBossFloor { get; set; }

        /// <summary>
        /// 基本エンカウント率 (0.0 ~ 1.0)。歩数ごとの判定に使用。
        /// </summary>
        public float BaseEncounterRate { get; set; } = 0.065f; // 6.5% (5-8% range)

        /// <summary>
        /// 深層でのエンカウント率上昇係数。floor / totalFloors * この値が加算される。
        /// </summary>
        public float DepthEncounterBonus { get; set; } = 0.02f;

        public bool IsPortalFloor(int floor)
        {
            return floor > 0 && floor % PortalInterval == 0;
        }

        public float GetEncounterRate(int floor)
        {
            if (TotalFloors <= 0) return BaseEncounterRate;
            float depthFactor = (float)floor / TotalFloors;
            return BaseEncounterRate + depthFactor * DepthEncounterBonus;
        }
    }
}
