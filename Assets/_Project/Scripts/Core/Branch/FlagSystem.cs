using System;
using System.Collections.Generic;
using System.Linq;

namespace GuildAcademy.Core.Branch
{
    public class FlagSystem
    {
        public static class Flags
        {
            public const string ShionPast = "flag_shion_past";
            public const string CarlosPlan = "flag_carlos_plan";
            public const string VesselTruth = "flag_vessel_truth";
            public const string AcademySecret = "flag_academy_secret";
            public const string SealMethod = "flag_seal_method";
            public const string DarkPowerRisk = "flag_dark_power_risk";
            public const string TrustBetrayal = "flag_trust_betrayal";
            public const string SalvationPath = "flag_salvation_path";
            public const string AcademyRefused = "academy_refused";
        }

        private static readonly string[] InfoFlagNames =
        {
            Flags.ShionPast,
            Flags.CarlosPlan,
            Flags.VesselTruth,
            Flags.AcademySecret,
            Flags.SealMethod,
            Flags.DarkPowerRisk,
            Flags.TrustBetrayal,
            Flags.SalvationPath
        };

        private readonly Dictionary<string, bool> _flags;

        public FlagSystem()
        {
            _flags = new Dictionary<string, bool>();
            foreach (var name in InfoFlagNames)
                _flags[name] = false;
            _flags[Flags.AcademyRefused] = false;
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
