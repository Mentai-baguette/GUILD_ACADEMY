using NUnit.Framework;
using GuildAcademy.Core.Character;
using GuildAcademy.Core.Data;
using GuildAcademy.Tests.EditMode.TestHelpers;

namespace GuildAcademy.Tests.EditMode.Character
{
    [TestFixture]
    public class LevelSystemTests
    {
        private LevelSystem _levelSystem;
        private GrowthRates _defaultRates;

        [SetUp]
        public void SetUp()
        {
            _levelSystem = new LevelSystem();
            _defaultRates = new GrowthRates
            {
                Hp = 50, Mp = 30, Atk = 40, Def = 40,
                Int = 30, Res = 30, Agi = 35, Dex = 35
            };
        }

        // --- 必要EXP計算テスト ---

        [Test]
        public void GetExpToNextLevel_Lv1ToLv2_Returns20()
        {
            Assert.AreEqual(20, LevelSystem.GetExpToNextLevel(1));
        }

        [Test]
        public void GetExpToNextLevel_Lv9ToLv10_Returns500()
        {
            Assert.AreEqual(500, LevelSystem.GetExpToNextLevel(9));
        }

        [Test]
        public void GetExpToNextLevel_Lv99ToLv100_Returns50000()
        {
            Assert.AreEqual(50000, LevelSystem.GetExpToNextLevel(99));
        }

        [Test]
        public void GetExpToNextLevel_AtMaxLevel_ReturnsZero()
        {
            Assert.AreEqual(0, LevelSystem.GetExpToNextLevel(100));
        }

        // --- EXP加算+レベルアップテスト ---

        [Test]
        public void AddExp_EnoughForOneLevel_LevelsUp()
        {
            var stats = CreateStats(lv: 1);
            var random = new FixedRandom(99); // 成長なし

            var result = _levelSystem.AddExp(stats, _defaultRates, 20, random);

            Assert.AreEqual(2, result.NewLevel);
            Assert.AreEqual(2, stats.Lv);
            Assert.AreEqual(0, stats.Exp);
        }

        [Test]
        public void AddExp_NotEnoughExp_NoLevelUp()
        {
            var stats = CreateStats(lv: 1);
            var random = new FixedRandom(99);

            var result = _levelSystem.AddExp(stats, _defaultRates, 10, random);

            Assert.AreEqual(1, result.NewLevel);
            Assert.AreEqual(1, stats.Lv);
            Assert.AreEqual(10, stats.Exp);
        }

        [Test]
        public void AddExp_ExtraExpCarriesOver()
        {
            var stats = CreateStats(lv: 1);
            var random = new FixedRandom(99);

            var result = _levelSystem.AddExp(stats, _defaultRates, 25, random);

            Assert.AreEqual(2, result.NewLevel);
            Assert.AreEqual(5, stats.Exp); // 25 - 20 = 5
        }

        // --- 連続レベルアップテスト ---

        [Test]
        public void AddExp_LargeExp_MultipleLevelUps()
        {
            var stats = CreateStats(lv: 1);
            var random = new FixedRandom(99);

            // Lv1→2: 20, Lv2→3: 45 => 合計65で2レベルアップ
            var result = _levelSystem.AddExp(stats, _defaultRates, 65, random);

            Assert.AreEqual(3, result.NewLevel);
            Assert.AreEqual(3, stats.Lv);
            Assert.AreEqual(0, stats.Exp);
        }

        // --- Lv100上限テスト ---

        [Test]
        public void AddExp_AtMaxLevel_NoChange()
        {
            var stats = CreateStats(lv: 100);
            var random = new FixedRandom(99);

            var result = _levelSystem.AddExp(stats, _defaultRates, 99999, random);

            Assert.AreEqual(100, result.NewLevel);
            Assert.AreEqual(100, stats.Lv);
        }

        [Test]
        public void AddExp_ExcessExpAtMaxLevel_ExpSetToZero()
        {
            var stats = CreateStats(lv: 99);
            var random = new FixedRandom(99);

            // Lv99→100: 50000
            _levelSystem.AddExp(stats, _defaultRates, 999999, random);

            Assert.AreEqual(100, stats.Lv);
            Assert.AreEqual(0, stats.Exp);
        }

        // --- 成長率テスト ---

        [Test]
        public void AddExp_GrowthRate100_AllStatsIncrease()
        {
            var stats = CreateStats(lv: 1);
            var rates = new GrowthRates
            {
                Hp = 100, Mp = 100, Atk = 100, Def = 100,
                Int = 100, Res = 100, Agi = 100, Dex = 100
            };
            var random = new FixedRandom(0); // 0 < 100 → 全て成長

            int oldMaxHp = stats.MaxHp;
            int oldMaxMp = stats.MaxMp;
            int oldAtk = stats.Atk;
            int oldDef = stats.Def;
            int oldInt = stats.Int;
            int oldRes = stats.Res;
            int oldAgi = stats.Agi;
            int oldDex = stats.Dex;

            var result = _levelSystem.AddExp(stats, rates, 20, random);

            Assert.AreEqual(1, result.HpGain);
            Assert.AreEqual(1, result.MpGain);
            Assert.AreEqual(1, result.AtkGain);
            Assert.AreEqual(1, result.DefGain);
            Assert.AreEqual(1, result.IntGain);
            Assert.AreEqual(1, result.ResGain);
            Assert.AreEqual(1, result.AgiGain);
            Assert.AreEqual(1, result.DexGain);

            Assert.AreEqual(oldMaxHp + 1, stats.MaxHp);
            Assert.AreEqual(oldMaxMp + 1, stats.MaxMp);
            Assert.AreEqual(oldAtk + 1, stats.Atk);
            Assert.AreEqual(oldDef + 1, stats.Def);
            Assert.AreEqual(oldInt + 1, stats.Int);
            Assert.AreEqual(oldRes + 1, stats.Res);
            Assert.AreEqual(oldAgi + 1, stats.Agi);
            Assert.AreEqual(oldDex + 1, stats.Dex);
        }

        [Test]
        public void AddExp_GrowthRate0_NoStatsIncrease()
        {
            var stats = CreateStats(lv: 1);
            var rates = new GrowthRates
            {
                Hp = 0, Mp = 0, Atk = 0, Def = 0,
                Int = 0, Res = 0, Agi = 0, Dex = 0
            };
            var random = new FixedRandom(0);

            int oldMaxHp = stats.MaxHp;

            var result = _levelSystem.AddExp(stats, rates, 20, random);

            Assert.AreEqual(0, result.HpGain);
            Assert.AreEqual(0, result.MpGain);
            Assert.AreEqual(0, result.AtkGain);
            Assert.AreEqual(0, result.DefGain);
            Assert.AreEqual(0, result.IntGain);
            Assert.AreEqual(0, result.ResGain);
            Assert.AreEqual(0, result.AgiGain);
            Assert.AreEqual(0, result.DexGain);
            Assert.AreEqual(oldMaxHp, stats.MaxHp);
        }

        [Test]
        public void AddExp_GrowthDoesNotChangeCurrentHpMp()
        {
            var stats = CreateStats(lv: 1);
            var rates = new GrowthRates { Hp = 100, Mp = 100 };
            var random = new FixedRandom(0);

            int oldCurrentHp = stats.CurrentHp;
            int oldCurrentMp = stats.CurrentMp;

            _levelSystem.AddExp(stats, rates, 20, random);

            Assert.AreEqual(oldCurrentHp, stats.CurrentHp);
            Assert.AreEqual(oldCurrentMp, stats.CurrentMp);
        }

        // --- SP獲得テスト ---

        [Test]
        public void AddExp_LevelUpToEvenLevel_GainsSp()
        {
            var stats = CreateStats(lv: 1);
            var random = new FixedRandom(99);

            var result = _levelSystem.AddExp(stats, _defaultRates, 20, random);

            Assert.AreEqual(2, stats.Lv);
            Assert.AreEqual(1, result.SpGained);
            Assert.AreEqual(1, stats.Sp);
        }

        [Test]
        public void AddExp_LevelUpToOddLevel_NoSp()
        {
            var stats = CreateStats(lv: 2);
            var random = new FixedRandom(99);

            // Lv2→3: 45
            var result = _levelSystem.AddExp(stats, _defaultRates, 45, random);

            Assert.AreEqual(3, stats.Lv);
            Assert.AreEqual(0, result.SpGained);
            Assert.AreEqual(0, stats.Sp);
        }

        [Test]
        public void AddExp_MultipleLevelUps_AccumulatesSp()
        {
            var stats = CreateStats(lv: 1);
            var random = new FixedRandom(99);

            // Lv1→2: 20, Lv2→3: 45, Lv3→4: 80 => 合計145
            // Lv2(偶数)=+1SP, Lv3(奇数)=+0SP, Lv4(偶数)=+1SP => 計2SP
            var result = _levelSystem.AddExp(stats, _defaultRates, 145, random);

            Assert.AreEqual(4, stats.Lv);
            Assert.AreEqual(2, result.SpGained);
            Assert.AreEqual(2, stats.Sp);
        }

        // --- EXPカーブの累計テスト ---

        [Test]
        public void GetRequiredExp_Lv1_ReturnsZero()
        {
            Assert.AreEqual(0, LevelSystem.GetRequiredExp(1));
        }

        [Test]
        public void GetRequiredExp_Lv2_Returns20()
        {
            Assert.AreEqual(20, LevelSystem.GetRequiredExp(2));
        }

        [Test]
        public void GetRequiredExp_CumulativeIsConsistent()
        {
            // 累計EXP(Lv N) = 累計EXP(Lv N-1) + ExpToNextLevel(N-1)
            for (int lv = 2; lv <= 10; lv++)
            {
                int cumulative = LevelSystem.GetRequiredExp(lv);
                int previous = LevelSystem.GetRequiredExp(lv - 1);
                int step = LevelSystem.GetExpToNextLevel(lv - 1);
                Assert.AreEqual(previous + step, cumulative,
                    $"Cumulative EXP mismatch at Lv{lv}");
            }
        }

        // --- 0/負EXP境界テスト ---

        [Test]
        public void AddExp_ZeroExp_DoesNotChangeLevel()
        {
            var stats = CreateStats(lv: 1);
            stats.Exp = 7;
            var random = new FixedRandom(99);

            var result = _levelSystem.AddExp(stats, _defaultRates, 0, random);

            Assert.AreEqual(1, result.NewLevel);
            Assert.AreEqual(1, stats.Lv);
            Assert.AreEqual(7, stats.Exp);
        }

        [Test]
        public void AddExp_NegativeExp_DoesNotChangeLevel()
        {
            var stats = CreateStats(lv: 1);
            stats.Exp = 7;
            var random = new FixedRandom(99);

            var result = _levelSystem.AddExp(stats, _defaultRates, -5, random);

            Assert.AreEqual(1, result.NewLevel);
            Assert.AreEqual(1, stats.Lv);
            Assert.AreEqual(7, stats.Exp);
        }

        // --- ヘルパー ---

        private CharacterStats CreateStats(int lv = 1)
        {
            return new CharacterStats("TestChar", 100, 50, 10, 10, 10, lv: lv);
        }
    }
}
