using System;
using System.Collections.Generic;
using System.Linq;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Branch
{
    public class TrustSystem
    {
        private const int MinTrust = 0;
        private const int MaxTrust = 100;

        private readonly Dictionary<CharacterId, int> _trust;

        private static readonly CharacterId[] TrustTargets =
        {
            CharacterId.Yuna,
            CharacterId.Mio,
            CharacterId.Kaito,
            CharacterId.Shion
        };

        public TrustSystem()
        {
            _trust = new Dictionary<CharacterId, int>();
            foreach (var id in TrustTargets)
                _trust[id] = 0;
        }

        public void AddTrust(CharacterId id, int amount)
        {
            ValidateTrustTarget(id);
            _trust[id] = Math.Clamp(_trust[id] + amount, MinTrust, MaxTrust);
        }

        public void SetTrust(CharacterId id, int value)
        {
            ValidateTrustTarget(id);
            _trust[id] = Math.Clamp(value, MinTrust, MaxTrust);
        }

        public int GetTrust(CharacterId id)
        {
            ValidateTrustTarget(id);
            return _trust[id];
        }

        public bool MeetsThreshold(CharacterId id, int threshold)
        {
            ValidateTrustTarget(id);
            return _trust[id] >= threshold;
        }

        public bool IsTrustTarget(CharacterId id)
        {
            return _trust.ContainsKey(id);
        }

        private void ValidateTrustTarget(CharacterId id)
        {
            if (!_trust.ContainsKey(id))
                throw new NotSupportedException($"{id} is not a trust target. Only {string.Join(", ", TrustTargets)} are supported.");
        }

        public bool AllMeetThreshold(int threshold)
        {
            return _trust.Values.All(v => v >= threshold);
        }

        public void Reset()
        {
            foreach (var key in _trust.Keys.ToList())
                _trust[key] = 0;
        }
    }
}
