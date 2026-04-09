using System;
using System.Collections.Generic;
using GuildAcademy.MonoBehaviours.Field;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace GuildAcademy.EditorTools
{
    public static class AcademyMapBootstrapper
    {
        private const string SceneRoot = "Assets/Scenes/Academy";
        private const string TileSearchRoot = "Assets/_Project/Data/maptile/tilemap";
        private const float MinimumCameraSize = 1f;
        private const float DefaultCameraSize = 5f;
        private const float DefaultCameraZ = -10f;
        private static readonly string[] PreferredTileAssetPaths =
        {
            "Assets/_Project/Data/maptile/tilemap/Floor_230.asset",
            "Assets/_Project/Data/maptile/tilemap/Floor_170.asset",
            "Assets/_Project/Data/maptile/tilemap/Floor_131.asset",
            "Assets/_Project/Data/maptile/tilemap/Floor_33.asset",
        };

        private sealed class SceneLayout
        {
            public string SceneName;
            public int HalfWidth;
            public int HalfHeight;
            public Vector2[] NpcPoints;
            public Vector2[] EventPoints;
        }

        private readonly struct BootstrapReport
        {
            public BootstrapReport(string sceneName, int groundPlacedTiles, int collisionPlacedTiles, bool groundHasCenterTile, bool collisionHasBottomLeftBorderTile, int collisionBoundaryColliderCount, int npcSpotCount, int eventTriggerCount, int adjustedCameraCount)
            {
                SceneName = sceneName;
                GroundPlacedTiles = groundPlacedTiles;
                CollisionPlacedTiles = collisionPlacedTiles;
                GroundHasCenterTile = groundHasCenterTile;
                CollisionHasBottomLeftBorderTile = collisionHasBottomLeftBorderTile;
                CollisionBoundaryColliderCount = collisionBoundaryColliderCount;
                NpcSpotCount = npcSpotCount;
                EventTriggerCount = eventTriggerCount;
                AdjustedCameraCount = adjustedCameraCount;
            }

            public string SceneName { get; }
            public int GroundPlacedTiles { get; }
            public int CollisionPlacedTiles { get; }
            public bool GroundHasCenterTile { get; }
            public bool CollisionHasBottomLeftBorderTile { get; }
            public int CollisionBoundaryColliderCount { get; }
            public int NpcSpotCount { get; }
            public int EventTriggerCount { get; }
            public int AdjustedCameraCount { get; }
        }

        [MenuItem("GuildAcademy/Maps/Bootstrap Academy 9 Areas")]
        public static void BootstrapAcademyMaps()
        {
            var groundTile = FindFirstTile(out var selectedTilePath);
            if (groundTile == null)
            {
                EditorUtility.DisplayDialog("Academy Map Bootstrapper", "TileBase が見つかりません。先に共通タイル素材を配置してください。", "OK");
                return;
            }

            Debug.Log($"[AcademyMapBootstrapper] Selected tile: {selectedTilePath}");

            var reports = new List<BootstrapReport>();

            foreach (var layout in BuildLayouts())
            {
                var report = ApplyLayout(layout, groundTile);
                reports.Add(report);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            LogSummary(reports, selectedTilePath);
            EditorUtility.DisplayDialog("Academy Map Bootstrapper", "9エリアに初期レイアウトとイベントトリガーを配置しました。", "OK");
        }

        private static BootstrapReport ApplyLayout(SceneLayout layout, TileBase groundTile)
        {
            var scenePath = $"{SceneRoot}/{layout.SceneName}.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            var grid = GameObject.Find("Grid");
            if (grid == null)
                throw new InvalidOperationException($"Grid が見つかりません: {layout.SceneName}");

            var ground = FindChildTilemap(grid.transform, "Ground");
            var decoration = FindChildTilemap(grid.transform, "Decoration");
            var collision = FindChildTilemap(grid.transform, "Collision");
            var collisionRenderer = collision.GetComponent<TilemapRenderer>();
            if (collisionRenderer != null)
                collisionRenderer.enabled = false;

            ground.ClearAllTiles();
            decoration.ClearAllTiles();
            collision.ClearAllTiles();

            var groundPlacedTiles = FillRect(ground, -layout.HalfWidth, -layout.HalfHeight, layout.HalfWidth, layout.HalfHeight, groundTile);

            // 衝突境界はタイルでも引いておき、TilemapCollider2D の可視確認を容易にする。
            var collisionPlacedTiles = DrawBorder(collision, -layout.HalfWidth, -layout.HalfHeight, layout.HalfWidth, layout.HalfHeight, groundTile);

            ground.CompressBounds();
            collision.CompressBounds();
            ground.RefreshAllTiles();
            collision.RefreshAllTiles();
            EditorUtility.SetDirty(ground);
            EditorUtility.SetDirty(collision);

            var npcSpotCount = EnsureNpcSpots(layout.SceneName, layout.NpcPoints);
            var eventTriggerCount = EnsureEventTriggers(layout.SceneName, layout.EventPoints);
            var adjustedCameraCount = NormalizeSceneCameras(layout.SceneName);

            var groundHasCenterTile = ground.GetTile(new Vector3Int(0, 0, 0)) != null;
            var collisionHasBottomLeftBorderTile = collision.GetTile(new Vector3Int(-layout.HalfWidth, -layout.HalfHeight, 0)) != null;
            var collisionBoundaryColliderCount = CountCollisionBoundaryColliders();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            return new BootstrapReport(
                layout.SceneName,
                groundPlacedTiles,
                collisionPlacedTiles,
                groundHasCenterTile,
                collisionHasBottomLeftBorderTile,
                collisionBoundaryColliderCount,
                npcSpotCount,
                eventTriggerCount,
                adjustedCameraCount);
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

        private static Tilemap FindChildTilemap(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child == null)
                throw new InvalidOperationException($"Tilemap '{name}' が見つかりません。親: {parent.name}");

            var tilemap = child.GetComponent<Tilemap>();
            if (tilemap == null)
                throw new InvalidOperationException($"Tilemap コンポーネントが見つかりません。オブジェクト: {name}");

            return tilemap;
        }

        private static int FillRect(Tilemap tilemap, int minX, int minY, int maxX, int maxY, TileBase tile)
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
            return tiles.Length;
        }

        private static int DrawBorder(Tilemap tilemap, int minX, int minY, int maxX, int maxY, TileBase tile)
        {
            var placed = 0;

            for (var x = minX; x <= maxX; x++)
            {
                tilemap.SetTile(new Vector3Int(x, minY, 0), tile);
                tilemap.SetTile(new Vector3Int(x, maxY, 0), tile);
                placed += 2;
            }

            for (var y = minY; y <= maxY; y++)
            {
                tilemap.SetTile(new Vector3Int(minX, y, 0), tile);
                tilemap.SetTile(new Vector3Int(maxX, y, 0), tile);
                placed += 2;
            }

            return placed;
        }

        private static int EnsureEventTriggers(string sceneName, IReadOnlyList<Vector2> points)
        {
            var root = GameObject.Find("EventTriggers");
            if (root == null)
                root = new GameObject("EventTriggers");

            for (var i = root.transform.childCount - 1; i >= 0; i--)
            {
                var child = root.transform.GetChild(i);
                if (child != null)
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
            }

            for (var i = 0; i < points.Count; i++)
            {
                var triggerObj = new GameObject($"Event_{sceneName}_{i + 1}");
                triggerObj.transform.SetParent(root.transform, false);
                triggerObj.transform.localPosition = points[i];

                var collider = triggerObj.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
                collider.size = new Vector2(1.2f, 1.2f);

                var trigger = triggerObj.AddComponent<AreaEventTrigger2D>();
                var serialized = new SerializedObject(trigger);
                serialized.FindProperty("_eventId").stringValue = $"{sceneName.ToLowerInvariant()}_event_{i + 1}";
                serialized.FindProperty("_debugMessage").stringValue = $"{sceneName} event point {i + 1}";
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            return points.Count;
        }

        private static int EnsureNpcSpots(string sceneName, IReadOnlyList<Vector2> points)
        {
            var root = GameObject.Find("NPCSpots");
            if (root == null)
                root = new GameObject("NPCSpots");

            for (var i = root.transform.childCount - 1; i >= 0; i--)
            {
                var child = root.transform.GetChild(i);
                if (child != null)
                    UnityEngine.Object.DestroyImmediate(child.gameObject);
            }

            for (var i = 0; i < points.Count; i++)
            {
                var spot = new GameObject($"NpcSpot_{sceneName}_{i + 1}");
                spot.transform.SetParent(root.transform, false);
                spot.transform.localPosition = points[i];
            }

            return points.Count;
        }

        private static int NormalizeSceneCameras(string sceneName)
        {
            var adjusted = 0;
            var cameras = UnityEngine.Object.FindObjectsByType<Camera>();
            foreach (var camera in cameras)
            {
                var changed = false;

                if (!camera.orthographic)
                {
                    camera.orthographic = true;
                    changed = true;
                }

                if (camera.orthographicSize < MinimumCameraSize)
                {
                    camera.orthographicSize = DefaultCameraSize;
                    changed = true;
                }

                var position = camera.transform.position;
                if (position.z > -0.1f)
                {
                    position.z = DefaultCameraZ;
                    camera.transform.position = position;
                    changed = true;
                }

                if (changed)
                    adjusted++;
            }

            if (cameras.Length == 0)
                Debug.LogWarning($"[AcademyMapBootstrapper] Camera not found in {sceneName}");

            return adjusted;
        }

        private static bool IsValidForBootstrap(TileBase tile)
        {
            if (tile == null)
                return false;

            if (tile is not Tile unityTile)
                return false;

            if (unityTile.sprite == null)
                return false;

            // ほぼ空スプライトを掴む確率を下げるため、最小限のサイズを持つもののみ採用する。
            if (unityTile.sprite.rect.width <= 1f || unityTile.sprite.rect.height <= 1f)
                return false;

            return true;
        }

        private static int CountCollisionBoundaryColliders()
        {
            var root = GameObject.Find("CollisionBounds");
            if (root == null)
                return 0;

            return root.GetComponentsInChildren<BoxCollider2D>(true).Length;
        }

        private static void LogSummary(IReadOnlyList<BootstrapReport> reports, string selectedTilePath)
        {
            Debug.Log($"[AcademyMapBootstrapper] Bootstrap finished. Tile: {selectedTilePath}");

            foreach (var report in reports)
            {
                Debug.Log(
                    $"[AcademyMapBootstrapper] {report.SceneName}: GroundPlaced={report.GroundPlacedTiles}, CollisionPlaced={report.CollisionPlacedTiles}, GroundCenterTile={report.GroundHasCenterTile}, CollisionCornerTile={report.CollisionHasBottomLeftBorderTile}, CollisionBoundsColliders={report.CollisionBoundaryColliderCount}, NPCSpots={report.NpcSpotCount}, EventTriggers={report.EventTriggerCount}, CamerasAdjusted={report.AdjustedCameraCount}");

                if (!report.GroundHasCenterTile)
                {
                    Debug.LogWarning($"[AcademyMapBootstrapper] Tile placement may have failed in {report.SceneName}. Ground tile presence check failed.");
                }

                if (report.CollisionBoundaryColliderCount <= 0)
                {
                    Debug.LogWarning($"[AcademyMapBootstrapper] CollisionBounds colliders are missing in {report.SceneName}.");
                }
            }
        }

        private static IEnumerable<SceneLayout> BuildLayouts()
        {
            yield return new SceneLayout
            {
                SceneName = "Academy_Classroom",
                HalfWidth = 10,
                HalfHeight = 7,
                NpcPoints = new[] { new Vector2(-3f, -1f), new Vector2(2f, -1f), new Vector2(0f, 3f) },
                EventPoints = new[] { new Vector2(-4f, 2f), new Vector2(3f, 2f) },
            };

            yield return new SceneLayout
            {
                SceneName = "Academy_Hallway",
                HalfWidth = 12,
                HalfHeight = 10,
                NpcPoints = new[] { new Vector2(-6f, 0f), new Vector2(0f, 0f), new Vector2(6f, 0f), new Vector2(0f, 6f) },
                EventPoints = new[] { new Vector2(0f, 0f), new Vector2(-5f, 4f), new Vector2(5f, 4f) },
            };

            yield return new SceneLayout
            {
                SceneName = "Academy_Cafeteria",
                HalfWidth = 11,
                HalfHeight = 8,
                NpcPoints = new[] { new Vector2(-5f, 2f), new Vector2(-1f, 2f), new Vector2(3f, 2f) },
                EventPoints = new[] { new Vector2(-2f, 1f), new Vector2(2f, -2f) },
            };

            yield return new SceneLayout
            {
                SceneName = "Academy_Library",
                HalfWidth = 11,
                HalfHeight = 8,
                NpcPoints = new[] { new Vector2(-4f, -1f), new Vector2(0f, -1f), new Vector2(4f, -1f) },
                EventPoints = new[] { new Vector2(-3f, 2f), new Vector2(4f, -1f) },
            };

            yield return new SceneLayout
            {
                SceneName = "Academy_Schoolyard",
                HalfWidth = 14,
                HalfHeight = 10,
                NpcPoints = new[] { new Vector2(-7f, 1f), new Vector2(0f, 1f), new Vector2(7f, 1f), new Vector2(0f, -4f) },
                EventPoints = new[] { new Vector2(0f, 2f), new Vector2(-6f, -3f), new Vector2(6f, -3f) },
            };

            yield return new SceneLayout
            {
                SceneName = "Academy_Rooftop",
                HalfWidth = 10,
                HalfHeight = 7,
                NpcPoints = new[] { new Vector2(-3f, 0f), new Vector2(3f, 0f) },
                EventPoints = new[] { new Vector2(0f, 1f), new Vector2(-3f, -2f) },
            };

            yield return new SceneLayout
            {
                SceneName = "Academy_StudentCouncilRoom",
                HalfWidth = 10,
                HalfHeight = 7,
                NpcPoints = new[] { new Vector2(-2f, 1f), new Vector2(2f, 1f), new Vector2(0f, -2f) },
                EventPoints = new[] { new Vector2(0f, 2f), new Vector2(3f, -2f) },
            };

            yield return new SceneLayout
            {
                SceneName = "Academy_Infirmary",
                HalfWidth = 10,
                HalfHeight = 7,
                NpcPoints = new[] { new Vector2(-4f, 1f), new Vector2(0f, 1f), new Vector2(4f, 1f) },
                EventPoints = new[] { new Vector2(-2f, 2f), new Vector2(2f, -1f) },
            };

            yield return new SceneLayout
            {
                SceneName = "Academy_SchoolGate",
                HalfWidth = 12,
                HalfHeight = 8,
                NpcPoints = new[] { new Vector2(-3f, 2f), new Vector2(3f, 2f), new Vector2(0f, -2f) },
                EventPoints = new[] { new Vector2(0f, 0f), new Vector2(0f, -4f) },
            };
        }
    }
}