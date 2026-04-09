using GuildAcademy.Core.Battle;

namespace GuildAcademy.Core.Character
{
    public enum ErosionStage { Normal, Unstable, Dangerous, Critical }

    public class ErosionSystem
    {
        public const float MaxErosion = 100f;

        // 通常時の閾値
        public const float NormalThreshold = 0f;
        public const float UnstableThreshold = 25f;
        public const float DangerousThreshold = 50f;
        public const float CriticalThreshold = 75f;

        // エルト撃破後の閾値
        public const float UnstableThresholdEnhanced = 35f;
        public const float DangerousThresholdEnhanced = 60f;
        public const float CriticalThresholdEnhanced = 85f;

        // ATK倍率
        public const float NormalAtkMultiplier = 1.0f;
        public const float UnstableAtkMultiplier = 1.3f;
        public const float DangerousAtkMultiplier = 1.6f;
        public const float CriticalAtkMultiplier = 2.0f;

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
            if (_currentErosion > MaxErosion)
                _currentErosion = MaxErosion;
        }

        /// <summary>バトル中浄化: 1段階下げる（1バトル1回制限）</summary>
        /// <returns>浄化が実行された場合true</returns>
        public bool PurifyInBattle()
        {
            if (_purifiedThisBattle) return false;

            var currentStage = CurrentStage;
            if (currentStage == ErosionStage.Normal) return false;

            // 1段階下の閾値ちょうどに設定
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
            if (random == null) throw new System.ArgumentNullException(nameof(random));
            if (CurrentStage != ErosionStage.Critical) return false;
            if (!usedDarkSkill) return false;

            float criticalThreshold = _eltDefeated ? CriticalThresholdEnhanced : CriticalThreshold;
            int rampageChance = (int)((_currentErosion - criticalThreshold) * 2f);
            rampageChance = System.Math.Max(0, System.Math.Min(100, rampageChance));

            int roll = random.Range(0, 100);
            return roll < rampageChance;
        }

        private ErosionStage DetermineStage(float erosion)
        {
            float criticalThreshold = _eltDefeated ? CriticalThresholdEnhanced : CriticalThreshold;
            float dangerousThreshold = _eltDefeated ? DangerousThresholdEnhanced : DangerousThreshold;
            float unstableThreshold = _eltDefeated ? UnstableThresholdEnhanced : UnstableThreshold;

            if (erosion >= criticalThreshold) return ErosionStage.Critical;
            if (erosion >= dangerousThreshold) return ErosionStage.Dangerous;
            if (erosion >= unstableThreshold) return ErosionStage.Unstable;
            return ErosionStage.Normal;
        }

        private float GetAtkMultiplier(ErosionStage stage)
        {
            switch (stage)
            {
                case ErosionStage.Normal: return NormalAtkMultiplier;
                case ErosionStage.Unstable: return UnstableAtkMultiplier;
                case ErosionStage.Dangerous: return DangerousAtkMultiplier;
                case ErosionStage.Critical: return CriticalAtkMultiplier;
                default: return NormalAtkMultiplier;
            }
        }

        private float GetThreshold(ErosionStage stage)
        {
            switch (stage)
            {
                case ErosionStage.Normal:
                    return NormalThreshold;
                case ErosionStage.Unstable:
                    return _eltDefeated ? UnstableThresholdEnhanced : UnstableThreshold;
                case ErosionStage.Dangerous:
                    return _eltDefeated ? DangerousThresholdEnhanced : DangerousThreshold;
                case ErosionStage.Critical:
                    return _eltDefeated ? CriticalThresholdEnhanced : CriticalThreshold;
                default:
                    return NormalThreshold;
            }
        }
    }
}
