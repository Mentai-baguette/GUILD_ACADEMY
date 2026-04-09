using System;
using System.Collections.Generic;
using System.Linq;
using GuildAcademy.Core.Branch;
using GuildAcademy.Core.Dialogue;
using GuildAcademy.MonoBehaviours.Dialogue;
using NUnit.Framework;

namespace GuildAcademy.Tests.EditMode.Dialogue.Unity
{
    [TestFixture]
    public class Chapter1DialogueBranchCoverageTests
    {
        private const string SourceKey = "Dialogues/chapter1_dialogue";
        private const string StartNodeId = "ch1_home_yuna_wake";

        private sealed class Step
        {
            public bool IsChoice;
            public int ChoiceIndex;
        }

        private static Dictionary<string, List<Step>> BuildRepresentativePaths(List<DialogueEntry> entries)
        {
            var byId = entries.ToDictionary(e => e.Id);
            var paths = new Dictionary<string, List<Step>>
            {
                [StartNodeId] = new List<Step>()
            };

            var queue = new Queue<string>();
            queue.Enqueue(StartNodeId);

            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                var current = byId[currentId];
                var pathToCurrent = paths[currentId];

                if (current.Choices != null && current.Choices.Count > 0)
                {
                    for (var i = 0; i < current.Choices.Count; i++)
                    {
                        var nextId = current.Choices[i].Next;
                        if (string.IsNullOrWhiteSpace(nextId) || !byId.ContainsKey(nextId) || paths.ContainsKey(nextId))
                            continue;

                        var nextPath = new List<Step>(pathToCurrent)
                        {
                            new Step { IsChoice = true, ChoiceIndex = i }
                        };

                        paths[nextId] = nextPath;
                        queue.Enqueue(nextId);
                    }

                    continue;
                }

                if (string.IsNullOrWhiteSpace(current.Next) || !byId.ContainsKey(current.Next) || paths.ContainsKey(current.Next))
                    continue;

                var advancedPath = new List<Step>(pathToCurrent)
                {
                    new Step { IsChoice = false }
                };

                paths[current.Next] = advancedPath;
                queue.Enqueue(current.Next);
            }

            return paths;
        }

        private static DialogueManager StartAndReplay(List<Step> steps)
        {
            var manager = new DialogueManager(new ResourcesDialogueJsonLoader(), new FlagSystem(), new TrustSystem());
            manager.LoadFromSource(SourceKey);
            manager.Start(StartNodeId);

            foreach (var step in steps)
            {
                if (!manager.IsActive)
                    throw new InvalidOperationException("Dialogue ended while replaying path.");

                if (step.IsChoice)
                {
                    if (!manager.HasChoices)
                        throw new InvalidOperationException("Expected choices during replay.");

                    manager.SelectChoice(step.ChoiceIndex);
                }
                else
                {
                    manager.Advance();
                }
            }

            return manager;
        }

        [Test]
        public void Chapter1Choices_AllChoiceNodesReachableFromStart()
        {
            var loader = new ResourcesDialogueJsonLoader();
            var entries = loader.LoadEntries(SourceKey);
            var byId = entries.ToDictionary(e => e.Id);
            var paths = BuildRepresentativePaths(entries);

            var choiceNodeIds = entries
                .Where(e => e.Choices != null && e.Choices.Count > 0)
                .Select(e => e.Id)
                .ToList();

            var unreachable = choiceNodeIds.Where(id => !paths.ContainsKey(id)).ToList();
            Assert.IsEmpty(unreachable, "Unreachable choice nodes: " + string.Join(", ", unreachable));

            foreach (var nodeId in choiceNodeIds)
            {
                var manager = StartAndReplay(paths[nodeId]);
                Assert.IsTrue(manager.IsActive, "Dialogue inactive at node: " + nodeId);
                Assert.AreEqual(nodeId, manager.Current.Id, "Reached unexpected node while validating reachability.");
                Assert.IsTrue(manager.HasChoices, "Expected node to present choices: " + nodeId);

                var expectedChoiceCount = byId[nodeId].Choices.Count;
                Assert.AreEqual(expectedChoiceCount, manager.Current.Choices.Count, "Choice count mismatch at node: " + nodeId);
            }
        }

        [Test]
        public void Chapter1Choices_AllChoiceTransitionsMatchJson()
        {
            var loader = new ResourcesDialogueJsonLoader();
            var entries = loader.LoadEntries(SourceKey);
            var byId = entries.ToDictionary(e => e.Id);
            var paths = BuildRepresentativePaths(entries);

            var choiceNodes = entries.Where(e => e.Choices != null && e.Choices.Count > 0).ToList();
            var failures = new List<string>();

            foreach (var node in choiceNodes)
            {
                if (!paths.TryGetValue(node.Id, out var pathToNode))
                {
                    failures.Add($"node:{node.Id} is unreachable");
                    continue;
                }

                for (var i = 0; i < node.Choices.Count; i++)
                {
                    var manager = StartAndReplay(pathToNode);
                    var choice = node.Choices[i];

                    if (!manager.HasChoices)
                    {
                        failures.Add($"node:{node.Id} missing choices at runtime");
                        break;
                    }

                    manager.SelectChoice(i);

                    if (string.IsNullOrWhiteSpace(choice.Next))
                    {
                        if (manager.IsActive)
                            failures.Add($"node:{node.Id} choice[{i}] expected end, but dialogue still active at {manager.Current?.Id}");

                        continue;
                    }

                    if (!byId.ContainsKey(choice.Next))
                    {
                        failures.Add($"node:{node.Id} choice[{i}] points to missing id:{choice.Next}");
                        continue;
                    }

                    if (!manager.IsActive)
                    {
                        failures.Add($"node:{node.Id} choice[{i}] expected next:{choice.Next}, but dialogue ended");
                        continue;
                    }

                    if (manager.Current.Id != choice.Next)
                        failures.Add($"node:{node.Id} choice[{i}] expected next:{choice.Next}, actual:{manager.Current.Id}");
                }
            }

            Assert.IsEmpty(failures, "Choice transition failures: " + string.Join(" | ", failures));
        }
    }
}