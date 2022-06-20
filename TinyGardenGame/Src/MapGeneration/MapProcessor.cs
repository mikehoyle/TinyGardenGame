using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using TinyGardenGame.Core;
using TinyGardenGame.MapGeneration.MapTiles;
using static TinyGardenGame.MapPlacementHelper;
using static TinyGardenGame.MapPlacementHelper.Direction;
using static TinyGardenGame.Core.SpriteName;

namespace TinyGardenGame.MapGeneration {
  /**
   * Note that map management exists outside of the ECS framework,
   * for now.
   */
  public class MapProcessor {
    private readonly MainGame _game;
    private readonly GameMap _map;

    public MapProcessor(MainGame game, GameMap map) {
      _game = game;
      _map = map;
    }

    public void Draw(SpriteBatch spriteBatch, RectangleF viewBounds) {
      _map.ForEachTileInBounds(viewBounds, (x, y, tile) => {
        RenderTile(spriteBatch, tile.Sprite, x, y);
      });
      
      // For now, draw water in a second pass so it never is occluded
      _map.ForEachTileInBounds(viewBounds, (x, y, tile) => {
        if (tile.ContainsWater) {
          RenderWater(spriteBatch, x, y);
        }
      });
    }

    private void RenderTile(
        SpriteBatch spriteBatch,
        SpriteName spriteName,
        int x,
        int y,
        SpriteEffects effects = SpriteEffects.None) {
      var sprite = _game.Content.LoadAnimated(spriteName);
      // TODO create a helper for this ridiculously complicated call
      spriteBatch.Draw(
          sprite.Texture,
          MapCoordToAbsoluteCoord(new Vector2(x, y)),
          sprite.SourceRectangle,
          Color.White,
          rotation: 0f,
          sprite.Origin,
          scale: Vector2.One,
          effects,
          0f);
    }

    /**
     * This whole function is questionably efficient, and ridiculously implemented.
     * There are surely better ways. It will work fine for now.
     */
    private void RenderWater(SpriteBatch spriteBatch, int x, int y) {
      var totalConnections = 0;
      // Check all adjacent coords
      Dictionary<Direction, bool> connections = new Dictionary<Direction, bool> {
          {North, false},
          {East, false},
          {South, false},
          {West, false},
      };
      _map.ForEachAdjacentTile(x, y, (direction, adjX, adjY, tile) => {
        if (tile.ContainsWater) {
          totalConnections++;
          connections[direction] = true;
        }
      });

      if (totalConnections == 0) {
        RenderTile(spriteBatch, Water0, x, y);
      } else if (totalConnections == 1) {
        var effects = SpriteEffects.None;
        if (connections[North] || connections[East]) {
          effects |= SpriteEffects.FlipHorizontally;
        }

        if (connections[East] || connections[South]) {
          effects |= SpriteEffects.FlipVertically;
        }
        RenderTile(spriteBatch, Water1, x, y, effects);
      } else if (totalConnections == 2) {
        // Corner cases
        if (connections[North] && connections[East]) {
          RenderTile(spriteBatch, Water2Sw, x, y, SpriteEffects.FlipHorizontally);
        } else if (connections[East] && connections[South]) {
          RenderTile(spriteBatch, Water2Nw, x, y, SpriteEffects.FlipVertically);
        } else if (connections[South] && connections[West]) {
          RenderTile(spriteBatch, Water2Sw, x, y);
        } else if (connections[West] && connections[North]) {
          RenderTile(spriteBatch, Water2Nw, x, y);
        }

        // Straight across cases
        else if (connections[North] && connections[South]) {
          RenderTile(spriteBatch, Water2Ew, x, y, SpriteEffects.FlipHorizontally);
        } else if (connections[East] && connections[West]) {
          RenderTile(spriteBatch, Water2Ew, x, y);
        }
      } else if (totalConnections == 3) {
        var effects = SpriteEffects.None;
        if (!connections[North] || !connections[East]) {
          effects |= SpriteEffects.FlipHorizontally;
        }
        if (!connections[North] || !connections[West]) {
          effects |= SpriteEffects.FlipVertically;
        }
        RenderTile(spriteBatch, Water3, x, y, effects);
      } else {
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