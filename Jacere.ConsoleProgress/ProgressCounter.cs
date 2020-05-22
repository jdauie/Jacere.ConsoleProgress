using System;
using System.Threading;

namespace Jacere.ConsoleProgress
{
    public class ProgressCounter
    {
        public string Name { get; }

        private long _current;

        public long Current => _current;
        public long? Total => Countable?.Count;
        public ICountable Countable { get; private set; }

        public Func<ProgressCounter, string> Formatter { get; private set; }

        public string FormattedValue => Formatter != null
            ? Formatter(this)
            : Current.ToString("n0");

        public ProgressCounter(string name)
        {
            Name = name;
        }

        public ProgressCounter Format(Func<ProgressCounter, string> format)
        {
            Formatter = format;
            return this;
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

        public void SetCount(long total)
        {
            SetCount(new StaticCountable(total));
        }

        public ProgressCounter SetCount(ICountable countable)
        {
            if (Countable != null)
            {
                throw new InvalidOperationException("count is already initialized");
            }
            Countable = countable;
            return this;
        }

        private class StaticCountable : ICountable
        {
            public DateTime? Started { get; }
            public long? Count { get; }

            public StaticCountable(long total)
            {
                Started = DateTime.UtcNow;
                Count = total;
            }
        }
    }
}