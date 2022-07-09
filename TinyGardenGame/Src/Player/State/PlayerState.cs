using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using TinyGardenGame.Core;
using TinyGardenGame.Core.Components;
using TinyGardenGame.Core.Components.Drawables;
using TinyGardenGame.Player.Components;
using TinyGardenGame.Player.State.Inventory;

namespace TinyGardenGame.Player.State {
  /**
   * Encapsulate all state of Player:
   * - ECS Entity
   * - Inventory
   * - Tools
   * - More?
   */
  public class PlayerState {
    public Entity PlayerEntity { get; private set; }
    public PlayerInventory Inventory { get; }

    public PlayerState() {
      Inventory = new PlayerInventory();
    }
    
    public void InitializePlayerCharacter(World world, MainGame game) {
      var playerSprite = game.Content.LoadAnimated(SpriteName.Player);
      PlayerEntity = world.CreateEntity()
          .AttachAnd(new DrawableComponent(new AnimatedSpriteDrawable(playerSprite)))
          .AttachAnd(new CameraFollowComponent())
          .AttachAnd(new MotionComponent(game.Config.PlayerSpeed))
          .AttachAnd(new PlayerInputComponent())
          .AttachAnd(new PositionComponent(MapPlacementHelper.CenterOfMapTile(0, 0)))
          .AttachAnd(new CollisionFootprintComponent(new RectangleF(-0.35f, -0.35f, 0.6f, 0.6f)))
          .AttachAnd(new SelectionComponent());
    }
  }
}