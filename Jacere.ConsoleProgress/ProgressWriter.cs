using System;

namespace Jacere.ConsoleProgress
{
    public class ProgressWriter
    {
        private readonly Action<WriterContext, ConsoleColor> _write;

        public ProgressWriter(Action<WriterContext, ConsoleColor> write)
        {
            _write = write;
        }

        public void Write(WriterContext writer, ConsoleColor color)
        {
            _write(writer, color);
        }
    }
}