using GuildAcademy.Core.Battle;

namespace GuildAcademy.Core.Character
{
    public enum ErosionStage { Normal, Unstable, Dangerous, Critical }

    public class ErosionSystem
    {
        public const float MAX_EROSION = 100f;

        // 通常時の閾値
        public const float NORMAL_THRESHOLD = 0f;
        public const float UNSTABLE_THRESHOLD = 25f;
        public const float DANGEROUS_THRESHOLD = 50f;
        public const float CRITICAL_THRESHOLD = 75f;

        // エルト撃破後の閾値
        public const float UNSTABLE_THRESHOLD_ENHANCED = 35f;
        public const float DANGEROUS_THRESHOLD_ENHANCED = 60f;
        public const float CRITICAL_THRESHOLD_ENHANCED = 85f;

        // ATK倍率
        public const float NORMAL_ATK_MULTIPLIER = 1.0f;
        public const float UNSTABLE_ATK_MULTIPLIER = 1.3f;
        public const float DANGEROUS_ATK_MULTIPLIER = 1.6f;
        public const float CRITICAL_ATK_MULTIPLIER = 2.0f;

        private float _currentErosion;
        private bool _eltDefeated;
        private bool _purifiedThisBattle;

        public float CurrentErosion => _currentErosion;
        public ErosionStage CurrentStage => DetermineStage(_currentErosion);
        public float AtkMultiplier => GetAtkMultiplier(CurrentStage);
        public bool IsEltDefeated => _eltDefeated;

        /// <summary>侵蝕値を増加させる（0-100にクランプ）</summary>
        public void AddErosion(float amount)
        {
            if (amount <= 0f) return;
            _currentErosion += amount;
            if (_currentErosion > MAX_EROSION)
                _currentErosion = MAX_EROSION;
        }

        /// <summary>バトル中浄化: 1段階下げる（1バトル1回制限）</summary>
        /// <returns>浄化が実行された場合true</returns>
        public bool PurifyInBattle()
        {
            if (_purifiedThisBattle) return false;

            var currentStage = CurrentStage;
            if (currentStage == ErosionStage.Normal) return false;

            // 1段階下の閾値の直下に設定
            var targetStage = (ErosionStage)((int)currentStage - 1);
            float targetThreshold = GetThreshold(targetStage);
            _currentErosion = targetThreshold;

            _purifiedThisBattle = true;
            return true;
        }

        /// <summary>フィールド浄化: 指定値だけ侵蝕を減少させる</summary>
        public void PurifyField(float amount)
        {
            if (amount <= 0f) return;
            _currentErosion -= amount;
            if (_currentErosion < 0f)
                _currentErosion = 0f;
        }

        /// <summary>バトル開始時に浄化済みフラグをリセット</summary>
        public void OnBattleStart()
        {
            _purifiedThisBattle = false;
        }

        /// <summary>エルト撃破フラグを設定（閾値が緩和される）</summary>
        public void SetEltDefeated()
        {
            _eltDefeated = true;
        }

        /// <summary>
        /// 暴走判定（Critical時かつ闇スキル使用時に呼ぶ）
        /// 暴走確率 = (侵蝕値 - Critical閾値) × 2%
        /// 通常時: 75%で0%、100%で50%
        /// エルト撃破後: 85%で0%、100%で30%
        /// </summary>
        /// <param name="random">乱数生成インターフェース</param>
        /// <param name="usedDarkSkill">闇スキルを使用したかどうか</param>
        /// <returns>暴走する場合true</returns>
        public bool CheckRampage(IRandom random, bool usedDarkSkill = true)
        {
            if (CurrentStage != ErosionStage.Critical) return false;
            if (!usedDarkSkill) return false;

            float criticalThreshold = _eltDefeated ? CRITICAL_THRESHOLD_ENHANCED : CRITICAL_THRESHOLD;
            int rampageChance = (int)((_currentErosion - criticalThreshold) * 2f);
            rampageChance = System.Math.Max(0, System.Math.Min(100, rampageChance));

            int roll = random.Range(0, 100);
            return roll < rampageChance;
        }

        private ErosionStage DetermineStage(float erosion)
        {
            float criticalThreshold = _eltDefeated ? CRITICAL_THRESHOLD_ENHANCED : CRITICAL_THRESHOLD;
            float dangerousThreshold = _eltDefeated ? DANGEROUS_THRESHOLD_ENHANCED : DANGEROUS_THRESHOLD;
            float unstableThreshold = _eltDefeated ? UNSTABLE_THRESHOLD_ENHANCED : UNSTABLE_THRESHOLD;

            if (erosion >= criticalThreshold) return ErosionStage.Critical;
            if (erosion >= dangerousThreshold) return ErosionStage.Dangerous;
            if (erosion >= unstableThreshold) return ErosionStage.Unstable;
            return ErosionStage.Normal;
        }

        private float GetAtkMultiplier(ErosionStage stage)
        {
            switch (stage)
            {
                case ErosionStage.Normal: return NORMAL_ATK_MULTIPLIER;
                case ErosionStage.Unstable: return UNSTABLE_ATK_MULTIPLIER;
                case ErosionStage.Dangerous: return DANGEROUS_ATK_MULTIPLIER;
                case ErosionStage.Critical: return CRITICAL_ATK_MULTIPLIER;
                default: return NORMAL_ATK_MULTIPLIER;
            }
        }

        private float GetThreshold(ErosionStage stage)
        {
            switch (stage)
            {
                case ErosionStage.Normal:
                    return NORMAL_THRESHOLD;
                case ErosionStage.Unstable:
                    return _eltDefeated ? UNSTABLE_THRESHOLD_ENHANCED : UNSTABLE_THRESHOLD;
                case ErosionStage.Dangerous:
                    return _eltDefeated ? DANGEROUS_THRESHOLD_ENHANCED : DANGEROUS_THRESHOLD;
                case ErosionStage.Critical:
                    return _eltDefeated ? CRITICAL_THRESHOLD_ENHANCED : CRITICAL_THRESHOLD;
                default:
                    return NORMAL_THRESHOLD;
            }
        }
    }
}
