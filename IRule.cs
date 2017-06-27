using System.IO;

namespace OnCallScheduler
{
    /// <summary>
    /// The interface for a generic rule.
    /// </summary>
    public interface IRule
    {
        /// <summary>
        /// Compute a fitness score for this rule on the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <returns>A fitness score for the schedule.</returns>
        double ComputeFitness(Schedule schedule);

        /// <summary>
        /// Dump the statistics for the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <param name="writer">The output for the statistics.</param>
        void DumpStats(Schedule schedule, TextWriter writer);
    }
}
