using System;
using System.Linq;

namespace Steady_Image_Merger
{
    /// <summary>
    /// http://stackoverflow.com/a/7204071
    /// </summary>
    public static class TimeSpanExtensions
    {
        private enum TimeSpanElement
        {
            Millisecond,
            Second,
            Minute,
            Hour,
            Day
        }

        public static string ToFriendlyString(this TimeSpan timeSpan, int maxNrOfElements = 3)
        {
            maxNrOfElements = Math.Max(Math.Min(maxNrOfElements, 5), 1);
            var parts = new[]
                        {
                            Tuple.Create(TimeSpanElement.Day, timeSpan.Days),
                            Tuple.Create(TimeSpanElement.Hour, timeSpan.Hours),
                            Tuple.Create(TimeSpanElement.Minute, timeSpan.Minutes),
                            Tuple.Create(TimeSpanElement.Second, timeSpan.Seconds),
                            Tuple.Create(TimeSpanElement.Millisecond, timeSpan.Milliseconds)
                        }
                                        .SkipWhile(i => i.Item2 <= 0)
                                        .Take(maxNrOfElements);

            return string.Join(", ", parts.Select(p => string.Format("{0} {1}{2}", p.Item2, p.Item1, p.Item2 > 1 ? "s" : string.Empty)));
        }
    }
}