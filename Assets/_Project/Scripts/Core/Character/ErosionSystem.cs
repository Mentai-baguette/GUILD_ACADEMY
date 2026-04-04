using System;

namespace GuildAcademy.Core.Character
{
    public enum ErosionStage
    {
        Normal,     // 0-29%
        Unstable,   // 30-59%
        Dangerous,  // 60-89%
        Critical    // 90-100%
    }

    public class ErosionSystem
    {
        public const int MAX_EROSION = 100;
        public const int MIN_EROSION = 0;
        public const int UNSTABLE_THRESHOLD = 30;
        public const int DANGEROUS_THRESHOLD = 60;
        public const int CRITICAL_THRESHOLD = 90;

        private int _currentErosion;

        public int CurrentErosion => _currentErosion;
        public ErosionStage CurrentStage => GetStage(_currentErosion);

        public event Action<int, ErosionStage> OnErosionChanged;
        public event Action<ErosionStage> OnStageChanged;

        public ErosionSystem(int initialErosion = 0)
        {
            _currentErosion = Math.Clamp(initialErosion, MIN_EROSION, MAX_EROSION);
        }

        public void AddErosion(int amount)
        {
            if (amount <= 0) return;
            var oldStage = CurrentStage;
            _currentErosion = Math.Min(_currentErosion + amount, MAX_EROSION);
            OnErosionChanged?.Invoke(_currentErosion, CurrentStage);
            if (CurrentStage != oldStage)
                OnStageChanged?.Invoke(CurrentStage);
        }

        public void ReduceErosion(int amount)
        {
            if (amount <= 0) return;
            var oldStage = CurrentStage;
            _currentErosion = Math.Max(_currentErosion - amount, MIN_EROSION);
            OnErosionChanged?.Invoke(_currentErosion, CurrentStage);
            if (CurrentStage != oldStage)
                OnStageChanged?.Invoke(CurrentStage);
        }

        public void Purify(int basePower, int bondLevel)
        {
            // Purification scales with bond level: basePower * (1 + bondLevel * 0.5)
            int amount = (int)(basePower * (1.0f + bondLevel * 0.5f));
            ReduceErosion(amount);
        }

        public float GetAtkMultiplier()
        {
            return CurrentStage switch
            {
                ErosionStage.Normal => 1.0f,
                ErosionStage.Unstable => 1.15f,
                ErosionStage.Dangerous => 1.3f,
                ErosionStage.Critical => 1.5f,
                _ => 1.0f
            };
        }

        public float GetRampageChance()
        {
            return CurrentStage switch
            {
                ErosionStage.Normal => 0f,
                ErosionStage.Unstable => 0f,
                ErosionStage.Dangerous => 0.15f,
                ErosionStage.Critical => 0.4f,
                _ => 0f
            };
        }

        public void Reset()
        {
            _currentErosion = MIN_EROSION;
            OnErosionChanged?.Invoke(_currentErosion, CurrentStage);
        }

        private static ErosionStage GetStage(int erosion)
        {
            if (erosion >= CRITICAL_THRESHOLD) return ErosionStage.Critical;
            if (erosion >= DANGEROUS_THRESHOLD) return ErosionStage.Dangerous;
            if (erosion >= UNSTABLE_THRESHOLD) return ErosionStage.Unstable;
            return ErosionStage.Normal;
        }
    }
}
