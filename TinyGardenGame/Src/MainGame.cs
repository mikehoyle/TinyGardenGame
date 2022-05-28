﻿using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;
using MonoGame.Extended.Screens;
using TinyGardenGame.Screens;

namespace TinyGardenGame {
  public class MainGame : Game {
    public static readonly int RenderResolutionWidth = 320;
    public static readonly int RenderResolutionHeight = 180;
    
    private GraphicsDeviceManager _graphics;
    private readonly ScreenManager _screenManager;
    private SpriteBatch _spriteBatch;
    public InGameConsole Console { get; }

    public MainGame() {
      _graphics = new GraphicsDeviceManager(this);
      _screenManager = new ScreenManager();
      Console = new InGameConsole();
      Components.Add(_screenManager);
      Content.RootDirectory = "Content";
      IsMouseVisible = true;
    }

    protected override void Initialize() {
      Debug.WriteLine("Initializing game");
      InitializeGraphics();
      _screenManager.LoadScreen(new PrimaryGameplayScreen(this));
      base.Initialize();
    }

    private void InitializeGraphics() {
      _graphics.PreferredBackBufferWidth = 1920;
      _graphics.PreferredBackBufferHeight = 1080;
      _graphics.ApplyChanges();
    }

    protected override void LoadContent() {
      _spriteBatch = new SpriteBatch(GraphicsDevice);
      // TODO this is referencing a compiled xna from the MonoGameConsoleCore lib that could get
      // cleaned up.
      var font = Content.Load<SpriteFont>("ConsoleFont");
      Console.Initialize(this, _spriteBatch, font);
    }

    protected override void Update(GameTime gameTime) {
      if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
          Keyboard.GetState().IsKeyDown(Keys.Escape))
        Exit();
      base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime) {
      base.Draw(gameTime);
    }
  }
}