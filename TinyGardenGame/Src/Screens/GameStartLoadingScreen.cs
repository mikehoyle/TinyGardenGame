﻿#nullable enable
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MonoGame.Extended.Screens;
using NLog;
using TinyGardenGame.Config;
using TinyGardenGame.Core;
using TinyGardenGame.MapGeneration;

namespace TinyGardenGame.Screens {
  /** Currently, just load all textures with no UI */
  public class GameStartLoadingScreen : GameScreen {
    public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    
    private readonly MainGame _game;
    private Task? _loadingTask;
    private Task<GameMap>? _mapGenerationTask;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;
    private readonly Stopwatch _loadTimer;

    public GameStartLoadingScreen(MainGame game) : base(game) {
      _game = game;
      _loadTimer = new Stopwatch();
    }

    public override void Initialize() {
      Logger.Info("Beginning loading assets and generating map");
      _spriteBatch = new SpriteBatch(_game.GraphicsDevice);
      _loadTimer.Start();
    }

    public override void LoadContent() {
      // Cant use AssetLoading as we haven't initialized it yet
      _font = _game.Content.Load<SpriteFont>("fonts/Arial");
    }

    public override void Update(GameTime gameTime) {
      _loadingTask ??= AssetLoading.LoadAllAssets(Content, GameConfig.Config.AssetsConfigPath);
      _mapGenerationTask ??= GenerateMap();

      if (_loadingTask.IsCanceled || _loadingTask.IsFaulted) {
        throw new Exception("Loading assets failed", _loadingTask.Exception);
      }

      if (_mapGenerationTask.IsCanceled || _mapGenerationTask.IsFaulted) {
        throw new Exception("Map generation failed", _mapGenerationTask.Exception);
      }

      if (_loadingTask.IsCompleted && _mapGenerationTask.IsCompleted) {
        _loadTimer.Stop();
        _game.LoadScreen((game) => new PrimaryGameplayScreen(game, _mapGenerationTask.Result));
      }
    }

    public override void Draw(GameTime gameTime) {
      _game.GraphicsDevice.Clear(Color.Cornsilk);
      _spriteBatch.Begin(
          sortMode: SpriteSortMode.Deferred,
          samplerState: SamplerState.PointClamp);
      _spriteBatch.DrawString(
          _font, $"Loading... {_loadTimer.Elapsed}", new Vector2(5, 5), Color.Red);
      _spriteBatch.End();
    }

    private Task<GameMap> GenerateMap() {
      return Task.Run(() => new MapGenerator().GenerateMap());
    }
  }
}