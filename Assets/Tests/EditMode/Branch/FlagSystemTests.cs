using System;
using NUnit.Framework;
using GuildAcademy.Core.Branch;

namespace GuildAcademy.Tests.EditMode.Branch
{
    [TestFixture]
    public class FlagSystemTests
    {
        private FlagSystem _flagSystem;

        [SetUp]
        public void SetUp()
        {
            _flagSystem = new FlagSystem();
        }

        [Test]
        public void NewFlagSystem_AllFlagsAreFalse()
        {
            Assert.IsFalse(_flagSystem.Get("flag_shion_past"));
            Assert.IsFalse(_flagSystem.Get("flag_carlos_plan"));
            Assert.IsFalse(_flagSystem.Get("flag_vessel_truth"));
            Assert.IsFalse(_flagSystem.Get("flag_academy_secret"));
            Assert.IsFalse(_flagSystem.Get("flag_seal_method"));
            Assert.IsFalse(_flagSystem.Get("flag_dark_power_risk"));
            Assert.IsFalse(_flagSystem.Get("flag_trust_betrayal"));
            Assert.IsFalse(_flagSystem.Get("flag_salvation_path"));
            Assert.IsFalse(_flagSystem.Get("academy_refused"));
        }

        [Test]
        public void Set_SingleFlag_ReturnsTrue()
        {
            _flagSystem.Set("flag_carlos_plan", true);
            Assert.IsTrue(_flagSystem.Get("flag_carlos_plan"));
        }

        [Test]
        public void Set_FlagToFalse_ReturnsFalse()
        {
            _flagSystem.Set("flag_carlos_plan", true);
            _flagSystem.Set("flag_carlos_plan", false);
            Assert.IsFalse(_flagSystem.Get("flag_carlos_plan"));
        }

        [Test]
        public void Get_UndefinedFlag_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _flagSystem.Get("nonexistent_flag"));
        }

        [Test]
        public void GetActiveCount_NoFlagsSet_ReturnsZero()
        {
            Assert.AreEqual(0, _flagSystem.GetActiveCount());
        }

        [Test]
        public void GetActiveCount_ThreeFlagsSet_ReturnsThree()
        {
            _flagSystem.Set("flag_shion_past", true);
            _flagSystem.Set("flag_carlos_plan", true);
            _flagSystem.Set("flag_vessel_truth", true);
            Assert.AreEqual(3, _flagSystem.GetActiveCount());
        }

        [Test]
        public void GetActiveCount_AllEightFlagsSet_ReturnsEight()
        {
            _flagSystem.Set("flag_shion_past", true);
            _flagSystem.Set("flag_carlos_plan", true);
            _flagSystem.Set("flag_vessel_truth", true);
            _flagSystem.Set("flag_academy_secret", true);
            _flagSystem.Set("flag_seal_method", true);
            _flagSystem.Set("flag_dark_power_risk", true);
            _flagSystem.Set("flag_trust_betrayal", true);
            _flagSystem.Set("flag_salvation_path", true);
            Assert.AreEqual(8, _flagSystem.GetActiveCount());
        }

        [Test]
        public void AreAllSet_NotAllFlags_ReturnsFalse()
        {
            _flagSystem.Set("flag_shion_past", true);
            _flagSystem.Set("flag_carlos_plan", true);
            _flagSystem.Set("flag_vessel_truth", true);
            _flagSystem.Set("flag_academy_secret", true);
            _flagSystem.Set("flag_seal_method", true);
            _flagSystem.Set("flag_dark_power_risk", true);
            _flagSystem.Set("flag_trust_betrayal", true);
            Assert.IsFalse(_flagSystem.AreAllSet());
        }

        [Test]
        public void AreAllSet_AllEightFlags_ReturnsTrue()
        {
            _flagSystem.Set("flag_shion_past", true);
            _flagSystem.Set("flag_carlos_plan", true);
            _flagSystem.Set("flag_vessel_truth", true);
            _flagSystem.Set("flag_academy_secret", true);
            _flagSystem.Set("flag_seal_method", true);
            _flagSystem.Set("flag_dark_power_risk", true);
            _flagSystem.Set("flag_trust_betrayal", true);
            _flagSystem.Set("flag_salvation_path", true);
            Assert.IsTrue(_flagSystem.AreAllSet());
        }

        [Test]
        public void Reset_ClearsAllFlags()
        {
            _flagSystem.Set("flag_shion_past", true);
            _flagSystem.Set("academy_refused", true);
            _flagSystem.Reset();
            Assert.AreEqual(0, _flagSystem.GetActiveCount());
            Assert.IsFalse(_flagSystem.Get("flag_shion_past"));
            Assert.IsFalse(_flagSystem.Get("academy_refused"));
        }

        [Test]
        public void AcademyRefused_IsSeparateFromInfoFlags()
        {
            _flagSystem.Set("academy_refused", true);
            Assert.AreEqual(0, _flagSystem.GetActiveCount());
            Assert.IsFalse(_flagSystem.AreAllSet());
        }
    }
}
