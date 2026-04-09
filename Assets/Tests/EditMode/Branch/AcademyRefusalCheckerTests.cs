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
        public void RegisterOppositeWalk_WithoutBedReturn_ReturnsZero()
        {
            int result = _checker.RegisterOppositeWalk();
            Assert.AreEqual(0, result);
            Assert.AreEqual(0, _checker.OppositeWalkCount);
        }

        [Test]
        public void RegisterOppositeWalk_WithBedReturn_IncrementsCount()
        {
            _checker.RegisterBedReturn();
            _checker.RegisterOppositeWalk();
            Assert.AreEqual(1, _checker.OppositeWalkCount);
            _checker.RegisterOppositeWalk();
            Assert.AreEqual(2, _checker.OppositeWalkCount);
        }

        [Test]
        public void FiveWarningWalks_DoesNotTriggerRefusal()
        {
            _checker.RegisterBedReturn();
            for (int i = 0; i < AcademyRefusalChecker.WarningCount; i++)
                _checker.RegisterOppositeWalk();
            Assert.IsFalse(_checker.IsRefusalTriggered);
            Assert.AreEqual(AcademyRefusalChecker.WarningCount, _checker.OppositeWalkCount);
        }

        [Test]
        public void SixthWalk_AfterFiveWarnings_TriggersRefusal()
        {
            _checker.RegisterBedReturn();
            // 5 warning walks + 1 refusal walk = 6 total
            for (int i = 0; i < AcademyRefusalChecker.WarningCount + 1; i++)
                _checker.RegisterOppositeWalk();
            Assert.IsTrue(_checker.IsRefusalTriggered);
            Assert.AreEqual(AcademyRefusalChecker.WarningCount + 1, _checker.OppositeWalkCount);
        }

        [Test]
        public void OnOppositeWalk_EventFiresForWarningWalks()
        {
            _checker.RegisterBedReturn();
            int lastCount = -1;
            _checker.OnOppositeWalk += count => lastCount = count;

            _checker.RegisterOppositeWalk();
            Assert.AreEqual(1, lastCount);

            _checker.RegisterOppositeWalk();
            Assert.AreEqual(2, lastCount);
        }

        [Test]
        public void OnOppositeWalk_DoesNotFireOnRefusalWalk()
        {
            _checker.RegisterBedReturn();
            int eventFireCount = 0;
            _checker.OnOppositeWalk += _ => eventFireCount++;

            // Walk WarningCount + 1 times (5 warnings + 1 refusal)
            for (int i = 0; i < AcademyRefusalChecker.WarningCount + 1; i++)
                _checker.RegisterOppositeWalk();

            // OnOppositeWalk should only fire for warning walks (1-5), not the refusal walk
            Assert.AreEqual(AcademyRefusalChecker.WarningCount, eventFireCount);
        }

        [Test]
        public void OnRefusalTriggered_EventFires()
        {
            _checker.RegisterBedReturn();
            bool triggered = false;
            _checker.OnRefusalTriggered += () => triggered = true;

            for (int i = 0; i < AcademyRefusalChecker.WarningCount + 1; i++)
                _checker.RegisterOppositeWalk();

            Assert.IsTrue(triggered);
        }

        [Test]
        public void AfterRefusal_WalkDoesNotIncrementFurther()
        {
            _checker.RegisterBedReturn();
            for (int i = 0; i < AcademyRefusalChecker.WarningCount + 2; i++)
                _checker.RegisterOppositeWalk();
            Assert.AreEqual(AcademyRefusalChecker.WarningCount + 1, _checker.OppositeWalkCount);
        }

        [Test]
        public void Reset_ClearsAllState()
        {
            _checker.RegisterBedReturn();
            for (int i = 0; i < AcademyRefusalChecker.WarningCount + 1; i++)
                _checker.RegisterOppositeWalk();

            _checker.Reset();

            Assert.IsFalse(_checker.BedReturned);
            Assert.AreEqual(0, _checker.OppositeWalkCount);
            Assert.IsFalse(_checker.IsRefusalTriggered);
        }

        [Test]
        public void WarningCount_IsFive()
        {
            Assert.AreEqual(5, AcademyRefusalChecker.WarningCount);
        }

        [Test]
        public void WithoutBedReturn_WalksDoNotCount_CannotTriggerRefusal()
        {
            // Without bed return, even many walks should not trigger
            for (int i = 0; i < 20; i++)
                _checker.RegisterOppositeWalk();
            Assert.IsFalse(_checker.IsRefusalTriggered);
            Assert.AreEqual(0, _checker.OppositeWalkCount);
        }

        [Test]
        public void BedReturnThenWalks_CorrectSequenceTriggersRefusal()
        {
            // Walks before bed return don't count
            _checker.RegisterOppositeWalk();
            _checker.RegisterOppositeWalk();
            Assert.AreEqual(0, _checker.OppositeWalkCount);

            // Register bed return
            _checker.RegisterBedReturn();

            // Now walks count
            for (int i = 0; i < AcademyRefusalChecker.WarningCount + 1; i++)
                _checker.RegisterOppositeWalk();

            Assert.IsTrue(_checker.IsRefusalTriggered);
            Assert.AreEqual(AcademyRefusalChecker.WarningCount + 1, _checker.OppositeWalkCount);
        }
    }
}
