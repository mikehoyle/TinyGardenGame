﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using TinyGardenGame.Core;
using TinyGardenGame.MapGeneration.MapTiles;
using static TinyGardenGame.MapPlacementHelper;
using static TinyGardenGame.MapPlacementHelper.Direction;

namespace TinyGardenGame.MapGeneration {
  /**
   * Note that map management exists outside of the ECS framework,
   * for now.
   */
  public class MapProcessor {
    private static int[] SpriteRows = { 0, 16 };
    private static int[] SpriteRowHeights = { 16, 24 };
    
    private readonly SpriteBatch _spriteBatch;
    private readonly GameMap _map;
    private readonly Texture2D _tileSpriteSheet;
    // Maps types to their options in the spritesheet
    private readonly Dictionary<Type, (int X, int Y)[]> _textures;
    // Seven water textures, by connection:
    // 0, 1 (W), 2 (N & W), 2 (S & W), 2 (W & E), 3 (N & W & E), 4 (all)
    private readonly (int X, int Y)[] _waterTextures;

    public MapProcessor(MainGame game, SpriteBatch spriteBatch, GameMap map) {
      _spriteBatch = spriteBatch;
      _map = map;
      _tileSpriteSheet = game.Content.Load<Texture2D>(Assets.TileSprites);
      _textures = new Dictionary<Type, (int X, int Y)[]>();
      _waterTextures = new (int X, int Y)[7];
      BuildTexturesMap();
    }

    private void BuildTexturesMap() {
      _textures.Add(
          typeof(WeedsTile), new []{ (0, 1) }
      );

      var waterTextureStartX = 2;
      for (var i = 0; i < 7; i++) {
        _waterTextures[i] = (i + waterTextureStartX, 0);
      }
    }

    public void Draw(RectangleF viewBounds) {
      _map.ForEachTileInBounds(viewBounds, (x, y, tile) => {
        if (_textures.TryGetValue(tile.GetType(), out var textureList)) {
          RenderTile(textureList[tile.TextureVariant], x, y);
        }
      });
      
      // For now, draw water in a second pass so it never is occluded
      _map.ForEachTileInBounds(viewBounds, (x, y, tile) => {
        if (tile.ContainsWater) {
          RenderWater(x, y);
        }
      });
    }

    private void RenderTile(
        (int X, int Y) spriteSheetCoords,
        int x,
        int y,
        SpriteEffects effects = SpriteEffects.None) {
      // TODO create a helper for this ridiculously complicated call
      _spriteBatch.Draw(
          _tileSpriteSheet,
          MapCoordToAbsoluteCoord(new Vector2(x, y)),
          GetBounds(spriteSheetCoords.X, spriteSheetCoords.Y),
          Color.White,
          rotation: 0f,
          origin: GetSpriteOrigin(spriteSheetCoords.Y),
          scale: Vector2.One,
          effects,
          0f);
    }

    /**
     * This whole function is questionably efficient, and ridiculously implemented.
     * There are surely better ways. It will work fine for now.
     */
    private void RenderWater(int x, int y) {
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
        RenderTile(_waterTextures[0], x, y);
      } else if (totalConnections == 1) {
        var effects = SpriteEffects.None;
        if (connections[North] || connections[East]) {
          effects |= SpriteEffects.FlipHorizontally;
        }

        if (connections[East] || connections[South]) {
          effects |= SpriteEffects.FlipVertically;
        }
        RenderTile(_waterTextures[1], x, y, effects);
      } else if (totalConnections == 2) {
        // Corner cases
        if (connections[North] && connections[East]) {
          RenderTile(_waterTextures[3], x, y, SpriteEffects.FlipHorizontally);
        } else if (connections[East] && connections[South]) {
          RenderTile(_waterTextures[2], x, y, SpriteEffects.FlipVertically);
        } else if (connections[South] && connections[West]) {
          RenderTile(_waterTextures[3], x, y);
        } else if (connections[West] && connections[North]) {
          RenderTile(_waterTextures[2], x, y);
        }

        // Straight across cases
        else if (connections[North] && connections[South]) {
          RenderTile(_waterTextures[4], x, y, SpriteEffects.FlipHorizontally);
        } else if (connections[East] && connections[West]) {
          RenderTile(_waterTextures[4], x, y);
        }
      } else if (totalConnections == 3) {
        var effects = SpriteEffects.None;
        if (!connections[North] || !connections[East]) {
          effects |= SpriteEffects.FlipHorizontally;
        }
        if (!connections[North] || !connections[West]) {
          effects |= SpriteEffects.FlipVertically;
        }
        RenderTile(_waterTextures[5], x, y, effects);
      } else {
        RenderTile(_waterTextures[6], x, y);
      }
    }

    public void Update(GameTime gameTime) {
      foreach (var dirtyTile in _map.DirtyTiles) {
        if (_map[dirtyTile.X, dirtyTile.Y].ContainsWater) {
          UpdateSurroundingWaterTiles(dirtyTile.X, dirtyTile.Y);
        }
      }
      _map.DirtyTiles.Clear();
    }

    private void UpdateSurroundingWaterTiles(int dirtyTileX, int dirtyTileY) {
      // Update non-traversable tiles for dirty tile and all surrounding water tiles
      // TODO: consider caching this information to prevent taxing extra calculations.
      MarkWaterNonTraversableIfSurrounded(
          North /* unused */, dirtyTileX, dirtyTileY, _map[dirtyTileX, dirtyTileY]);
      _map.ForEachAdjacentTile(dirtyTileX, dirtyTileY, MarkWaterNonTraversableIfSurrounded);
    }

    private void MarkWaterNonTraversableIfSurrounded(
        Direction _, int x, int y, MapTile tile) {
      if (tile.ContainsWater) {
        var surroundingWaterTiles = 0;
        _map.ForEachAdjacentTile(x, y, (_, adjX, adjY, adjTile) => {
          if (adjTile.ContainsWater) {
            surroundingWaterTiles++;
          }
        });
        if (surroundingWaterTiles == 4) {
          tile.IsNonTraversable = true;
        }
      }
    }

    /**
     * x & y are row/column coords on the spritesheet
     */
    private Rectangle GetBounds(int x, int y) {
      return new Rectangle(
          x * TileWidthPixels,
          SpriteRows[y],
          TileWidthPixels,
          SpriteRowHeights[y]);
    }

    /**
     * x & y are row/column coords on the spritesheet
     */
    private Vector2 GetSpriteOrigin(int y) {
      return new Vector2(
          TileWidthPixels / 2 /* intentional truncation */,
          SpriteRowHeights[y] - TileHeightPixels);
    }
  }
}