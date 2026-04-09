using System;
using GuildAcademy.MonoBehaviours.Field;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace GuildAcademy.EditorTools
{
    public static class AcademyCourtyardBootstrapper
    {
        private const string ScenePath = "Assets/Scenes/Academy/Academy_Courtyard.unity";
        private const string TileSearchRoot = "Assets/_Project/Data/maptile/tilemap";
        private const float DefaultCameraZ = -10f;

        private static readonly string[] PreferredTileAssetPaths =
        {
            "Assets/_Project/Data/maptile/tilemap/Floor_230.asset",
            "Assets/_Project/Data/maptile/tilemap/Floor_170.asset",
            "Assets/_Project/Data/maptile/tilemap/Floor_131.asset",
            "Assets/_Project/Data/maptile/tilemap/Floor_33.asset",
        };

        [MenuItem("GuildAcademy/Maps/Create Academy Courtyard")]
        public static void CreateAcademyCourtyard()
        {
            var tile = FindFirstTile(out var selectedTilePath);
            if (tile == null)
            {
                EditorUtility.DisplayDialog("Academy Courtyard", "TileBase が見つかりません。先に共通タイル素材を配置してください。", "OK");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateMainCamera();
            var grid = CreateGrid();
            var ground = CreateTilemapLayer(grid.transform, "Ground", 0, false);
            var decoration = CreateTilemapLayer(grid.transform, "Decoration", 1, false);
            var collision = CreateTilemapLayer(grid.transform, "Collision", 2, true);

            BuildGroundLayout(ground, decoration, collision, tile);
            BuildGameplayRoots();

            CreatePortals();
            CreateSpawnPoints();
            CreateNpcSpots();
            CreateEventTriggers();
            CreateFountainCollider();

            ground.CompressBounds();
            decoration.CompressBounds();
            collision.CompressBounds();
            ground.RefreshAllTiles();
            decoration.RefreshAllTiles();
            collision.RefreshAllTiles();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath, true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[AcademyCourtyardBootstrapper] Created {ScenePath} with tile: {selectedTilePath}");
            EditorUtility.DisplayDialog("Academy Courtyard", "Academy_Courtyard シーンを作成しました。", "OK");
        }

        private static void CreateMainCamera()
        {
            var cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            cameraObj.transform.position = new Vector3(0f, 0f, DefaultCameraZ);

            var camera = cameraObj.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6.5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.76f, 0.88f, 0.95f, 1f);
        }

        private static GameObject CreateGrid()
        {
            var gridObj = new GameObject("Grid");
            gridObj.AddComponent<Grid>();
            return gridObj;
        }

        private static Tilemap CreateTilemapLayer(Transform parent, string name, int order, bool withCollision)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var tilemap = go.AddComponent<Tilemap>();
            var renderer = go.AddComponent<TilemapRenderer>();
            renderer.sortingOrder = order;

            if (!withCollision)
                return tilemap;

            renderer.enabled = false;
            var tileCollider = go.AddComponent<TilemapCollider2D>();
            tileCollider.usedByComposite = true;
            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            go.AddComponent<CompositeCollider2D>();

            return tilemap;
        }

        private static void BuildGroundLayout(Tilemap ground, Tilemap decoration, Tilemap collision, TileBase tile)
        {
            FillRect(ground, -18, -12, 18, 12, tile);
            DrawBorder(collision, -18, -12, 18, 12, tile);

            // Center fountain ring with open conversation space around it.
            DrawDisc(decoration, 0, 0, 3, tile);
            DrawDisc(collision, 0, 0, 2, tile);

            // Four flowerbeds around the fountain while keeping central visibility.
            FillRect(decoration, -14, 5, -9, 8, tile);
            FillRect(decoration, 9, 5, 14, 8, tile);
            FillRect(decoration, -14, -8, -9, -5, tile);
            FillRect(decoration, 9, -8, 14, -5, tile);

            FillRect(collision, -14, 5, -9, 8, tile);
            FillRect(collision, 9, 5, 14, 8, tile);
            FillRect(collision, -14, -8, -9, -5, tile);
            FillRect(collision, 9, -8, 14, -5, tile);
        }

        private static void BuildGameplayRoots()
        {
            new GameObject("Portals");
            new GameObject("SpawnPoints");
            new GameObject("NPCSpots");
            new GameObject("EventTriggers");
        }

        private static void CreatePortals()
        {
            var root = GameObject.Find("Portals");
            CreatePortal(root.transform, "Portal_To_Hallway", new Vector2(0f, -11f), new Vector2(2.6f, 1.2f), "Academy_Hallway", "from_courtyard");
            CreatePortal(root.transform, "Portal_To_Schoolyard", new Vector2(17f, 0f), new Vector2(1.2f, 3f), "Academy_Schoolyard", "from_courtyard");
        }

        private static void CreateSpawnPoints()
        {
            var root = GameObject.Find("SpawnPoints");
            CreateSpawnPoint(root.transform, "Spawn_From_Hallway", new Vector2(0f, -10f), "from_hallway", Vector2.up);
            CreateSpawnPoint(root.transform, "Spawn_From_Schoolyard", new Vector2(16f, 0f), "from_schoolyard", Vector2.left);
        }

        private static void CreateNpcSpots()
        {
            var root = GameObject.Find("NPCSpots");
            CreateNpcSpot(root.transform, "NpcSpot_Courtyard_1", new Vector2(-5f, 1.5f));
            CreateNpcSpot(root.transform, "NpcSpot_Courtyard_2", new Vector2(5f, 1.5f));
            CreateNpcSpot(root.transform, "NpcSpot_Courtyard_3", new Vector2(-10.5f, -1f));
            CreateNpcSpot(root.transform, "NpcSpot_Courtyard_4", new Vector2(10.5f, -1f));
            CreateNpcSpot(root.transform, "NpcSpot_Courtyard_5", new Vector2(0f, 6f));
        }

        private static void CreateEventTriggers()
        {
            var root = GameObject.Find("EventTriggers");
            CreateEventTrigger(root.transform, "Event_Courtyard_SL_1", new Vector2(-3.5f, 1f), "courtyard_sl_01", false, "Courtyard SL event 01");
            CreateEventTrigger(root.transform, "Event_Courtyard_SL_2", new Vector2(3.5f, 1f), "courtyard_sl_02", false, "Courtyard SL event 02");
            CreateEventTrigger(root.transform, "Event_Courtyard_SL_3", new Vector2(0f, 6.2f), "courtyard_sl_03", false, "Courtyard SL event 03");
            CreateEventTrigger(root.transform, "Event_Courtyard_Sunday_1", new Vector2(-10f, 0f), "courtyard_sunday_01", true, "Courtyard Sunday event 01");
            CreateEventTrigger(root.transform, "Event_Courtyard_Sunday_2", new Vector2(10f, 0f), "courtyard_sunday_02", true, "Courtyard Sunday event 02");
        }

        private static void CreateFountainCollider()
        {
            var fountain = new GameObject("Fountain_Center");
            fountain.transform.position = Vector3.zero;
            var collider = fountain.AddComponent<CircleCollider2D>();
            collider.radius = 1.8f;
        }

        private static void CreatePortal(Transform parent, string name, Vector2 localPos, Vector2 size, string targetScene, string targetSpawnPointId)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;

            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = size;

            var portal = go.AddComponent<ScenePortal2D>();
            var so = new SerializedObject(portal);
            so.FindProperty("_targetSceneName").stringValue = targetScene;
            so.FindProperty("_targetSpawnPointId").stringValue = targetSpawnPointId;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateSpawnPoint(Transform parent, string name, Vector2 localPos, string spawnId, Vector2 facing)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;

            var spawnPoint = go.AddComponent<SceneSpawnPoint>();
            var so = new SerializedObject(spawnPoint);
            so.FindProperty("_spawnId").stringValue = spawnId;
            so.FindProperty("_facingDirection").vector2Value = facing.normalized;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateNpcSpot(Transform parent, string name, Vector2 localPos)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
        }

        private static void CreateEventTrigger(Transform parent, string name, Vector2 localPos, string eventId, bool oneShot, string message)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;

            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1.4f, 1.4f);

            var trigger = go.AddComponent<AreaEventTrigger2D>();
            var so = new SerializedObject(trigger);
            so.FindProperty("_eventId").stringValue = eventId;
            so.FindProperty("_debugMessage").stringValue = message;
            so.FindProperty("_oneShot").boolValue = oneShot;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static TileBase FindFirstTile(out string selectedTilePath)
        {
            selectedTilePath = string.Empty;

            foreach (var preferredPath in PreferredTileAssetPaths)
            {
                var preferredTile = AssetDatabase.LoadAssetAtPath<TileBase>(preferredPath);
                if (IsValidForBootstrap(preferredTile))
                {
                    selectedTilePath = preferredPath;
                    return preferredTile;
                }
            }

            var guids = AssetDatabase.FindAssets("t:TileBase", new[] { TileSearchRoot });
            if (guids == null || guids.Length == 0)
                return null;

            Array.Sort(guids, StringComparer.Ordinal);

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var tile = AssetDatabase.LoadAssetAtPath<TileBase>(path);
                if (!IsValidForBootstrap(tile))
                    continue;

                selectedTilePath = path;
                return tile;
            }

            return null;
        }

        private static bool IsValidForBootstrap(TileBase tile)
        {
            if (tile == null)
                return false;

            if (tile is not Tile unityTile)
                return false;

            if (unityTile.sprite == null)
                return false;

            return unityTile.sprite.rect.width > 1f && unityTile.sprite.rect.height > 1f;
        }

        private static void FillRect(Tilemap tilemap, int minX, int minY, int maxX, int maxY, TileBase tile)
        {
            var width = maxX - minX + 1;
            var height = maxY - minY + 1;
            var bounds = new BoundsInt(minX, minY, 0, width, height, 1);
            var tiles = new TileBase[width * height];

            for (var i = 0; i < tiles.Length; i++)
            {
                tiles[i] = tile;
            }

            tilemap.SetTilesBlock(bounds, tiles);
        }

        private static void DrawBorder(Tilemap tilemap, int minX, int minY, int maxX, int maxY, TileBase tile)
        {
            for (var x = minX; x <= maxX; x++)
            {
                tilemap.SetTile(new Vector3Int(x, minY, 0), tile);
                tilemap.SetTile(new Vector3Int(x, maxY, 0), tile);
            }

            for (var y = minY; y <= maxY; y++)
            {
                tilemap.SetTile(new Vector3Int(minX, y, 0), tile);
                tilemap.SetTile(new Vector3Int(maxX, y, 0), tile);
            }
        }

        private static void DrawDisc(Tilemap tilemap, int centerX, int centerY, int radius, TileBase tile)
        {
            var radiusSq = radius * radius;

            for (var y = centerY - radius; y <= centerY + radius; y++)
            {
                for (var x = centerX - radius; x <= centerX + radius; x++)
                {
                    var dx = x - centerX;
                    var dy = y - centerY;
                    if ((dx * dx) + (dy * dy) <= radiusSq)
                        tilemap.SetTile(new Vector3Int(x, y, 0), tile);
                }
            }
        }
    }
}
