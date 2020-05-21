using System;

namespace Jacere.ConsoleProgress
{
    public class Writer : IConsole
    {
        public void Write(string value)
        {
            Console.Write(value);
        }

        public int WindowWidth => Console.WindowWidth;
        public int WindowHeight => Console.WindowHeight;

        public int CursorLeft
        {
            get => Console.CursorLeft;
            set => Console.CursorLeft = value;
        }

        public int CursorTop
        {
            get => Console.CursorTop;
            set => Console.CursorTop = value;
        }

        public int Y
        {
            get => Console.CursorTop;
            set => Console.CursorTop = value;
        }

        public int X
        {
            get => Console.CursorLeft;
            set => Console.CursorLeft = value;
        }

        public ConsoleColor ForegroundColor
        {
            get => Console.ForegroundColor;
            set => Console.ForegroundColor = value;
        }

        public ConsoleColor BackgroundColor
        {
            get => Console.BackgroundColor;
            set => Console.BackgroundColor = value;
        }
    }
}