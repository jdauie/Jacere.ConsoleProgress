using System;
using System.Threading;

namespace Jacere.ConsoleProgress
{
    public class ProgressCounter
    {
        public string Name { get; }

        private long _current;

        public long Current => _current;

        public Func<long, string> Formatter { get; private set; }

        public ProgressCounter(string name)
        {
            Name = name;
        }

        public void Format(Func<long, string> format)
        {
            Formatter = format;
        }

        public void Increment()
        {
            Interlocked.Increment(ref _current);
        }

        public void Add(long count)
        {
            Interlocked.Add(ref _current, count);
        }

        public void Set(long current)
        {
            Interlocked.Exchange(ref _current, current);
        }
    }
}