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
        private static readonly TimeSpan UpdateInterval = TimeSpan.FromMilliseconds(20);
        private static readonly TimeSpan SpinnerProgressInterval = TimeSpan.FromMilliseconds(100);

        private readonly WriterContext _console;
        private readonly Spinner _spinner;
        private readonly Task _task;
        private readonly DateTime _init;

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

        public static Progress Unknown(string name, ICountable countable)
        {
            var progress = new Progress(name);
            progress.SetCount(countable);
            return progress;
        }

        public static Progress<T> Unknown<T>(string name, ICountable countable)
        {
            var progress = new Progress<T>(name);
            progress.SetCount(countable);
            return progress;
        }

        protected Progress(string name) : base(name)
        {
            _console = WriterContext.FromConsole(new Writer());
            _spinner = new Spinner(SpinnerProgressInterval);
            _init = DateTime.UtcNow;
            _task = UpdateDisplay(_source.Token);
        }

        private async Task UpdateDisplay(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Write(ConsoleColor.Yellow);

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

        private void Write(ConsoleColor color)
        {
            var writer = _console.Write("");

            var start = _init;

            var progress = 0.0;
            var count = Countable?.Count;
            if (count.HasValue)
            {
                progress = Math.Min((double)Current / count.Value, 1.0);

                start = Countable.Started.Value;
                // todo: estimate remaining time
            }

            var progressBackgroundChar = '\u2593';

            var progressWidth = (int)(progress * writer.WindowWidth);

            var spinner = "";
            if (progress < 1)
            {
                spinner = _spinner.ToString();
            }

            // todo: integrate this?
            Console.CursorVisible = false;

            var progressInfoLeft = $" {progress:P} ({Current} of {count}) {Name}";
            var progressInfoRight = $"(started {start:O}) {DateTime.UtcNow - start:dd\\.hh\\:mm\\:ss} ";

            writer
                .Background(color).Write("".PadRight(progressWidth))
                .Foreground(color).Write(spinner)
                .WriteLine("".PadRight(writer.WindowWidth - progressWidth - spinner.Length, progressBackgroundChar))
                .Foreground(color).WriteLine($"{progressInfoLeft}{"".PadRight(writer.WindowWidth - progressInfoLeft.Length - progressInfoRight.Length)}{progressInfoRight}")
                ;

            foreach (var counter in _counters.Values)
            {
                writer.WriteLine($"  {counter.Name}: {counter.FormattedValue}");
            }
        }

        public ProgressCounter Counter(string name)
        {
            return _counters.GetOrAdd(name, x => new ProgressCounter(x));
        }

        public void Dispose()
        {
            _source.Cancel();
            _task.Wait();

            _source.Dispose();

            // todo: show different output for !/complete, depending on settings

            //var totalTime = DateTime.UtcNow - _init;

            // todo: turn progress bar green, for instance?

            Write(ConsoleColor.Green);
        }
    }
}
