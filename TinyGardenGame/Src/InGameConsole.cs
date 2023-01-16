using System;
using MonoGameConsole;
using TinyGardenGame.Config;
using TinyGardenGame.Core;

namespace TinyGardenGame {
  public class InGameConsole {
    private GameConsole _gameConsole;

    public delegate void MovePlayerHandler(int x, int y);

    public event MovePlayerHandler MovePlayer;
    public event EventHandler<int> SetHp;
    public event EventHandler<int> SetEnergy;

    public InGameConsole(MainGame game) {
      var spriteFont = game.Content.LoadFont(Vars.SpriteFont.Type.ConsoleFont);
      Initialize(game, new SpriteBatch(game.GraphicsDevice), spriteFont);
    }

    private void Initialize(MainGame game, SpriteBatch spriteBatch, SpriteFont spriteFont) {
      if (!GameConfig.Config.Debug.EnableConsole) {
        return;
      }

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
              BufferColor = Color.DarkBlue
          });
      _gameConsole.AddCommand("move", args => {
        MovePlayer?.Invoke(int.Parse(args[0]), int.Parse(args[1]));
        return "";
      });
      _gameConsole.AddCommand("sethp", args => {
        SetHp?.Invoke(this, int.Parse(args[0]));
        return "";
      });
      _gameConsole.AddCommand("setenergy", args => {
        SetEnergy?.Invoke(this, int.Parse(args[0]));
        return "";
      });
    }

    public void WriteLine(String text) => _gameConsole?.WriteLine(text);
  }
}