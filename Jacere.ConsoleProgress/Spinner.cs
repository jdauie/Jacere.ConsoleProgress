using System;

namespace Jacere.ConsoleProgress
{
    public class Spinner
    {
        private const string ProgressIndicatorChars = @"/-\|";

        private readonly TimeSpan _progressInterval;

        private char _lastProgressIndicator = ProgressIndicatorChars[0];
        private DateTime _lastProgressIndicatorTime;

        public Spinner(TimeSpan progressInterval)
        {
            _progressInterval = progressInterval;
        }

        public override string ToString()
        {
            if (_lastProgressIndicatorTime.Add(_progressInterval) <= DateTime.UtcNow)
            {
                var lastProgressIndicatorIndex = ProgressIndicatorChars.IndexOf(_lastProgressIndicator);
                var nextProgressIndicator = ProgressIndicatorChars[(lastProgressIndicatorIndex + 1) % ProgressIndicatorChars.Length];

                _lastProgressIndicator = nextProgressIndicator;
                _lastProgressIndicatorTime = DateTime.UtcNow;
            }

            return _lastProgressIndicator.ToString();
        }
    }
}