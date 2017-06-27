using System;
using System.IO;
using System.Linq;

namespace OnCallScheduler.Rules
{
    /// <summary>
    /// Rule that prefers pairing of non-incident manager with incident manager over two incident managers or no incident managers at all
    /// </summary>
    public class IncidentManagerRule : IRule
    {
        /// <summary>
        /// Compute a fitness score for this rule on the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <returns>A fitness score for the schedule.</returns>
        public double ComputeFitness(Schedule schedule)
        {
            double score = 0;

            // Get all days in the scheduke
            foreach (IGrouping<DateTime, Assignment> day in schedule.GroupBy(s => s.Key.Date))
            {
                // For each day calculate the number of non-incident managers and incident managers
                int incidentManagerCount = day.Count(a => a.Engineer.IsIncidentManager);

                // Best option is to have no more than 1 incident manager
                if (incidentManagerCount != 1)
                {
                    score -= 7;  // 7 is less than 10 used in "fairness" rules so it has lesser impact on the schedule, but still impacts best schedule selection
                }
            }

            return score;
        }

        public void DumpStats(Schedule schedule, TextWriter writer)
        {
        }
    }
}
