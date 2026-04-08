using System.Collections.Generic;
using System.Linq;
using GuildAcademy.MonoBehaviours.Dialogue;
using NUnit.Framework;

namespace GuildAcademy.Tests.EditMode.Dialogue.Unity
{
    [TestFixture]
    public class Chapter1DialogueJsonStructureTests
    {
        private static readonly ResourcesDialogueJsonLoader Loader = new ResourcesDialogueJsonLoader();

        [Test]
        public void Chapter1Json_EntryIdsAreUnique()
        {
            var entries = Loader.LoadEntries("Dialogues/chapter1_dialogue");
            var duplicatedIds = entries
                .Where(e => !string.IsNullOrWhiteSpace(e.Id))
                .GroupBy(e => e.Id)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            Assert.IsEmpty(duplicatedIds, "Duplicated entry ids: " + string.Join(", ", duplicatedIds));
        }

        [Test]
        public void Chapter1Json_AllNextReferencesPointToExistingEntries()
        {
            var entries = Loader.LoadEntries("Dialogues/chapter1_dialogue");
            var ids = new HashSet<string>(entries.Select(e => e.Id));
            var missingReferences = new List<string>();

            foreach (var entry in entries)
            {
                if (!string.IsNullOrWhiteSpace(entry.Next) && !ids.Contains(entry.Next))
                    missingReferences.Add($"entry:{entry.Id} -> next:{entry.Next}");

                if (entry.Choices == null)
                    continue;

                for (var i = 0; i < entry.Choices.Count; i++)
                {
                    var nextId = entry.Choices[i].Next;
                    if (!string.IsNullOrWhiteSpace(nextId) && !ids.Contains(nextId))
                        missingReferences.Add($"entry:{entry.Id} -> choice[{i}].next:{nextId}");
                }
            }

            Assert.IsEmpty(missingReferences, "Broken references: " + string.Join(" | ", missingReferences));
        }

        [Test]
        public void Chapter1Json_HasAcademyRefusedChoiceFlag()
        {
            var entries = Loader.LoadEntries("Dialogues/chapter1_dialogue");

            var hasAcademyRefused = entries
                .Where(e => e.Choices != null)
                .SelectMany(e => e.Choices)
                .Any(choice => choice.Flag == "academy_refused");

            Assert.IsTrue(hasAcademyRefused);
        }
    }
}
