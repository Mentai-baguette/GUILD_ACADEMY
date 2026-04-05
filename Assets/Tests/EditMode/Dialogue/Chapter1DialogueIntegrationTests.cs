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
        private static void AdvanceUntilNode(DialogueManager manager, string nodeId, int maxSteps = 64)
        {
            for (var i = 0; i < maxSteps && manager.IsActive && manager.Current?.Id != nodeId; i++)
                manager.Advance();
        }

        private static void AdvanceToEnd(DialogueManager manager, int maxSteps = 64)
        {
            for (var i = 0; i < maxSteps && manager.IsActive; i++)
                manager.Advance();
        }

        [Test]
        public void Chapter1Flow_SelectYunaBranch_MovesToCorrectNodeAndAppliesEffects()
        {
            var flags = new FlagSystem();
            var trust = new TrustSystem();
            var manager = new DialogueManager(new ResourcesDialogueJsonLoader(), flags, trust);

            manager.LoadFromSource("Dialogues/chapter1_dialogue");
            manager.Start("ch1_start_intro");

            AdvanceUntilNode(manager, "ch1_choice_enter_academy");
            manager.SelectChoice(0);

            AdvanceUntilNode(manager, "ch1_choice_consult");

            Assert.AreEqual("ch1_choice_consult", manager.Current.Id);
            Assert.IsTrue(manager.HasChoices);

            manager.SelectChoice(1);

            Assert.AreEqual("ch1_consult_yuna", manager.Current.Id);
            Assert.AreEqual(10, trust.GetTrust(CharacterId.Yuna));
            Assert.IsFalse(flags.Get(FlagSystem.Flags.CarlosPlan));
            Assert.IsFalse(flags.Get(FlagSystem.Flags.DarkPowerRisk));

            AdvanceToEnd(manager);
            Assert.IsFalse(manager.IsActive);
        }

        [Test]
        public void Chapter1Flow_SelectCarlosBranch_AppliesCarlosPlanFlag()
        {
            var flags = new FlagSystem();
            var trust = new TrustSystem();
            var manager = new DialogueManager(new ResourcesDialogueJsonLoader(), flags, trust);

            manager.LoadFromSource("Dialogues/chapter1_dialogue");
            manager.Start("ch1_start_intro");
            AdvanceUntilNode(manager, "ch1_choice_enter_academy");
            manager.SelectChoice(0);

            AdvanceUntilNode(manager, "ch1_choice_consult");

            manager.SelectChoice(0);

            Assert.AreEqual("ch1_consult_carlos", manager.Current.Id);
            Assert.IsTrue(flags.Get(FlagSystem.Flags.CarlosPlan));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Yuna));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Mio));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Kaito));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Shion));

            AdvanceToEnd(manager);
            Assert.IsFalse(manager.IsActive);
        }

        [Test]
        public void Chapter1Flow_SelectHideBranch_AppliesDarkPowerRiskFlag()
        {
            var flags = new FlagSystem();
            var trust = new TrustSystem();
            var manager = new DialogueManager(new ResourcesDialogueJsonLoader(), flags, trust);

            manager.LoadFromSource("Dialogues/chapter1_dialogue");
            manager.Start("ch1_start_intro");

            AdvanceUntilNode(manager, "ch1_choice_enter_academy");
            manager.SelectChoice(0);
            AdvanceUntilNode(manager, "ch1_choice_consult");

            manager.SelectChoice(2);

            Assert.AreEqual("ch1_consult_hide", manager.Current.Id);
            Assert.IsTrue(flags.Get(FlagSystem.Flags.DarkPowerRisk));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Yuna));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Mio));

            AdvanceToEnd(manager);
            Assert.IsFalse(manager.IsActive);
        }

        [Test]
        public void Chapter1Flow_RefuseAcademy_AppliesRefusedFlagAndEnds()
        {
            var flags = new FlagSystem();
            var trust = new TrustSystem();
            var manager = new DialogueManager(new ResourcesDialogueJsonLoader(), flags, trust);

            manager.LoadFromSource("Dialogues/chapter1_dialogue");
            manager.Start("ch1_start_intro");

            AdvanceUntilNode(manager, "ch1_choice_enter_academy");
            Assert.IsTrue(manager.HasChoices);

            manager.SelectChoice(1);

            Assert.AreEqual("ch1_end1_refuse", manager.Current.Id);
            Assert.IsTrue(flags.Get(FlagSystem.Flags.AcademyRefused));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Yuna));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Mio));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Kaito));
            Assert.AreEqual(0, trust.GetTrust(CharacterId.Shion));

            AdvanceToEnd(manager);
            Assert.IsFalse(manager.IsActive);
        }
    }
}
