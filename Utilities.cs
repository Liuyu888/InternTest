using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnCallScheduler
{
    public static class Utilities
    {
        /// <summary>
        /// Normalize the date such that the time of day part is removed.
        /// </summary>
        /// <param name="date">The date to normalize.</param>
        /// <returns>A new <see cref="DateTime"/> with time of day removed.</returns>
        public static DateTime NormalizeDate(this DateTime date)
        {
            return date - date.TimeOfDay;
        }

        /// <summary>
        /// Generate a sequence of dates.
        /// </summary>
        /// <param name="start">The start date. (inclusive)</param>
        /// <param name="end">The end date. (inclusive)</param>
        /// <returns>A sequence of dates between start and end.</returns>
        public static IEnumerable<DateTime> DateSequence(DateTime start, DateTime end)
        {
            for (DateTime date = start; date <= end; date = date.AddDays(1))
            {
                yield return date;
            }
        }

        /// <summary>
        /// Dump a dictionary to console.
        /// </summary>
        /// <typeparam name="T1">Dictionary key.</typeparam>
        /// <typeparam name="T2">Dictionary value.</typeparam>
        /// <param name="dictionary">The dictionary to dump.</param>
        public static void Dump<T1, T2>(this Dictionary<T1, T2> dictionary)
        {
            Dump(dictionary, Console.Out);
        }

        /// <summary>
        /// Dump a dictionary to the specified <paramref name="writer"/>.
        /// </summary>
        /// <typeparam name="T1">Dictionary key.</typeparam>
        /// <typeparam name="T2">Dictionary value.</typeparam>
        /// <param name="dictionary">The dictionary to dump.</param>
        /// <param name="writer">The writer to dump to.</param>
        public static void Dump<T1, T2>(this Dictionary<T1, T2> dictionary, TextWriter writer)
        {
            foreach (var entry in dictionary.OrderBy(e => e.Key))
            {
                writer.WriteLine("{0}\t{1}", entry.Key, entry.Value);
            }
        }

        /// <summary>
        /// Calculate standard deviation of a set of values.
        /// </summary>
        /// <param name="values">The values to compute over.</param>
        /// <returns>The standard deviation for the set.</returns>
        public static double CalculateStdDev(this IEnumerable<double> values)
        {
            double ret = 0;
            if (values.Count() > 1)
            {
                //Compute the Average      
                double avg = values.Average();

                //Perform the Sum of (value-avg)_2_2      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));

                //Put it all together      
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }

            return ret;
        }

        /// <summary>
        /// Check if the given date is a weekend.
        /// </summary>
        /// <param name="date">The date to check.</param>
        /// <returns><c>true</c> iff it's a weekend.</returns>
        public static bool IsWeekend(this DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday ||
                date.DayOfWeek == DayOfWeek.Sunday;
        }

        /// <summary>
        /// Get all week day assignments.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <returns>All weekday assignments.</returns>
        public static IEnumerable<Assignment> Weekdays(this IEnumerable<Assignment> schedule)
        {
            IEnumerable<Assignment> results = schedule
                .Where(a => !a.Key.Date.IsWeekend());

            return results;
        }

        /// <summary>
        /// Get all weekend assignments.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <returns>All weekend assignments.</returns>
        public static IEnumerable<Assignment> Weekends(this IEnumerable<Assignment> schedule)
        {
            IEnumerable<Assignment> results = schedule
                .Where(a => a.Key.Date.IsWeekend());

            return results;
        }
    }
}
