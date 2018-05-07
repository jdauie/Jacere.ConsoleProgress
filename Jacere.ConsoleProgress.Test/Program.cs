using System;
using System.Threading.Tasks;

namespace Jacere.ConsoleProgress.Test
{
    internal class Program
    {
        public static async Task Main(string[] args)
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
    }
}
