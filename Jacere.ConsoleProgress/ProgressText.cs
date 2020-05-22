using System;

namespace Jacere.ConsoleProgress
{
    public static class ProgressText
    {
        public static ProgressWriter Create(string left, string right = "")
        {
            return new ProgressWriter((writer, color) => Write(writer, color, left, right));
        }
        public static ProgressWriter CreateUncolored(string left, string right = "")
        {
            return new ProgressWriter((writer, color) => Write(writer, null, left, right));
        }

        private static void Write(WriterContext writer, ConsoleColor? color, string left, string right)
        {
            var padding = "".PadRight(writer.WindowWidth - left.Length - right.Length);

            if (color.HasValue)
            {
                writer = writer.Foreground(color.Value);
            }

            writer.WriteLine($"{left}{padding}{right}");
        }
    }
}