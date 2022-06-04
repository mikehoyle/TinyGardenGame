using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using TinyGardenGame.Core;
using TinyGardenGame.MapGeneration.MapTiles;
using static TinyGardenGame.MapPlacementHelper;

namespace TinyGardenGame.MapGeneration {
  /**
   * Note that map management exists outside of the ECS framework,
   * for now.
   */
  public class MapRenderer {
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

    public MapRenderer(MainGame game, SpriteBatch spriteBatch, GameMap map) {
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
      // First we create bounds that are slightly bigger than the viewport, to not attempt
      // to render far more than is necessary
      // +Extra margin of error on N & W to accomodate for viewport cutting off mid-tile
      var leftBound = (int)AbsoluteCoordToMapCoord(viewBounds.TopLeft).X - 1;
      var topBound = (int)AbsoluteCoordToMapCoord(viewBounds.TopRight).Y - 1;
      var rightBound = (int)AbsoluteCoordToMapCoord(viewBounds.BottomRight).X;
      var bottomBound = (int)AbsoluteCoordToMapCoord(viewBounds.BottomLeft).Y;

      for (var x = leftBound; x <= rightBound; x++) {
        for (var y = topBound; y <= bottomBound; y++) {
          if (_map.Contains(x, y)
              && _textures.TryGetValue(_map[x, y].GetType(), out var textureList)) {
            RenderTile(textureList[_map[x, y].TextureVariant], x, y);
            if (_map[x, y].ContainsWater) {
              RenderWater(x, y);
            }
          }
        }
      }
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
      // Clockwise starting at N
      var connections = new[] { false, false, false, false };
      var totalConnections = 0;
      // Check all adjacent coords
      for (var i = 0; i < connections.Length; i ++) {
        var xDiff = i % 2 == 1 ? (i - 2) * -1 : 0;
        var yDiff = i % 2 == 0 ? i - 1 : 0;
        if (_map.Contains(x + xDiff, y + yDiff) && _map[x + xDiff, y + yDiff].ContainsWater) {
          connections[i] = true;
          totalConnections++;
        }
      }

      if (totalConnections == 0) {
        RenderTile(_waterTextures[0], x, y);
      } else if (totalConnections == 1) {
        var effects = SpriteEffects.None;
        if (connections[0] || connections[1]) {
          effects |= SpriteEffects.FlipHorizontally;
        }

        if (connections[1] || connections[2]) {
          effects |= SpriteEffects.FlipVertically;
        }
        RenderTile(_waterTextures[1], x, y, effects);
      } else if (totalConnections == 2) {
        // Corner cases
        if (connections[0] && connections[1]) {
          RenderTile(_waterTextures[3], x, y, SpriteEffects.FlipHorizontally);
        } else if (connections[1] && connections[2]) {
          RenderTile(_waterTextures[2], x, y, SpriteEffects.FlipVertically);
        } else if (connections[2] && connections[3]) {
          RenderTile(_waterTextures[3], x, y);
        } else if (connections[3] && connections[0]) {
          RenderTile(_waterTextures[2], x, y);
        }

        // Straight across cases
        else if (connections[0] && connections[2]) {
          RenderTile(_waterTextures[4], x, y, SpriteEffects.FlipHorizontally);
        } else if (connections[1] && connections[3]) {
          RenderTile(_waterTextures[4], x, y);
        }
      } else if (totalConnections == 3) {
        var effects = SpriteEffects.None;
        if (!connections[0] || !connections[1]) {
          effects |= SpriteEffects.FlipHorizontally;
        }
        if (!connections[0] || !connections[3]) {
          effects |= SpriteEffects.FlipVertically;
        }
        RenderTile(_waterTextures[5], x, y, effects);
      } else {
        RenderTile(_waterTextures[6], x, y);
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