using GuildAcademy.Core.Branch;
using GuildAcademy.Core.Data;
using GuildAcademy.Core.Dialogue;
using GuildAcademy.MonoBehaviours.Dialogue;
using NUnit.Framework;

namespace GuildAcademy.Tests.EditMode.Dialogue.Unity
{
    [TestFixture]
    public class Chapter1DialogueIntegrationTests
    {
        private static bool AdvanceUntilNode(DialogueManager manager, string nodeId, int maxSteps = 256)
        {
            for (var i = 0; i < maxSteps && manager.IsActive && manager.Current?.Id != nodeId; i++)
                manager.Advance();

            return manager.IsActive && manager.Current?.Id == nodeId;
        }

        [Test]
        public void Chapter1Flow_RefuseAtHome_SetsAcademyRefusedFlag()
        {
            var flags = new FlagSystem();
            var trust = new TrustSystem();
            var manager = new DialogueManager(new ResourcesDialogueJsonLoader(), flags, trust);

            manager.LoadFromSource("Dialogues/chapter1_dialogue");
            manager.Start("ch1_home_yuna_wake");

            Assert.IsTrue(AdvanceUntilNode(manager, "ch1_home_bed_return_4", 64));
            Assert.IsTrue(manager.HasChoices);

            manager.SelectChoice(1);

            Assert.AreEqual("ch1_home_refuse_end", manager.Current.Id);
            Assert.IsTrue(flags.Get(FlagSystem.Flags.AcademyRefused));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Yuna));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Mio));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Kaito));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Shion));
        }

        [Test]
        public void Chapter1Flow_ChooseGoToAcademy_DoesNotSetAcademyRefusedFlag()
        {
            var flags = new FlagSystem();
            var trust = new TrustSystem();
            var manager = new DialogueManager(new ResourcesDialogueJsonLoader(), flags, trust);

            manager.LoadFromSource("Dialogues/chapter1_dialogue");
            manager.Start("ch1_home_yuna_wake");

            Assert.IsTrue(AdvanceUntilNode(manager, "ch1_home_bed_return_4", 64));
            manager.SelectChoice(0);

            Assert.AreEqual("ch1_home_depart", manager.Current.Id);
            Assert.IsFalse(flags.Get(FlagSystem.Flags.AcademyRefused));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Yuna));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Mio));

            Assert.IsTrue(AdvanceUntilNode(manager, "ch1_arrival_return", 256));
            Assert.IsTrue(manager.HasChoices);
        }

        [Test]
        public void Chapter1Flow_WrongWayAtArrival_StillDoesNotSetAcademyRefusedFlag()
        {
            var flags = new FlagSystem();
            var manager = new DialogueManager(new ResourcesDialogueJsonLoader(), flags, new TrustSystem());

            manager.LoadFromSource("Dialogues/chapter1_dialogue");
            manager.Start("ch1_home_yuna_wake");
            Assert.IsTrue(AdvanceUntilNode(manager, "ch1_home_bed_return_4", 64));
            manager.SelectChoice(0);

            Assert.IsTrue(AdvanceUntilNode(manager, "ch1_arrival_return", 256));
            manager.SelectChoice(1);
            Assert.AreEqual("ch1_wrongway_1", manager.Current.Id);

            manager.SelectChoice(1);
            Assert.AreEqual("ch1_wrongway_2", manager.Current.Id);

            Assert.IsFalse(flags.Get(FlagSystem.Flags.AcademyRefused));
        }
    }
}