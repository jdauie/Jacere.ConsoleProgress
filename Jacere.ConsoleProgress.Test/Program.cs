using System;
using System.Linq;
using System.Threading.Tasks;
using Jacere.ConsoleProgress.Test.Extensions;

namespace Jacere.ConsoleProgress.Test
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            await TestProgress2();
        }

        private static async Task TestProgress1()
        {
            Console.WriteLine("before");

            const int count = 100;

            using (var progress = new ConsoleProgress("test", count))
            {
                for (var i = 0; i < count; i++)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(80));
                    progress.Increment();
                }
            }

            Console.WriteLine("after");
        }

        private static async Task TestProgress2()
        {
            Console.WriteLine("before");

            const int count = 1000;
            var totalTime = TimeSpan.FromSeconds(10);

            var randomRanges = new RandomRangeBuilder(1)
                .Range(10 * (1L << 10), 0.99)
                .Range(4 * (1L << 30), 0.0)
                .Range(10 * (1L << 30), 0.01)
                .Create();

            var files = Enumerable.Range(1, count)
                .Select(i => new File
                {
                    Name = $"test file {i}",
                    Size = randomRanges.Next(),
                }).ToList();

            var totalSize = files.Sum(x => x.Size);

            using (var progress = Progress.Known<File>("test", count))
            {
                progress.Counter("size").Format(x => $"{x:n0} ({x.BytesToString()})");

                foreach (var file in files)
                {
                    var size = file.Size;
                    var delay = file.Size * totalTime.TotalMilliseconds / totalSize;
                    while (delay > 0)
                    {
                        var tinyDelay = delay < 10 ? delay : 10;
                        var tinySize = delay < 10 ? size : (long)(10 * size / delay);
                        await Task.Delay(TimeSpan.FromMilliseconds(tinyDelay));
                        delay -= tinyDelay;
                        size -= tinySize;

                        progress.Counter("size").Add(tinySize);
                    }

                    progress.Increment(file);

                    if (file.Size > 1 * (1L << 30))
                    {
                        progress.Counter("large files").Increment();
                    }
                }
            }

            Console.WriteLine("after");
        }

        private class File
        {
            public string Name { get; set; }
            public long Size { get; set; }
        }
    }
}
