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

        [Header("Default Party (used when PartyManagerMB is not available)")]
        [SerializeField] private CharacterDataSO[] _fallbackParty;

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
            InitializeDungeonManager();
        }

        private void InitializeDungeonManager()
        {
            var scheduleMB = ScheduleManagerMB.Instance;
            if (scheduleMB == null)
            {
                Debug.LogWarning("[DungeonManagerMB] ScheduleManagerMB not found. DungeonManager will be initialized on first use.");
                return;
            }

            Dungeon = new DungeonManager(scheduleMB.Schedule);
            SubscribeEvents();
        }

        private void EnsureInitialized()
        {
            if (Dungeon != null) return;

            var scheduleMB = ScheduleManagerMB.Instance;
            if (scheduleMB == null)
            {
                Debug.LogError("[DungeonManagerMB] ScheduleManagerMB is required but not found.");
                return;
            }

            Dungeon = new DungeonManager(scheduleMB.Schedule);
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            Dungeon.OnEncounterTriggered += HandleEncounter;
            Dungeon.OnBossFloorReached += HandleBossFloor;
            Dungeon.OnDungeonExited += HandleDungeonExited;
        }

        public void EnterDungeon(DungeonDataSO dungeonSO, int startFloor = 1)
        {
            EnsureInitialized();
            if (Dungeon == null)
            {
                Debug.LogError("[DungeonManagerMB] Cannot enter dungeon: DungeonManager not initialized.");
                return;
            }

            _dungeonData = dungeonSO;
            var data = dungeonSO.ToDungeonData();
            Dungeon.EnterDungeon(data, startFloor);
        }

        private void HandleEncounter(float rate)
        {
            if (_dungeonData == null) return;

            var enemies = _dungeonData.normalEnemies;
            if (enemies == null || enemies.Length == 0) return;

            var enemy = enemies[Random.Range(0, enemies.Length)];
            if (enemy == null) return;

            var enemyStats = enemy.ToCharacterStats();

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

            StartBattle(new List<CharacterStats> { enemyStats }, isBoss: false);
        }

        private void HandleBossFloor(int floor)
        {
            if (_dungeonData == null || _dungeonData.bossEnemy == null) return;
            var bossStats = _dungeonData.bossEnemy.ToCharacterStats();
            StartBattle(new List<CharacterStats> { bossStats }, isBoss: true);
        }

        private void StartBattle(List<CharacterStats> enemyList, bool isBoss)
        {
            var party = GetBattleParty();
            if (party.Count == 0)
            {
                Debug.LogError("[DungeonManagerMB] No party members available for battle.");
                return;
            }

            var setup = new BattleSetupData(party, enemyList,
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                IsBossBattle = isBoss,
                CanFlee = !isBoss
            };

            BattleSetupData.Current = setup;

            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(SceneNames.Battle);
            }
            else
            {
                Debug.LogError("[DungeonManagerMB] SceneTransitionManager not found. Battle scene transition failed.");
            }
        }

        private List<CharacterStats> GetBattleParty()
        {
            // _fallbackParty(Inspector設定)からパーティを構築
            if (_fallbackParty != null && _fallbackParty.Length > 0)
            {
                var party = new List<CharacterStats>();
                int count = System.Math.Min(_fallbackParty.Length, 5);
                for (int i = 0; i < count; i++)
                {
                    if (_fallbackParty[i] != null)
                        party.Add(_fallbackParty[i].ToCharacterStats());
                }
                if (party.Count > 0) return party;
            }

            Debug.LogWarning("[DungeonManagerMB] No party configured. Using fallback.");
            return new List<CharacterStats>
            {
                new CharacterStats("レイ", 105, 25, 12, 10, 10, ElementType.Dark)
            };
        }

        private void HandleDungeonExited()
        {
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(SceneNames.Field);
            }
            else
            {
                Debug.LogError("[DungeonManagerMB] SceneTransitionManager not found. Field return failed.");
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
