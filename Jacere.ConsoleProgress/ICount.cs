using System;

namespace Jacere.ConsoleProgress
{
    public interface ICount
    {
        DateTime? Started { get; }
        long? Count { get; }
    }
}