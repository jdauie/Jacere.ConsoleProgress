using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ProgressBar _progressBar;
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
            _progressBar = new ProgressBar(SpinnerProgressInterval);
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

            // todo: integrate this?
            Console.CursorVisible = false;

            foreach (var componentWriter in GetComponentWriters())
            {
                componentWriter.Write(writer, color);
            }
        }

        private IEnumerable<ProgressWriter> GetComponentWriters()
        {
            var start = _init;

            var progress = 0.0;
            var count = Countable?.Count;
            if (count.HasValue)
            {
                progress = Math.Min((double)Current / count.Value, 1.0);

                start = Countable.Started.Value;
                // todo: estimate remaining time
            }

            yield return _progressBar.Create(progress);

            yield return ProgressText.Create(
                $" {progress:P} ({Current} of {count}) {Name}",
                $"(started {start:O}) {DateTime.UtcNow - start:dd\\.hh\\:mm\\:ss} ");

            foreach (var counter in _counters.Values)
            {
                yield return ProgressText.CreateUncolored($"  {counter.Name}: {counter.FormattedValue}");
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

            Write(ConsoleColor.Green);
        }
    }
}
