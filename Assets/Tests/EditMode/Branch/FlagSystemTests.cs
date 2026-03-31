using System;
using NUnit.Framework;
using GuildAcademy.Core.Branch;
using F = GuildAcademy.Core.Branch.FlagSystem.Flags;

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

        private void SetAllInfoFlags()
        {
            _flagSystem.Set(F.ShionPast, true);
            _flagSystem.Set(F.CarlosPlan, true);
            _flagSystem.Set(F.VesselTruth, true);
            _flagSystem.Set(F.AcademySecret, true);
            _flagSystem.Set(F.SealMethod, true);
            _flagSystem.Set(F.DarkPowerRisk, true);
            _flagSystem.Set(F.TrustBetrayal, true);
            _flagSystem.Set(F.SalvationPath, true);
        }

        [Test]
        public void NewFlagSystem_AllFlagsAreFalse()
        {
            Assert.IsFalse(_flagSystem.Get(F.ShionPast));
            Assert.IsFalse(_flagSystem.Get(F.CarlosPlan));
            Assert.IsFalse(_flagSystem.Get(F.VesselTruth));
            Assert.IsFalse(_flagSystem.Get(F.AcademySecret));
            Assert.IsFalse(_flagSystem.Get(F.SealMethod));
            Assert.IsFalse(_flagSystem.Get(F.DarkPowerRisk));
            Assert.IsFalse(_flagSystem.Get(F.TrustBetrayal));
            Assert.IsFalse(_flagSystem.Get(F.SalvationPath));
            Assert.IsFalse(_flagSystem.Get(F.AcademyRefused));
        }

        [Test]
        public void Set_SingleFlag_ReturnsTrue()
        {
            _flagSystem.Set(F.CarlosPlan, true);
            Assert.IsTrue(_flagSystem.Get(F.CarlosPlan));
        }

        [Test]
        public void Set_FlagToFalse_ReturnsFalse()
        {
            _flagSystem.Set(F.CarlosPlan, true);
            _flagSystem.Set(F.CarlosPlan, false);
            Assert.IsFalse(_flagSystem.Get(F.CarlosPlan));
        }

        [Test]
        public void Get_UndefinedFlag_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _flagSystem.Get("nonexistent_flag"));
        }

        [Test]
        public void Set_UndefinedFlag_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _flagSystem.Set("nonexistent_flag", true));
        }

        [Test]
        public void GetActiveCount_NoFlagsSet_ReturnsZero()
        {
            Assert.AreEqual(0, _flagSystem.GetActiveCount());
        }

        [Test]
        public void GetActiveCount_ThreeFlagsSet_ReturnsThree()
        {
            _flagSystem.Set(F.ShionPast, true);
            _flagSystem.Set(F.CarlosPlan, true);
            _flagSystem.Set(F.VesselTruth, true);
            Assert.AreEqual(3, _flagSystem.GetActiveCount());
        }

        [Test]
        public void GetActiveCount_AllEightFlagsSet_ReturnsEight()
        {
            SetAllInfoFlags();
            Assert.AreEqual(8, _flagSystem.GetActiveCount());
        }

        [Test]
        public void AreAllSet_NotAllFlags_ReturnsFalse()
        {
            _flagSystem.Set(F.ShionPast, true);
            _flagSystem.Set(F.CarlosPlan, true);
            _flagSystem.Set(F.VesselTruth, true);
            _flagSystem.Set(F.AcademySecret, true);
            _flagSystem.Set(F.SealMethod, true);
            _flagSystem.Set(F.DarkPowerRisk, true);
            _flagSystem.Set(F.TrustBetrayal, true);
            Assert.IsFalse(_flagSystem.AreAllSet());
        }

        [Test]
        public void AreAllSet_AllEightFlags_ReturnsTrue()
        {
            SetAllInfoFlags();
            Assert.IsTrue(_flagSystem.AreAllSet());
        }

        [Test]
        public void Reset_ClearsAllFlags()
        {
            _flagSystem.Set(F.ShionPast, true);
            _flagSystem.Set(F.AcademyRefused, true);
            _flagSystem.Reset();
            Assert.AreEqual(0, _flagSystem.GetActiveCount());
            Assert.IsFalse(_flagSystem.Get(F.ShionPast));
            Assert.IsFalse(_flagSystem.Get(F.AcademyRefused));
        }

        [Test]
        public void AcademyRefused_IsSeparateFromInfoFlags()
        {
            _flagSystem.Set(F.AcademyRefused, true);
            Assert.AreEqual(0, _flagSystem.GetActiveCount());
            Assert.IsFalse(_flagSystem.AreAllSet());
        }
    }
}
