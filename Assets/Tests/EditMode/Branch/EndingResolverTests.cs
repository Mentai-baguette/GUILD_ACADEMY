using System;
using NUnit.Framework;
using GuildAcademy.Core.Branch;
using GuildAcademy.Core.Data;
using F = GuildAcademy.Core.Branch.FlagSystem.Flags;

namespace GuildAcademy.Tests.EditMode.Branch
{
    [TestFixture]
    public class EndingResolverTests
    {
        private FlagSystem _flags;
        private TrustSystem _trust;

        [SetUp]
        public void SetUp()
        {
            _flags = new FlagSystem();
            _trust = new TrustSystem();
        }

        private EndingContext CreateContext(BattlePhase phase = BattlePhase.AcademyLife,
            BattleResult result = BattleResult.None, bool shionRescued = false,
            bool carlosDefeated = false, int erosionPercent = 0,
            int shionTrust = 0, bool greyveEventCleared = false, int setsunaTrust = 0)
        {
            return new EndingContext
            {
                Flags = _flags,
                Trust = _trust,
                Phase = phase,
                Result = result,
                ShionRescued = shionRescued,
                CarlosDefeated = carlosDefeated,
                ErosionPercent = erosionPercent,
                ShionTrust = shionTrust,
                GreyveEventCleared = greyveEventCleared,
                SetsunaTrust = setsunaTrust
            };
        }

        private void SetAllInfoFlags()
        {
            _flags.Set(F.ShionPast, true);
            _flags.Set(F.CarlosPlan, true);
            _flags.Set(F.VesselTruth, true);
            _flags.Set(F.AcademySecret, true);
            _flags.Set(F.SealMethod, true);
            _flags.Set(F.DarkPowerRisk, true);
            _flags.Set(F.TrustBetrayal, true);
            _flags.Set(F.SalvationPath, true);
        }

        private void SetAllTrust(int value)
        {
            _trust.SetTrust(CharacterId.Yuna, value);
            _trust.SetTrust(CharacterId.Mio, value);
            _trust.SetTrust(CharacterId.Kaito, value);
            _trust.SetTrust(CharacterId.Shion, value);
        }

        [Test]
        public void END1_AcademyRefused_ReturnsHiddenHappy()
        {
            _flags.Set(F.AcademyRefused, true);
            var context = CreateContext(BattlePhase.PreAcademy);
            Assert.AreEqual(EndingType.HiddenHappy, EndingResolver.Resolve(context));
        }

        [Test]
        public void END1_TakesPriorityOverOtherConditions()
        {
            _flags.Set(F.AcademyRefused, true);
            SetAllInfoFlags();
            SetAllTrust(100);
            var context = CreateContext(BattlePhase.CarlosBattle, BattleResult.PlayerVictory,
                shionRescued: true, carlosDefeated: true);
            Assert.AreEqual(EndingType.HiddenHappy, EndingResolver.Resolve(context));
        }

        [Test]
        public void END2_ShionPhase1_EnemyVictory_ReturnsTrue()
        {
            var context = CreateContext(BattlePhase.ShionPhase1, BattleResult.EnemyVictory);
            Assert.AreEqual(EndingType.True, EndingResolver.Resolve(context));
        }

        [Test]
        public void END2_ShionPhase1_PlayerVictory_DoesNotReturnTrue()
        {
            var context = CreateContext(BattlePhase.ShionPhase1, BattleResult.PlayerVictory);
            Assert.AreNotEqual(EndingType.True, EndingResolver.Resolve(context));
        }

        [Test]
        public void END3_ShionPhase2_EnemyVictory_ReturnsShionRoute()
        {
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.EnemyVictory);
            Assert.AreEqual(EndingType.ShionRoute, EndingResolver.Resolve(context));
        }

        [Test]
        public void END4_ShionPhase2_PlayerVictory_NoRescue_ReturnsNormal()
        {
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.PlayerVictory,
                shionRescued: false);
            Assert.AreEqual(EndingType.Normal, EndingResolver.Resolve(context));
        }

        [Test]
        public void END5_AllConditionsMet_ReturnsTrueHappy()
        {
            SetAllInfoFlags();
            SetAllTrust(80);
            var context = CreateContext(BattlePhase.CarlosBattle, BattleResult.PlayerVictory,
                shionRescued: true, carlosDefeated: true);
            Assert.AreEqual(EndingType.TrueHappy, EndingResolver.Resolve(context));
        }

        [Test]
        public void END5_MissingOneFlag_DoesNotReturnTrueHappy()
        {
            _flags.Set(F.ShionPast, true);
            _flags.Set(F.CarlosPlan, true);
            _flags.Set(F.VesselTruth, true);
            _flags.Set(F.AcademySecret, true);
            _flags.Set(F.SealMethod, true);
            _flags.Set(F.DarkPowerRisk, true);
            _flags.Set(F.TrustBetrayal, true);
            SetAllTrust(80);
            var context = CreateContext(BattlePhase.CarlosBattle, BattleResult.PlayerVictory,
                shionRescued: true, carlosDefeated: true);
            Assert.AreNotEqual(EndingType.TrueHappy, EndingResolver.Resolve(context));
        }

        [Test]
        public void END5_OneTrustBelow80_DoesNotReturnTrueHappy()
        {
            SetAllInfoFlags();
            _trust.SetTrust(CharacterId.Yuna, 80);
            _trust.SetTrust(CharacterId.Mio, 80);
            _trust.SetTrust(CharacterId.Kaito, 80);
            _trust.SetTrust(CharacterId.Shion, 79);
            var context = CreateContext(BattlePhase.CarlosBattle, BattleResult.PlayerVictory,
                shionRescued: true, carlosDefeated: true);
            Assert.AreNotEqual(EndingType.TrueHappy, EndingResolver.Resolve(context));
        }

        [Test]
        public void END6_AllPartyDefeated_ReturnsTotalDefeat()
        {
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.AllDefeated);
            Assert.AreEqual(EndingType.TotalDefeat, EndingResolver.Resolve(context));
        }

        [Test]
        public void END6_Erosion90_ReturnsTotalDefeat()
        {
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.PlayerVictory,
                erosionPercent: 90);
            Assert.AreEqual(EndingType.TotalDefeat, EndingResolver.Resolve(context));
        }

        [Test]
        public void END6_Erosion89_DoesNotReturnTotalDefeat()
        {
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.PlayerVictory,
                erosionPercent: 89);
            Assert.AreNotEqual(EndingType.TotalDefeat, EndingResolver.Resolve(context));
        }

        [Test]
        public void END6_TakesPriorityOverEND3()
        {
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.AllDefeated);
            Assert.AreEqual(EndingType.TotalDefeat, EndingResolver.Resolve(context));
        }

        // === END4.5 テスト ===

        [Test]
        public void END4_5_ShionTrust35_GreyveEvent_ReturnsHalfLight()
        {
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.PlayerVictory,
                shionRescued: false, shionTrust: 35, greyveEventCleared: true);
            Assert.AreEqual(EndingType.HalfLight, EndingResolver.Resolve(context));
        }

        [Test]
        public void END4_5_ShionTrust40_SetsunaTrust60_ReturnsHalfLight()
        {
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.PlayerVictory,
                shionRescued: false, shionTrust: 40, setsunaTrust: 60);
            Assert.AreEqual(EndingType.HalfLight, EndingResolver.Resolve(context));
        }

        [Test]
        public void END4_5_ShionTrust29_ReturnsNormal_NotHalfLight()
        {
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.PlayerVictory,
                shionRescued: false, shionTrust: 29, greyveEventCleared: true);
            Assert.AreEqual(EndingType.Normal, EndingResolver.Resolve(context));
        }

        [Test]
        public void END4_5_ShionTrust50_ReturnsNormal_NotHalfLight()
        {
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.PlayerVictory,
                shionRescued: false, shionTrust: 50, greyveEventCleared: true);
            Assert.AreEqual(EndingType.Normal, EndingResolver.Resolve(context));
        }

        [Test]
        public void END4_5_TakesPriorityOverEND4()
        {
            // END4条件（ShionPhase2勝利+救出失敗）を満たしつつ、END4.5の追加条件も満たす
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.PlayerVictory,
                shionRescued: false, shionTrust: 30, greyveEventCleared: true);
            Assert.AreEqual(EndingType.HalfLight, EndingResolver.Resolve(context));
        }

        [Test]
        public void END4_5_BothHiddenConditions_ReturnsHalfLight()
        {
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.PlayerVictory,
                shionRescued: false, shionTrust: 49, greyveEventCleared: true, setsunaTrust: 60);
            Assert.AreEqual(EndingType.HalfLight, EndingResolver.Resolve(context));
        }

        [Test]
        public void END4_5_NoHiddenCondition_ReturnsNormal()
        {
            // ShionTrust範囲内だがGreyveもSetsuna条件も満たさない→END4
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.PlayerVictory,
                shionRescued: false, shionTrust: 35, greyveEventCleared: false, setsunaTrust: 59);
            Assert.AreEqual(EndingType.Normal, EndingResolver.Resolve(context));
        }

        [Test]
        public void NoEndingTriggered_ReturnsNone()
        {
            var context = CreateContext(BattlePhase.AcademyLife);
            Assert.AreEqual(EndingType.None, EndingResolver.Resolve(context));
        }

        [Test]
        public void Resolve_NullContext_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => EndingResolver.Resolve(null));
        }

        [Test]
        public void Resolve_NullFlags_ThrowsArgumentNullException()
        {
            var context = new EndingContext { Flags = null, Trust = _trust };
            Assert.Throws<ArgumentNullException>(() => EndingResolver.Resolve(context));
        }

        [Test]
        public void Resolve_NullTrust_ThrowsArgumentNullException()
        {
            var context = new EndingContext { Flags = _flags, Trust = null };
            Assert.Throws<ArgumentNullException>(() => EndingResolver.Resolve(context));
        }

        // === 境界値・エッジケース追加テスト ===

        [Test]
        public void END5_TrustExactly80_ReturnsTrueHappy()
        {
            SetAllInfoFlags();
            SetAllTrust(80);
            var context = CreateContext(BattlePhase.CarlosBattle, BattleResult.PlayerVictory,
                shionRescued: true, carlosDefeated: true);
            Assert.AreEqual(EndingType.TrueHappy, EndingResolver.Resolve(context));
        }

        [Test]
        public void END5_TrustAt100_ReturnsTrueHappy()
        {
            SetAllInfoFlags();
            SetAllTrust(100);
            var context = CreateContext(BattlePhase.CarlosBattle, BattleResult.PlayerVictory,
                shionRescued: true, carlosDefeated: true);
            Assert.AreEqual(EndingType.TrueHappy, EndingResolver.Resolve(context));
        }

        [Test]
        public void END5_ShionNotRescued_DoesNotReturnTrueHappy()
        {
            SetAllInfoFlags();
            SetAllTrust(100);
            var context = CreateContext(BattlePhase.CarlosBattle, BattleResult.PlayerVictory,
                shionRescued: false, carlosDefeated: true);
            Assert.AreNotEqual(EndingType.TrueHappy, EndingResolver.Resolve(context));
        }

        [Test]
        public void END5_CarlosNotDefeated_DoesNotReturnTrueHappy()
        {
            SetAllInfoFlags();
            SetAllTrust(100);
            var context = CreateContext(BattlePhase.CarlosBattle, BattleResult.PlayerVictory,
                shionRescued: true, carlosDefeated: false);
            Assert.AreNotEqual(EndingType.TrueHappy, EndingResolver.Resolve(context));
        }

        [Test]
        public void END6_Erosion100_ReturnsTotalDefeat()
        {
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.PlayerVictory,
                erosionPercent: 100);
            Assert.AreEqual(EndingType.TotalDefeat, EndingResolver.Resolve(context));
        }

        [Test]
        public void END6_PreShionPhase2_DoesNotTrigger()
        {
            var context = CreateContext(BattlePhase.ShionPhase1, BattleResult.AllDefeated);
            Assert.AreNotEqual(EndingType.TotalDefeat, EndingResolver.Resolve(context));
        }

        [Test]
        public void END6_TakesPriorityOverEND4_5()
        {
            // Erosion 90% + END4.5 conditions both met → END6 wins
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.PlayerVictory,
                shionRescued: false, erosionPercent: 90, shionTrust: 35, greyveEventCleared: true);
            Assert.AreEqual(EndingType.TotalDefeat, EndingResolver.Resolve(context));
        }

        [Test]
        public void END4_5_ExactBoundary_ShionTrust30_ReturnsHalfLight()
        {
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.PlayerVictory,
                shionRescued: false, shionTrust: 30, greyveEventCleared: true);
            Assert.AreEqual(EndingType.HalfLight, EndingResolver.Resolve(context));
        }

        [Test]
        public void END4_5_ExactBoundary_ShionTrust49_ReturnsHalfLight()
        {
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.PlayerVictory,
                shionRescued: false, shionTrust: 49, greyveEventCleared: true);
            Assert.AreEqual(EndingType.HalfLight, EndingResolver.Resolve(context));
        }

        [Test]
        public void END4_5_SetsunaTrust59_NoGreyve_ReturnsNormal()
        {
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.PlayerVictory,
                shionRescued: false, shionTrust: 35, greyveEventCleared: false, setsunaTrust: 59);
            Assert.AreEqual(EndingType.Normal, EndingResolver.Resolve(context));
        }

        [Test]
        public void END4_5_SetsunaTrust60_ReturnsHalfLight()
        {
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.PlayerVictory,
                shionRescued: false, shionTrust: 35, setsunaTrust: 60);
            Assert.AreEqual(EndingType.HalfLight, EndingResolver.Resolve(context));
        }

        [TestCase(BattlePhase.PreAcademy)]
        [TestCase(BattlePhase.AcademyLife)]
        public void NoConditionsMet_EarlyPhase_ReturnsNone(BattlePhase phase)
        {
            var context = CreateContext(phase, BattleResult.None);
            Assert.AreEqual(EndingType.None, EndingResolver.Resolve(context));
        }

        [Test]
        public void END3_EnemyVictory_WithErosion89_ReturnsShionRoute_NotTotalDefeat()
        {
            // Erosion 89 is below 90 threshold, so END6 doesn't trigger
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.EnemyVictory,
                erosionPercent: 89);
            Assert.AreEqual(EndingType.ShionRoute, EndingResolver.Resolve(context));
        }
    }
}
