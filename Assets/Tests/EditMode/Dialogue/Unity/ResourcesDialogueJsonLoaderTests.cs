using System;
using System.IO;
using GuildAcademy.MonoBehaviours.Dialogue;
using NUnit.Framework;

namespace GuildAcademy.Tests.EditMode.Dialogue.Unity
{
    [TestFixture]
    public class ResourcesDialogueJsonLoaderTests
    {
        private readonly ResourcesDialogueJsonLoader _loader = new ResourcesDialogueJsonLoader();

        [Test]
        public void LoadEntries_Chapter1DialogueFromResources_ReturnsEntries()
        {
            var entries = _loader.LoadEntries("Dialogues/chapter1_dialogue");

            Assert.IsNotNull(entries);
            Assert.Greater(entries.Count, 0);
            Assert.AreEqual("ch1_home_yuna_wake", entries[0].Id);
        }

        [Test]
        public void LoadEntries_MissingResource_ThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => _loader.LoadEntries("Dialogues/not_found_dialogue"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void LoadEntries_InvalidSourceKey_ThrowsArgumentException(string sourceKey)
        {
            var ex = Assert.Throws<ArgumentException>(() => _loader.LoadEntries(sourceKey));
            Assert.That(ex.Message, Does.Contain("Source key cannot be null or empty."));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("\n\t")]
        public void Parse_EmptyJson_ThrowsArgumentException(string json)
        {
            var ex = Assert.Throws<ArgumentException>(() => _loader.Parse(json, "test_source"));
            Assert.That(ex.Message, Does.Contain("JSON content is empty."));
        }

        [Test]
        public void Parse_MalformedJson_ThrowsInvalidDataException()
        {
            const string malformedJson = "{\"entries\": [{\"id\": \"x\"";

            var ex = Assert.Throws<InvalidDataException>(() => _loader.Parse(malformedJson, "malformed"));
            Assert.That(ex.Message, Does.Contain("Failed to parse dialogue JSON: malformed"));
        }

        [Test]
        public void Parse_MissingEntries_ThrowsInvalidDataException()
        {
            const string json = "{\"foo\":\"bar\"}";

            var ex = Assert.Throws<InvalidDataException>(() => _loader.Parse(json, "no_entries"));
            Assert.That(ex.Message, Does.Contain("Dialogue JSON has no entries: no_entries"));
        }

        [Test]
        public void Parse_EmptyEntries_ThrowsInvalidDataException()
        {
            const string json = "{\"entries\": []}";

            var ex = Assert.Throws<InvalidDataException>(() => _loader.Parse(json, "empty_entries"));
            Assert.That(ex.Message, Does.Contain("Dialogue JSON has no entries: empty_entries"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Parse_EmptyEntryId_ThrowsInvalidDataException(string id)
        {
            var json =
                "{\"entries\":[{\"id\":" +
                (id == null ? "null" : "\"" + id + "\"") +
                ",\"speaker\":\"Narration\",\"text\":\"hello\"}]}";

            var ex = Assert.Throws<InvalidDataException>(() => _loader.Parse(json, "empty_id"));
            Assert.That(ex.Message, Does.Contain("Dialogue entry id is empty: empty_id"));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void Parse_EmptyTrustEffectsCharacterId_ThrowsInvalidDataException(string characterId)
        {
            var json =
                "{\"entries\":[{\"id\":\"entry_1\",\"choices\":[{\"text\":\"choice\",\"trustEffects\":[{\"characterId\":" +
                (characterId == null ? "null" : "\"" + characterId + "\"") +
                ",\"value\":1}]}]}]}";

            var ex = Assert.Throws<InvalidDataException>(() => _loader.Parse(json, "empty_character_id"));
            Assert.That(ex.Message, Does.Contain("trustEffects.characterId cannot be empty."));
        }

    }
}