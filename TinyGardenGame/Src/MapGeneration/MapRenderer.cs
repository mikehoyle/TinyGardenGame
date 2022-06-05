using System;
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
          }
        }
      }
      
      // For now, draw water in a second pass so it never is occluded
      for (var x = leftBound; x <= rightBound; x++) {
        for (var y = topBound; y <= bottomBound; y++) {
          if (_map.Contains(x, y) && _map[x, y].Has(TileFlags.ContainsWater)) {
            RenderWater(x, y);
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
      var totalConnections = 0;
      // Check all adjacent coords
      var connections = ForEachAdjacentTile(x, y, (direction, adjX, adjY) => {
        if (_map.Contains(adjX, adjY) && _map[adjX, adjY].Has(TileFlags.ContainsWater)) {
          totalConnections++;
          return true;
        }
        return false;
      });

      if (totalConnections == 0) {
        RenderTile(_waterTextures[0], x, y);
      } else if (totalConnections == 1) {
        var effects = SpriteEffects.None;
        if (connections[(int)North] || connections[(int)East]) {
          effects |= SpriteEffects.FlipHorizontally;
        }

        if (connections[(int)East] || connections[(int)South]) {
          effects |= SpriteEffects.FlipVertically;
        }
        RenderTile(_waterTextures[1], x, y, effects);
      } else if (totalConnections == 2) {
        // Corner cases
        if (connections[(int)North] && connections[(int)East]) {
          RenderTile(_waterTextures[3], x, y, SpriteEffects.FlipHorizontally);
        } else if (connections[(int)East] && connections[(int)South]) {
          RenderTile(_waterTextures[2], x, y, SpriteEffects.FlipVertically);
        } else if (connections[(int)South] && connections[(int)West]) {
          RenderTile(_waterTextures[3], x, y);
        } else if (connections[(int)West] && connections[(int)North]) {
          RenderTile(_waterTextures[2], x, y);
        }

        // Straight across cases
        else if (connections[(int)North] && connections[(int)South]) {
          RenderTile(_waterTextures[4], x, y, SpriteEffects.FlipHorizontally);
        } else if (connections[(int)East] && connections[(int)West]) {
          RenderTile(_waterTextures[4], x, y);
        }
      } else if (totalConnections == 3) {
        var effects = SpriteEffects.None;
        if (!connections[(int)North] || !connections[(int)East]) {
          effects |= SpriteEffects.FlipHorizontally;
        }
        if (!connections[(int)North] || !connections[(int)West]) {
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