using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Jacere.ConsoleProgress
{
    public class Progress<T> : Progress
    {
        public Progress(string name) : base(name)
        {
        }

        public void Increment(T item)
        {
            // todo: display current item? (if desired)
            Increment();
        }
    }

    public class Progress : ProgressCounter, IDisposable
    {
        private const string ProgressIndicatorChars = @"/-\|";

        private static readonly TimeSpan UpdateInterval = TimeSpan.FromMilliseconds(20);
        private static readonly TimeSpan SpinnerProgressInterval = TimeSpan.FromMilliseconds(100);

        private readonly WriterContext _console;
        private readonly Task _task;
        private readonly DateTime _init;
        private DateTime? _start;

        private ICount _countable;
        private long? _total;
        private bool _complete;
        private char _lastProgressIndicator = ProgressIndicatorChars[0];
        private DateTime _lastProgressIndicatorTime;

        // todo: start time

        private readonly ConcurrentDictionary<string, ProgressCounter> _counters = new ConcurrentDictionary<string, ProgressCounter>();

        private readonly CancellationTokenSource _source = new CancellationTokenSource();

        public static Progress Known(string name, long count)
        {
            var progress = new Progress(name);
            progress.SetCount(count);
            return progress;
        }

        public static Progress<T> Known<T>(string name, long count)
        {
            var progress = new Progress<T>(name);
            progress.SetCount(count);
            return progress;
        }

        public static Progress Unknown(string name, ICount countable)
        {
            var progress = new Progress(name);
            progress.SetCount(countable);
            return progress;
        }

        public static Progress<T> Unknown<T>(string name, ICount countable)
        {
            var progress = new Progress<T>(name);
            progress.SetCount(countable);
            return progress;
        }

        protected Progress(string name) : base(name)
        {
            _console = WriterContext.FromConsole(new Writer());
            _init = DateTime.UtcNow;
            _task = UpdateDisplay(_source.Token);
        }

        private async Task UpdateDisplay(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                WriteStuff(ConsoleColor.Yellow);

                try
                {
                    await Task.Delay(UpdateInterval, token);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private void WriteStuff(ConsoleColor color)
        {
            var writer = _console.Write("");

            var start = _init;

            var progress = 0.0;
            var count = _countable?.Count ?? _total;
            if (count.HasValue)
            {
                progress = Math.Min((double)Current / count.Value, 1.0);

                start = _countable?.Started ?? _start.Value;
                // todo: estimate remaining time
            }
            else
            {
                //var start = _init;
            }

            var progressBackgroundChar = '\u2593';

            var progressWidth = (int)(progress * writer.WindowWidth);

            var progressIndicator = "";
            if (progress < 1)
            {
                if (_lastProgressIndicatorTime.Add(SpinnerProgressInterval) <= DateTime.UtcNow)
                {
                    var lastProgressIndicatorIndex = ProgressIndicatorChars.IndexOf(_lastProgressIndicator);
                    var nextProgressIndicator = ProgressIndicatorChars[(lastProgressIndicatorIndex + 1) % ProgressIndicatorChars.Length];

                    _lastProgressIndicator = nextProgressIndicator;
                    _lastProgressIndicatorTime = DateTime.UtcNow;
                }

                progressIndicator = _lastProgressIndicator.ToString();
            }

            // todo: integrate this
            Console.CursorVisible = false;

            var progressInfoLeft = $" {progress:P} ({Current} of {count}) {Name}";
            var progressInfoRight = $"(started {start:O}) {DateTime.UtcNow - start:dd\\.hh\\:mm\\:ss} ";

            writer
                .Background(color).Write("".PadRight(progressWidth))
                .Foreground(color).Write(progressIndicator)
                .WriteLine("".PadRight(writer.WindowWidth - progressWidth - progressIndicator.Length, progressBackgroundChar))
                .Foreground(color).WriteLine($"{progressInfoLeft}{"".PadRight(writer.WindowWidth - progressInfoLeft.Length - progressInfoRight.Length)}{progressInfoRight}")
                ;

            foreach (var counter in _counters.Values)
            {
                var value = counter.Formatter != null
                    ? counter.Formatter(counter.Current)
                    : counter.Current.ToString("n0");
                writer.WriteLine($"  {counter.Name}: {value}");
            }
        }

        public ProgressCounter Counter(string name)
        {
            return _counters.GetOrAdd(name, x => new ProgressCounter(x));
        }

        public void SetCount(long total)
        {
            if (_countable != null)
            {
                throw new InvalidOperationException("count is already initialized");
            }

            if (!_total.HasValue)
            {
                _start = DateTime.UtcNow;
            }
            _total = total;
        }

        public void SetCount(ICount countable)
        {
            if (_countable != null || _total.HasValue)
            {
                throw new InvalidOperationException("count is already initialized");
            }
            _countable = countable;
        }

        public void Complete()
        {
            _complete = true;
        }

        public void Dispose()
        {
            _source.Cancel();
            _task.Wait();

            _source.Dispose();

            // todo: show different output for !/complete, depending on settings

            var totalTime = DateTime.UtcNow - _init;

            // todo: turn progress bar green, for instance?

            WriteStuff(ConsoleColor.Green);
        }
    }
}
