using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Jacere.ConsoleProgress
{
    public class ConsoleProgress : IDisposable
    {
        private readonly int _updateInterval;
        private readonly string _title;
        private readonly DateTime _startTime;
        private readonly bool _showRate;
        private readonly Dictionary<string, long> _counts;
        private readonly Task _task;
        private long _totalCount;
        private long _progressCount;
        private bool _dirty;
        private bool _disposing;

        public ConsoleProgress(string title, long totalCount = 0, bool showRate = true, int updateInterval = 100)
        {
            _updateInterval = updateInterval;
            _title = title;
            _startTime = DateTime.UtcNow;
            _totalCount = totalCount;
            _showRate = showRate;
            _counts = new Dictionary<string, long>();
            _progressCount = 0;
            _dirty = true;
            _disposing = false;

            _task = UpdateDisplay();
        }

        private async Task UpdateDisplay()
        {
            while (!_disposing)
            {
                if (_dirty)
                {
                    Write();
                }
                await Task.Delay(_updateInterval);
            }
        }

        private IEnumerable<string> GetRemainingTimeEstimate(long progressCount)
        {
            var elapsed = DateTime.UtcNow - _startTime;
            var itemsPerMinute = (long)(progressCount / elapsed.TotalMinutes);
            var parts = new List<string>();
            if (_showRate)
            {
                parts.Add($"{itemsPerMinute} items/m");
            }
            if (progressCount <= _totalCount)
            {
                var remainingSeconds = elapsed.TotalSeconds * (_totalCount - progressCount) / progressCount;
                parts.Add($@"{TimeSpan.FromSeconds(remainingSeconds):dd\.hh\:mm\:ss} remaining");
            }
            else
            {
                parts.Add("unknown time remaining");
            }
            return parts;
        }

        public void SetTotal(long count)
        {
            Interlocked.Exchange(ref _totalCount, count);
            _dirty = true;
        }

        public void Increment()
        {
            Interlocked.Increment(ref _progressCount);
            _dirty = true;
        }

        public void Add(long value)
        {
            Interlocked.Add(ref _progressCount, value);
            _dirty = true;
        }

        public void Set(long value)
        {
            Interlocked.Exchange(ref _progressCount, value);
            _dirty = true;
        }

        public void Increment(string name)
        {
            lock (_counts)
            {
                if (!_counts.ContainsKey(name))
                {
                    _counts[name] = 1;
                }
                else
                {
                    ++_counts[name];
                }
            }

            _dirty = true;
        }

        private void Write()
        {
            var progressCount = _progressCount;
            var additionalParts = new List<string>();

            _dirty = false;

            lock (_counts)
            {
                if (_counts.Count > 0)
                {
                    additionalParts.AddRange(_counts.Select(x => $@"{x.Value} {x.Key}"));
                }
            }

            if (_totalCount > 0 && progressCount > 0)
            {
                additionalParts.AddRange(GetRemainingTimeEstimate(progressCount));
            }
            var additionalInfo = additionalParts.Any()
                ? $"({string.Join(", ", additionalParts)})"
                : "";

            Console.Write($"{new string(' ', Console.BufferWidth - 1)}\r");
            Console.Write($"{_title}: {progressCount} {additionalInfo}\r");
        }

        public void Dispose()
        {
            if (_disposing)
            {
                return;
            }

            _disposing = true;
            _task.GetAwaiter().GetResult();
            var totalTime = DateTime.UtcNow - _startTime;

            Console.Write($"{new string(' ', Console.BufferWidth - 1)}\r");
            Console.WriteLine($@"{_title}: {_progressCount} in {totalTime:dd\.hh\:mm\:ss}");
        }
    }
}