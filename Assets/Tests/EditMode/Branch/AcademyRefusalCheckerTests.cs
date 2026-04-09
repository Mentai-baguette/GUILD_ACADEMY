using NUnit.Framework;
using GuildAcademy.Core.Branch;

namespace GuildAcademy.Tests.EditMode.Branch
{
    [TestFixture]
    public class AcademyRefusalCheckerTests
    {
        private AcademyRefusalChecker _checker;

        [SetUp]
        public void SetUp()
        {
            _checker = new AcademyRefusalChecker();
        }

        [Test]
        public void InitialState_NotTriggered()
        {
            Assert.IsFalse(_checker.BedReturned);
            Assert.AreEqual(0, _checker.OppositeWalkCount);
            Assert.IsFalse(_checker.IsRefusalTriggered);
        }

        [Test]
        public void RegisterBedReturn_SetsBedReturned()
        {
            _checker.RegisterBedReturn();
            Assert.IsTrue(_checker.BedReturned);
        }

        [Test]
        public void RegisterOppositeWalk_IncrementsCount()
        {
            _checker.RegisterOppositeWalk();
            Assert.AreEqual(1, _checker.OppositeWalkCount);
            _checker.RegisterOppositeWalk();
            Assert.AreEqual(2, _checker.OppositeWalkCount);
        }

        [Test]
        public void FiveWalks_DoesNotTriggerRefusal()
        {
            for (int i = 0; i < 5; i++)
                _checker.RegisterOppositeWalk();
            Assert.IsFalse(_checker.IsRefusalTriggered);
            Assert.AreEqual(5, _checker.OppositeWalkCount);
        }

        [Test]
        public void SixthWalk_TriggersRefusal()
        {
            for (int i = 0; i < 6; i++)
                _checker.RegisterOppositeWalk();
            Assert.IsTrue(_checker.IsRefusalTriggered);
        }

        [Test]
        public void OnOppositeWalk_EventFiresWithCount()
        {
            int lastCount = -1;
            _checker.OnOppositeWalk += count => lastCount = count;

            _checker.RegisterOppositeWalk();
            Assert.AreEqual(1, lastCount);

            _checker.RegisterOppositeWalk();
            Assert.AreEqual(2, lastCount);
        }

        [Test]
        public void OnRefusalTriggered_EventFires()
        {
            bool triggered = false;
            _checker.OnRefusalTriggered += () => triggered = true;

            for (int i = 0; i < 6; i++)
                _checker.RegisterOppositeWalk();

            Assert.IsTrue(triggered);
        }

        [Test]
        public void AfterRefusal_WalkDoesNotIncrementFurther()
        {
            for (int i = 0; i < 7; i++)
                _checker.RegisterOppositeWalk();
            Assert.AreEqual(6, _checker.OppositeWalkCount);
        }

        [Test]
        public void Reset_ClearsAllState()
        {
            _checker.RegisterBedReturn();
            for (int i = 0; i < 6; i++)
                _checker.RegisterOppositeWalk();

            _checker.Reset();

            Assert.IsFalse(_checker.BedReturned);
            Assert.AreEqual(0, _checker.OppositeWalkCount);
            Assert.IsFalse(_checker.IsRefusalTriggered);
        }

        [Test]
        public void RequiredOppositeWalks_IsFive()
        {
            Assert.AreEqual(5, AcademyRefusalChecker.RequiredOppositeWalks);
        }
    }
}
