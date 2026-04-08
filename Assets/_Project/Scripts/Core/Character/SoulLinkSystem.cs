using System;
using System.Collections.Generic;
using System.Linq;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Character
{
    public enum SoulLinkLevel
    {
        None = 0,
        Level1 = 1,  // Dual Arts available
        Level2 = 2,  // Auto protect/heal
        Level3 = 3,  // Soul Link Ultimate
        Max = 4      // Strongest combo + story unlock
    }

    public class SoulLinkSystem
    {
        public const int MAX_BOND_POINTS = 100;
        public const int LEVEL1_THRESHOLD = 20;
        public const int LEVEL2_THRESHOLD = 40;
        public const int LEVEL3_THRESHOLD = 70;
        public const int MAX_THRESHOLD = 100;

        private readonly Dictionary<CharacterId, int> _bondPoints;
        private bool _shionFrozen;

        public event Action<CharacterId, SoulLinkLevel> OnLevelUp;

        // All party members except Ray (protagonist)
        private static readonly CharacterId[] LinkTargets =
        {
            CharacterId.Yuna,
            CharacterId.Mio,
            CharacterId.Kaito,
            CharacterId.Shion,
            CharacterId.Rin,
            CharacterId.Vein,
            CharacterId.Mel,
            CharacterId.Jin,
            CharacterId.Setsuna,
            CharacterId.Renji
        };

        public SoulLinkSystem()
        {
            _bondPoints = new Dictionary<CharacterId, int>();
            _shionFrozen = false;
            foreach (var id in LinkTargets)
                _bondPoints[id] = 0;
        }

        public void AddBondPoints(CharacterId id, int amount)
        {
            ValidateTarget(id);
            if (amount <= 0) return;
            if (id == CharacterId.Shion && _shionFrozen) return;
            var oldLevel = GetLevel(id);
            _bondPoints[id] = Math.Min(_bondPoints[id] + amount, MAX_BOND_POINTS);
            var newLevel = GetLevel(id);
            if (newLevel > oldLevel)
                OnLevelUp?.Invoke(id, newLevel);
        }

        /// <summary>
        /// Freeze Shion's soul link (called at Chapter 2 Week 11).
        /// While frozen, bond points cannot be added to Shion.
        /// </summary>
        public void FreezeShion()
        {
            _shionFrozen = true;
        }

        /// <summary>
        /// Unfreeze Shion's soul link (called on END5 rescue).
        /// </summary>
        public void UnfreezeShion()
        {
            _shionFrozen = false;
        }

        /// <summary>
        /// Whether Shion's soul link is currently frozen.
        /// </summary>
        public bool IsShionFrozen => _shionFrozen;

        public int GetBondPoints(CharacterId id)
        {
            ValidateTarget(id);
            return _bondPoints[id];
        }

        public SoulLinkLevel GetLevel(CharacterId id)
        {
            ValidateTarget(id);
            int points = _bondPoints[id];
            if (points >= MAX_THRESHOLD) return SoulLinkLevel.Max;
            if (points >= LEVEL3_THRESHOLD) return SoulLinkLevel.Level3;
            if (points >= LEVEL2_THRESHOLD) return SoulLinkLevel.Level2;
            if (points >= LEVEL1_THRESHOLD) return SoulLinkLevel.Level1;
            return SoulLinkLevel.None;
        }

        public bool IsDualArtsAvailable(CharacterId id)
        {
            return GetLevel(id) >= SoulLinkLevel.Level1;
        }

        public bool IsAutoProtectAvailable(CharacterId id)
        {
            return GetLevel(id) >= SoulLinkLevel.Level2;
        }

        public bool IsUltimateAvailable(CharacterId id)
        {
            return GetLevel(id) >= SoulLinkLevel.Level3;
        }

        public bool IsMaxBond(CharacterId id)
        {
            return GetLevel(id) == SoulLinkLevel.Max;
        }

        public bool IsLinkTarget(CharacterId id)
        {
            return _bondPoints.ContainsKey(id);
        }

        public void Reset()
        {
            foreach (var key in _bondPoints.Keys.ToList())
                _bondPoints[key] = 0;
        }

        private void ValidateTarget(CharacterId id)
        {
            if (!_bondPoints.ContainsKey(id))
                throw new NotSupportedException($"{id} is not a soul link target. Only {string.Join(", ", LinkTargets)} are supported.");
        }
    }
}
