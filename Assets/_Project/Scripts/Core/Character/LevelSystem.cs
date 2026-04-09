using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Character
{
    public class GrowthRates
    {
        public int Hp { get; set; }  // 0-100の成長率(%)
        public int Mp { get; set; }
        public int Atk { get; set; }
        public int Def { get; set; }
        public int Int { get; set; }
        public int Res { get; set; }
        public int Agi { get; set; }
        public int Dex { get; set; }
        // LUKは成長しない（固定）
    }

    public class LevelUpResult
    {
        public int NewLevel { get; set; }
        public int HpGain { get; set; }
        public int MpGain { get; set; }
        public int AtkGain { get; set; }
        public int DefGain { get; set; }
        public int IntGain { get; set; }
        public int ResGain { get; set; }
        public int AgiGain { get; set; }
        public int DexGain { get; set; }
        public int SpGained { get; set; } // 連続LvUP時は累積（偶数Lvごとに+1）
    }

    public class LevelSystem
    {
        public const int MAX_LEVEL = 100;
        public const int EXP_MULTIPLIER = 5;

        /// <summary>
        /// 指定レベルに到達するために必要な累計EXP。
        /// Lv1は0、Lv2は20、LvNはΣ(lv=2..N) lv*lv*5。
        /// </summary>
        public static int GetRequiredExp(int level)
        {
            if (level <= 1) return 0;

            int total = 0;
            for (int lv = 2; lv <= level; lv++)
            {
                total += lv * lv * EXP_MULTIPLIER;
            }
            return total;
        }

        /// <summary>
        /// 現在のレベルから次のレベルに上がるために必要なEXP。
        /// </summary>
        public static int GetExpToNextLevel(int currentLevel)
        {
            if (currentLevel >= MAX_LEVEL) return 0;

            int nextLevel = currentLevel + 1;
            return nextLevel * nextLevel * EXP_MULTIPLIER;
        }

        /// <summary>
        /// EXPを加算し、レベルアップがあれば成長処理を行う。
        /// </summary>
        public LevelUpResult AddExp(CharacterStats stats, GrowthRates rates, int expGained, IRandom random)
        {
            var result = new LevelUpResult();

            if (stats.Lv >= MAX_LEVEL)
            {
                result.NewLevel = stats.Lv;
                return result;
            }

            if (expGained <= 0)
            {
                result.NewLevel = stats.Lv;
                return result;
            }

            stats.Exp += expGained;

            while (stats.Lv < MAX_LEVEL)
            {
                int needed = GetExpToNextLevel(stats.Lv);
                if (stats.Exp < needed) break;

                stats.Exp -= needed;
                stats.Lv++;

                ApplyGrowth(stats, rates, result, random);

                int sp = CalculateSpGain(stats.Lv);
                result.SpGained += sp;
                stats.Sp += sp;
            }

            // レベル上限に達した場合、余剰EXPを0にする
            if (stats.Lv >= MAX_LEVEL)
            {
                stats.Exp = 0;
            }

            result.NewLevel = stats.Lv;
            return result;
        }

        /// <summary>
        /// レベルアップ時のステータス成長（成長率に基づく確率判定）。
        /// </summary>
        private void ApplyGrowth(CharacterStats stats, GrowthRates rates, LevelUpResult result, IRandom random)
        {
            if (random.Range(0, 100) < rates.Hp)
            {
                stats.MaxHp++;
                result.HpGain++;
            }
            if (random.Range(0, 100) < rates.Mp)
            {
                stats.MaxMp++;
                result.MpGain++;
            }
            if (random.Range(0, 100) < rates.Atk)
            {
                stats.Atk++;
                result.AtkGain++;
            }
            if (random.Range(0, 100) < rates.Def)
            {
                stats.Def++;
                result.DefGain++;
            }
            if (random.Range(0, 100) < rates.Int)
            {
                stats.Int++;
                result.IntGain++;
            }
            if (random.Range(0, 100) < rates.Res)
            {
                stats.Res++;
                result.ResGain++;
            }
            if (random.Range(0, 100) < rates.Agi)
            {
                stats.Agi++;
                result.AgiGain++;
            }
            if (random.Range(0, 100) < rates.Dex)
            {
                stats.Dex++;
                result.DexGain++;
            }
        }

        /// <summary>
        /// SP判定：偶数レベルで+1SP。
        /// </summary>
        private int CalculateSpGain(int newLevel)
        {
            return (newLevel % 2 == 0) ? 1 : 0;
        }
    }
}
