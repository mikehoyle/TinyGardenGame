﻿using MonoGame.Extended.Sprites;
using MonoGame.Extended.ViewportAdapters;
using TinyGardenGame.Player.State.Tools;

namespace TinyGardenGame.Hud {
  public class ToolDisplay {
    private const int MarginPx = 2;
    private readonly ScalingViewportAdapter _hudScale;
    private readonly PlayerTools _tools;

    public ToolDisplay(ScalingViewportAdapter hudScale, PlayerTools tools) {
      _hudScale = hudScale;
      _tools = tools;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
      var sprite = _tools.CurrentlySelectedTool.GetSprite(Platform.Content);
      var position =
          new Vector2(MarginPx, _hudScale.VirtualHeight - sprite.TextureRegion.Height - MarginPx);
      sprite.Draw(
          spriteBatch,
          position,
          rotation: 0f,
          scale: Vector2.One);
    }
  }
}