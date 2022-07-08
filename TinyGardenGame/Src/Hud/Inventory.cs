#nullable enable
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using MonoGame.Extended.ViewportAdapters;
using TinyGardenGame.Core;
using TinyGardenGame.Plants;

namespace TinyGardenGame.Hud {
  public class Inventory {
    private const int InventoryBottomMarginPx = 2; 
    private const int InventoryWidth = 10;

    private readonly ContentManager _content;
    private readonly ScalingViewportAdapter _hudScale;
    
    private readonly InventorySlot?[] _inventoryContents;
    private int _currentlySelectedSlot;
    private readonly Texture2D _borderSprite;
    private readonly Texture2D _selectedSprite;
    private readonly int _inventoryContainerWidth;
    private readonly int _inventoryContainerHeight;
    private readonly SpriteFont _font;

    public int CurrentlySelectedSlot {
      get => _currentlySelectedSlot;
      set => _currentlySelectedSlot = value < 0 ? InventoryWidth -1 : value % InventoryWidth;
    }

    public InventorySlot? CurrentlySelectedItem => ContentsOfSlot(CurrentlySelectedSlot);

    public Inventory(ContentManager content, ScalingViewportAdapter hudScale) {
      _content = content;
      _hudScale = hudScale;
      _inventoryContents = new InventorySlot?[InventoryWidth];
      _currentlySelectedSlot = 0;
      _borderSprite = content.LoadTexture(SpriteName.InventoryContainer);
      _selectedSprite = content.LoadTexture(SpriteName.InventorySelected);
      _font = content.LoadFont(SpriteName.ConsoleFont);

      _inventoryContainerWidth = _borderSprite.Width;
      _inventoryContainerHeight = _borderSprite.Height;
    }
    
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
      var renderY = _hudScale.VirtualHeight - _inventoryContainerHeight - InventoryBottomMarginPx;
      var renderX = (_hudScale.VirtualWidth - _inventoryContainerWidth * InventoryWidth) / 2;
      
      for (var i = 0; i < InventoryWidth; i++) {
        var position = new Vector2(renderX + (i * _inventoryContainerWidth), renderY);
        // Draw background
        spriteBatch.Draw(
            _borderSprite,
            position,
            Color.White);

        // Draw item sprite
        var itemAtSlot = ContentsOfSlot(i);
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

      var selectedX =
          new Vector2(renderX + (CurrentlySelectedSlot * _inventoryContainerWidth), renderY);
      // Accommodate larger size of selected texture
      selectedX -= Vector2.One;
      spriteBatch.Draw(
          _selectedSprite,
          selectedX,
          Color.White);
    }

    public void AddItem(InventoryItem item, uint amount = 1) {
      var firstAvailableSlot = -1;
      for (var i = 0; i < _inventoryContents.Length; i++) {
        var contents = ContentsOfSlot(i);
        if (contents == null) {
          if (firstAvailableSlot == -1) {
            firstAvailableSlot = i;
          }
        } else {
          if (contents.Item == item) {
            contents.Count += amount;
            return;
          }
        }
      }

      if (firstAvailableSlot == -1) {
        // TODO support this
        throw new Exception("No support yet for more items than slots");
      }

      _inventoryContents[firstAvailableSlot] = new InventorySlot {
          Item = item,
          Count = amount,
      };
    }

    private InventorySlot? ContentsOfSlot(int slot) {
      if (slot >= _inventoryContents.Length || slot < 0) {
        return null;
      }
      if (_inventoryContents[slot] == null) {
        return null;
      }
      return _inventoryContents[slot].Count < 1 ? null : _inventoryContents[slot];
    }
  }

  public class InventorySlot {
    private Sprite? _cachedSprite;
    
    public InventoryItem Item;
    public uint Count;

    public PlantType PlantType() {
      return Items.ItemMap[Item].PlantType;
    }

    public Sprite GetSprite(ContentManager content) {
      _cachedSprite ??= content.LoadSprite(Items.ItemMap[Item].Sprite);
      return _cachedSprite;
    }

    // Returns whether the specified amount is available to be used
    public bool Expend(uint amount) {
      if (amount > Count) {
        Count = 0;
        return false;
      }

      Count -= amount;
      return true;
    }
  }
}