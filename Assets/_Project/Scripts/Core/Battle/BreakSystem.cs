using System;
using System.Collections.Generic;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Battle
{
    public class BreakSystem
    {
        public const int WEAK_HIT_VALUE = 25;
        public const int NORMAL_HIT_VALUE = 5;
        public const int BREAK_DURATION = 2;
        public const int DEFAULT_MAX = 100;

        public event Action<CharacterStats> OnBreakTriggered;
        public event Action<CharacterStats> OnBreakRecovered;

        private readonly Dictionary<CharacterStats, BreakState> _states =
            new Dictionary<CharacterStats, BreakState>();

        public void Register(CharacterStats stats, int breakMax = DEFAULT_MAX)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));

            _states[stats] = new BreakState
            {
                Gauge = 0,
                Max = breakMax,
                IsBroken = false,
                RemainingTurns = 0
            };
        }

        public bool ApplyHit(CharacterStats target, bool isWeakElement)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            if (!_states.TryGetValue(target, out var state)) return false;
            if (state.IsBroken) return false;

            state.Gauge += isWeakElement ? WEAK_HIT_VALUE : NORMAL_HIT_VALUE;

            if (state.Gauge >= state.Max)
            {
                state.Gauge = state.Max;
                state.IsBroken = true;
                state.RemainingTurns = BREAK_DURATION;
                OnBreakTriggered?.Invoke(target);
                return true;
            }

            return false;
        }

        public bool IsBreaking(CharacterStats target)
        {
            if (target == null) return false;
            return _states.TryGetValue(target, out var state) && state.IsBroken;
        }

        public float GetBreakGaugePercent(CharacterStats target)
        {
            if (target == null) return 0f;
            if (!_states.TryGetValue(target, out var state)) return 0f;
            return state.Max > 0 ? (float)state.Gauge / state.Max : 0f;
        }

        public void TickBreakRecovery(CharacterStats target)
        {
            if (target == null) return;
            if (!_states.TryGetValue(target, out var state)) return;
            if (!state.IsBroken) return;

            state.RemainingTurns--;

            if (state.RemainingTurns <= 0)
            {
                state.IsBroken = false;
                state.Gauge = 0;
                state.RemainingTurns = 0;
                OnBreakRecovered?.Invoke(target);
            }
        }

        public void Reset(CharacterStats target)
        {
            if (target == null) return;
            if (!_states.TryGetValue(target, out var state)) return;

            state.Gauge = 0;
            state.IsBroken = false;
            state.RemainingTurns = 0;
        }

        private class BreakState
        {
            public int Gauge;
            public int Max;
            public bool IsBroken;
            public int RemainingTurns;
        }
    }
}
