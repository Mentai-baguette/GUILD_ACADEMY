using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;

namespace GuildAcademy.Tests.EditMode.ProjectSettings
{
    [TestFixture]
    public class BuildSettingsValidationTests
    {
        private static readonly string[] ExpectedAcademyScenes =
        {
            "Assets/Scenes/Academy/Academy_Classroom.unity",
            "Assets/Scenes/Academy/Academy_Hallway.unity",
            "Assets/Scenes/Academy/Academy_Cafeteria.unity",
            "Assets/Scenes/Academy/Academy_Library.unity",
            "Assets/Scenes/Academy/Academy_Schoolyard.unity",
            "Assets/Scenes/Academy/Academy_dormitory.unity",
            "Assets/Scenes/Academy/Academy_StudentCouncilRoom.unity",
            "Assets/Scenes/Academy/Academy_SchoolGate.unity",
            "Assets/Scenes/Academy/Academy_TrainingGround.unity",
        };

        [Test]
        public void AcademyScenes_AreRegisteredExactlyOnce_AndPathsMatch()
        {
            var academyScenes = EditorBuildSettings.scenes
                .Where(scene => scene.path.StartsWith("Assets/Scenes/Academy/", StringComparison.Ordinal))
                .ToArray();

            Assert.AreEqual(ExpectedAcademyScenes.Length, academyScenes.Length, "Academy scene count must stay at 9.");
            CollectionAssert.AreEquivalent(ExpectedAcademyScenes, academyScenes.Select(scene => scene.path).ToArray());

            foreach (var scene in academyScenes)
            {
                Assert.IsTrue(scene.enabled, $"Scene must be enabled: {scene.path}");
                Assert.IsTrue(File.Exists(scene.path), $"Scene file missing: {scene.path}");
                Assert.IsTrue(File.Exists(scene.path + ".meta"), $"Scene meta missing: {scene.path}");
            }
        }
    }
}