using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OnCallScheduler.Rules
{
    /// <summary>
    /// The rule to compute fitness based on paid holidays.
    /// </summary>
    public class PaidHolidayRule : IRule
    {
        /// <summary>
        /// Stores the exclusion rules.
        /// </summary>
        private readonly HashSet<DateTime> _vacationDays;

        /// <summary>
        /// Stores the old schedule.
        /// </summary>
        private readonly Schedule _oldSchedule;

        /// <summary>
        /// Initialize a new instance of <see cref="VacationRule"/>.
        /// </summary>
        /// <param name="file">The csv file holding the holidays.</param>
        /// <param name="oldSchedule">The schedule history.</param>
        public PaidHolidayRule(string file, Schedule oldSchedule)
        {
            List<Assignment> exclusions = new List<Assignment>();
            IEnumerable<DateTime> vacationDays = Enumerable.Empty<DateTime>();

            string[] lines = File.ReadAllLines(file);
            foreach (string line in lines)
            {
                string[] rule = line.Split(',');
                DateTime holidayStart = DateTime.SpecifyKind(
                    DateTime.Parse(rule[0]),
                    DateTimeKind.Local);
                DateTime holidayEnd = holidayStart;
                if (rule.Length > 1)
                {
                    holidayEnd = DateTime.SpecifyKind(
                        DateTime.Parse(rule[1]),
                        DateTimeKind.Local);
                }

                // Holiday long weekends
                if (holidayStart.DayOfWeek == DayOfWeek.Monday)
                {
                    holidayStart -= TimeSpan.FromDays(2);
                }

                if (holidayEnd.DayOfWeek == DayOfWeek.Friday)
                {
                    holidayEnd += TimeSpan.FromDays(2);
                }

                // Generate sequence of vacation days
                vacationDays = vacationDays.Concat(Utilities.DateSequence(holidayStart, holidayEnd));
            }

            _vacationDays = new HashSet<DateTime>(vacationDays);

            _oldSchedule = oldSchedule;
        }

        /// <summary>
        /// Compute a fitness score for this rule on the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <returns>A fitness score for the schedule.</returns>
        public double ComputeFitness(Schedule schedule)
        {
            return 0;
        }

        /// <summary>
        /// Compute a per engineer fitness score for this rule on the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <returns>A fitness score for the schedule.</returns>
        public Dictionary<Engineer, int> ComputePerEngineerFitness(Schedule schedule)
        {
            Assignment[] holidayAssignments = _oldSchedule.Combine(schedule)
                .Where(a => schedule
                    .ScheduleConfig
                    .IsActiveEngineer(a.Engineer))
                .Where(a => _vacationDays.Contains(a.Key.Date))
                .ToArray();

            Dictionary<Engineer, int> results = schedule
                .ScheduleConfig
                .ActiveEngineers
                .ToDictionary(e => e, e => holidayAssignments.Where(a => a.Engineer == e).Count());

            return results;
        }

        /// <summary>
        /// Dump the statistics for the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <param name="writer">The output for the statistics.</param>
        public void DumpStats(Schedule schedule, TextWriter writer)
        {
            writer.WriteLine("PaidHolidayRule");
            ComputePerEngineerFitness(schedule).Dump(writer);
        }
    }
}
