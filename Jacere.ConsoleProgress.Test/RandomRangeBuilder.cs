using System;
using System.Collections.Generic;
using System.Linq;

namespace Jacere.ConsoleProgress.Test
{
    internal class RandomRangeBuilder
    {
        private readonly int? _seed;
        private readonly Dictionary<long, double> _probabilities = new Dictionary<long, double>();

        public RandomRangeBuilder()
        {
        }

        public RandomRangeBuilder(int seed)
        {
            _seed = seed;
        }

        public RandomRangeBuilder Range(long max, double probability)
        {
            _probabilities.Add(max, probability);
            return this;
        }

        public RandomRanges Create()
        {
            return new RandomRanges(this);
        }

        public class RandomRanges
        {
            private readonly Random _rand;
            private readonly List<MinMax> _ranges;

            public RandomRanges(RandomRangeBuilder thing)
            {
                _rand = thing._seed.HasValue ? new Random(thing._seed.Value) : new Random();

                _ranges = new List<MinMax>();
                foreach (var (key, value) in thing._probabilities.OrderBy(x => x.Key))
                {
                    var previous = _ranges.LastOrDefault();
                    var previousProbabilityMax = previous?.ProbabilityMax ?? 0;
                    _ranges.Add(new MinMax(previousProbabilityMax + value, previous?.Max ?? 0, key));
                }
            }

            public long Next()
            {
                var rand = _rand.NextDouble();
                var range = _ranges.First(x => x.ProbabilityMax > rand);
                return (long)(_rand.NextDouble() * (range.Max - range.Min)) + range.Min;
            }
        }

        private class MinMax
        {
            public double ProbabilityMax { get; set; }

            public long Min { get; set; }
            public long Max { get; set; }

            public MinMax(double probabilityMax, long min, long max)
            {
                ProbabilityMax = probabilityMax;
                Min = min;
                Max = max;
            }
        }
    }
}