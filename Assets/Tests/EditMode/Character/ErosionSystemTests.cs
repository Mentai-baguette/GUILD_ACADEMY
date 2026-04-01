using NUnit.Framework;
using GuildAcademy.Core.Character;

namespace GuildAcademy.Tests.EditMode.Character
{
    [TestFixture]
    public class ErosionSystemTests
    {
        private ErosionSystem _erosion;

        [SetUp]
        public void SetUp()
        {
            _erosion = new ErosionSystem();
        }

        [Test]
        public void NewSystem_StartsAtZero()
        {
            Assert.AreEqual(0, _erosion.CurrentErosion);
            Assert.AreEqual(ErosionStage.Normal, _erosion.CurrentStage);
        }

        [Test]
        public void AddErosion_IncreasesValue()
        {
            _erosion.AddErosion(20);
            Assert.AreEqual(20, _erosion.CurrentErosion);
        }

        [Test]
        public void AddErosion_ClampsAtMax()
        {
            _erosion.AddErosion(150);
            Assert.AreEqual(100, _erosion.CurrentErosion);
        }

        [Test]
        public void AddErosion_NegativeAmount_DoesNothing()
        {
            _erosion.AddErosion(10);
            _erosion.AddErosion(-5);
            Assert.AreEqual(10, _erosion.CurrentErosion);
        }

        [Test]
        public void ReduceErosion_DecreasesValue()
        {
            _erosion.AddErosion(50);
            _erosion.ReduceErosion(20);
            Assert.AreEqual(30, _erosion.CurrentErosion);
        }

        [Test]
        public void ReduceErosion_ClampsAtZero()
        {
            _erosion.AddErosion(10);
            _erosion.ReduceErosion(50);
            Assert.AreEqual(0, _erosion.CurrentErosion);
        }

        [Test]
        public void GetStage_Normal_Under30()
        {
            _erosion.AddErosion(29);
            Assert.AreEqual(ErosionStage.Normal, _erosion.CurrentStage);
        }

        [Test]
        public void GetStage_Unstable_30To59()
        {
            _erosion.AddErosion(30);
            Assert.AreEqual(ErosionStage.Unstable, _erosion.CurrentStage);

            var erosion59 = new ErosionSystem(59);
            Assert.AreEqual(ErosionStage.Unstable, erosion59.CurrentStage);
        }

        [Test]
        public void GetStage_Dangerous_60To89()
        {
            _erosion.AddErosion(60);
            Assert.AreEqual(ErosionStage.Dangerous, _erosion.CurrentStage);

            var erosion89 = new ErosionSystem(89);
            Assert.AreEqual(ErosionStage.Dangerous, erosion89.CurrentStage);
        }

        [Test]
        public void GetStage_Critical_90Plus()
        {
            _erosion.AddErosion(90);
            Assert.AreEqual(ErosionStage.Critical, _erosion.CurrentStage);

            var erosion100 = new ErosionSystem(100);
            Assert.AreEqual(ErosionStage.Critical, erosion100.CurrentStage);
        }

        [Test]
        public void Purify_ScalesWithBondLevel()
        {
            // basePower=10, bondLevel=2 → 10 * (1 + 2*0.5) = 10 * 2 = 20
            _erosion.AddErosion(50);
            _erosion.Purify(10, 2);
            Assert.AreEqual(30, _erosion.CurrentErosion);
        }

        [Test]
        public void GetAtkMultiplier_ReturnsCorrectForStage()
        {
            Assert.AreEqual(1.0f, _erosion.GetAtkMultiplier());

            _erosion.AddErosion(30);
            Assert.AreEqual(1.15f, _erosion.GetAtkMultiplier());

            _erosion.AddErosion(30);
            Assert.AreEqual(1.3f, _erosion.GetAtkMultiplier());

            _erosion.AddErosion(30);
            Assert.AreEqual(1.5f, _erosion.GetAtkMultiplier());
        }

        [Test]
        public void GetRampageChance_ZeroForNormalAndUnstable()
        {
            Assert.AreEqual(0f, _erosion.GetRampageChance());

            _erosion.AddErosion(30);
            Assert.AreEqual(0f, _erosion.GetRampageChance());

            _erosion.AddErosion(30);
            Assert.AreEqual(0.15f, _erosion.GetRampageChance());

            _erosion.AddErosion(30);
            Assert.AreEqual(0.4f, _erosion.GetRampageChance());
        }

        [Test]
        public void OnStageChanged_FiresOnTransition()
        {
            ErosionStage? receivedStage = null;
            _erosion.OnStageChanged += stage => receivedStage = stage;

            _erosion.AddErosion(30);
            Assert.AreEqual(ErosionStage.Unstable, receivedStage);

            receivedStage = null;
            _erosion.AddErosion(5);
            Assert.IsNull(receivedStage, "Should not fire when stage does not change");

            _erosion.AddErosion(30);
            Assert.AreEqual(ErosionStage.Dangerous, receivedStage);
        }
    }
}
