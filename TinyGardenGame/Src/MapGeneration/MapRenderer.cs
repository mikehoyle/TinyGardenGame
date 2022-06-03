using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.TextureAtlases;
using TinyGardenGame.Core;
using static TinyGardenGame.MapPlacementHelper;

namespace TinyGardenGame.MapGeneration {
  /**
   * Note that map management exists outside of the ECS framework,
   * for now.
   */
  public class MapRenderer {
    private readonly SpriteBatch _spriteBatch;
    private readonly GameMap _map;
    private readonly TextureRegion2D _testTileTexture;

    public MapRenderer(MainGame game, SpriteBatch spriteBatch, GameMap map) {
      _spriteBatch = spriteBatch;
      _map = map;
      _testTileTexture = new TextureRegion2D(
          game.Content.Load<Texture2D>(Assets.TileSprites),
          TileWidthPixels, 0, TileWidthPixels, TileHeightPixels);
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
          if (_map.Contains(x, y)) {
            RenderTile(_map[x, y], x, y);  
          }
        }
      }
    }

    // We don't even use tile right now, there's only one type
    private void RenderTile(MapTileType tile, int x, int y) {
      // TODO create a helper for this ridiculously complicated call
      _spriteBatch.Draw(
          _testTileTexture.Texture,
          MapCoordToAbsoluteCoord(new Vector2(x, y)),
          _testTileTexture.Bounds,
          Color.White,
          rotation: 0f,
          origin: new Vector2(TileWidthPixels / 2 /* intentional truncation */, 0),
          scale: Vector2.One,
          SpriteEffects.None,
          0f);
    }
  }
}