using System;
using System.Collections.Generic;
using System.Linq;

namespace GuildAcademy.Core.Branch
{
    public class FlagSystem
    {
        private static readonly string[] InfoFlagNames =
        {
            "flag_shion_past",
            "flag_carlos_plan",
            "flag_vessel_truth",
            "flag_academy_secret",
            "flag_seal_method",
            "flag_dark_power_risk",
            "flag_trust_betrayal",
            "flag_salvation_path"
        };

        private const string AcademyRefusedFlag = "academy_refused";

        private readonly Dictionary<string, bool> _flags;

        public FlagSystem()
        {
            _flags = new Dictionary<string, bool>();
            foreach (var name in InfoFlagNames)
                _flags[name] = false;
            _flags[AcademyRefusedFlag] = false;
        }

        public void Set(string flagName, bool value)
        {
            if (!_flags.ContainsKey(flagName))
                throw new ArgumentException($"Unknown flag: {flagName}");
            _flags[flagName] = value;
        }

        public bool Get(string flagName)
        {
            if (!_flags.ContainsKey(flagName))
                throw new ArgumentException($"Unknown flag: {flagName}");
            return _flags[flagName];
        }

        public int GetActiveCount()
        {
            return InfoFlagNames.Count(name => _flags[name]);
        }

        public bool AreAllSet()
        {
            return InfoFlagNames.All(name => _flags[name]);
        }

        public void Reset()
        {
            foreach (var key in _flags.Keys.ToList())
                _flags[key] = false;
        }
    }
}
