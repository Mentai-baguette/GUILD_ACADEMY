using System;
using System.Collections.Generic;
using NUnit.Framework;
using GuildAcademy.Core.Dialogue;

namespace GuildAcademy.Tests.EditMode.Dialogue
{
    [TestFixture]
    public class DialogueParserTests
    {
        private DialogueParser _parser;

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
        }

        [Test]
        public void Load_ValidEntries_SetsCount()
        {
            _parser.Load(CreateTestDialogues());
            Assert.AreEqual(4, _parser.EntryCount);
        }

        [Test]
        public void GetById_ExistingId_ReturnsEntry()
        {
            _parser.Load(CreateTestDialogues());
            var entry = _parser.GetById("start");
            Assert.AreEqual("Yuna", entry.Speaker);
            Assert.AreEqual("Hello!", entry.Text);
        }

        [Test]
        public void GetById_NonExistentId_ThrowsKeyNotFoundException()
        {
            _parser.Load(CreateTestDialogues());
            Assert.Throws<KeyNotFoundException>(() => _parser.GetById("nonexistent"));
        }

        [Test]
        public void Load_EmptyId_ThrowsArgumentException()
        {
            var entries = new List<DialogueEntry>
            {
                new DialogueEntry { Id = "", Speaker = "Test", Text = "Test" }
            };
            Assert.Throws<ArgumentException>(() => _parser.Load(entries));
        }

        [Test]
        public void HasEntry_ExistingId_ReturnsTrue()
        {
            _parser.Load(CreateTestDialogues());
            Assert.IsTrue(_parser.HasEntry("start"));
            Assert.IsFalse(_parser.HasEntry("nonexistent"));
        }
    }
}
