using NUnit.Framework;
using GuildAcademy.Core.Branch;
using GuildAcademy.Core.Data;

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
            bool carlosDefeated = false, int erosionPercent = 0)
        {
            return new EndingContext
            {
                Flags = _flags,
                Trust = _trust,
                Phase = phase,
                Result = result,
                ShionRescued = shionRescued,
                CarlosDefeated = carlosDefeated,
                ErosionPercent = erosionPercent
            };
        }

        private void SetAllInfoFlags()
        {
            _flags.Set("flag_shion_past", true);
            _flags.Set("flag_carlos_plan", true);
            _flags.Set("flag_vessel_truth", true);
            _flags.Set("flag_academy_secret", true);
            _flags.Set("flag_seal_method", true);
            _flags.Set("flag_dark_power_risk", true);
            _flags.Set("flag_trust_betrayal", true);
            _flags.Set("flag_salvation_path", true);
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
            _flags.Set("academy_refused", true);
            var context = CreateContext(BattlePhase.PreAcademy);
            Assert.AreEqual(EndingType.HiddenHappy, EndingResolver.Resolve(context));
        }

        [Test]
        public void END1_TakesPriorityOverOtherConditions()
        {
            _flags.Set("academy_refused", true);
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
            _flags.Set("flag_shion_past", true);
            _flags.Set("flag_carlos_plan", true);
            _flags.Set("flag_vessel_truth", true);
            _flags.Set("flag_academy_secret", true);
            _flags.Set("flag_seal_method", true);
            _flags.Set("flag_dark_power_risk", true);
            _flags.Set("flag_trust_betrayal", true);
            // flag_salvation_path not set
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
        public void END6_ErosionOver90_ReturnsTotalDefeat()
        {
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.PlayerVictory,
                erosionPercent: 90);
            Assert.AreEqual(EndingType.TotalDefeat, EndingResolver.Resolve(context));
        }

        [Test]
        public void END6_TakesPriorityOverEND3()
        {
            var context = CreateContext(BattlePhase.ShionPhase2, BattleResult.AllDefeated);
            Assert.AreEqual(EndingType.TotalDefeat, EndingResolver.Resolve(context));
        }

        [Test]
        public void NoEndingTriggered_ReturnsNone()
        {
            var context = CreateContext(BattlePhase.AcademyLife);
            Assert.AreEqual(EndingType.None, EndingResolver.Resolve(context));
        }
    }
}
