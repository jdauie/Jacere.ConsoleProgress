using System;

namespace Jacere.ConsoleProgress
{
    public class ProgressBar
    {
        private const char ProgressBackgroundChar = '\u2593';

        private readonly Spinner _spinner;

        public ProgressBar(TimeSpan spinnerProgressInterval)
        {
            _spinner = new Spinner(spinnerProgressInterval);
        }

        public ProgressWriter Create(double progress)
        {
            return new ProgressWriter((writer, color) => Write(writer, color, progress));
        }

        private void Write(WriterContext writer, ConsoleColor color, double progress)
        {
            var spinner = "";
            if (progress < 1)
            {
                spinner = _spinner.ToString();
            }

            var progressWidth = (int)(progress * writer.WindowWidth);

            writer
                .Background(color).Write("".PadRight(progressWidth))
                .Foreground(color).Write(spinner)
                .WriteLine("".PadRight(writer.WindowWidth - progressWidth - spinner.Length, ProgressBackgroundChar));
        }
    }
}