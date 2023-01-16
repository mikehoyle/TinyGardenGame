#nullable enable
using System;
using System.Collections.Generic;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using TinyGardenGame.Core;
using TinyGardenGame.MapGeneration.MapTiles;
using TinyGardenGame.Plants;
using static TinyGardenGame.Vars.Sprite.Type;

namespace TinyGardenGame.MapGeneration {
  /**
   * Note that map management exists outside of the ECS framework,
   * for now.
   */
  public class MapProcessor {
    private readonly MainGame _game;
    private readonly GameMap _map;
    private readonly Sprite _tileHighlightSprite;

    public PlantEntityFactory.CanGrowOn? TileHighlightCondition { get; set; }

    public MapProcessor(MainGame game, GameMap map) {
      _game = game;
      _map = map;
      _tileHighlightSprite = game.Content.LoadSprite(ValidTile);
    }

    public void Draw(SpriteBatch spriteBatch, RectangleF viewBounds) {
      // OPTIMIZE: Excess allocation issues?
      _map.ForEachTileInBounds(viewBounds,
          (x, y, tile) => { RenderTile(spriteBatch, tile.Sprite, x, y); });

      // For now, draw water in a second pass so it never is occluded
      _map.ForEachTileInBounds(viewBounds, (x, y, tile) => {
        if (tile.ContainsWater) {
          RenderWater(spriteBatch, x, y);
        }

        // Draw highlights, if relevant
        // TODO Cache these values on update to prevent draw lag
        if (TileHighlightCondition != null && TileHighlightCondition(tile)) {
          spriteBatch.Draw(
              _tileHighlightSprite.TextureRegion.Texture,
              MapCoordToAbsoluteCoord(new Vector2(x, y)),
              _tileHighlightSprite.TextureRegion.Bounds,
              Color.White,
              rotation: 0f,
              _tileHighlightSprite.Origin,
              scale: Vector2.One,
              SpriteEffects.None,
              0f);
        }
      });
    }

    private void RenderTile(
        SpriteBatch spriteBatch,
        Vars.Sprite.Type spriteName,
        int x,
        int y,
        SpriteEffects effects = SpriteEffects.None) {
      var sprite = _game.Content.LoadSprite(spriteName);
      // TODO create a helper for this ridiculously complicated call
      sprite.Effect = effects;
      sprite.Draw(
          spriteBatch,
          MapCoordToAbsoluteCoord(new Vector2(x, y)),
          rotation: 0f,
          scale: Vector2.One);
    }

    /**
     * This whole function is questionably efficient, and ridiculously implemented.
     * There are surely better ways. It will work fine for now.
     */
    private void RenderWater(SpriteBatch spriteBatch, int x, int y) {
      var totalConnections = 0;
      // Check all adjacent coords
      Dictionary<Direction, bool> connections = new Dictionary<Direction, bool> {
          { North, false },
          { East, false },
          { South, false },
          { West, false },
      };
      _map.ForEachAdjacentTile(x, y, (direction, adjX, adjY, tile) => {
        if (tile.ContainsWater) {
          totalConnections++;
          connections[direction] = true;
        }
      });

      if (totalConnections == 0) {
        RenderTile(spriteBatch, Water0, x, y);
      }
      else if (totalConnections == 1) {
        var effects = SpriteEffects.None;
        if (connections[North] || connections[East]) {
          effects |= SpriteEffects.FlipHorizontally;
        }

        if (connections[East] || connections[South]) {
          effects |= SpriteEffects.FlipVertically;
        }

        RenderTile(spriteBatch, Water1, x, y, effects);
      }
      else if (totalConnections == 2) {
        // Corner cases
        if (connections[North] && connections[East]) {
          RenderTile(spriteBatch, Water2Sw, x, y, SpriteEffects.FlipHorizontally);
        }
        else if (connections[East] && connections[South]) {
          RenderTile(spriteBatch, Water2Nw, x, y, SpriteEffects.FlipVertically);
        }
        else if (connections[South] && connections[West]) {
          RenderTile(spriteBatch, Water2Sw, x, y);
        }
        else if (connections[West] && connections[North]) {
          RenderTile(spriteBatch, Water2Nw, x, y);
        }

        // Straight across cases
        else if (connections[North] && connections[South]) {
          RenderTile(spriteBatch, Water2Ew, x, y, SpriteEffects.FlipHorizontally);
        }
        else if (connections[East] && connections[West]) {
          RenderTile(spriteBatch, Water2Ew, x, y);
        }
      }
      else if (totalConnections == 3) {
        var effects = SpriteEffects.None;
        if (!connections[North] || !connections[East]) {
          effects |= SpriteEffects.FlipHorizontally;
        }

        if (!connections[North] || !connections[West]) {
          effects |= SpriteEffects.FlipVertically;
        }

        RenderTile(spriteBatch, Water3, x, y, effects);
      }
      else {
        RenderTile(spriteBatch, Water4, x, y);
      }
    }

    public void Update(GameTime gameTime) {
      foreach (var dirtyTile in _map.DirtyTiles) {
        if (_map.TryGet(dirtyTile.X, dirtyTile.Y, out var tile)) {
          if (tile.ContainsWater) {
            UpdateSurroundingWaterTiles(dirtyTile.X, dirtyTile.Y, tile);
            ProcessWaterProximity(_map, dirtyTile.X, dirtyTile.Y, tile);
          }
        }
      }

      _map.DirtyTiles.Clear();
    }

    private void UpdateSurroundingWaterTiles(int dirtyTileX, int dirtyTileY, MapTile tile) {
      // Update non-traversable tiles for dirty tile and all surrounding water tiles
      // OPTIMIZE: consider caching this information to prevent taxing extra calculations.
      MarkWaterNonTraversableIfSurrounded(_map, dirtyTileX, dirtyTileY, tile);
      _map.ForEachAdjacentTile(dirtyTileX, dirtyTileY,
          (dir, x, y, tile) => MarkWaterNonTraversableIfSurrounded(_map, x, y, tile));
    }

    public static void MarkWaterNonTraversableIfSurrounded(
        GameMap map, int x, int y, MapTile tile) {
      if (!tile.ContainsWater) {
        return;
      }

      var surroundingWaterTiles = 0;
      map.ForEachAdjacentTile(x, y, (_, adjX, adjY, adjTile) => {
        if (adjTile.ContainsWater) {
          surroundingWaterTiles++;
        }
      });
      if (surroundingWaterTiles == 4) {
        tile.IsNonTraversable = true;
      }
    }

    // OPTIMIZE: This is a TON of repeated work. If necessary, may need optimizations
    public static void ProcessWaterProximity(GameMap map, int x, int y, MapTile tile) {
      if (!tile.ContainsWater) {
        return;
      }

      tile.WaterProximity = 0;
      for (var xDiff = -MapTile.MaxWaterProximity; xDiff <= MapTile.MaxWaterProximity; xDiff++) {
        var yRange = MapTile.MaxWaterProximity - Math.Abs(xDiff);
        for (var yDiff = -yRange; yDiff <= yRange; yDiff++) {
          if (map.TryGet(x + xDiff, y + yDiff, out var adjTile)) {
            if (!adjTile.ContainsWater) {
              adjTile.WaterProximity = adjTile.WaterProximity == 0
                  ? Math.Abs(xDiff) + Math.Abs(yDiff)
                  : Math.Min(adjTile.WaterProximity, Math.Abs(xDiff) + Math.Abs(yDiff));
            }
          }
        }
      }
    }
  }
}