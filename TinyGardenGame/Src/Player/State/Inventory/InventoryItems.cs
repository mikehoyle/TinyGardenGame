using System.Collections.Generic;
using TinyGardenGame.Core;
using TinyGardenGame.Plants;
using static TinyGardenGame.Player.State.Inventory.InventoryItem;

namespace TinyGardenGame.Player.State.Inventory {
  public enum InventoryItem {
    ReedSeeds,
  }

  public struct ItemMetadata {
    public ItemMetadata(PlantType plantType, SpriteName sprite) {
      PlantType = plantType;
      Sprite = sprite;
    }

    public readonly PlantType PlantType;
    public readonly SpriteName Sprite;
  }

  public static class Items {
    public static readonly Dictionary<InventoryItem, ItemMetadata> ItemMap =
        new Dictionary<InventoryItem, ItemMetadata> {
            { ReedSeeds, new ItemMetadata(PlantType.Reeds, SpriteName.InventoryReedSeeds) },
        };
  }
}