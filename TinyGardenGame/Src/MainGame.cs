using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Screens;
using TinyGardenGame.Core;
using TinyGardenGame.Screens;

namespace TinyGardenGame {
  public class MainGame : Game {
    public static readonly int RenderResolutionWidth = 320;
    public static readonly int RenderResolutionHeight = 180;
    
    private GraphicsDeviceManager _graphics;
    private readonly ScreenManager _screenManager;
    private SpriteBatch _spriteBatch;
    
    public InGameConsole Console { get; }
    public Config Config { get; }

    public MainGame() {
      Config = new Config();
      _graphics = new GraphicsDeviceManager(this);
      _screenManager = new ScreenManager();
      Console = new InGameConsole();
      Components.Add(_screenManager);
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
      _screenManager.LoadScreen(new GameStartLoadingScreen(this));
    }

    private void InitializeGraphics() {
      _graphics.PreferredBackBufferWidth = 1920;
      _graphics.PreferredBackBufferHeight = 1080;
      IsFixedTimeStep = true;
      TargetElapsedTime = TimeSpan.FromSeconds(1d / Config.FpsCap);
      _graphics.ApplyChanges();
    }

    protected override void LoadContent() {
      _spriteBatch = new SpriteBatch(GraphicsDevice);
      // TODO this is referencing a compiled xna from the MonoGameConsoleCore lib that could get
      // cleaned up.
      var font = Content.Load<SpriteFont>(Assets.ConsoleFont);
      Console.Initialize(this, _spriteBatch, font);
    }

    protected override void Update(GameTime gameTime) {
      if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
          Keyboard.GetState().IsKeyDown(Keys.Escape))
        Exit();
      base.Update(gameTime);
    }
  }
}