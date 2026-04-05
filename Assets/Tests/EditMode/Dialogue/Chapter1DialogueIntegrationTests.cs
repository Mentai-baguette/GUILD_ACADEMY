using GuildAcademy.Core.Branch;
using GuildAcademy.Core.Data;
using GuildAcademy.Core.Dialogue;
using GuildAcademy.MonoBehaviours.Dialogue;
using NUnit.Framework;

namespace GuildAcademy.Tests.EditMode.Dialogue
{
    [TestFixture]
    public class Chapter1DialogueIntegrationTests
    {
        [Test]
        public void Chapter1Flow_SelectYunaBranch_MovesToCorrectNodeAndAppliesEffects()
        {
            var flags = new FlagSystem();
            var trust = new TrustSystem();
            var manager = new DialogueManager(new ResourcesDialogueJsonLoader(), flags, trust);

            manager.LoadFromSource("Dialogues/chapter1_dialogue");
            manager.Start("chapter1_start");

            Assert.AreEqual("第1章の幕が上がる。", manager.Current.Text);

            manager.Advance();
            Assert.AreEqual("ch1_entrance_academy", manager.Current.Id);

            manager.Advance();
            Assert.AreEqual("ch1_choice_consult", manager.Current.Id);
            Assert.IsTrue(manager.HasChoices);

            manager.SelectChoice(1);

            Assert.AreEqual("ch1_yuna_talk", manager.Current.Id);
            Assert.IsTrue(flags.Get(FlagSystem.Flags.VesselTruth));
            Assert.AreEqual(5, trust.GetTrust(CharacterId.Yuna));
            Assert.AreEqual(3, trust.GetTrust(CharacterId.Mio));
        }

        [Test]
        public void Chapter1Flow_SelectCarlosBranch_AppliesShionPastFlagAndNegativeTrust()
        {
            var flags = new FlagSystem();
            var trust = new TrustSystem();
            var manager = new DialogueManager(new ResourcesDialogueJsonLoader(), flags, trust);

            manager.LoadFromSource("Dialogues/chapter1_dialogue");
            manager.Start("chapter1_start");
            manager.Advance();
            manager.Advance();

            manager.SelectChoice(0);

            Assert.AreEqual("ch1_carlos_talk", manager.Current.Id);
            Assert.IsTrue(flags.Get(FlagSystem.Flags.ShionPast));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Yuna));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Mio));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Kaito));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Shion), "Shion trust should be clamped at 0.");
        }
    }
}
