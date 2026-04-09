using System;
using System.Collections.Generic;
using GuildAcademy.Core.Data;
using GuildAcademy.MonoBehaviours.Field;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace GuildAcademy.EditorTools
{
    public static class AcademyMapScaffolder
    {
        private const string SceneRoot = "Assets/Scenes/Academy";

        private sealed class PortalLink
        {
            public string Name;
            public Vector2 Position;
            public string TargetScene;
            public string TargetSpawn;
        }

        [MenuItem("GuildAcademy/Maps/Create Academy Scene Scaffolds")]
        public static void CreateAcademySceneScaffolds()
        {
            EnsureFolder("Assets/Scenes", "Academy");

            var sceneNames = new[]
            {
                SceneNames.AcademyClassroom,
                SceneNames.AcademyHallway,
                SceneNames.AcademyCafeteria,
                SceneNames.AcademyLibrary,
                SceneNames.AcademySchoolyard,
                SceneNames.AcademyRooftop,
                SceneNames.AcademyStudentCouncilRoom,
                SceneNames.AcademyInfirmary,
                SceneNames.AcademySchoolGate,
            };

            var links = BuildLinks();

            foreach (var sceneName in sceneNames)
            {
                var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                var grid = new GameObject("Grid");
                grid.AddComponent<Grid>().cellSize = Vector3.one;

                CreateTilemap(grid.transform, "Ground");
                CreateTilemap(grid.transform, "Decoration");
                var collision = CreateTilemap(grid.transform, "Collision");

                var rb = collision.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;
                var composite = collision.AddComponent<CompositeCollider2D>();
                var tilemapCollider = collision.AddComponent<TilemapCollider2D>();

                var boundsRoot = new GameObject("CollisionBounds");
                CreateBoundary(boundsRoot.transform, "Top", new Vector2(0f, 9f), new Vector2(20f, 1f));
                CreateBoundary(boundsRoot.transform, "Bottom", new Vector2(0f, -9f), new Vector2(20f, 1f));
                CreateBoundary(boundsRoot.transform, "Left", new Vector2(-10f, 0f), new Vector2(1f, 18f));
                CreateBoundary(boundsRoot.transform, "Right", new Vector2(10f, 0f), new Vector2(1f, 18f));

                var portalRoot = new GameObject("Portals");
                var spawnRoot = new GameObject("SpawnPoints");

                CreateSpawnPoint(spawnRoot.transform, "default", Vector2.zero);

                if (links.TryGetValue(sceneName, out var sceneLinks))
                {
                    foreach (var link in sceneLinks)
                    {
                        CreatePortal(portalRoot.transform, link);
                        CreateSpawnPoint(spawnRoot.transform, link.TargetSpawn, link.Position * -0.7f);
                    }
                }

                var scenePath = SceneRoot + "/" + sceneName + ".unity";
                EditorSceneManager.SaveScene(scene, scenePath);
                Debug.Log($"Created scene: {scenePath}");
            }

            AddScenesToBuildSettings(sceneNames);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Academy Map Scaffolder", "9エリアの雛形シーンを作成しました。", "OK");
        }

        private static void EnsureFolder(string parent, string child)
        {
            var fullPath = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(fullPath))
                AssetDatabase.CreateFolder(parent, child);
        }

        private static GameObject CreateTilemap(Transform parent, string name)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<Tilemap>();
            obj.AddComponent<TilemapRenderer>();
            return obj;
        }

        private static void CreateBoundary(Transform parent, string name, Vector2 position, Vector2 size)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = position;
            var box = obj.AddComponent<BoxCollider2D>();
            box.size = size;
        }

        private static void CreateSpawnPoint(Transform parent, string spawnId, Vector2 localPosition)
        {
            var obj = new GameObject("Spawn_" + spawnId);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPosition;

            var spawn = obj.AddComponent<SceneSpawnPoint>();
            var serialized = new SerializedObject(spawn);
            serialized.FindProperty("_spawnId").stringValue = spawnId;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreatePortal(Transform parent, PortalLink link)
        {
            var obj = new GameObject(link.Name);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = link.Position;

            var collider = obj.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1.5f, 1.5f);

            var portal = obj.AddComponent<ScenePortal2D>();
            var serialized = new SerializedObject(portal);
            serialized.FindProperty("_targetSceneName").stringValue = link.TargetScene;
            serialized.FindProperty("_targetSpawnPointId").stringValue = link.TargetSpawn;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Dictionary<string, List<PortalLink>> BuildLinks()
        {
            return new Dictionary<string, List<PortalLink>>
            {
                {
                    SceneNames.AcademyClassroom,
                    new List<PortalLink>
                    {
                        new PortalLink { Name = "ToHallway", Position = new Vector2(0f, -7f), TargetScene = SceneNames.AcademyHallway, TargetSpawn = "from_classroom" },
                    }
                },
                {
                    SceneNames.AcademyHallway,
                    new List<PortalLink>
                    {
                        new PortalLink { Name = "ToClassroom", Position = new Vector2(0f, 7f), TargetScene = SceneNames.AcademyClassroom, TargetSpawn = "from_hallway" },
                        new PortalLink { Name = "ToCafeteria", Position = new Vector2(-7f, 0f), TargetScene = SceneNames.AcademyCafeteria, TargetSpawn = "from_hallway" },
                        new PortalLink { Name = "ToLibrary", Position = new Vector2(7f, 0f), TargetScene = SceneNames.AcademyLibrary, TargetSpawn = "from_hallway" },
                        new PortalLink { Name = "ToInfirmary", Position = new Vector2(-7f, 5f), TargetScene = SceneNames.AcademyInfirmary, TargetSpawn = "from_hallway" },
                        new PortalLink { Name = "ToStudentCouncil", Position = new Vector2(7f, 5f), TargetScene = SceneNames.AcademyStudentCouncilRoom, TargetSpawn = "from_hallway" },
                        new PortalLink { Name = "ToRooftop", Position = new Vector2(0f, 9f), TargetScene = SceneNames.AcademyRooftop, TargetSpawn = "from_hallway" },
                        new PortalLink { Name = "ToSchoolyard", Position = new Vector2(0f, -9f), TargetScene = SceneNames.AcademySchoolyard, TargetSpawn = "from_hallway" },
                    }
                },
                {
                    SceneNames.AcademyCafeteria,
                    new List<PortalLink>
                    {
                        new PortalLink { Name = "ToHallway", Position = new Vector2(7f, 0f), TargetScene = SceneNames.AcademyHallway, TargetSpawn = "from_cafeteria" },
                    }
                },
                {
                    SceneNames.AcademyLibrary,
                    new List<PortalLink>
                    {
                        new PortalLink { Name = "ToHallway", Position = new Vector2(-7f, 0f), TargetScene = SceneNames.AcademyHallway, TargetSpawn = "from_library" },
                    }
                },
                {
                    SceneNames.AcademySchoolyard,
                    new List<PortalLink>
                    {
                        new PortalLink { Name = "ToHallway", Position = new Vector2(0f, 8f), TargetScene = SceneNames.AcademyHallway, TargetSpawn = "from_schoolyard" },
                        new PortalLink { Name = "ToSchoolGate", Position = new Vector2(0f, -8f), TargetScene = SceneNames.AcademySchoolGate, TargetSpawn = "from_schoolyard" },
                    }
                },
                {
                    SceneNames.AcademyRooftop,
                    new List<PortalLink>
                    {
                        new PortalLink { Name = "ToHallway", Position = new Vector2(0f, -8f), TargetScene = SceneNames.AcademyHallway, TargetSpawn = "from_rooftop" },
                    }
                },
                {
                    SceneNames.AcademyStudentCouncilRoom,
                    new List<PortalLink>
                    {
                        new PortalLink { Name = "ToHallway", Position = new Vector2(-8f, 0f), TargetScene = SceneNames.AcademyHallway, TargetSpawn = "from_student_council" },
                    }
                },
                {
                    SceneNames.AcademyInfirmary,
                    new List<PortalLink>
                    {
                        new PortalLink { Name = "ToHallway", Position = new Vector2(8f, 0f), TargetScene = SceneNames.AcademyHallway, TargetSpawn = "from_infirmary" },
                    }
                },
                {
                    SceneNames.AcademySchoolGate,
                    new List<PortalLink>
                    {
                        new PortalLink { Name = "ToSchoolyard", Position = new Vector2(0f, 8f), TargetScene = SceneNames.AcademySchoolyard, TargetSpawn = "from_school_gate" },
                    }
                },
            };
        }

        private static void AddScenesToBuildSettings(IEnumerable<string> sceneNames)
        {
            var current = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);

            foreach (var sceneName in sceneNames)
            {
                var path = SceneRoot + "/" + sceneName + ".unity";
                var exists = false;
                foreach (var scene in current)
                {
                    if (string.Equals(scene.path, path, StringComparison.Ordinal))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                    current.Add(new EditorBuildSettingsScene(path, true));
            }

            EditorBuildSettings.scenes = current.ToArray();
        }
    }
}
