using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OnCallScheduler.Rules
{
    /// <summary>
    /// Rule that verifies we have no missing people on the schedule. Due to random nature of generation we can end-up missing a person or two.
    /// </summary>
    public class EveryoneParticipatesRule : IRule
    {
        /// <summary>
        /// Schedule configuration with a collection of engineers
        /// </summary>
        private ScheduleConfig _scheduleConfig;

        /// <summary>
        /// Initialization constructor that takes schedule generation with a full set of engineers
        /// </summary>
        /// <param name="scheduleConfig">Schedule configuration with people collection</param>
        public EveryoneParticipatesRule(ScheduleConfig scheduleConfig)
        {
            _scheduleConfig = scheduleConfig;
        }

        /// <summary>
        /// Compute a fitness score for this rule on the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <returns>A fitness score for the schedule.</returns>
        public double ComputeFitness(Schedule schedule)
        {
            // Set of engineers participating in assignments
            ISet<Engineer> uniqueEngineers = new HashSet<Engineer>();

            // Iterate through all assignments
            foreach (Assignment assignment in schedule)
            {
                // Record engineer's participation
                uniqueEngineers.Add(assignment.Engineer);
            }

            // Check if everyone participates and deduct 10000 for every missing person
            return (uniqueEngineers.Count - _scheduleConfig.ActiveEngineers.Count()) * 10000;
        }

        public void DumpStats(Schedule schedule, TextWriter writer)
        {
        }
    }
}
