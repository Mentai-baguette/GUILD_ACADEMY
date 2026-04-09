using NUnit.Framework;
using GuildAcademy.Core.Branch;
using GuildAcademy.Core.Data;
using F = GuildAcademy.Core.Branch.FlagSystem.Flags;

namespace GuildAcademy.Tests.EditMode.Branch
{
    [TestFixture]
    public class BranchServiceTests
    {
        private BranchService _service;

        [SetUp]
        public void SetUp()
        {
            _service = new BranchService();
        }

        [Test]
        public void SetFlag_SetsAndGetsCorrectly()
        {
            _service.SetFlag(F.ShionPast, true);
            Assert.IsTrue(_service.GetFlag(F.ShionPast));
        }

        [Test]
        public void SetFlag_FiresOnFlagChangedEvent()
        {
            string changedFlag = null;
            bool changedValue = false;
            _service.OnFlagChanged += (name, val) => { changedFlag = name; changedValue = val; };

            _service.SetFlag(F.CarlosPlan, true);

            Assert.AreEqual(F.CarlosPlan, changedFlag);
            Assert.IsTrue(changedValue);
        }

        [Test]
        public void AddTrust_AddsAndGetsCorrectly()
        {
            _service.AddTrust(CharacterId.Yuna, 30);
            Assert.AreEqual(30, _service.GetTrust(CharacterId.Yuna));
        }

        [Test]
        public void AddTrust_FiresOnTrustChangedEvent()
        {
            CharacterId changedId = CharacterId.Ray;
            int changedValue = -1;
            _service.OnTrustChanged += (id, val) => { changedId = id; changedValue = val; };

            _service.AddTrust(CharacterId.Mio, 25);

            Assert.AreEqual(CharacterId.Mio, changedId);
            Assert.AreEqual(25, changedValue);
        }

        [Test]
        public void CheckEnding_DelegatesToEndingResolver()
        {
            _service.SetFlag(F.AcademyRefused, true);
            var context = _service.CreateEndingContext(
                BattlePhase.PreAcademy, BattleResult.None,
                false, false, 0);
            Assert.AreEqual(EndingType.HiddenHappy, _service.CheckEnding(context));
        }

        [Test]
        public void CreateEndingContext_ContainsCorrectFlagsAndTrust()
        {
            _service.SetFlag(F.ShionPast, true);
            _service.AddTrust(CharacterId.Yuna, 50);

            var context = _service.CreateEndingContext(
                BattlePhase.AcademyLife, BattleResult.None,
                false, false, 0);

            Assert.IsTrue(context.Flags.Get(F.ShionPast));
            Assert.AreEqual(50, context.Trust.GetTrust(CharacterId.Yuna));
        }

        [Test]
        public void Reset_ClearsAllFlagsAndTrust()
        {
            _service.SetFlag(F.ShionPast, true);
            _service.AddTrust(CharacterId.Yuna, 50);

            _service.Reset();

            Assert.IsFalse(_service.GetFlag(F.ShionPast));
            Assert.AreEqual(0, _service.GetTrust(CharacterId.Yuna));
        }

        [Test]
        public void Constructor_WithExistingSystems_UsesThemDirectly()
        {
            var flags = new FlagSystem();
            var trust = new TrustSystem();
            flags.Set(F.VesselTruth, true);
            trust.AddTrust(CharacterId.Kaito, 40);

            var service = new BranchService(flags, trust);

            Assert.IsTrue(service.GetFlag(F.VesselTruth));
            Assert.AreEqual(40, service.GetTrust(CharacterId.Kaito));
        }
    }
}
