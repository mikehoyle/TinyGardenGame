#nullable enable
using MonoGame.Extended.Sprites;
using TinyGardenGame.Core;

namespace TinyGardenGame.Player.State.Tools {
  /**
   * Represents a player-usable tool
   */
  public abstract class Tool {
    private Sprite? _cachedSprite;

    protected abstract SpriteName SpriteName { get; }

    public Sprite GetSprite(ContentManager contentManager) {
      return _cachedSprite ??= contentManager.LoadSprite(SpriteName);
    }
  }
}