using System;
using System.Collections.Generic;
using System.IO;
using GuildAcademy.Core.Data;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace GuildAcademy.EditorTools
{
    public static class AcademyCourtyardBootstrapper
    {
        private const string SceneRoot = "Assets/Scenes/Academy";
        private const string CourtyardScenePath = SceneRoot + "/" + SceneNames.AcademyCourtyard + ".unity";
        private const string HallwayScenePath = SceneRoot + "/" + SceneNames.AcademyHallway + ".unity";
        private const string SchoolyardScenePath = SceneRoot + "/" + SceneNames.AcademySchoolyard + ".unity";

        private sealed class PortalLink
        {
            public string Name;
            public Vector2 Position;
        }

        [MenuItem("GuildAcademy/Maps/Create Academy Courtyard")]
        public static void CreateAcademyCourtyard()
        {
            EnsureFolder("Assets/Scenes", "Academy");

            var hallwayExists = File.Exists(HallwayScenePath);
            var schoolyardExists = File.Exists(SchoolyardScenePath);

            CreateOrUpdateCourtyardScene();
            CreateOrUpdateHallwayLink();
            CreateOrUpdateSchoolyardLink();

            AddSceneToBuildSettings(CourtyardScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (!Application.isBatchMode)
            {
                var message = "Academy_Courtyard を生成しました。";
                if (!hallwayExists || !schoolyardExists)
                {
                    message += "\n一部接続更新をスキップしました。";
                    if (!hallwayExists)
                        message += "\n- Hallway scene が未存在";
                    if (!schoolyardExists)
                        message += "\n- Schoolyard scene が未存在";
                }

                EditorUtility.DisplayDialog("Academy Courtyard", message, "OK");
            }
        }

        private static void CreateOrUpdateCourtyardScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var sceneName = SceneNames.AcademyCourtyard;

            CreateCourtyardBase(scene);
            CreateCourtyardLayout(scene);
            SaveScene(scene, CourtyardScenePath, sceneName);
        }

        private static void CreateOrUpdateHallwayLink()
        {
            if (!File.Exists(HallwayScenePath))
            {
                Debug.LogWarning("Skipped Hallway link update. Scene not found: " + HallwayScenePath);
                return;
            }

            var scene = EditorSceneManager.OpenScene(HallwayScenePath, OpenSceneMode.Single);
            EnsurePortal(scene, new PortalLink
            {
                Name = "ToCourtyard",
                Position = new Vector2(0f, -9f),
            });
            EnsureSpawnPoint(scene, "from_courtyard", new Vector2(0f, -5.5f));
            SaveScene(scene, HallwayScenePath, SceneNames.AcademyHallway);
        }

        private static void CreateOrUpdateSchoolyardLink()
        {
            if (!File.Exists(SchoolyardScenePath))
            {
                Debug.LogWarning("Skipped Schoolyard link update. Scene not found: " + SchoolyardScenePath);
                return;
            }

            var scene = EditorSceneManager.OpenScene(SchoolyardScenePath, OpenSceneMode.Single);
            EnsurePortal(scene, new PortalLink
            {
                Name = "ToCourtyard",
                Position = new Vector2(0f, 9f),
            });
            EnsureSpawnPoint(scene, "from_courtyard", new Vector2(0f, 5.5f));
            SaveScene(scene, SchoolyardScenePath, SceneNames.AcademySchoolyard);
        }

        private static void CreateCourtyardBase(Scene scene)
        {
            var grid = new GameObject("Grid");
            SceneManager.MoveGameObjectToScene(grid, scene);
            grid.AddComponent<Grid>().cellSize = Vector3.one;

            CreateTilemap(grid.transform, "Ground");
            CreateTilemap(grid.transform, "Decoration");
            var collision = CreateTilemap(grid.transform, "Collision");

            var rigidbody = collision.AddComponent<Rigidbody2D>();
            rigidbody.bodyType = RigidbodyType2D.Static;
            var tilemapCollider = collision.AddComponent<TilemapCollider2D>();
            tilemapCollider.usedByComposite = true;
            collision.AddComponent<CompositeCollider2D>();

            var camera = new GameObject("Main Camera");
            SceneManager.MoveGameObjectToScene(camera, scene);
            camera.tag = "MainCamera";
            camera.AddComponent<Camera>();
            camera.AddComponent<AudioListener>();
            camera.transform.position = new Vector3(0f, 0f, -10f);

            var portals = new GameObject("Portals");
            SceneManager.MoveGameObjectToScene(portals, scene);

            var spawnPoints = new GameObject("SpawnPoints");
            SceneManager.MoveGameObjectToScene(spawnPoints, scene);

            CreateSpawnPoint(spawnPoints.transform, "default", Vector2.zero);
            CreateSpawnPoint(spawnPoints.transform, "from_hallway", new Vector2(0f, 5.5f));
            CreateSpawnPoint(spawnPoints.transform, "from_schoolyard", new Vector2(0f, -5.5f));

            var savePoint = new GameObject("SavePoint_S");
            SceneManager.MoveGameObjectToScene(savePoint, scene);
            savePoint.transform.position = Vector3.zero;
        }

        private static void CreateCourtyardLayout(Scene scene)
        {
            EnsurePortal(scene, new PortalLink
            {
                Name = "ToHallway",
                Position = new Vector2(0f, 8.5f),
            });

            EnsurePortal(scene, new PortalLink
            {
                Name = "ToSchoolyard",
                Position = new Vector2(0f, -8.5f),
            });

            var savePoint = FindOrCreateRoot(scene, "SavePoint_S");
            savePoint.transform.position = Vector3.zero;
        }

        private static void EnsurePortal(Scene scene, PortalLink link)
        {
            var root = FindOrCreateRoot(scene, "Portals");
            var existing = FindChild(root.transform, link.Name);
            if (existing == null)
            {
                existing = new GameObject(link.Name);
                existing.transform.SetParent(root.transform, false);
            }

            existing.transform.localPosition = link.Position;
        }

        private static void EnsureSpawnPoint(Scene scene, string spawnId, Vector2 localPosition)
        {
            var root = FindOrCreateRoot(scene, "SpawnPoints");
            var existing = FindChild(root.transform, "Spawn_" + spawnId);
            if (existing == null)
            {
                existing = new GameObject("Spawn_" + spawnId);
                existing.transform.SetParent(root.transform, false);
            }

            existing.transform.localPosition = localPosition;
        }

        private static GameObject CreateTilemap(Transform parent, string name)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<Tilemap>();
            obj.AddComponent<TilemapRenderer>();
            return obj;
        }

        private static void CreateSpawnPoint(Transform parent, string spawnId, Vector2 localPosition)
        {
            var obj = new GameObject("Spawn_" + spawnId);
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPosition;
        }

        private static GameObject FindOrCreateRoot(Scene scene, string name)
        {
            var root = FindRoot(scene, name);
            if (root != null)
                return root;

            root = new GameObject(name);
            SceneManager.MoveGameObjectToScene(root, scene);
            return root;
        }

        private static GameObject FindRoot(Scene scene, string name)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                if (string.Equals(root.name, name, StringComparison.Ordinal))
                    return root;
            }

            return null;
        }

        private static GameObject FindChild(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (string.Equals(child.name, name, StringComparison.Ordinal))
                    return child.gameObject;
            }

            return null;
        }

        private static void SaveScene(Scene scene, string path, string sceneName)
        {
            var saved = EditorSceneManager.SaveScene(scene, path);
            if (!saved)
                throw new InvalidOperationException("Failed to save scene: " + path);

            Debug.Log("Saved scene: " + sceneName + " -> " + path);
        }

        private static void EnsureFolder(string parent, string child)
        {
            var fullPath = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(fullPath))
                AssetDatabase.CreateFolder(parent, child);
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            var current = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            foreach (var scene in current)
            {
                if (string.Equals(scene.path, scenePath, StringComparison.Ordinal))
                    return;
            }

            current.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = current.ToArray();
        }
    }
}