using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OnCallScheduler.Rules
{
    /// <summary>
    /// The rule to compute fitness based everyone having the same number of on call days.
    /// </summary>
    public class EveryoneOncallSameNumberOfTimesRule : IRule
    {
        /// <summary>
        /// A struct that caches how many weekdays and weekends exists between two time ranges.
        /// </summary>
        public struct DateCounts
        {
            public int Weekdays { get; set; }

            public int Weekends { get; set; }

            /// <summary>
            /// Compute how many weekdays and weekends are in the given date range.
            /// </summary>
            /// <param name="minDate">Inclusive min date of the range.</param>
            /// <param name="maxDate">Inclusive max date of the range.</param>
            /// <returns>The date counts between the date range.</returns>
            public static DateCounts GetDateCounts(DateTime minDate, DateTime maxDate)
            {
                DateCounts result = new DateCounts();
                foreach (DateTime date in Utilities.DateSequence(minDate, maxDate))
                {
                    if (date.IsWeekend())
                    {
                        result.Weekends++;
                    }
                    else
                    {
                        result.Weekdays++;
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// A cache of date counts.
        /// </summary>
        private static ConcurrentDictionary<Tuple<DateTime, DateTime>, DateCounts> _dateCountCache =
            new ConcurrentDictionary<Tuple<DateTime, DateTime>, DateCounts>();

        /// <summary>
        /// Stores the old schedule.
        /// </summary>
        private readonly Schedule _oldSchedule;

        /// <summary>
        /// Stores the min date of the schedule.
        /// </summary>
        private readonly DateTime _minDate;

        /// <summary>
        /// A cache of engineer credit counts.
        /// </summary>
        private readonly ConcurrentDictionary<Engineer, DateCounts> _engineerCreditCounts;

        /// <summary>
        /// Initialize a new instance of <see cref="EveryoneOncallSameNumberOfTimesRule"/>.
        /// </summary>
        /// <param name="oldSchedule">The schedule history.</param>
        public EveryoneOncallSameNumberOfTimesRule(Schedule oldSchedule)
        {
            _oldSchedule = oldSchedule;
            _minDate = _oldSchedule.Min(a => a.Key.Date);
            DateTime oldScheduleEnd = _oldSchedule.Max(a => a.Key.Date);

            int countNewEngineers = _oldSchedule
                .ScheduleConfig
                .ActiveEngineers
                .Where(e => e.StartDate != null)
                .Count();

            Dictionary<Engineer, int> perEngineerWeekdayCounts, perEngineerWeekendCounts;
            CalculateEngineerCounts(_oldSchedule, out perEngineerWeekdayCounts, out perEngineerWeekendCounts);

            double oldAverageWeekdays = perEngineerWeekdayCounts.Keys
                .Where(e => e.StartDate == null)
                .Select(e => perEngineerWeekdayCounts[e])
                .Average();
            double oldAverageWeekends = perEngineerWeekendCounts.Keys
                .Where(e => e.StartDate == null)
                .Select(e => perEngineerWeekendCounts[e])
                .Average();

            TimeSpan oldScheduleDuration = oldScheduleEnd - _minDate;
            double averageWeekdaysPerDay = oldAverageWeekdays / oldScheduleDuration.TotalDays;
            double averageWeekendsPerDay = oldAverageWeekends / oldScheduleDuration.TotalDays;

            _engineerCreditCounts = new ConcurrentDictionary<Engineer, DateCounts>(_oldSchedule
                .ScheduleConfig
                .ActiveEngineers
                .Where(e => e.StartDate != null)
                .ToDictionary(e => e, e =>
                {
                    DateCounts counts = DateCounts.GetDateCounts(_minDate, e.StartDate.Value);

                    TimeSpan timeBeforeStartDate = e.StartDate.Value - _minDate;
                    counts.Weekdays = (int)(averageWeekdaysPerDay * timeBeforeStartDate.TotalDays * 1.15);
                    counts.Weekends = (int)(averageWeekendsPerDay * timeBeforeStartDate.TotalDays * 1.15);

                    return counts;
                }));
        }

        /// <summary>
        /// Compute a fitness score for this rule on the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <returns>A fitness score for the schedule.</returns>
        public double ComputeFitness(Schedule schedule)
        {
            DateTime maxDate = schedule.MaxDate;

            DateCounts dateCounts = _dateCountCache.GetOrAdd(new Tuple<DateTime, DateTime>(_minDate, maxDate), (k) => DateCounts.GetDateCounts(k.Item1, k.Item2));

            double averageWeekdaysPerEngineer = dateCounts.Weekdays * schedule.ScheduleConfig.ActiveQueues.Count() / schedule.ScheduleConfig.ActiveEngineers.Count();
            double averageWeekendDaysPerEngineer = dateCounts.Weekends * schedule.ScheduleConfig.ActiveQueues.Count() / schedule.ScheduleConfig.ActiveEngineers.Count();

            IEnumerable<Assignment> combinedSchedule = _oldSchedule
                .Combine(schedule)
                .Where(a => schedule
                    .ScheduleConfig
                    .IsActiveEngineer(a.Engineer));

            Dictionary<Engineer, int> perEngineerWeekdayCounts, perEngineerWeekendCounts;
            CalculateEngineerCounts(combinedSchedule, out perEngineerWeekdayCounts, out perEngineerWeekendCounts);

            double weekdayFitnessMax = perEngineerWeekdayCounts.Max(s => Math.Abs(s.Value - averageWeekdaysPerEngineer));
            double weekendFitnessMax = perEngineerWeekendCounts.Max(s => Math.Abs(s.Value - averageWeekendDaysPerEngineer));
            double weekdayFitnessSum = perEngineerWeekdayCounts.Sum(s => Math.Abs(s.Value - averageWeekdaysPerEngineer));
            double weekendFitnessSum = perEngineerWeekendCounts.Sum(s => Math.Abs(s.Value - averageWeekendDaysPerEngineer));

            return weekdayFitnessMax * -15
                + weekdayFitnessSum * -1
                + weekendFitnessMax * -15
                + weekendFitnessSum * -1;
        }

        private void CalculateEngineerCounts(
            IEnumerable<Assignment> combinedSchedule,
            out Dictionary<Engineer, int> perEngineerWeekdayCounts,
            out Dictionary<Engineer, int> perEngineerWeekendCounts)
        {
            perEngineerWeekdayCounts = ComputePerEngineerCounts(combinedSchedule.Weekdays(), c => c.Weekdays);
            perEngineerWeekendCounts = ComputePerEngineerCounts(combinedSchedule.Weekends(), c => c.Weekends);
        }

        /// <summary>
        /// Compute how many times an engineer is on call on the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <returns>A dictionary of how many times an engineer is on call.</returns>
        public Dictionary<Engineer, int> ComputePerEngineerCounts(IEnumerable<Assignment> schedule, Func<DateCounts, int> countSelector)
        {
            Dictionary<Engineer, int> results = schedule
                .GroupBy(a => a.Engineer)
                .ToDictionary(g => g.Key, g => g.Count());

            // Add per engineer credits
            if (_engineerCreditCounts != null)
            {
                foreach (KeyValuePair<Engineer, DateCounts> engineerCredit in _engineerCreditCounts)
                {
                    if (results.ContainsKey(engineerCredit.Key))
                    {
                        results[engineerCredit.Key] += countSelector(engineerCredit.Value);
                    }
                    else
                    {
                        results[engineerCredit.Key] = countSelector(engineerCredit.Value);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Dump the statistics for the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <param name="writer">The output for the statistics.</param>
        public void DumpStats(Schedule schedule, TextWriter writer)
        {
            Dictionary<Engineer, int> currentPerEngineerWeekdayCounts, currentPerEngineerWeekendCounts;
            CalculateEngineerCounts(schedule, out currentPerEngineerWeekdayCounts, out currentPerEngineerWeekendCounts);

            Schedule combinedSchedule = _oldSchedule.Combine(schedule);
            Dictionary<Engineer, int> perEngineerWeekdayCounts, perEngineerWeekendCounts;
            CalculateEngineerCounts(combinedSchedule, out perEngineerWeekdayCounts, out perEngineerWeekendCounts);

            writer.WriteLine();
            writer.WriteLine("EveryoneOncallSameNumberOfTimesRule");
            writer.WriteLine("Current Period\t| All Time\t| Credits\t| All+Credits");
            writer.WriteLine("Week\t Week\t| Week\t Week\t| Week\t Week\t| Week\t Week");
            writer.WriteLine("Day\t End\t| Day\t End\t| Day\t End\t| Day\t End");

            foreach (Engineer engineer in schedule.ScheduleConfig.ActiveEngineers.OrderBy(e => e.Alias))
            {
                DateCounts credits = default(DateCounts);
                if (_engineerCreditCounts != null)
                {
                    credits = _engineerCreditCounts.SingleOrDefault(e => e.Key == engineer).Value;
                }

                writer.WriteLine(" {0}\t  {1}\t|  {2}\t  {3}\t|  {4}\t  {5}\t|  {6}\t  {7}\t- {8}",
                    currentPerEngineerWeekdayCounts.SingleOrDefault(e => e.Key == engineer).Value - credits.Weekdays,
                    currentPerEngineerWeekendCounts.SingleOrDefault(e => e.Key == engineer).Value - credits.Weekends,
                    perEngineerWeekdayCounts.SingleOrDefault(e => e.Key == engineer).Value - credits.Weekdays,
                    perEngineerWeekendCounts.SingleOrDefault(e => e.Key == engineer).Value - credits.Weekends,
                    credits.Weekdays,
                    credits.Weekends,
                    perEngineerWeekdayCounts.SingleOrDefault(e => e.Key == engineer).Value,
                    perEngineerWeekendCounts.SingleOrDefault(e => e.Key == engineer).Value,
                    engineer);
            }
        }
    }
}
