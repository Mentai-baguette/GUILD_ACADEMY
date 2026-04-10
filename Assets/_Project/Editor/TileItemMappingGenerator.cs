using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using GuildAcademy.Core.Data;

/// <summary>
/// Automatic Tile Item Mapping Generator
/// 
/// Scans generated tile assets and populates TileItemMapping ScriptableObject
/// with ground/furniture/decoration classifications.
/// </summary>
public class TileItemMappingGenerator
{
    private const string TILEMAP_ASSET_PATH = "Assets/Project/maptile/tilemap/";
    private const string MAPPING_OUTPUT_PATH = "Assets/_Project/ScriptableObjects/Maptile/TileItemMapping.asset";

    [MenuItem("Assets/Maptile/Generate Tile Item Mapping")]
    public static void GenerateTileItemMapping()
    {
        EditorUtility.DisplayProgressBar("Tile Mapping", "Loading tile assets...", 0.1f);

        try
        {
            // Load all Tile assets
            var tileAssets = LoadAllTileAssets();
            Debug.Log($"[Tile Mapping] Found {tileAssets.Count} tile assets");

            if (tileAssets.Count == 0)
            {
                EditorUtility.DisplayDialog("Tile Mapping",
                    "No tile assets found in " + TILEMAP_ASSET_PATH, "OK");
                return;
            }

            EditorUtility.DisplayProgressBar("Tile Mapping", "Creating mapping asset...", 0.3f);

            // Create or load TileItemMapping
            var mapping = CreateOrLoadMapping();

            EditorUtility.DisplayProgressBar("Tile Mapping", "Categorizing tiles...", 0.5f);

            // Categorize and populate
            var (floorCount, fenceCount, otherCount) = CategorizeTiles(mapping, tileAssets);

            EditorUtility.DisplayProgressBar("Tile Mapping", "Saving asset...", 0.8f);

            EditorUtility.SetDirty(mapping);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Tile Item Mapping Generated",
                $"✓ Floor ground items: {floorCount}\n" +
                $"✓ Fence furniture items: {fenceCount}\n" +
                $"✓ Other items: {otherCount}\n\n" +
                $"Asset saved: {MAPPING_OUTPUT_PATH}",
                "OK");

            Debug.Log("[Tile Mapping] Generation complete!");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private static Dictionary<string, TileBase> LoadAllTileAssets()
    {
        var tileAssets = new Dictionary<string, TileBase>();
        var tileGUIDs = AssetDatabase.FindAssets("t:Tile", new[] { TILEMAP_ASSET_PATH });

        foreach (var guid in tileGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var tile = AssetDatabase.LoadAssetAtPath<TileBase>(path);
            if (tile != null)
            {
                tileAssets[tile.name] = tile;
            }
        }

        return tileAssets;
    }

    private static TileItemMapping CreateOrLoadMapping()
    {
        var existing = AssetDatabase.LoadAssetAtPath<TileItemMapping>(MAPPING_OUTPUT_PATH);
        if (existing != null)
        {
            return existing;
        }

        // Create new asset
        var newMapping = ScriptableObject.CreateInstance<TileItemMapping>();

        // Ensure directory exists
        string dir = System.IO.Path.GetDirectoryName(MAPPING_OUTPUT_PATH);
        if (!System.IO.Directory.Exists(dir))
        {
            System.IO.Directory.CreateDirectory(dir);
        }

        AssetDatabase.CreateAsset(newMapping, MAPPING_OUTPUT_PATH);
        return newMapping;
    }

    private static (int floorCount, int fenceCount, int otherCount) CategorizeTiles(
        TileItemMapping mapping, Dictionary<string, TileBase> tileAssets)
    {
        int floorCount = 0, fenceCount = 0, otherCount = 0;

        foreach (var kvp in tileAssets)
        {
            string tileName = kvp.Key;
            TileBase tile = kvp.Value;

            if (tileName.StartsWith("Floor"))
            {
                mapping.AddGroundItem(tileName, tile);
                floorCount++;
            }
            else if (tileName.StartsWith("Fence"))
            {
                mapping.AddFurnitureItem(tileName, tile);
                fenceCount++;
            }
            else
            {
                // Reptile0, magecity, roguelikeIndoor_transparent → decoration or other
                // For now, classify as decoration
                otherCount++;
            }
        }

        return (floorCount, fenceCount, otherCount);
    }

    [MenuItem("Assets/Maptile/Show Tile Mapping Report")]
    public static void ShowMappingReport()
    {
        var mapping = AssetDatabase.LoadAssetAtPath<TileItemMapping>(MAPPING_OUTPUT_PATH);
        if (mapping == null)
        {
            EditorUtility.DisplayDialog("Tile Mapping Report",
                "TileItemMapping not found. Please generate it first using\n" +
                "Assets > Maptile > Generate Tile Item Mapping",
                "OK");
            return;
        }

        var report = $"Tile Item Mapping Report\n" +
                     $"=========================\n\n" +
                     $"Ground Items (Floor): {mapping.GetGroundItemCount()}\n" +
                     $"Furniture Items (Fence): {mapping.GetFurnitureItemCount()}\n" +
                     $"Decoration Items: {mapping.GetDecorationItemCount()}\n" +
                     $"\nAsset: {MAPPING_OUTPUT_PATH}";

        Debug.Log(report);
        EditorUtility.DisplayDialog("Tile Mapping Report", report, "OK");
    }
}
