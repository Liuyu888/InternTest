using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OnCallScheduler.Rules
{
    /// <summary>
    /// The rule to compute fitness based on vacations.
    /// </summary>
    public class VacationRule : IRule
    {
        /// <summary>
        /// Stores the scheduler's config.
        /// </summary>
        private readonly SchedulerConfig _config;

        /// <summary>
        /// Stores the exclusion rules.
        /// </summary>
        private readonly ConcurrentDictionary<DateTime, Assignment[]> _exclusionRules;

        /// <summary>
        /// Initialize a new instance of <see cref="VacationRule"/>.
        /// </summary>
        /// <param name="config">The scheduler's config.</param>
        /// <param name="file">The csv file holding the vacation dates.</param>
        public VacationRule(SchedulerConfig config, string file)
        {
            bool warning = false;

            _config = config;

            List<Assignment> exclusions = new List<Assignment>();

            string[] lines = File.ReadAllLines(file);
            foreach (string line in lines)
            {
                string[] rule = line.Split(',');
                string engineer = rule[0];
                DateTime exclusionStart = DateTime.SpecifyKind(
                    DateTime.Parse(rule[1]),
                    DateTimeKind.Local);
                DateTime exclusionEnd = DateTime.SpecifyKind(
                    DateTime.Parse(rule[2]),
                    DateTimeKind.Local);

                // If the start is a saturday, let them not be on call on a friday.
                if (exclusionStart.DayOfWeek == DayOfWeek.Saturday)
                {
                    exclusionStart -= TimeSpan.FromDays(1);
                }

                DateTime[] vacationDates = Utilities.DateSequence(exclusionStart, exclusionEnd).Where(d => d > config.StartDate).ToArray();
                foreach (DateTime date in vacationDates)
                {
                    exclusions.Add(new Assignment(new AssignmentKey(date, null), config.ScheduleConfig.GetEngineerFromAlias(engineer)));
                }

                if (vacationDates.Length > 90)
                {
                    ConsoleColor originalColor = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Warning: Engineer {0}'s vacation {1} - {2} exceeds 90 days!", engineer, exclusionStart, exclusionEnd);
                    Console.ForegroundColor = originalColor;
                    warning = true;
                }
            }

            // Add a default "vacation" rule up to the date that the new engineer starts
            foreach (Engineer addedEngineer in config.ScheduleConfig.ActiveEngineers.Where(e => e.StartDate != null))
            {
                foreach (DateTime date in Utilities.DateSequence(config.StartDate, addedEngineer.StartDate.Value - TimeSpan.FromDays(1)))
                {
                    exclusions.Add(new Assignment(new AssignmentKey(date, null), addedEngineer));
                }
            }

            _exclusionRules = new ConcurrentDictionary<DateTime, Assignment[]>(
                exclusions
                    .GroupBy(e => e.Key.Date)
                    .ToDictionary(g => g.Key, g => g.ToArray()));

            if (warning)
            {
                Console.WriteLine("Warnings detected, please double check then press ENTER to continue...");
                Console.ReadLine();
            }
        }

        /// <summary>
        /// Compute a fitness score for this rule on the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <returns>A fitness score for the schedule.</returns>
        public double ComputeFitness(Schedule schedule)
        {
            double score = 0;
            foreach (Assignment assignment in schedule.Where(a => a.Key.Date > _config.StartDate))
            {
                Assignment[] excludedAssignmentsOnDay;
                if (_exclusionRules.TryGetValue(assignment.Key.Date, out excludedAssignmentsOnDay))
                {
                    foreach (Assignment excludedAssignment in excludedAssignmentsOnDay)
                    {
                        if (excludedAssignment.Key.Date == assignment.Key.Date &&
                            excludedAssignment.Engineer == assignment.Engineer &&
                            (excludedAssignment.Key.Queue == null || excludedAssignment.Key.Queue == assignment.Key.Queue))
                        {
                            score -= 10000;
                        }
                    }
                }
            }

            return score;
        }

        /// <summary>
        /// Dump the statistics for the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <param name="writer">The output for the statistics.</param>
        public void DumpStats(Schedule schedule, TextWriter writer)
        {
        }
    }
}
