using System.Collections.Generic;
using System.Linq;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Battle
{
    public class ATBSystem
    {
        public const float GaugeRate = 10.0f;
        public const float MaxGauge = 100.0f;

        private readonly List<CombatantState> _combatants = new List<CombatantState>();

        public void AddCombatant(CharacterStats stats)
        {
            _combatants.Add(new CombatantState { Stats = stats, Gauge = 0f });
        }

        public void RemoveCombatant(CharacterStats stats)
        {
            _combatants.RemoveAll(c => c.Stats == stats);
        }

        public void Tick(float deltaTime)
        {
            foreach (var combatant in _combatants)
            {
                if (combatant.Gauge < MaxGauge)
                {
                    combatant.Gauge += combatant.Stats.Spd * deltaTime * GaugeRate;
                    if (combatant.Gauge > MaxGauge)
                        combatant.Gauge = MaxGauge;
                }
            }
        }

        public List<CharacterStats> GetReadyCharacters()
        {
            return _combatants
                .Where(c => c.Gauge >= MaxGauge)
                .OrderByDescending(c => c.Stats.Spd)
                .Select(c => c.Stats)
                .ToList();
        }

        public float GetGauge(CharacterStats stats)
        {
            var combatant = _combatants.FirstOrDefault(c => c.Stats == stats);
            return combatant?.Gauge ?? 0f;
        }

        public void ResetGauge(CharacterStats stats)
        {
            var combatant = _combatants.FirstOrDefault(c => c.Stats == stats);
            if (combatant != null)
                combatant.Gauge = 0f;
        }

        private class CombatantState
        {
            public CharacterStats Stats;
            public float Gauge;
        }
    }
}
