using System.Collections.Generic;
using System.Linq;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Battle
{
    public enum StatusEffectType
    {
        // デバフ
        Poison,
        Paralysis,
        Blindness,
        Silence,
        Slow,
        Stop,
        // バフ
        AtkUp,
        DefUp,
        Haste,
        Regen,
        // ステデバフ
        AtkDown,
        DefDown
    }

    public class StatusEffect
    {
        public StatusEffectType Type { get; set; }
        public int RemainingTurns { get; set; }
        public int StackCount { get; set; }

        public StatusEffect(StatusEffectType type, int remainingTurns, int stackCount = 1)
        {
            Type = type;
            RemainingTurns = remainingTurns;
            StackCount = stackCount;
        }
    }

    public class StatusEffectSystem
    {
        private const int DefaultDuration = 3;
        private const int StopDuration = 2;
        private const int PermanentDuration = -1; // 毒はバトル終了まで

        private readonly Dictionary<CharacterStats, List<StatusEffect>> _effects = new();

        /// <summary>
        /// 状態異常の付与（耐性判定込み）
        /// ボスはストップ完全無効。他の状態異常は耐性値で成功率が低下する
        /// </summary>
        public bool TryApply(CharacterStats target, StatusEffectType type,
            int baseChancePercent, float resistanceValue, IRandom random,
            bool isEnhanced = false)
        {
            // ストップはボス完全無効（耐性1.0以上）
            if (type == StatusEffectType.Stop && resistanceValue >= 1.0f)
                return false;

            // 他の状態異常は耐性で確率低下（1.0でも0%にはなるが、ストップとは別ルート）
            int effectiveChance = (int)(baseChancePercent * (1f - resistanceValue));
            if (effectiveChance <= 0) return false;

            int roll = random.Range(0, 100);
            if (roll >= effectiveChance)
                return false;

            var effects = GetOrCreateEffectList(target);
            var existing = effects.FirstOrDefault(e => e.Type == type);

            if (existing != null)
            {
                // エンハンス時のみ2段階重ねがけ可能
                if (isEnhanced && existing.StackCount < 2)
                {
                    existing.StackCount = 2;
                    existing.RemainingTurns = GetDuration(type);
                }
                else
                {
                    // 重ねがけ不可：持続ターンをリセット
                    existing.RemainingTurns = GetDuration(type);
                }
                return true;
            }

            int duration = GetDuration(type);
            int stackCount = 1;
            effects.Add(new StatusEffect(type, duration, stackCount));
            return true;
        }

        /// <summary>
        /// 状態異常の解除
        /// </summary>
        public void Remove(CharacterStats target, StatusEffectType type)
        {
            if (!_effects.TryGetValue(target, out var effects))
                return;

            effects.RemoveAll(e => e.Type == type);
        }

        /// <summary>
        /// 全解除
        /// </summary>
        public void RemoveAll(CharacterStats target)
        {
            if (_effects.ContainsKey(target))
                _effects[target].Clear();
        }

        /// <summary>
        /// ターン経過処理：毒ダメージ、リジェネ回復、持続ターン減少
        /// </summary>
        public void OnTurnEnd(CharacterStats target)
        {
            if (!_effects.TryGetValue(target, out var effects))
                return;

            foreach (var effect in effects.ToList())
            {
                switch (effect.Type)
                {
                    case StatusEffectType.Poison:
                        // HP8%ダメージ（スタック数に応じて）
                        int poisonDamage = (int)(target.MaxHp * 0.08f) * effect.StackCount;
                        target.CurrentHp = System.Math.Max(0, target.CurrentHp - poisonDamage);
                        break;

                    case StatusEffectType.Regen:
                        // HP3%回復（スタック数に応じて）
                        int regenAmount = (int)(target.MaxHp * 0.03f) * effect.StackCount;
                        target.CurrentHp = System.Math.Min(target.MaxHp, target.CurrentHp + regenAmount);
                        break;
                }

                // 毒は永続（RemainingTurns == -1）
                if (effect.RemainingTurns == PermanentDuration)
                    continue;

                effect.RemainingTurns--;
                if (effect.RemainingTurns <= 0)
                    effects.Remove(effect);
            }
        }

        /// <summary>
        /// 状態異常判定
        /// </summary>
        public bool HasEffect(CharacterStats target, StatusEffectType type)
        {
            if (!_effects.TryGetValue(target, out var effects))
                return false;

            return effects.Any(e => e.Type == type);
        }

        /// <summary>
        /// 状態異常一覧取得
        /// </summary>
        public List<StatusEffect> GetEffects(CharacterStats target)
        {
            if (!_effects.TryGetValue(target, out var effects))
                return new List<StatusEffect>();

            return new List<StatusEffect>(effects);
        }

        /// <summary>
        /// ATK倍率取得（ATKアップ/ダウンの合算）
        /// </summary>
        public float GetAtkMultiplier(CharacterStats target)
        {
            float multiplier = 1.0f;

            if (!_effects.TryGetValue(target, out var effects))
                return multiplier;

            var atkUp = effects.FirstOrDefault(e => e.Type == StatusEffectType.AtkUp);
            if (atkUp != null)
            {
                for (int i = 0; i < atkUp.StackCount; i++)
                    multiplier *= 1.2f;
            }

            var atkDown = effects.FirstOrDefault(e => e.Type == StatusEffectType.AtkDown);
            if (atkDown != null)
            {
                for (int i = 0; i < atkDown.StackCount; i++)
                    multiplier *= 0.7f;
            }

            return multiplier;
        }

        /// <summary>
        /// DEF倍率取得
        /// </summary>
        public float GetDefMultiplier(CharacterStats target)
        {
            float multiplier = 1.0f;

            if (!_effects.TryGetValue(target, out var effects))
                return multiplier;

            var defUp = effects.FirstOrDefault(e => e.Type == StatusEffectType.DefUp);
            if (defUp != null)
            {
                for (int i = 0; i < defUp.StackCount; i++)
                    multiplier *= 1.2f;
            }

            var defDown = effects.FirstOrDefault(e => e.Type == StatusEffectType.DefDown);
            if (defDown != null)
            {
                for (int i = 0; i < defDown.StackCount; i++)
                    multiplier *= 0.7f;
            }

            return multiplier;
        }

        /// <summary>
        /// ATB速度倍率取得（ヘイスト/スロウ/ストップ）
        /// </summary>
        public float GetAtbSpeedMultiplier(CharacterStats target)
        {
            if (!_effects.TryGetValue(target, out var effects))
                return 1.0f;

            // ストップ時はATB停止（0倍）
            if (effects.Any(e => e.Type == StatusEffectType.Stop))
                return 0f;

            float multiplier = 1.0f;

            var haste = effects.FirstOrDefault(e => e.Type == StatusEffectType.Haste);
            if (haste != null)
            {
                for (int i = 0; i < haste.StackCount; i++)
                    multiplier *= 1.3f;
            }

            var slow = effects.FirstOrDefault(e => e.Type == StatusEffectType.Slow);
            if (slow != null)
            {
                for (int i = 0; i < slow.StackCount; i++)
                    multiplier *= 0.5f;
            }

            return multiplier;
        }

        /// <summary>
        /// 麻痺判定（30%行動不能）
        /// </summary>
        public bool IsActionBlocked(CharacterStats target, IRandom random)
        {
            if (!HasEffect(target, StatusEffectType.Paralysis))
                return false;

            return random.Range(0, 100) < 30;
        }

        /// <summary>
        /// 沈黙判定（スキル不可）
        /// </summary>
        public bool IsSkillBlocked(CharacterStats target)
        {
            return HasEffect(target, StatusEffectType.Silence);
        }

        /// <summary>
        /// 暗闇時の物理命中修正値（-50%）
        /// </summary>
        public float GetPhysicalHitModifier(CharacterStats target)
        {
            if (HasEffect(target, StatusEffectType.Blindness))
                return -0.5f;

            return 0f;
        }

        /// <summary>
        /// バトル終了時クリア
        /// </summary>
        public void OnBattleEnd()
        {
            _effects.Clear();
        }

        private List<StatusEffect> GetOrCreateEffectList(CharacterStats target)
        {
            if (!_effects.TryGetValue(target, out var effects))
            {
                effects = new List<StatusEffect>();
                _effects[target] = effects;
            }
            return effects;
        }

        private static int GetDuration(StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.Poison:
                    return PermanentDuration; // バトル終了まで
                case StatusEffectType.Stop:
                    return StopDuration; // 2ターン
                default:
                    return DefaultDuration; // 3ターン
            }
        }
    }
}
