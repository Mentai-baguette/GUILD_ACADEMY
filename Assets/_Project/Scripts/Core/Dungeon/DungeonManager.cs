using System;
using System.Collections.Generic;
using GuildAcademy.Core.Schedule;

namespace GuildAcademy.Core.Dungeon
{
    public class DungeonManager
    {
        private readonly ScheduleManager _schedule;
        private readonly IDungeonRandom _random;
        private DungeonData _currentDungeon;
        private int _currentFloor;
        private int _layersUsed;
        private int _maxLayerBudget;
        private readonly Dictionary<string, int> _maxReachedFloors = new Dictionary<string, int>();
        private readonly Dictionary<string, HashSet<int>> _activatedPortals = new Dictionary<string, HashSet<int>>();
        private int _stepsSinceLastEncounter;

        public DungeonData CurrentDungeon => _currentDungeon;
        public int CurrentFloor => _currentFloor;
        public bool IsInDungeon => _currentDungeon != null;
        public int LayersRemaining => Math.Max(0, _maxLayerBudget - _layersUsed);

        public event Action<int> OnFloorChanged;
        public event Action<float> OnEncounterTriggered;
        public event Action<int> OnBossFloorReached;
        public event Action<int> OnMidBossFloorReached;
        public event Action OnDungeonExited;
        public event Action<int> OnPortalActivated;

        public DungeonManager(ScheduleManager schedule, IDungeonRandom random = null)
        {
            _schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
            _random = random ?? new DefaultDungeonRandom();
        }

        public int GetMaxReachedFloor(string dungeonId)
        {
            return _maxReachedFloors.TryGetValue(dungeonId, out int floor) ? floor : 0;
        }

        public bool IsPortalActivated(string dungeonId, int floor)
        {
            return _activatedPortals.TryGetValue(dungeonId, out var portals) && portals.Contains(floor);
        }

        /// <summary>
        /// ダンジョンに入る。startFloorはポータル階から。
        /// </summary>
        public void EnterDungeon(DungeonData dungeon, int startFloor = 1)
        {
            if (dungeon == null) throw new ArgumentNullException(nameof(dungeon));
            if (IsInDungeon) throw new InvalidOperationException("Already in a dungeon.");

            _currentDungeon = dungeon;
            _currentFloor = startFloor;
            _layersUsed = 0;
            _stepsSinceLastEncounter = 0;

            // Calculate layer budget
            bool isNight = _schedule.IsNight();
            _maxLayerBudget = _schedule.CalculateDungeonLayers();
            if (isNight)
                _maxLayerBudget += _schedule.GetNightBonusLayers();

            UpdateMaxReachedFloor();
            OnFloorChanged?.Invoke(_currentFloor);
        }

        /// <summary>
        /// 次の階に進む。層予算を1消費する。
        /// </summary>
        public bool AdvanceFloor()
        {
            if (!IsInDungeon) return false;
            if (_currentFloor >= _currentDungeon.TotalFloors) return false;
            if (LayersRemaining <= 0) return false;

            _currentFloor++;
            _layersUsed++;
            _stepsSinceLastEncounter = 0;
            UpdateMaxReachedFloor();

            OnFloorChanged?.Invoke(_currentFloor);

            // Check for boss/mid-boss floors
            if (_currentFloor == _currentDungeon.BossFloor)
                OnBossFloorReached?.Invoke(_currentFloor);
            else if (_currentFloor == _currentDungeon.MidBossFloor)
                OnMidBossFloorReached?.Invoke(_currentFloor);

            // Activate portal if on portal floor
            if (_currentDungeon.IsPortalFloor(_currentFloor))
                ActivatePortal(_currentFloor);

            return true;
        }

        /// <summary>
        /// 歩行時のエンカウント判定。trueならエンカウント発生。
        /// </summary>
        public bool CheckEncounter()
        {
            if (!IsInDungeon) return false;
            if (_currentFloor == _currentDungeon.BossFloor) return false; // Boss floors use fixed encounters
            if (_currentFloor == _currentDungeon.MidBossFloor) return false;

            _stepsSinceLastEncounter++;
            float rate = _currentDungeon.GetEncounterRate(_currentFloor);

            // Increase rate slightly with steps to prevent long dry spells
            float adjustedRate = rate + (_stepsSinceLastEncounter * 0.005f);
            adjustedRate = Math.Min(adjustedRate, 0.25f); // Cap at 25%

            bool encountered = _random.NextFloat() < adjustedRate;
            if (encountered)
            {
                _stepsSinceLastEncounter = 0;
                OnEncounterTriggered?.Invoke(rate);
            }
            return encountered;
        }

        /// <summary>
        /// 最寄りのポータル階に帰還する。
        /// </summary>
        public int ReturnToPortal()
        {
            if (!IsInDungeon) return 0;

            int portalFloor = GetNearestActivatedPortal();
            if (portalFloor <= 0) portalFloor = 1; // Entrance

            _currentFloor = portalFloor;
            OnFloorChanged?.Invoke(_currentFloor);
            return portalFloor;
        }

        /// <summary>
        /// ダンジョンから退出する。
        /// </summary>
        public void ExitDungeon()
        {
            if (!IsInDungeon) return;

            _currentDungeon = null;
            _currentFloor = 0;
            _layersUsed = 0;
            OnDungeonExited?.Invoke();
        }

        /// <summary>
        /// 夜ダンジョンの敵強化倍率を取得。
        /// </summary>
        public float GetCurrentEnemyMultiplier()
        {
            return _schedule.IsNight() ? _schedule.GetNightEnemyMultiplier() : 1.0f;
        }

        private void ActivatePortal(int floor)
        {
            if (_currentDungeon == null) return;
            string id = _currentDungeon.DungeonId;

            if (!_activatedPortals.ContainsKey(id))
                _activatedPortals[id] = new HashSet<int>();

            if (_activatedPortals[id].Add(floor))
                OnPortalActivated?.Invoke(floor);
        }

        private int GetNearestActivatedPortal()
        {
            if (_currentDungeon == null) return 0;
            if (!_activatedPortals.TryGetValue(_currentDungeon.DungeonId, out var portals))
                return 0;

            int nearest = 0;
            foreach (int p in portals)
            {
                if (p <= _currentFloor && p > nearest)
                    nearest = p;
            }
            return nearest;
        }

        private void UpdateMaxReachedFloor()
        {
            if (_currentDungeon == null) return;
            string id = _currentDungeon.DungeonId;

            if (!_maxReachedFloors.ContainsKey(id) || _currentFloor > _maxReachedFloors[id])
                _maxReachedFloors[id] = _currentFloor;
        }
    }

    public interface IDungeonRandom
    {
        float NextFloat();
    }

    internal class DefaultDungeonRandom : IDungeonRandom
    {
        private readonly Random _rng = new Random();
        public float NextFloat() => (float)_rng.NextDouble();
    }
}
