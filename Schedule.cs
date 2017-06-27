using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;

namespace OnCallScheduler
{
    public class Schedule : ReadOnlyCollection<Assignment>
    {
        /// <summary>
        /// Stores the instance of random that will be used.
        /// </summary>
        private static readonly StrongRandom _random = new StrongRandom();

        /// <summary>
        /// Stores the configuration for the schedule.
        /// </summary>
        private readonly ScheduleConfig _config;

        /// <summary>
        /// Caches the max date for this schedule.
        /// </summary>
        private readonly DateTime _maxDate;

        /// <summary>
        /// Initialize a new instance of <see cref="Schedule"/>.
        /// </summary>
        /// <param name="config">The configuration for the schedule.</param>
        /// <param name="schedule">The list of assignments in the schedule.</param>
        public Schedule(ScheduleConfig config, IEnumerable<Assignment> schedule)
            : base(schedule.ToList())
        {
            _config = config;
            _maxDate = schedule.Max(a => a.Key.Date);
        }

        /// <summary>
        /// Gets the config for this schedule.
        /// </summary>
        public ScheduleConfig ScheduleConfig
        {
            get
            {
                return _config;
            }
        }

        /// <summary>
        /// Gets the max date for this schedule.
        /// </summary>
        public DateTime MaxDate
        {
            get
            {
                return _maxDate;
            }
        }

        /// <summary>
        /// Save a schedule to a csv file.
        /// </summary>
        /// <param name="fileName">The name of the file to save to.</param>
        public void Save(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                Queue[] queues = _config.ActiveQueues.ToArray();

                writer.WriteLine("Rotation Start,{0}",
                    string.Join(",", queues.Select(q => q.Name)));

                foreach (IGrouping<DateTime, Assignment> rotationDay in this.GroupBy(s => s.Key.Date))
                {
                    writer.WriteLine("{0:yyyy-M-d hh:mm:ss},{1}",
                        rotationDay.Key + _config.RotationStartTime,
                        string.Join(",", queues.Select(q =>
                        {
                            Assignment assignment = rotationDay.Where(d => d.Key.Queue == q).SingleOrDefault();
                            return assignment == null ? "null" : assignment.Engineer.IcmName;
                        })));
                }
            }
        }

        /// <summary>
        /// Load an schedule from a csv file.
        /// </summary>
        /// <param name="config">The configurations related to the schedule.</param>
        /// <param name="fileName">The name of the file to load.</param>
        /// <returns>An instance of <see cref="Schedule"/> from the csv file.</returns>
        public static Schedule Load(ScheduleConfig config, string fileName)
        {
            List<Assignment> schedule = new List<Assignment>();

            IEnumerable<string> lines = File.ReadLines(fileName);
            IEnumerator<string> enumerator = lines.GetEnumerator();
            enumerator.MoveNext();
            string[] definition = enumerator.Current.Split(',');
            Queue[] queueDefinition = definition
                .Skip(1)
                .Select(n => config.GetQueueFromName(n))
                .ToArray();

            while (enumerator.MoveNext())
            {
                string[] assignment = enumerator.Current.Split(',');
                DateTime rotationStart = DateTime.SpecifyKind(
                    DateTime.Parse(assignment[0]),
                    DateTimeKind.Local);
                rotationStart = rotationStart.NormalizeDate();
                Engineer[] engineers = assignment
                    .Skip(1)
                    .Select(a => config.GetEngineerFromIcmName(a))
                    .ToArray();

                for (int i = 0; i < queueDefinition.Length; i++)
                {
                    schedule.Add(new Assignment(new AssignmentKey(rotationStart, queueDefinition[i]), engineers[i]));
                }
            }

            return new Schedule(config, schedule);
        }

        /// <summary>
        /// Create a new random schedule.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>A new random schedule.</returns>
        public static Schedule MakeRandom(ScheduleConfig config, DateTime startDate, DateTime endDate)
        {
            Dictionary<AssignmentKey, Assignment> schedule = new Dictionary<AssignmentKey, Assignment>(AssignmentKeyComparer.Instance);

            // Start from the first day available on the schedule.
            for (DateTime date = startDate; date < endDate; date = date.AddDays(1))
            {
                foreach (Queue queue in config.ActiveQueues)
                {
                    AssignmentKey key = new AssignmentKey(date, queue);

                    // Get a random engineer that will fill the queue.
                    Assignment randomAssignment = new Assignment(key, config.GetRandomEngineer());

                    // Add it to the schedule.
                    schedule[key] = randomAssignment;
                }
            }

            return new Schedule(config, schedule.Values);
        }

        /// <summary>
        /// Make random mutations in the schedule.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <returns>A new schedule with random mutations.</returns>
        public Schedule Mutate(int mutationSize)
        {
            // Clone the current schedule
            List<Assignment> schedule = this.ToList();

            // Swap mutations
            int swapMutations = _random.Next(0, mutationSize);
            for (int i = 0; i < swapMutations; i++)
            {
                int idx1 = _random.Next(0, schedule.Count);
                int idx2 = _random.Next(0, schedule.Count);
                Engineer engineer1 = schedule[idx1].Engineer;
                Engineer engineer2 = schedule[idx2].Engineer;
                schedule[idx1] = new Assignment(schedule[idx1].Key, engineer2);
                schedule[idx2] = new Assignment(schedule[idx2].Key, engineer1);
            }

            // Random mutations
            for (int i = 0; i < mutationSize - swapMutations; i++)
            {
                int idx = _random.Next(0, schedule.Count);
                schedule[idx] = new Assignment(schedule[idx].Key, _config.GetRandomEngineer());
            }

            return new Schedule(_config, schedule);
        }

        /// <summary>
        /// Combines the schedule with a new schedule.
        /// </summary>
        /// <param name="newSchedule">The new schedule to combine with.</param>
        /// <returns>A new schedule that contain the old schedule and the new schedule.</returns>
        /// <remarks>If the same assignment key exists in both the old and new schedule, the new schedule wins.</remarks>
        public Schedule Combine(Schedule newSchedule)
        {
            // Clone the current schedule
            Dictionary<AssignmentKey, Assignment> schedule = this.ToDictionary(s => s.Key, AssignmentKeyComparer.Instance);

            // Start from the first day available on the schedule.
            foreach (Assignment assignment in newSchedule)
            {
                // Try to replace existing, if non exists, add it.
                schedule[assignment.Key] = assignment;
            }

            return new Schedule(_config, schedule.Values);
        }

        /// <summary>
        /// Crossover with a partner.
        /// </summary>
        /// <param name="partner">The partner for the cross operation.</param>
        /// <returns>A new schedule that is crossed with the partner schedule.</returns>
        public Schedule Cross(Schedule partner)
        {
            if (this.Count != partner.Count)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Length of the two schedules must be identical. {0}!={1}",
                        this.Count,
                        partner.Count));
            }

            // Clone the current schedule
            List<Assignment> schedule = this.ToList();

            // Copy the engineer assignments from the partner between the cross points
            int crossStart = _random.Next(0, schedule.Count);
            int crossEnd = _random.Next(0, schedule.Count);
            for (int i = crossStart; i <= crossEnd; i++)
            {
                schedule[i] = new Assignment(schedule[i].Key, partner[i].Engineer);
            }

            return new Schedule(_config, schedule);
        }
    }
}
