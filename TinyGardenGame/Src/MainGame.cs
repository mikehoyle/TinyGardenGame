﻿using System;
using System.Diagnostics;
using MonoGame.Extended.Screens;
using NLog;
using TinyGardenGame.Config;
using TinyGardenGame.Core;
using TinyGardenGame.Screens;

namespace TinyGardenGame; 

public class MainGame : Game {
  private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
  public static readonly int RenderResolutionWidth = 320;
  public static readonly int RenderResolutionHeight = 180;

  private GraphicsDeviceManager _graphics;
  private readonly ScreenManager _screenManager;

  public MainGame() {
    Logger.Info("Starting up game");
    GameConfig.Init();
    Platform.Init(Content);
    _graphics = new GraphicsDeviceManager(this);
    _screenManager = new ScreenManager();
    Content.RootDirectory = "Content";
    IsMouseVisible = true;
  }

  public void LoadScreen(Func<MainGame, GameScreen> screenCreator) {
    _screenManager.LoadScreen(screenCreator(this));
  }

  protected override void Initialize() {
    Debug.WriteLine("Initializing game");
    InitializeGraphics();
    base.Initialize();
    Services.AddService(Content);
    _screenManager.LoadScreen(new GameStartLoadingScreen(this));
  }

  private void InitializeGraphics() {
    _graphics.PreferredBackBufferWidth = 1920;
    _graphics.PreferredBackBufferHeight = 1080;
    IsFixedTimeStep = true;
    TargetElapsedTime = TimeSpan.FromSeconds(1d / GameConfig.Config.FpsCap);
    _graphics.ApplyChanges();
  }

  protected override void Update(GameTime gameTime) {
    KeyboardInputState.Update();
    _screenManager.Update(gameTime);
  }

  protected override void Draw(GameTime gameTime) {
    _screenManager.Draw(gameTime);
  }
}