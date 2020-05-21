using System;
using System.Drawing;
using System.Linq;

namespace Jacere.ConsoleProgress
{
    public class WriterContext
    {
        private readonly IConsole _console;
        private readonly Point? _position;
        private readonly ConsoleColor? _foreground;
        private readonly ConsoleColor? _background;

        public int WindowWidth => _console.WindowWidth;

        private WriterContext(IConsole console, Point? position, ConsoleColor? foreground, ConsoleColor? background)
        {
            _console = console;
            _position = position;
            _foreground = foreground;
            _background = background;
        }

        public static WriterContext FromConsole(IConsole console)
        {
            return new WriterContext(console, new Point(console.CursorLeft, console.CursorTop), null, null);
        }

        public WriterContext Foreground(ConsoleColor foreground)
        {
            return new WriterContext(_console, _position, foreground, _background);
        }

        public WriterContext Background(ConsoleColor background)
        {
            return new WriterContext(_console, _position, _foreground, background);
        }

        public WriterContext Write(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var lines = value.Split('\n')
                    .Select(x => x.TrimEnd('\r'))
                    .ToList();

                for (var i = 0; i < lines.Count; i++)
                {
                    if (i < lines.Count - 1)
                    {
                        lines[i] = lines[i].PadRight(_console.WindowWidth - _console.CursorLeft - 1);
                    }
                }

                value = string.Join(Environment.NewLine, lines);
            }

            if (_position.HasValue)
            {
                _console.CursorLeft = _position.Value.X;
                _console.CursorTop = _position.Value.Y;
            }

            var foreground = _console.ForegroundColor;
            var background = _console.BackgroundColor;

            if (_foreground.HasValue)
            {
                _console.ForegroundColor = _foreground.Value;
            }

            if (_background.HasValue)
            {
                _console.BackgroundColor = _background.Value;
            }

            _console.Write(value);

            _console.ForegroundColor = foreground;
            _console.BackgroundColor = background;

            return new WriterContext(_console, null, null, null);
        }

        public WriterContext WriteLine(string value)
        {
            return Write(value).Write(Environment.NewLine);
        }
    }
}