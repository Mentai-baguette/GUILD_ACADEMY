using System;
using System.Collections.Generic;
using GuildAcademy.Core.Branch;
using GuildAcademy.Core.Data;
using GuildAcademy.Core.Dialogue;
using NUnit.Framework;

namespace GuildAcademy.Tests.EditMode.Dialogue
{
    [TestFixture]
    public class DialogueManagerTests
    {
        private class FakeDialogueSource : IDialogueSource
        {
            private readonly List<DialogueEntry> _entries;

            public FakeDialogueSource(List<DialogueEntry> entries)
            {
                _entries = entries;
            }

            public List<DialogueEntry> LoadEntries(string sourceKey)
            {
                return _entries;
            }
        }

        private static List<DialogueEntry> CreateEntries()
        {
            return new List<DialogueEntry>
            {
                new DialogueEntry
                {
                    Id = "chapter1_start",
                    Speaker = "Narration",
                    Text = "Start",
                    Next = "ch1_choice"
                },
                new DialogueEntry
                {
                    Id = "ch1_choice",
                    Speaker = "Ray",
                    Text = "Choose",
                    Choices = new List<DialogueChoice>
                    {
                        new DialogueChoice
                        {
                            Text = "Yuna route",
                            Next = "ch1_yuna",
                            Flag = FlagSystem.Flags.VesselTruth,
                            TrustEffects = new Dictionary<string, int>
                            {
                                { "Yuna", 5 }
                            }
                        },
                        new DialogueChoice
                        {
                            Text = "Carlos route",
                            Next = "ch1_carlos",
                            Flag = FlagSystem.Flags.ShionPast,
                            TrustEffects = new Dictionary<string, int>
                            {
                                { "Shion", -10 }
                            }
                        }
                    }
                },
                new DialogueEntry { Id = "ch1_yuna", Speaker = "Yuna", Text = "Y route", Next = "ch1_end" },
                new DialogueEntry { Id = "ch1_carlos", Speaker = "Carlos", Text = "C route", Next = "ch1_end" },
                new DialogueEntry { Id = "ch1_end", Speaker = "Narration", Text = "End" }
            };
        }

        [Test]
        public void Start_WithoutLoad_ThrowsInvalidOperationException()
        {
            var manager = new DialogueManager(new FakeDialogueSource(CreateEntries()), new FlagSystem(), new TrustSystem());
            Assert.Throws<InvalidOperationException>(() => manager.Start("chapter1_start"));
        }

        [Test]
        public void LoadAndStart_RaisesDialogueAdvanced()
        {
            var manager = new DialogueManager(new FakeDialogueSource(CreateEntries()), new FlagSystem(), new TrustSystem());
            DialogueEntry received = null;
            manager.OnDialogueAdvanced += entry => received = entry;

            manager.LoadFromSource("any");
            manager.Start("chapter1_start");

            Assert.IsNotNull(received);
            Assert.AreEqual("chapter1_start", received.Id);
            Assert.AreEqual("Narration", received.Speaker);
        }

        [Test]
        public void SelectChoice_TransitionsAndAppliesFlagAndTrust()
        {
            var flags = new FlagSystem();
            var trust = new TrustSystem();
            var manager = new DialogueManager(new FakeDialogueSource(CreateEntries()), flags, trust);

            manager.LoadFromSource("any");
            manager.Start("chapter1_start");
            manager.Advance();

            Assert.IsTrue(manager.HasChoices);

            manager.SelectChoice(0);

            Assert.AreEqual("ch1_yuna", manager.Current.Id);
            Assert.IsTrue(flags.Get(FlagSystem.Flags.VesselTruth));
            Assert.AreEqual(5, trust.GetTrust(CharacterId.Yuna));
        }
    }
}
