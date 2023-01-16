using MonoGame.Extended.Sprites;
using MonoGame.Extended.TextureAtlases;
using MonoGame.Extended.ViewportAdapters;
using TinyGardenGame.Core;
using TinyGardenGame.Player.State;

namespace TinyGardenGame.Hud {
  public class ResourcesDisplay {
    private const int TopMargin = 10;
    private const int MeterHeight = 45;

    private readonly Point _energyMeterOrigin;
    private readonly Point _hpMeterOrigin;
    private readonly int _meterWidth;
    private readonly int _meterInnerHeight;
    private readonly ScalingViewportAdapter _hudScale;
    private readonly PlayerState _playerState;
    private readonly NinePatchRegion2D _meterBorder;
    private readonly Sprite _meterFillEmpty;
    private readonly Sprite _meterFillHp;
    private readonly Sprite _meterFillEnergy;

    public ResourcesDisplay(
        ContentManager gameContent, ScalingViewportAdapter hudScale, PlayerState playerState) {
      _hudScale = hudScale;
      _playerState = playerState;
      _meterBorder = gameContent.LoadNinepatch(Vars.Sprite.Type.InventoryContainer);
      _meterFillEmpty = gameContent.LoadSprite(Vars.Sprite.Type.ProgressBarFillEmpty);
      _meterFillHp = gameContent.LoadSprite(Vars.Sprite.Type.ProgressBarFillHp);
      _meterFillEnergy = gameContent.LoadSprite(Vars.Sprite.Type.ProgressBarFillEnergy);
      var paddingTopPx = HeadsUpDisplay.PaddingPx + TopMargin;
      var paddingRightPx = HeadsUpDisplay.PaddingPx;

      _meterWidth =
          _meterBorder.LeftPadding
          + _meterBorder.RightPadding
          + _meterFillEmpty.TextureRegion.Width;
      _energyMeterOrigin = new Point(
          hudScale.VirtualWidth - _meterWidth - paddingRightPx, paddingTopPx);
      _hpMeterOrigin = new Point(
          _energyMeterOrigin.X - _meterWidth - paddingRightPx, paddingTopPx);
      _meterInnerHeight = MeterHeight - _meterBorder.TopPadding - _meterBorder.TopPadding;
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime) {
      spriteBatch.Draw(
          _meterBorder,
          new Rectangle(_energyMeterOrigin, new Point(_meterWidth, MeterHeight)),
          Color.White);
      spriteBatch.Draw(
          _meterBorder,
          new Rectangle(_hpMeterOrigin, new Point(_meterWidth, MeterHeight)),
          Color.White);

      populateResourceBar(spriteBatch, _energyMeterOrigin, _playerState.Energy, _meterFillEnergy);
      populateResourceBar(spriteBatch, _hpMeterOrigin, _playerState.Hp, _meterFillHp);
    }

    /**
     * Fill pixel-by-pixel from top to bottom.
     */
    public void populateResourceBar(
        SpriteBatch spriteBatch, Point origin, ResourceMeter meter, Sprite fillTexture) {
      var innerOrigin = new Point(
          origin.X + _meterBorder.LeftPadding, origin.Y + _meterBorder.TopPadding);
      for (var y = 0; y < _meterInnerHeight; y++) {
        var texture = _meterFillEmpty;
        if (meter.PercentFull != 0) {
          texture =
              meter.PercentFull >= ((_meterInnerHeight - y - 1) / ((double)_meterInnerHeight - 1))
                  ? fillTexture
                  : _meterFillEmpty;
        }

        spriteBatch.Draw(texture, new Vector2(innerOrigin.X, y + innerOrigin.Y));
      }
    }
  }
}