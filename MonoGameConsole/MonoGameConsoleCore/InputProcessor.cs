using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace MonoGameConsole
{
    class InputProcessor
    {
        public event EventHandler Open = delegate { };
        public event EventHandler Close = delegate { };
        public event EventHandler PlayerCommand = delegate { };
        public event EventHandler ConsoleCommand = delegate { };

        public CommandHistory CommandHistory { get; set; }
        public OutputLine Buffer { get; set; }
        public List<OutputLine> Out { get; set; }

        private const int BACKSPACE = 8;
        private const int ENTER = 13;
        private const int TAB = 9;
        private bool isActive, isHandled;
        private CommandProcesser commandProcesser;
        private readonly KeyboardListener _keyboardListener;


        public InputProcessor(CommandProcesser commandProcesser, Game game)
        {
            this.commandProcesser = commandProcesser;
            isActive = false;
            CommandHistory = new CommandHistory();
            Out = new List<OutputLine>();
            Buffer = new OutputLine("", OutputLineType.Command);

            Debug.WriteLine("Initializing Input processor");
            _keyboardListener = new KeyboardListener();
            game.Components.Add(new InputListenerComponent(game, _keyboardListener));
            _keyboardListener.KeyPressed += OnKeyPressed;
        }


        private void OnKeyPressed(Object sender, KeyboardEventArgs e)
        {
            Debug.WriteLine($"Got keypress: {e.Key}");
            if ((int)e.Key == GameConsoleOptions.Options.ToggleKey) {
                ToggleConsole();
                return;
            }
            
            if (!isActive) return; // console is opened -> accept input

            CommandHistory.Reset();

            switch (e.Key) {
                case Keys.Enter:
                    ExecuteBuffer();
                    break;
                case Keys.Back:
                    if (Buffer.Output.Length > 0)
                    {
                        Buffer.Output = Buffer.Output.Substring(0, Buffer.Output.Length - 1);
                    }
                    break;
                case Keys.Tab:
                    AutoComplete();
                    break;
                case Keys.Up:
                    Buffer.Output = CommandHistory.Previous();
                    break;
                case Keys.Down:
                    Buffer.Output = CommandHistory.Next();
                    break;
                default:
                    if (e.Character != null && IsPrintable((char)e.Character))
                    {
                        Buffer.Output += (char)e.Character;
                    }
                    break;
            }
        }



        public void AddToBuffer(string text)
        {
            var lines = text.Split('\n').Where(line => line != "").ToArray();
            int i;
            for (i = 0; i < lines.Length - 1; i++)
            {
                var line = lines[i];
                Buffer.Output += line;
                ExecuteBuffer();
            }
            Buffer.Output += lines[i];
        }

        public void AddToOutput(string text)
        {
            if (GameConsoleOptions.Options.OpenOnWrite)
            {
                isActive = true;
                Open(this, EventArgs.Empty);
            }
            foreach (var line in text.Split('\n'))
            {
                Out.Add(new OutputLine(line, OutputLineType.Output));
            }
        }

        void ToggleConsole()
        {
            isActive = !isActive;
            if (isActive)
            {
                Open(this, EventArgs.Empty);
            }
            else
            {
                Close(this, EventArgs.Empty);
            }
        }


        void ExecuteBuffer()
        {
            if (Buffer.Output.Length == 0)
            {
                return;
            }
            var output = commandProcesser.Process(Buffer.Output).Split('\n').Where(l => l != "");
            Out.Add(new OutputLine(Buffer.Output, OutputLineType.Command));
            foreach (var line in output)
            {
                Out.Add(new OutputLine(line, OutputLineType.Output));
            }
            CommandHistory.Add(Buffer.Output);
            Buffer.Output = "";
        }

        void AutoComplete()
        {
            var lastSpacePosition = Buffer.Output.LastIndexOf(' ');
            var textToMatch = lastSpacePosition < 0 ? Buffer.Output : Buffer.Output.Substring(lastSpacePosition + 1, Buffer.Output.Length - lastSpacePosition - 1);
            var match = GetMatchingCommand(textToMatch);
            if (match == null)
            {
                return;
            }
            var restOfTheCommand = match.Name.Substring(textToMatch.Length);
            Buffer.Output += restOfTheCommand + " ";
        }

        static IConsoleCommand GetMatchingCommand(string command)
        {
            var matchingCommands = GameConsoleOptions.Commands.Where(c => c.Name != null && c.Name.StartsWith(command));
            return matchingCommands.FirstOrDefault();
        }


        static bool IsPrintable(char letter)
        {
            return GameConsoleOptions.Options.Font.Characters.Contains(letter);
        }
    }
}
