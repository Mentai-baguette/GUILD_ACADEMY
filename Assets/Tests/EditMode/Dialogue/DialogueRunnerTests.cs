using System;
using System.Collections.Generic;
using NUnit.Framework;
using GuildAcademy.Core.Branch;
using GuildAcademy.Core.Data;
using GuildAcademy.Core.Dialogue;

namespace GuildAcademy.Tests.EditMode.Dialogue
{
    [TestFixture]
    public class DialogueRunnerTests
    {
        private DialogueParser _parser;
        private FlagSystem _flags;
        private TrustSystem _trust;
        private DialogueRunner _runner;

        private List<DialogueEntry> CreateTestDialogues()
        {
            return new List<DialogueEntry>
            {
                new DialogueEntry { Id = "start", Speaker = "Yuna", Text = "Hello!", Next = "middle" },
                new DialogueEntry { Id = "middle", Speaker = "Ray", Text = "Hi.", Next = "end" },
                new DialogueEntry { Id = "end", Speaker = "Yuna", Text = "Bye!" },
                new DialogueEntry
                {
                    Id = "choice_node", Speaker = "Ray", Text = "What do?",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice { Text = "Go", Next = "end", Flag = "academy_refused", TrustEffects = new Dictionary<string, int> { { "Yuna", 5 } } },
                        new DialogueChoice { Text = "Stay", Next = "middle" }
                    }
                }
            };
        }

        [SetUp]
        public void SetUp()
        {
            _parser = new DialogueParser();
            _parser.Load(CreateTestDialogues());
            _flags = new FlagSystem();
            _trust = new TrustSystem();
            _runner = new DialogueRunner(_parser, _flags, _trust);
        }

        [Test]
        public void Start_SetsCurrentEntry()
        {
            _runner.Start("start");
            Assert.AreEqual("start", _runner.Current.Id);
            Assert.AreEqual("Yuna", _runner.Current.Speaker);
        }

        [Test]
        public void Start_FiresOnDialogueAdvanced()
        {
            DialogueEntry received = null;
            _runner.OnDialogueAdvanced += entry => received = entry;

            _runner.Start("start");
            Assert.IsNotNull(received);
            Assert.AreEqual("start", received.Id);
        }

        [Test]
        public void Advance_MovesToNextEntry()
        {
            _runner.Start("start");
            _runner.Advance();
            Assert.AreEqual("middle", _runner.Current.Id);
        }

        [Test]
        public void Advance_AtEnd_EndsDialogue()
        {
            bool ended = false;
            _runner.OnDialogueEnded += () => ended = true;

            _runner.Start("end");
            _runner.Advance();

            Assert.IsTrue(ended);
            Assert.IsFalse(_runner.IsActive);
        }

        [Test]
        public void Advance_WithChoices_DoesNotAdvance()
        {
            _runner.Start("choice_node");
            _runner.Advance();
            Assert.AreEqual("choice_node", _runner.Current.Id);
        }

        [Test]
        public void SelectChoice_AppliesFlag()
        {
            _runner.Start("choice_node");
            _runner.SelectChoice(0);
            Assert.IsTrue(_flags.Get("academy_refused"));
        }

        [Test]
        public void SelectChoice_AppliesTrustEffect()
        {
            _runner.Start("choice_node");
            _runner.SelectChoice(0);
            Assert.AreEqual(5, _trust.GetTrust(CharacterId.Yuna));
        }

        [Test]
        public void SelectChoice_InvalidIndex_ThrowsArgumentOutOfRangeException()
        {
            _runner.Start("choice_node");
            Assert.Throws<ArgumentOutOfRangeException>(() => _runner.SelectChoice(5));
        }

        [Test]
        public void IsActive_AfterStart_ReturnsTrue()
        {
            _runner.Start("start");
            Assert.IsTrue(_runner.IsActive);
        }

        [Test]
        public void IsActive_AfterEnd_ReturnsFalse()
        {
            _runner.Start("end");
            _runner.Advance();
            Assert.IsFalse(_runner.IsActive);
        }

        [Test]
        public void End_FiresOnDialogueEnded()
        {
            _runner.Start("start");
            bool ended = false;
            _runner.OnDialogueEnded += () => ended = true;
            _runner.End();
            Assert.IsTrue(ended);
            Assert.IsFalse(_runner.IsActive);
        }

        [Test]
        public void Start_WithChoices_FiresOnChoicesPresented()
        {
            List<DialogueChoice> presentedChoices = null;
            _runner.OnChoicesPresented += choices => presentedChoices = choices;
            _runner.Start("choice_node");
            Assert.IsNotNull(presentedChoices);
            Assert.AreEqual(2, presentedChoices.Count);
        }
    }
}
