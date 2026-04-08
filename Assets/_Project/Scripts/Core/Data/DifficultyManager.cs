using System;
using System.Collections.Generic;

namespace GuildAcademy.Core.Data
{
    public enum Difficulty { Easy, Normal, Hard, Nightmare }
    public enum SaveRestriction { Anywhere, DormOnly, Disabled }
    public enum DefeatPenalty { Retry, LastSave, ChapterStart }

    public class DifficultySettings
    {
        public float EnemyStatMultiplier { get; }
        public float ExpMultiplier { get; }
        public SaveRestriction SaveRule { get; }
        public DefeatPenalty MobDefeatPenalty { get; }
        public bool HasGoddessBlessing { get; }

        public DifficultySettings(
            float enemyStatMultiplier,
            float expMultiplier,
            SaveRestriction saveRule,
            DefeatPenalty mobDefeatPenalty,
            bool hasGoddessBlessing)
        {
            EnemyStatMultiplier = enemyStatMultiplier;
            ExpMultiplier = expMultiplier;
            SaveRule = saveRule;
            MobDefeatPenalty = mobDefeatPenalty;
            HasGoddessBlessing = hasGoddessBlessing;
        }
    }

    public class DifficultyManager
    {
        private static readonly Dictionary<Difficulty, DifficultySettings> SettingsMap =
            new Dictionary<Difficulty, DifficultySettings>
            {
                {
                    Difficulty.Easy,
                    new DifficultySettings(1.0f, 1.2f, SaveRestriction.Anywhere, DefeatPenalty.Retry, false)
                },
                {
                    Difficulty.Normal,
                    new DifficultySettings(1.5f, 1.0f, SaveRestriction.Anywhere, DefeatPenalty.Retry, false)
                },
                {
                    Difficulty.Hard,
                    new DifficultySettings(2.0f, 0.9f, SaveRestriction.DormOnly, DefeatPenalty.LastSave, false)
                },
                {
                    Difficulty.Nightmare,
                    new DifficultySettings(4.0f, 0.75f, SaveRestriction.Disabled, DefeatPenalty.ChapterStart, true)
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
                throw new InvalidOperationException("Cannot use Goddess Blessing.");
            }
            _goddessBlessingUsed = true;
        }

        public void ResetGoddessBlessing()
        {
            _goddessBlessingUsed = false;
        }

        public static DifficultySettings GetSettings(Difficulty difficulty)
        {
            return SettingsMap[difficulty];
        }
    }
}
