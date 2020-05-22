using System;

namespace Jacere.ConsoleProgress
{
    public interface ICountable
    {
        DateTime? Started { get; }
        long? Count { get; }
    }
}