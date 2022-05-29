﻿using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.Entities;
using MonoGame.Extended.Entities.Systems;
using MonoGame.Extended.ViewportAdapters;
using TinyGardenGame.Components;

namespace TinyGardenGame.Systems {
  public class CameraSystem : EntityUpdateSystem {
    private readonly int _resolutionX;
    private readonly int _resolutionY;
    private readonly Game _game;
    private Vector2 _cameraPosition;
    private ComponentMapper<PlacementComponent> _positionComponentMapper;

    public OrthographicCamera Camera { get; private set; }
    public Matrix ViewMatrix => Camera.GetViewMatrix();

    public CameraSystem(Game game, int resolutionX, int resolutionY)
        : base(Aspect.All(typeof(PlacementComponent), typeof(CameraFollowComponent))) {
      _resolutionX = resolutionX;
      _resolutionY = resolutionY;
      _game = game;
      _cameraPosition = Vector2.Zero;
    }

    public override void Initialize(IComponentMapperService mapperService) {
      Camera = new OrthographicCamera(
          new BoxingViewportAdapter(
              _game.Window, _game.GraphicsDevice, _resolutionX, _resolutionY));
      _positionComponentMapper = mapperService.GetMapper<PlacementComponent>();
    }

    public override void Update(GameTime gameTime) {
        AdjustCameraPosition();
        Camera.LookAt(_cameraPosition);
    }

    private void AdjustCameraPosition() {
      if (ActiveEntities.Count != 1) {
        // TODO do this or handle more gracefully with custom exception
        throw new NotImplementedException("cannot handle camera following zero or 2+ sprites");
      }

      _cameraPosition = _positionComponentMapper.Get(ActiveEntities[0]).AbsolutePosition;
    }
  }
}