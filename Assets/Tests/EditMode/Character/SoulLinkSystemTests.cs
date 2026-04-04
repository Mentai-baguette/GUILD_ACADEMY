using System;
using GuildAcademy.Core.Character;
using GuildAcademy.Core.Data;
using NUnit.Framework;

namespace GuildAcademy.Tests.EditMode.Character
{
    [TestFixture]
    public class SoulLinkSystemTests
    {
        private SoulLinkSystem _system;

        [SetUp]
        public void SetUp()
        {
            _system = new SoulLinkSystem();
        }

        [Test]
        public void NewSystem_AllBondPointsZero()
        {
            Assert.AreEqual(0, _system.GetBondPoints(CharacterId.Yuna));
            Assert.AreEqual(0, _system.GetBondPoints(CharacterId.Mio));
            Assert.AreEqual(0, _system.GetBondPoints(CharacterId.Kaito));
        }

        [Test]
        public void AddBondPoints_IncreasesValue()
        {
            _system.AddBondPoints(CharacterId.Yuna, 10);
            Assert.AreEqual(10, _system.GetBondPoints(CharacterId.Yuna));
        }

        [Test]
        public void AddBondPoints_ClampsAtMax()
        {
            _system.AddBondPoints(CharacterId.Yuna, 150);
            Assert.AreEqual(SoulLinkSystem.MAX_BOND_POINTS, _system.GetBondPoints(CharacterId.Yuna));
        }

        [Test]
        public void AddBondPoints_NegativeOrZero_DoesNothing()
        {
            _system.AddBondPoints(CharacterId.Yuna, 10);
            _system.AddBondPoints(CharacterId.Yuna, -5);
            _system.AddBondPoints(CharacterId.Yuna, 0);
            Assert.AreEqual(10, _system.GetBondPoints(CharacterId.Yuna));
        }

        [Test]
        public void GetLevel_None_Under20()
        {
            _system.AddBondPoints(CharacterId.Yuna, 19);
            Assert.AreEqual(SoulLinkLevel.None, _system.GetLevel(CharacterId.Yuna));
        }

        [Test]
        public void GetLevel_Level1_20To39()
        {
            _system.AddBondPoints(CharacterId.Yuna, 20);
            Assert.AreEqual(SoulLinkLevel.Level1, _system.GetLevel(CharacterId.Yuna));

            _system.AddBondPoints(CharacterId.Mio, 39);
            Assert.AreEqual(SoulLinkLevel.Level1, _system.GetLevel(CharacterId.Mio));
        }

        [Test]
        public void GetLevel_Level2_40To69()
        {
            _system.AddBondPoints(CharacterId.Yuna, 40);
            Assert.AreEqual(SoulLinkLevel.Level2, _system.GetLevel(CharacterId.Yuna));

            _system.AddBondPoints(CharacterId.Mio, 69);
            Assert.AreEqual(SoulLinkLevel.Level2, _system.GetLevel(CharacterId.Mio));
        }

        [Test]
        public void GetLevel_Level3_70To99()
        {
            _system.AddBondPoints(CharacterId.Yuna, 70);
            Assert.AreEqual(SoulLinkLevel.Level3, _system.GetLevel(CharacterId.Yuna));

            _system.AddBondPoints(CharacterId.Mio, 99);
            Assert.AreEqual(SoulLinkLevel.Level3, _system.GetLevel(CharacterId.Mio));
        }

        [Test]
        public void GetLevel_Max_At100()
        {
            _system.AddBondPoints(CharacterId.Yuna, 100);
            Assert.AreEqual(SoulLinkLevel.Max, _system.GetLevel(CharacterId.Yuna));
        }

        [Test]
        public void IsDualArtsAvailable_Level1Plus_True()
        {
            Assert.IsFalse(_system.IsDualArtsAvailable(CharacterId.Yuna));
            _system.AddBondPoints(CharacterId.Yuna, 20);
            Assert.IsTrue(_system.IsDualArtsAvailable(CharacterId.Yuna));
            _system.AddBondPoints(CharacterId.Mio, 100);
            Assert.IsTrue(_system.IsDualArtsAvailable(CharacterId.Mio));
        }

        [Test]
        public void IsAutoProtectAvailable_Level2Plus_True()
        {
            Assert.IsFalse(_system.IsAutoProtectAvailable(CharacterId.Yuna));
            _system.AddBondPoints(CharacterId.Yuna, 40);
            Assert.IsTrue(_system.IsAutoProtectAvailable(CharacterId.Yuna));
        }

        [Test]
        public void IsUltimateAvailable_Level3Plus_True()
        {
            Assert.IsFalse(_system.IsUltimateAvailable(CharacterId.Yuna));
            _system.AddBondPoints(CharacterId.Yuna, 70);
            Assert.IsTrue(_system.IsUltimateAvailable(CharacterId.Yuna));
        }

        [Test]
        public void IsMaxBond_OnlyAtMax()
        {
            _system.AddBondPoints(CharacterId.Yuna, 99);
            Assert.IsFalse(_system.IsMaxBond(CharacterId.Yuna));
            _system.AddBondPoints(CharacterId.Yuna, 1);
            Assert.IsTrue(_system.IsMaxBond(CharacterId.Yuna));
        }

        [Test]
        public void AddBondPoints_Ray_ThrowsNotSupportedException()
        {
            Assert.Throws<NotSupportedException>(() => _system.AddBondPoints(CharacterId.Ray, 10));
        }

        [Test]
        public void OnLevelUp_FiresOnTransition()
        {
            CharacterId firedId = default;
            SoulLinkLevel firedLevel = default;
            int fireCount = 0;

            _system.OnLevelUp += (id, level) =>
            {
                firedId = id;
                firedLevel = level;
                fireCount++;
            };

            _system.AddBondPoints(CharacterId.Yuna, 20);
            Assert.AreEqual(1, fireCount);
            Assert.AreEqual(CharacterId.Yuna, firedId);
            Assert.AreEqual(SoulLinkLevel.Level1, firedLevel);

            // Adding points without crossing threshold should not fire
            _system.AddBondPoints(CharacterId.Yuna, 5);
            Assert.AreEqual(1, fireCount);
        }

        [Test]
        public void IsLinkTarget_Yuna_True()
        {
            Assert.IsTrue(_system.IsLinkTarget(CharacterId.Yuna));
        }

        [Test]
        public void IsLinkTarget_Ray_False()
        {
            Assert.IsFalse(_system.IsLinkTarget(CharacterId.Ray));
        }
    }
}
