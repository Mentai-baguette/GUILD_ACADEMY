using UnityEngine;
using GuildAcademy.Core.Dungeon;
using GuildAcademy.Data;

namespace GuildAcademy.Data
{
    [CreateAssetMenu(fileName = "NewDungeon", menuName = "GuildAcademy/Dungeon Data")]
    public class DungeonDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string dungeonId;
        public string dungeonName;

        [Header("Structure")]
        public int totalFloors = 15;
        public int portalInterval = 5;
        public int bossFloor;
        public int midBossFloor;

        [Header("Encounter")]
        [Range(0f, 1f)]
        public float baseEncounterRate = 0.065f;
        public float depthEncounterBonus = 0.02f;

        [Header("Enemies")]
        public EnemyDataSO[] normalEnemies;
        public EnemyDataSO midBossEnemy;
        public EnemyDataSO bossEnemy;

        public DungeonData ToDungeonData()
        {
            return new DungeonData
            {
                DungeonId = dungeonId,
                Name = dungeonName,
                TotalFloors = totalFloors,
                PortalInterval = portalInterval,
                BossFloor = bossFloor,
                MidBossFloor = midBossFloor,
                BaseEncounterRate = baseEncounterRate,
                DepthEncounterBonus = depthEncounterBonus
            };
        }
    }
}
