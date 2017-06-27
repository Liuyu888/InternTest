using System;
using System.IO;
using System.Linq;

namespace OnCallScheduler.Rules
{
    /// <summary>
    /// The rule to compute fitness based on vacations.
    /// </summary>
    public class CannotBeInMultipleQueueOnSameDayRule : IRule
    {
        /// <summary>
        /// Compute a fitness score for this rule on the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <returns>A fitness score for the schedule.</returns>
        public double ComputeFitness(Schedule schedule)
        {
            double score = 0;

            foreach (var day in schedule.GroupBy(s => s.Key.Date))
            {
                if (day.GroupBy(d => d.Engineer).Any(e => e.Count() > 1))
                {
                    score -= 10000;
                }
            }

            return score;
        }

        public void DumpStats(Schedule schedule, TextWriter writer)
        {
        }
    }
}
