﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameConsole;

namespace TinyGardenGame {
  public class InGameConsole {
    private GameConsole _gameConsole;

    public delegate void MovePlayerHandler(int x, int y);
    public event MovePlayerHandler MovePlayer;

    public void Initialize(Game game, SpriteBatch spriteBatch, SpriteFont spriteFont) {
      _gameConsole = new GameConsole(
          game,
          spriteBatch,
          new GameConsoleOptions {
              ToggleKey = (int)Keys.OemTilde,
              Font = spriteFont,
              FontColor = Color.Black,
              Prompt = ">",
              PromptColor = Color.Crimson,
              CursorColor = Color.Black,
              BackgroundColor = new Color(Color.DarkGray, 150),
              PastCommandOutputColor = Color.Olive,
              BufferColor = Color.DarkBlue});
      _gameConsole.AddCommand("move", args => {
        MovePlayer?.Invoke(int.Parse(args[0]), int.Parse(args[1]));
        return "";
      });
    }

    public void WriteLine(String text) => _gameConsole.WriteLine(text);
  }
}