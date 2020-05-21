using System;

namespace Jacere.ConsoleProgress.Test.Extensions
{
    public static class NumberExtensions
    {
        public static string BytesToString(this long byteCount)
        {
            return BytesToString(byteCount, 0);
        }

        public static string BytesToString(this long byteCount, int decimalPlaces)
        {
            const string suf = "BKMGTPE";
            if (byteCount == 0)
                return $"0 {suf[0]}";
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), decimalPlaces);
            return $"{(Math.Sign(byteCount) * num)} {(place > 0 ? $"{suf[place]}i" : "")}{suf[0]}";
        }
    }
}
