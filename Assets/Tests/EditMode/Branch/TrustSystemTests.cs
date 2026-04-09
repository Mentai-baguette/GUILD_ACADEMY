using NUnit.Framework;
using GuildAcademy.Core.Branch;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Tests.EditMode.Branch
{
    [TestFixture]
    public class TrustSystemTests
    {
        private TrustSystem _trustSystem;

        [SetUp]
        public void SetUp()
        {
            _trustSystem = new TrustSystem();
        }

        [Test]
        public void NewTrustSystem_AllTrustIsZero()
        {
            Assert.AreEqual(0, _trustSystem.GetTrust(CharacterId.Yuna));
            Assert.AreEqual(0, _trustSystem.GetTrust(CharacterId.Mio));
            Assert.AreEqual(0, _trustSystem.GetTrust(CharacterId.Kaito));
            Assert.AreEqual(0, _trustSystem.GetTrust(CharacterId.Shion));
        }

        [Test]
        public void AddTrust_IncreasesValue()
        {
            _trustSystem.AddTrust(CharacterId.Yuna, 30);
            Assert.AreEqual(30, _trustSystem.GetTrust(CharacterId.Yuna));
        }

        [Test]
        public void AddTrust_ClampedAtMaximum100()
        {
            _trustSystem.AddTrust(CharacterId.Mio, 120);
            Assert.AreEqual(100, _trustSystem.GetTrust(CharacterId.Mio));
        }

        [Test]
        public void AddTrust_NegativeAmount_DecreasesValue()
        {
            _trustSystem.SetTrust(CharacterId.Kaito, 50);
            _trustSystem.AddTrust(CharacterId.Kaito, -20);
            Assert.AreEqual(30, _trustSystem.GetTrust(CharacterId.Kaito));
        }

        [Test]
        public void AddTrust_ClampedAtMinimum0()
        {
            _trustSystem.SetTrust(CharacterId.Shion, 10);
            _trustSystem.AddTrust(CharacterId.Shion, -50);
            Assert.AreEqual(0, _trustSystem.GetTrust(CharacterId.Shion));
        }

        [Test]
        public void MeetsThreshold_AboveThreshold_ReturnsTrue()
        {
            _trustSystem.SetTrust(CharacterId.Kaito, 85);
            Assert.IsTrue(_trustSystem.MeetsThreshold(CharacterId.Kaito, 80));
        }

        [Test]
        public void MeetsThreshold_BelowThreshold_ReturnsFalse()
        {
            _trustSystem.SetTrust(CharacterId.Shion, 40);
            Assert.IsFalse(_trustSystem.MeetsThreshold(CharacterId.Shion, 80));
        }

        [Test]
        public void MeetsThreshold_ExactlyAtThreshold_ReturnsTrue()
        {
            _trustSystem.SetTrust(CharacterId.Yuna, 80);
            Assert.IsTrue(_trustSystem.MeetsThreshold(CharacterId.Yuna, 80));
        }

        [Test]
        public void AllMeetThreshold_AllAbove_ReturnsTrue()
        {
            _trustSystem.SetTrust(CharacterId.Yuna, 80);
            _trustSystem.SetTrust(CharacterId.Mio, 90);
            _trustSystem.SetTrust(CharacterId.Kaito, 85);
            _trustSystem.SetTrust(CharacterId.Shion, 80);
            Assert.IsTrue(_trustSystem.AllMeetThreshold(80));
        }

        [Test]
        public void AllMeetThreshold_OneBelow_ReturnsFalse()
        {
            _trustSystem.SetTrust(CharacterId.Yuna, 80);
            _trustSystem.SetTrust(CharacterId.Mio, 90);
            _trustSystem.SetTrust(CharacterId.Kaito, 85);
            _trustSystem.SetTrust(CharacterId.Shion, 79);
            Assert.IsFalse(_trustSystem.AllMeetThreshold(80));
        }

        [Test]
        public void AddTrust_Ray_ThrowsNotSupportedException()
        {
            Assert.Throws<System.NotSupportedException>(() => _trustSystem.AddTrust(CharacterId.Ray, 10));
        }

        [Test]
        public void IsTrustTarget_Yuna_ReturnsTrue()
        {
            Assert.IsTrue(_trustSystem.IsTrustTarget(CharacterId.Yuna));
        }

        [Test]
        public void IsTrustTarget_Ray_ReturnsFalse()
        {
            Assert.IsFalse(_trustSystem.IsTrustTarget(CharacterId.Ray));
        }

        // === 追加テスト ===

        [Test]
        public void SetTrust_ClampsToMax100()
        {
            _trustSystem.SetTrust(CharacterId.Yuna, 150);
            Assert.AreEqual(100, _trustSystem.GetTrust(CharacterId.Yuna));
        }

        [Test]
        public void SetTrust_ClampsToMin0()
        {
            _trustSystem.SetTrust(CharacterId.Yuna, -50);
            Assert.AreEqual(0, _trustSystem.GetTrust(CharacterId.Yuna));
        }

        [Test]
        public void AddTrust_MultipleCallsAccumulate()
        {
            _trustSystem.AddTrust(CharacterId.Kaito, 20);
            _trustSystem.AddTrust(CharacterId.Kaito, 30);
            _trustSystem.AddTrust(CharacterId.Kaito, 10);
            Assert.AreEqual(60, _trustSystem.GetTrust(CharacterId.Kaito));
        }

        [Test]
        public void MeetsThreshold_BelowByOne_ReturnsFalse()
        {
            _trustSystem.SetTrust(CharacterId.Shion, 49);
            Assert.IsFalse(_trustSystem.MeetsThreshold(CharacterId.Shion, 50));
        }

        [Test]
        public void IsTrustTarget_Mio_ReturnsTrue()
        {
            Assert.IsTrue(_trustSystem.IsTrustTarget(CharacterId.Mio));
        }

        [Test]
        public void Reset_ClearsAllToZero()
        {
            _trustSystem.SetTrust(CharacterId.Yuna, 50);
            _trustSystem.SetTrust(CharacterId.Mio, 60);
            _trustSystem.Reset();
            Assert.AreEqual(0, _trustSystem.GetTrust(CharacterId.Yuna));
            Assert.AreEqual(0, _trustSystem.GetTrust(CharacterId.Mio));
        }
    }
}
