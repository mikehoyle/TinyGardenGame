using System.Collections.Generic;
using TinyGardenGame.Core;
using TinyGardenGame.Plants;
using static TinyGardenGame.Player.State.Inventory.InventoryItem;

namespace TinyGardenGame.Player.State.Inventory {
  public enum InventoryItem {
    ReedSeeds,
    GreatAcorn,
  }

  public struct ItemMetadata {
    public ItemMetadata(PlantType plantType, Vars.Sprite.Type sprite) {
      PlantType = plantType;
      Sprite = sprite;
    }

    public readonly PlantType PlantType;
    public readonly Vars.Sprite.Type Sprite;
  }

  public static class Items {
    public static readonly Dictionary<InventoryItem, ItemMetadata> ItemMap = new() {
        { ReedSeeds, new ItemMetadata(PlantType.Reeds, Vars.Sprite.Type.InventoryReedSeeds) },
        { GreatAcorn, new ItemMetadata(PlantType.GreatOak, Vars.Sprite.Type.InventoryGreatAcorn) },
    };
  }
}