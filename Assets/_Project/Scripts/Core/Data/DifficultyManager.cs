using System;
using System.Collections.Generic;

namespace GuildAcademy.Core.Data
{
    public enum Difficulty { Easy, Normal, Hard, Nightmare }
    public enum SaveRestriction { Anywhere, DormOnly, SavePointOnly, Disabled }
    public enum DefeatPenalty { Retry, RetryWithHp1, LastSave, ChapterStart }

    public class DifficultySettings
    {
        public float EnemyStatMultiplier { get; }
        public float ExpMultiplier { get; }
        public SaveRestriction SaveRule { get; }
        public DefeatPenalty MobDefeatPenalty { get; }
        public bool HasGoddessBlessing { get; }
        public bool HasAutoSave { get; }
        public bool HasSuspendSave { get; }

        public DifficultySettings(
            float enemyStatMultiplier,
            float expMultiplier,
            SaveRestriction saveRule,
            DefeatPenalty mobDefeatPenalty,
            bool hasGoddessBlessing,
            bool hasAutoSave,
            bool hasSuspendSave)
        {
            EnemyStatMultiplier = enemyStatMultiplier;
            ExpMultiplier = expMultiplier;
            SaveRule = saveRule;
            MobDefeatPenalty = mobDefeatPenalty;
            HasGoddessBlessing = hasGoddessBlessing;
            HasAutoSave = hasAutoSave;
            HasSuspendSave = hasSuspendSave;
        }
    }

    /// <summary>
    /// Manages difficulty settings for the game.
    /// Note: Boss defeat always triggers END6 regardless of difficulty.
    /// This is handled by EndingResolver, not DifficultyManager.
    /// </summary>
    public class DifficultyManager
    {
        private static readonly Dictionary<Difficulty, DifficultySettings> SettingsMap =
            new Dictionary<Difficulty, DifficultySettings>
            {
                {
                    Difficulty.Easy,
                    new DifficultySettings(1.0f, 1.2f, SaveRestriction.Anywhere, DefeatPenalty.RetryWithHp1, false, true, true)
                },
                {
                    Difficulty.Normal,
                    new DifficultySettings(1.5f, 1.0f, SaveRestriction.Anywhere, DefeatPenalty.Retry, false, true, true)
                },
                {
                    Difficulty.Hard,
                    new DifficultySettings(2.0f, 0.9f, SaveRestriction.DormOnly, DefeatPenalty.LastSave, false, false, true)
                },
                {
                    Difficulty.Nightmare,
                    new DifficultySettings(4.0f, 0.75f, SaveRestriction.Disabled, DefeatPenalty.ChapterStart, true, false, true)
                }
            };

        public Difficulty CurrentDifficulty { get; }
        public DifficultySettings CurrentSettings { get; }
        private bool _goddessBlessingUsed;

        public DifficultyManager(Difficulty difficulty)
        {
            CurrentDifficulty = difficulty;
            CurrentSettings = GetSettings(difficulty);
            _goddessBlessingUsed = false;
        }

        public bool CanUseGoddessBlessing()
        {
            return CurrentSettings.HasGoddessBlessing && !_goddessBlessingUsed;
        }

        public void UseGoddessBlessing()
        {
            if (!CanUseGoddessBlessing())
            {
                throw new InvalidOperationException(
                    $"Cannot use Goddess Blessing. CurrentDifficulty={CurrentDifficulty}, " +
                    $"HasGoddessBlessing={CurrentSettings.HasGoddessBlessing}, " +
                    $"AlreadyUsed={_goddessBlessingUsed}.");
            }
            _goddessBlessingUsed = true;
        }

        public void ResetGoddessBlessing()
        {
            _goddessBlessingUsed = false;
        }

        public static DifficultySettings GetSettings(Difficulty difficulty)
        {
            if (SettingsMap.TryGetValue(difficulty, out var settings))
                return settings;
            throw new System.ArgumentOutOfRangeException(nameof(difficulty), difficulty, "Unsupported difficulty.");
        }
    }
}
