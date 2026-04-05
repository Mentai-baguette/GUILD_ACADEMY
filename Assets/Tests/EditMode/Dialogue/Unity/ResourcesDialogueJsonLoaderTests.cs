using System.IO;
using GuildAcademy.MonoBehaviours.Dialogue;
using NUnit.Framework;

namespace GuildAcademy.Tests.EditMode.Dialogue.Unity
{
    [TestFixture]
    public class ResourcesDialogueJsonLoaderTests
    {
        [Test]
        public void LoadEntries_Chapter1DialogueFromResources_ReturnsEntries()
        {
            var loader = new ResourcesDialogueJsonLoader();

            var entries = loader.LoadEntries("Dialogues/chapter1_dialogue");

            Assert.IsNotNull(entries);
            Assert.Greater(entries.Count, 0);
            Assert.AreEqual("ch1_start_intro", entries[0].Id);
        }

        [Test]
        public void LoadEntries_MissingResource_ThrowsFileNotFoundException()
        {
            var loader = new ResourcesDialogueJsonLoader();

            Assert.Throws<FileNotFoundException>(() => loader.LoadEntries("Dialogues/not_found_dialogue"));
        }
    }
}