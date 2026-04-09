using System;
using System.Collections.Generic;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Battle
{
    public class BossPhaseData
    {
        public int PhaseNumber { get; set; }
        public float HpThresholdPercent { get; set; }
        public CharacterStats Stats { get; set; }
        public List<SkillData> Skills { get; set; }

        public BossPhaseData(int phaseNumber, float hpThresholdPercent, CharacterStats stats, List<SkillData> skills = null)
        {
            PhaseNumber = phaseNumber;
            HpThresholdPercent = hpThresholdPercent;
            Stats = stats ?? throw new ArgumentNullException(nameof(stats));
            Skills = skills ?? new List<SkillData>();
        }
    }

    public class BossPhaseManager
    {
        private readonly List<BossPhaseData> _phases = new List<BossPhaseData>();
        private int _currentPhaseIndex;

        public int CurrentPhase => _currentPhaseIndex < _phases.Count ? _phases[_currentPhaseIndex].PhaseNumber : 0;
        public BossPhaseData CurrentPhaseData => _currentPhaseIndex < _phases.Count ? _phases[_currentPhaseIndex] : null;
        public int TotalPhases => _phases.Count;

        public event Action<int, BossPhaseData> OnPhaseChanged;

        public void AddPhase(BossPhaseData phase)
        {
            if (phase == null) throw new ArgumentNullException(nameof(phase));
            _phases.Add(phase);
            // Sort by HP threshold descending (highest HP% = first phase)
            _phases.Sort((a, b) => b.HpThresholdPercent.CompareTo(a.HpThresholdPercent));
        }

        /// <summary>
        /// Check if a phase transition should occur based on current HP%.
        /// Returns true if phase changed.
        /// </summary>
        public bool CheckPhaseTransition(CharacterStats boss)
        {
            if (boss == null || _phases.Count == 0) return false;

            float hpPercent = boss.MaxHp > 0 ? (float)boss.CurrentHp / boss.MaxHp * 100f : 0f;

            for (int i = _phases.Count - 1; i > _currentPhaseIndex; i--)
            {
                if (hpPercent <= _phases[i].HpThresholdPercent)
                {
                    int oldPhase = _currentPhaseIndex;
                    _currentPhaseIndex = i;
                    OnPhaseChanged?.Invoke(_phases[i].PhaseNumber, _phases[i]);
                    return true;
                }
            }

            return false;
        }

        public void Reset()
        {
            _currentPhaseIndex = 0;
        }
    }
}
