﻿using System.Collections.Specialized;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace TinyGardenGame.Core.Components.Drawables {
  public abstract class BaseDrawable : IUpdate {
    public abstract void Draw(
        SpriteBatch spriteBatch, Vector2 position, SpriteEffects spriteEffects);
    
    public virtual void Update(GameTime gameTime) { }
    public virtual void OnAnimationChange(string name) { }
    
    protected void SpriteBatchDraw(
        SpriteBatch spriteBatch,
        Texture2D texture,
        Vector2 position,
        Vector2 origin = new Vector2(),
        SpriteEffects effects = SpriteEffects.None,
        Rectangle? bounds = null) {
      spriteBatch.Draw(
          texture,
          position,
          bounds,
          Color.White,
          rotation: 0f,
          origin,
          scale: Vector2.One,
          effects,
          0f);
    }
  }
}