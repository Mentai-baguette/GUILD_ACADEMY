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

        // ===== Existing Tests (unchanged) =====

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

        // ===== New Character Tests (GA-73) =====

        [Test]
        public void NewSystem_NewCharacters_AllBondPointsZero()
        {
            Assert.AreEqual(0, _system.GetBondPoints(CharacterId.Shion));
            Assert.AreEqual(0, _system.GetBondPoints(CharacterId.Rin));
            Assert.AreEqual(0, _system.GetBondPoints(CharacterId.Vein));
            Assert.AreEqual(0, _system.GetBondPoints(CharacterId.Mel));
            Assert.AreEqual(0, _system.GetBondPoints(CharacterId.Jin));
            Assert.AreEqual(0, _system.GetBondPoints(CharacterId.Setsuna));
            Assert.AreEqual(0, _system.GetBondPoints(CharacterId.Renji));
        }

        [Test]
        public void AddBondPoints_NewCharacters_IncreasesValue()
        {
            _system.AddBondPoints(CharacterId.Rin, 15);
            Assert.AreEqual(15, _system.GetBondPoints(CharacterId.Rin));

            _system.AddBondPoints(CharacterId.Vein, 25);
            Assert.AreEqual(25, _system.GetBondPoints(CharacterId.Vein));

            _system.AddBondPoints(CharacterId.Mel, 35);
            Assert.AreEqual(35, _system.GetBondPoints(CharacterId.Mel));

            _system.AddBondPoints(CharacterId.Jin, 45);
            Assert.AreEqual(45, _system.GetBondPoints(CharacterId.Jin));

            _system.AddBondPoints(CharacterId.Setsuna, 55);
            Assert.AreEqual(55, _system.GetBondPoints(CharacterId.Setsuna));

            _system.AddBondPoints(CharacterId.Renji, 65);
            Assert.AreEqual(65, _system.GetBondPoints(CharacterId.Renji));
        }

        [Test]
        public void IsLinkTarget_AllNewCharacters_True()
        {
            Assert.IsTrue(_system.IsLinkTarget(CharacterId.Shion));
            Assert.IsTrue(_system.IsLinkTarget(CharacterId.Rin));
            Assert.IsTrue(_system.IsLinkTarget(CharacterId.Vein));
            Assert.IsTrue(_system.IsLinkTarget(CharacterId.Mel));
            Assert.IsTrue(_system.IsLinkTarget(CharacterId.Jin));
            Assert.IsTrue(_system.IsLinkTarget(CharacterId.Setsuna));
            Assert.IsTrue(_system.IsLinkTarget(CharacterId.Renji));
        }

        // ===== Shion Freeze Tests =====

        [Test]
        public void FreezeShion_PreventsAddBondPoints()
        {
            _system.AddBondPoints(CharacterId.Shion, 10);
            Assert.AreEqual(10, _system.GetBondPoints(CharacterId.Shion));

            _system.FreezeShion();
            Assert.IsTrue(_system.IsShionFrozen);

            _system.AddBondPoints(CharacterId.Shion, 20);
            Assert.AreEqual(10, _system.GetBondPoints(CharacterId.Shion));
        }

        [Test]
        public void UnfreezeShion_AllowsAddBondPoints()
        {
            _system.FreezeShion();
            _system.AddBondPoints(CharacterId.Shion, 10);
            Assert.AreEqual(0, _system.GetBondPoints(CharacterId.Shion));

            _system.UnfreezeShion();
            Assert.IsFalse(_system.IsShionFrozen);

            _system.AddBondPoints(CharacterId.Shion, 30);
            Assert.AreEqual(30, _system.GetBondPoints(CharacterId.Shion));
        }

        [Test]
        public void FreezeShion_DoesNotAffectOtherCharacters()
        {
            _system.FreezeShion();
            _system.AddBondPoints(CharacterId.Yuna, 50);
            _system.AddBondPoints(CharacterId.Rin, 50);
            Assert.AreEqual(50, _system.GetBondPoints(CharacterId.Yuna));
            Assert.AreEqual(50, _system.GetBondPoints(CharacterId.Rin));
        }

        [Test]
        public void IsShionFrozen_DefaultFalse()
        {
            Assert.IsFalse(_system.IsShionFrozen);
        }

        // ===== 10-member Max Level Test =====

        [Test]
        public void AllTenMembers_CanReachMaxLevel()
        {
            CharacterId[] allTargets =
            {
                CharacterId.Yuna, CharacterId.Mio, CharacterId.Kaito,
                CharacterId.Shion, CharacterId.Rin, CharacterId.Vein,
                CharacterId.Mel, CharacterId.Jin, CharacterId.Setsuna,
                CharacterId.Renji
            };

            foreach (var id in allTargets)
            {
                _system.AddBondPoints(id, 100);
                Assert.AreEqual(SoulLinkLevel.Max, _system.GetLevel(id),
                    $"{id} should reach Max level");
                Assert.IsTrue(_system.IsMaxBond(id),
                    $"{id} should be max bond");
            }
        }

        // ===== Ray Error Handling =====

        [Test]
        public void GetBondPoints_Ray_ThrowsNotSupportedException()
        {
            Assert.Throws<NotSupportedException>(() => _system.GetBondPoints(CharacterId.Ray));
        }

        [Test]
        public void GetLevel_Ray_ThrowsNotSupportedException()
        {
            Assert.Throws<NotSupportedException>(() => _system.GetLevel(CharacterId.Ray));
        }

        [Test]
        public void IsDualArtsAvailable_Ray_ThrowsNotSupportedException()
        {
            Assert.Throws<NotSupportedException>(() => _system.IsDualArtsAvailable(CharacterId.Ray));
        }

        [Test]
        public void IsDualArtsAvailable_ShionFrozen_ReturnsFalse()
        {
            _system.AddBondPoints(CharacterId.Shion, 30);
            Assert.IsTrue(_system.IsDualArtsAvailable(CharacterId.Shion));

            _system.FreezeShion();
            Assert.IsFalse(_system.IsDualArtsAvailable(CharacterId.Shion));
        }

        [Test]
        public void IsDualArtsAvailable_ShionUnfrozen_ReturnsTrue()
        {
            _system.AddBondPoints(CharacterId.Shion, 30);
            _system.FreezeShion();
            Assert.IsFalse(_system.IsDualArtsAvailable(CharacterId.Shion));

            _system.UnfreezeShion();
            Assert.IsTrue(_system.IsDualArtsAvailable(CharacterId.Shion));
        }

        [Test]
        public void IsDualArtsAvailable_OtherCharacterNotAffectedByShionFreeze()
        {
            _system.AddBondPoints(CharacterId.Yuna, 30);
            _system.FreezeShion();
            Assert.IsTrue(_system.IsDualArtsAvailable(CharacterId.Yuna));
        }
    }
}
