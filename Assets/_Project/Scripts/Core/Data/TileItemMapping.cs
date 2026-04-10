using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace GuildAcademy.Core.Data
{
    /// <summary>
    /// Ground/Furniture Item mapping to Tile Assets
    /// 
    /// Purpose: Associate generated tile assets (Fence_n, Floor_n) with
    /// ground placement items and furniture objects for level design.
    /// </summary>
    [System.Serializable]
    public class TileItemReference
    {
        public string itemName;
        public TileBase tile;
        public ItemType itemType;

        public enum ItemType
        {
            Ground,
            Furniture,
            Decoration,
            Obstacle,
            Environmental
        }
    }

    [CreateAssetMenu(fileName = "TileItemMapping", menuName = "Maptile/Tile Item Mapping")]
    public class TileItemMapping : ScriptableObject
    {
        [SerializeField]
        private List<TileItemReference> groundItems = new List<TileItemReference>();

        [SerializeField]
        private List<TileItemReference> furnitureItems = new List<TileItemReference>();

        [SerializeField]
        private List<TileItemReference> decorationItems = new List<TileItemReference>();

        /// <summary>
        /// Get tile asset by item name and type
        /// </summary>
        public TileBase GetTile(string itemName, TileItemReference.ItemType itemType)
        {
            var list = GetItemListByType(itemType);
            var match = list.Find(item => item.itemName == itemName);
            return match?.tile;
        }

        /// <summary>
        /// Get all tiles of a specific type
        /// </summary>
        public List<TileItemReference> GetItemsByType(TileItemReference.ItemType itemType)
        {
            return GetItemListByType(itemType);
        }

        private List<TileItemReference> GetItemListByType(TileItemReference.ItemType itemType)
        {
            return itemType switch
            {
                TileItemReference.ItemType.Ground => groundItems,
                TileItemReference.ItemType.Furniture => furnitureItems,
                TileItemReference.ItemType.Decoration => decorationItems,
                _ => new List<TileItemReference>()
            };
        }

        public void AddGroundItem(string name, TileBase tile)
        {
            if (!TileExists(groundItems, name))
            {
                groundItems.Add(new TileItemReference 
                { 
                    itemName = name, 
                    tile = tile, 
                    itemType = TileItemReference.ItemType.Ground 
                });
            }
        }

        public void AddFurnitureItem(string name, TileBase tile)
        {
            if (!TileExists(furnitureItems, name))
            {
                furnitureItems.Add(new TileItemReference 
                { 
                    itemName = name, 
                    tile = tile, 
                    itemType = TileItemReference.ItemType.Furniture 
                });
            }
        }

        private bool TileExists(List<TileItemReference> list, string name)
        {
            return list.Exists(item => item.itemName == name);
        }

        public int GetGroundItemCount() => groundItems.Count;
        public int GetFurnitureItemCount() => furnitureItems.Count;
        public int GetDecorationItemCount() => decorationItems.Count;
    }
}
