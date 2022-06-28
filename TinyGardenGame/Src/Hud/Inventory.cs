using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite.Documents;
using MonoGame.Extended.TextureAtlases;
using MonoGame.Extended.ViewportAdapters;
using TinyGardenGame.Core;

namespace TinyGardenGame.Hud {
  public class Inventory {
    private const int InventoryBottomMarginPx = 2; 
    private const int InventoryWidth = 10;
    
    private readonly ScalingViewportAdapter _hudScale;
    
    private readonly IInventoryItem[] _inventoryContents;
    private int _currentlySelectedSlot;
    private readonly Texture2D _borderSprite;
    private readonly Texture2D _selectedSprite;
    private readonly int _inventoryContainerWidth;
    private readonly int _inventoryContainerHeight;

    public int CurrentlySelectedSlot {
      get => _currentlySelectedSlot;
      set => _currentlySelectedSlot = value < 0 ? InventoryWidth -1 : value % InventoryWidth;
    }
    public IInventoryItem CurrentlySelectedItem => _inventoryContents[CurrentlySelectedSlot];

    public Inventory(ContentManager content, ScalingViewportAdapter hudScale) {
      _hudScale = hudScale;
      _inventoryContents = new IInventoryItem[InventoryWidth];
      _currentlySelectedSlot = 0;
      _borderSprite = content.LoadTexture(SpriteName.InventoryContainer);
      _selectedSprite = content.LoadTexture(SpriteName.InventorySelected);
      
      _inventoryContainerWidth = _borderSprite.Width;
      _inventoryContainerHeight = _borderSprite.Height;
    }
    
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
      var renderY = _hudScale.VirtualHeight - _inventoryContainerHeight - InventoryBottomMarginPx;
      var renderX = (_hudScale.VirtualWidth - _inventoryContainerWidth * InventoryWidth) / 2;
      for (var i = 0; i < InventoryWidth; i++) {
        spriteBatch.Draw(
            _borderSprite,
            new Vector2(renderX + (i * _inventoryContainerWidth), renderY),
            Color.White);
      }

      var selectedX =
          new Vector2(renderX + (CurrentlySelectedSlot * _inventoryContainerWidth), renderY);
      // Accommodate larger size of selected texture
      selectedX -= Vector2.One;
      spriteBatch.Draw(
          _selectedSprite,
          selectedX,
          Color.White);
    }
  }
}