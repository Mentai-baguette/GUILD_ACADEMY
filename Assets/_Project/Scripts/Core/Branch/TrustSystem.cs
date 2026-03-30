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

        public TrustSystem()
        {
            _trust = new Dictionary<CharacterId, int>();
            foreach (CharacterId id in Enum.GetValues(typeof(CharacterId)))
                _trust[id] = 0;
        }

        public void AddTrust(CharacterId id, int amount)
        {
            _trust[id] = Math.Clamp(_trust[id] + amount, MinTrust, MaxTrust);
        }

        public void SetTrust(CharacterId id, int value)
        {
            _trust[id] = Math.Clamp(value, MinTrust, MaxTrust);
        }

        public int GetTrust(CharacterId id)
        {
            return _trust[id];
        }

        public bool MeetsThreshold(CharacterId id, int threshold)
        {
            return _trust[id] >= threshold;
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
