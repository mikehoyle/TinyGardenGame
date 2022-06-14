using System;
using System.Collections.Specialized;

namespace TinyGardenGame.MapGeneration.MapTiles {
  /**
   * Base class for all Map Tiles. This class and most subclasses are
   * *very* memory-sensitive, as there could be thousands of instances
   * pulled into memory.
   */
  public abstract class MapTile {
    private static readonly BitVector32.Section ContainsWaterMask;
    private static readonly BitVector32.Section CanContainWaterMask;
    private static readonly BitVector32.Section IsNonTraversableMask;
    private static readonly BitVector32.Section WaterProximityMask;
    private static readonly BitVector32.Section TextureVariantMask;
    
    private BitVector32 _metadata;

    public bool ContainsWater {
      get => GetFlag(ContainsWaterMask);
      set => SetFlag(ContainsWaterMask, value);
    }
    
    public bool CanContainWater {
      get => GetFlag(CanContainWaterMask);
      set => SetFlag(CanContainWaterMask, value);
    }

    public bool IsNonTraversable {
      get => GetFlag(IsNonTraversableMask);
      set => SetFlag(IsNonTraversableMask, value);
    }

    public int WaterProximity {
      get => _metadata[WaterProximityMask];
      set => _metadata[WaterProximityMask] = value;
    }

    public int TextureVariant {
      get => _metadata[TextureVariantMask];
      set => _metadata[TextureVariantMask] = value;
    }

    static MapTile() {
      ContainsWaterMask = BitVector32.CreateSection(1);
      CanContainWaterMask = BitVector32.CreateSection(1, ContainsWaterMask);
      IsNonTraversableMask = BitVector32.CreateSection(1, CanContainWaterMask);
      WaterProximityMask = BitVector32.CreateSection(8, IsNonTraversableMask);
      TextureVariantMask = BitVector32.CreateSection(4, WaterProximityMask);
    }

    protected MapTile() {
      _metadata = new BitVector32(0);
    }

    private bool GetFlag(BitVector32.Section section) {
      return Convert.ToBoolean(_metadata[section]);
    }

    private void SetFlag(BitVector32.Section section, bool value) {
      _metadata[section] = Convert.ToInt32(value);
    }
  }
}