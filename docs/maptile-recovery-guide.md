# Maptile Tile Asset Recovery Guide

## Overview

This recovery system restores and manages associations between generated tile assets (Fence_*, Floor_*, etc.) and game world items (ground, furniture, decorations).

## Generated Assets

- **Fence tiles**: Fence_0 ~ Fence_44 (45 tiles)
- **Floor tiles**: Floor_0 ~ Floor_479 (480 tiles)  
- **Reptile0 tiles**: Reptile0_0 ~ Reptile0_41 (42 tiles)
- **City tiles**: magecity_0 ~ magecity_42 (43 tiles)
- **Indoor tiles**: roguelikeIndoor_transparent_0 ~ roguelikeIndoor_transparent_477 (478 tiles)

All generated from source PNG sheets:
- Assets/Project/maptile/Fence.png
- Assets/Project/maptile/Floor.png
- Assets/Project/maptile/Reptile0.png
- Assets/Project/maptile/magecity.png
- Assets/Project/maptile/roguelikeIndoor_transparent.png

## Recovery Steps

### 1. Generate Tile Item Mapping

```
Assets > Maptile > Generate Tile Item Mapping
```

This will:
- Scan all tile assets in Assets/Project/maptile/tilemap/
- Categorize tiles by type:
  - Floor_* → Ground items
  - Fence_* → Furniture items
  - Others → Decorations
- Create or update: `Assets/_Project/ScriptableObjects/Maptile/TileItemMapping.asset`

### 2. Verify Integration

```
Assets > Maptile > Show Tile Mapping Report
```

Check that:
- Ground items (Floor): ~480 count
- Furniture items (Fence): ~45 count
- Other items as expected

### 3. Validate Asset Integrity

```
Assets > Maptile > Recover Tile Associations
```

Verifies:
- Tile Palette prefab structure
- Scene Tilemap references
- Asset database consistency

## Usage in Code

### Get Tile by Name

```csharp
using GuildAcademy.Maptile;

// Load mapping
var mapping = Resources.Load<TileItemMapping>("Maptile/TileItemMapping");

// Get a specific ground tile
var floorTile = mapping.GetTile("Floor_0", 
    TileItemReference.ItemType.Ground);

// Get a furniture tile
var fenceTile = mapping.GetTile("Fence_22", 
    TileItemReference.ItemType.Furniture);
```

### Get All Items of Type

```csharp
// Get all ground items
var groundItems = mapping.GetItemsByType(
    TileItemReference.ItemType.Ground);

// Iterate and place
foreach (var item in groundItems)
{
    // Use item.tile in tilemap placement
    tilemap.SetTile(position, item.tile);
}
```

### Runtime Placement Example

```csharp
public class GroundPlacer : MonoBehaviour
{
    private TileItemMapping tileMapping;
    private Tilemap groundTilemap;

    void Start()
    {
        tileMapping = Resources.Load<TileItemMapping>(
            "Maptile/TileItemMapping");
    }

    public void PlaceGround(Vector3Int position, string groundName)
    {
        var tile = tileMapping.GetTile(groundName, 
            TileItemReference.ItemType.Ground);
        
        if (tile != null)
        {
            groundTilemap.SetTile(position, tile);
        }
    }

    public void PlaceFurniture(Vector3Int position, string furnitureName)
    {
        var tile = tileMapping.GetTile(furnitureName,
            TileItemReference.ItemType.Furniture);
        
        if (tile != null)
        {
            groundTilemap.SetTile(position, tile);
        }
    }
}
```

## Asset Locations

| Component | Path |
|-----------|------|
| Tile Assets | Assets/Project/maptile/tilemap/*.asset |
| Tile Palette | Assets/Project/maptile/tilemap/New Tile Palette.prefab |
| Item Mapping | Assets/_Project/ScriptableObjects/Maptile/TileItemMapping.asset |
| Scene Reference | Assets/Scenes/ray_mura/mura/ray_room.unity |

## Troubleshooting

### No Tiles Found
- Verify Assets/Project/maptile/tilemap/ contains *.asset files
- Run: Assets > Maptile > Verify Tile Asset Integrity

### Mapping Asset Missing
- Run: Assets > Maptile > Generate Tile Item Mapping
- Create ScriptableObjects folder if needed

### Tilemap Not Displaying
- Check New Tile Palette.prefab has Tilemap component
- Verify tile assets have valid sprite references
- Reimport: Assets > Reimport All

## Recovery Commands Summary

| Menu Path | Action |
|-----------|--------|
| Assets > Maptile > Recover Tile Associations | Full validation & linking |
| Assets > Maptile > Generate Tile Item Mapping | Create TileItemMapping asset |
| Assets > Maptile > Verify Tile Asset Integrity | Check asset structure |
| Assets > Maptile > Show Tile Mapping Report | Display tile statistics |

## Architecture

```
Tile Assets (PNG) 
  ↓ [Import]
Tile.asset files
  ↓ [Categorize]
TileItemMapping.asset
  ↓ [Reference]
TilePalette.prefab + Scene Tilemaps
  ↓ [Runtime]
Ground Placement + Furniture System
```

---

Last Updated: 2026-04-10
Maptile Recovery Version: 1.0
