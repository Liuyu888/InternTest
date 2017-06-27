using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OnCallScheduler.Rules
{
    /// <summary>
    /// The rule to compute fitness based on day clustering.
    /// </summary>
    public class ClusteringRule : IRule
    {
        /// <summary>
        /// Stores a pre-computed dictionary of final date counts of all current active engineers in the previous schedule.
        /// </summary>
        private readonly ConcurrentDictionary<Engineer, DateCounts> _lastCounts;

        /// <summary>
        /// A struct that stores a cluster information.
        /// </summary>
        public class DateCounts
        {
            public Engineer Engineer { get; set; }

            public DateTime Start { get; set; }

            public int Length { get; set; }
        }

        /// <summary>
        /// A struct that stores a cluster information.
        /// </summary>
        public class DateStatistic
        {
            public int[] Durations { get; set; }

            public double[] Gaps { get; set; }

            public override string ToString()
            {
                return string.Format(
                    CultureInfo.InvariantCulture,
                    "\n Durations=({0:0.0}) {1}\n Gaps=({2:0.0}) {3}",
                    Durations.Sum() / (double)Durations.Length,
                    string.Join(",", Durations.Select(d => d.ToString("##"))),
                    Gaps.Sum() / Gaps.Length,
                    string.Join(",", Gaps.Select(d => d.ToString("##"))));
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ClusteringRule"/>.
        /// </summary>
        /// <param name="config">The scheduler's config.</param>
        /// <param name="oldSchedule">The schedule history.</param>
        public ClusteringRule(SchedulerConfig config, Schedule oldSchedule)
        {
            // Precompute date counts for the last time each engineer was on call in the old schedule.
            _lastCounts = new ConcurrentDictionary<Engineer, DateCounts>(config
                .ScheduleConfig
                .ActiveEngineers
                .ToDictionary(e => e, e =>
                {
                    // For the engineer in question, get the last few days where they are assigned.
                    DateCounts engineerLastDateCount = null;
                    foreach (Assignment assignment in oldSchedule
                        .Where(a => a.Engineer.Equals(e))
                        .Where(a => a.Key.Date < config.StartDate)
                        .OrderByDescending(a => a.Key.Date))
                    {
                        if (engineerLastDateCount == null)
                        {
                            // The final date in the old schedule that the engineer was assigned.
                            engineerLastDateCount = new DateCounts()
                            {
                                Engineer = e,
                                Length = 1,
                                Start = assignment.Key.Date,
                            };
                        }
                        else
                        {
                            if (engineerLastDateCount.Start - TimeSpan.FromDays(1) == assignment.Key.Date)
                            {
                                // If the next date is one day before the current count, increment the count.
                                engineerLastDateCount.Start = assignment.Key.Date;
                                engineerLastDateCount.Length++;
                            }
                            else
                            {
                                // This is another date cluster, we are done for this engineer.
                                break;
                            }
                        }
                    }

                    return engineerLastDateCount;
                }));
        }

        /// <summary>
        /// Compute a fitness score for this rule on the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <returns>A fitness score for the schedule.</returns>
        public double ComputeFitness(Schedule schedule)
        {
            Dictionary<Engineer, DateStatistic> statistics = ComputeEngineerClusteringStatistics(schedule);

            Dictionary<Engineer, int> meanDurations = statistics.ToDictionary(c => c.Key, c => c.Value.Durations.Select(d => Math.Abs(d - GetOptimalDuration(c.Key))).Sum());
            Dictionary<Engineer, int> overDurations = statistics.ToDictionary(c => c.Key, c => c.Value.Durations.Select(d => Math.Max(d - GetOptimalDuration(c.Key), 0)).Sum());
            Dictionary<Engineer, double> meanGaps = statistics.ToDictionary(c => c.Key, c => c.Value.Gaps.CalculateStdDev());

            double sumGaps = meanGaps.Sum(c => c.Value);
            double sumOvers = overDurations.Sum(c => c.Value);
            double sumDurations = meanDurations.Sum(c => c.Value);

            // Should be at least a week apart for standard schedules, linearly scaled to the engineer's optimal duration
            Dictionary<Engineer, int> closeGaps = statistics.ToDictionary(c => c.Key, c => c.Value.Gaps.Where(g => g < 7 * GetOptimalDuration(c.Key) / 2).Count());
            Dictionary<Engineer, int> tooCloseGaps = statistics.ToDictionary(c => c.Key, c => c.Value.Gaps.Where(g => g < 5).Count());

            return sumGaps * -1
                + sumOvers * -5
                + sumDurations * -1
                + closeGaps.Sum(c => c.Value) * -10
                + tooCloseGaps.Sum(c => c.Value) * -10000;
        }

        private int GetOptimalDuration(Engineer engineer)
        {
            return engineer.Alias == "jamestao" ? 4 :
                engineer.Alias == "alchin" ? 4 :
                engineer.Alias == "bcham" ? 4 : 2;
        }

        /// <summary>
        /// Compute how many times an engineer is on call on the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <returns>A dictionary of how many times an engineer is on call.</returns>
        public Dictionary<Engineer, DateStatistic> ComputeEngineerClusteringStatistics(IEnumerable<Assignment> schedule)
        {
            Dictionary<Engineer, DateCounts[]> counts = ComputeEngineerClustering(schedule)
                .GroupBy(c => c.Engineer)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(c => c.Start).ToArray());

            Dictionary<Engineer, DateStatistic> statistics = counts.ToDictionary(
                c => c.Key,
                c =>
                {
                    double[] gap = new double[c.Value.Length - 1];
                    for (int i = 0; i < c.Value.Length - 1; i++)
                    {
                        gap[i] = (c.Value[i + 1].Start - c.Value[i].Start).TotalDays;
                    }

                    return new DateStatistic()
                    {
                        Durations = c.Value.Select(c2 => c2.Length).ToArray(),
                        Gaps = gap.ToArray(),
                    };
                });

            return statistics;
        }

        /// <summary>
        /// Compute how many times an engineer is on call on the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <returns>A dictionary of how many times an engineer is on call.</returns>
        private IEnumerable<DateCounts> ComputeEngineerClustering(IEnumerable<Assignment> schedule)
        {
            HashSet<Engineer> engineersSeen = new HashSet<Engineer>();

            ConcurrentDictionary<Engineer, DateCounts> counts = new ConcurrentDictionary<Engineer, DateCounts>();
            foreach (IGrouping<DateTime, Assignment> day in schedule
                .GroupBy(a => a.Key.Date)
                .OrderBy(a => a.Key))
            {
                // Add counts for assignments today
                foreach (Assignment assignment in day)
                {
                    if (!_lastCounts.ContainsKey(assignment.Engineer))
                    {
                        continue;
                    }

                    if (!engineersSeen.Contains(assignment.Engineer))
                    {
                        engineersSeen.Add(assignment.Engineer);

                        DateCounts previousDateCount = _lastCounts[assignment.Engineer];
                        if (previousDateCount != null)
                        {
                            if (previousDateCount.Start + TimeSpan.FromDays(previousDateCount.Length) == assignment.Key.Date)
                            {
                                // Last time oncall was right on the border of the schedule, promote that to be the current cluster
                                counts.GetOrAdd(assignment.Engineer, new DateCounts()
                                {
                                    Engineer = previousDateCount.Engineer,
                                    Start = previousDateCount.Start,
                                    Length = previousDateCount.Length,
                                });
                            }
                            else
                            {
                                // Last time on call was in another cluster
                                yield return previousDateCount;
                            }
                        }
                    }

                    DateCounts count = counts.GetOrAdd(assignment.Engineer, e =>
                    {
                        return new DateCounts()
                        {
                            Engineer = e,
                            Start = day.Key,
                            Length = 0,
                        };
                    });

                    count.Length++;
                }

                // yield date counts for engineers no longer in the assignment
                foreach (Engineer engineer in counts
                    .Keys
                    .Where(e => !day.Any(d => d.Engineer.Equals(e)))
                    .ToArray())
                {
                    DateCounts count;
                    counts.TryRemove(engineer, out count);

                    yield return count;
                }
            }

            // Emit the final engineer's counts
            foreach (KeyValuePair<Engineer, DateCounts> count in counts)
            {
                yield return count.Value;
            }
        }

        /// <summary>
        /// Dump the statistics for the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <param name="writer">The output for the statistics.</param>
        public void DumpStats(Schedule schedule, TextWriter writer)
        {
            writer.WriteLine();
            writer.WriteLine("ClusteringRule");
            ComputeEngineerClusteringStatistics(schedule).Dump(writer);
        }
    }
}
