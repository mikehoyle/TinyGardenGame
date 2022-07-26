#nullable enable
using MonoGame.Extended.Sprites;
using MonoGame.Extended.ViewportAdapters;
using TinyGardenGame.Core;
using TinyGardenGame.Player.State.Inventory;

namespace TinyGardenGame.Hud {
  public class InventoryDisplay {
    private const int InventoryBottomMarginPx = 2;

    private readonly ContentManager _content;
    private readonly ScalingViewportAdapter _hudScale;
    private readonly PlayerInventory _playerInventory;

    private readonly Texture2D _borderSprite;
    private readonly Texture2D _selectedSprite;
    private readonly int _inventoryContainerWidth;
    private readonly int _inventoryContainerHeight;
    private readonly SpriteFont _font;

    public InventoryDisplay(
        ContentManager content, ScalingViewportAdapter hudScale, PlayerInventory playerInventory) {
      _content = content;
      _hudScale = hudScale;
      _playerInventory = playerInventory;
      _borderSprite = content.LoadTexture(SpriteName.InventoryContainer);
      _selectedSprite = content.LoadTexture(SpriteName.InventorySelected);
      _font = content.LoadFont(SpriteName.ConsoleFont);

      _inventoryContainerWidth = _borderSprite.Width;
      _inventoryContainerHeight = _borderSprite.Height;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
      var width = PlayerInventory.InventoryWidth;
      var renderY = _hudScale.VirtualHeight - _inventoryContainerHeight - InventoryBottomMarginPx;
      var renderX = (_hudScale.VirtualWidth - _inventoryContainerWidth * width) / 2;

      for (var i = 0; i < width; i++) {
        var position = new Vector2(renderX + (i * _inventoryContainerWidth), renderY);
        // Draw background
        spriteBatch.Draw(
            _borderSprite,
            position,
            Color.White);

        // Draw item sprite
        var itemAtSlot = _playerInventory.ContentsOfSlot(i);
        if (itemAtSlot != null && itemAtSlot.Count != 0) {
          itemAtSlot.GetSprite(_content).Draw(
              spriteBatch,
              position,
              rotation: 0f,
              scale: Vector2.One);

          // Draw amount
          spriteBatch.DrawString(
              _font,
              itemAtSlot.Count.ToString(),
              position + (Vector2.One * 2),
              Color.Black,
              rotation: 0,
              origin: Vector2.Zero,
              scale: 0.25f,
              SpriteEffects.None,
              layerDepth: 0);
        }
      }

      var selectedX = new Vector2(
          renderX + (_playerInventory.CurrentlySelectedSlot * _inventoryContainerWidth), renderY);
      // Accommodate larger size of selected texture
      selectedX -= Vector2.One;
      spriteBatch.Draw(
          _selectedSprite,
          selectedX,
          Color.White);
    }
  }
}