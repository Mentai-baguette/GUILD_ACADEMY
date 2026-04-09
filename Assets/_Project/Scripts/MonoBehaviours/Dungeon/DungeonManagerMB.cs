using System.Collections.Generic;
using UnityEngine;
using GuildAcademy.Core.Data;
using GuildAcademy.Core.Dungeon;
using GuildAcademy.Data;
using GuildAcademy.MonoBehaviours.Schedule;
using GuildAcademy.MonoBehaviours.UI;

namespace GuildAcademy.MonoBehaviours.Dungeon
{
    public class DungeonManagerMB : MonoBehaviour
    {
        public static DungeonManagerMB Instance { get; private set; }

        [SerializeField] private DungeonDataSO _dungeonData;

        public DungeonManager Dungeon { get; private set; }
        public DungeonDataSO CurrentDungeonSO => _dungeonData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            var scheduleMB = ScheduleManagerMB.Instance;
            if (scheduleMB == null)
            {
                Debug.LogWarning("[DungeonManagerMB] ScheduleManagerMB not found.");
                return;
            }

            Dungeon = new DungeonManager(scheduleMB.Schedule);
            Dungeon.OnEncounterTriggered += HandleEncounter;
            Dungeon.OnBossFloorReached += HandleBossFloor;
            Dungeon.OnDungeonExited += HandleDungeonExited;
        }

        public void EnterDungeon(DungeonDataSO dungeonSO, int startFloor = 1)
        {
            _dungeonData = dungeonSO;
            var data = dungeonSO.ToDungeonData();
            Dungeon.EnterDungeon(data, startFloor);
        }

        private void HandleEncounter(float rate)
        {
            if (_dungeonData == null) return;

            // Pick a random enemy from the dungeon's enemy pool
            var enemies = _dungeonData.normalEnemies;
            if (enemies == null || enemies.Length == 0) return;

            var enemy = enemies[Random.Range(0, enemies.Length)];
            if (enemy == null) return;

            var enemyStats = enemy.ToCharacterStats();

            // Apply night multiplier
            float multiplier = Dungeon.GetCurrentEnemyMultiplier();
            if (multiplier > 1f)
            {
                enemyStats = new CharacterStats(
                    enemyStats.Name,
                    (int)(enemyStats.MaxHp * multiplier),
                    enemyStats.MaxMp,
                    (int)(enemyStats.Atk * multiplier),
                    enemyStats.Def,
                    enemyStats.Agi,
                    enemyStats.Element);
            }

            var enemyList = new List<CharacterStats> { enemyStats };

            // Load default party from resources
            var partySOs = Resources.LoadAll<CharacterDataSO>("Data/Characters");
            var party = new List<CharacterStats>();
            foreach (var so in partySOs)
            {
                if (so != null) party.Add(so.ToCharacterStats());
            }

            var setup = new BattleSetupData(party, enemyList,
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                IsBossBattle = false,
                CanFlee = true
            };

            BattleSetupData.Current = setup;

            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene(SceneNames.Battle);
        }

        private void HandleBossFloor(int floor)
        {
            if (_dungeonData == null || _dungeonData.bossEnemy == null) return;

            var bossStats = _dungeonData.bossEnemy.ToCharacterStats();
            var enemyList = new List<CharacterStats> { bossStats };

            var partySOs = Resources.LoadAll<CharacterDataSO>("Data/Characters");
            var party = new List<CharacterStats>();
            foreach (var so in partySOs)
            {
                if (so != null) party.Add(so.ToCharacterStats());
            }

            var setup = new BattleSetupData(party, enemyList,
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                IsBossBattle = true,
                CanFlee = false
            };

            BattleSetupData.Current = setup;

            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene(SceneNames.Battle);
        }

        private void HandleDungeonExited()
        {
            // Return to field scene
            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene(SceneNames.Field);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
