using System;

namespace Jacere.ConsoleProgress
{
    public interface IConsole
    {
        int WindowWidth { get; }
        int WindowHeight { get; }
        int CursorTop { get; set; }
        int CursorLeft { get; set; }
        ConsoleColor ForegroundColor { get; set; }
        ConsoleColor BackgroundColor { get; set; }
        void Write(string value);
    }
}