using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OnCallScheduler
{
    /// <summary>
    /// A class for holding all the configurations related to the schedule.
    /// </summary>
    public class ScheduleConfig
    {
        /// <summary>
        /// Stores the instance of random that will be used.
        /// </summary>
        private static readonly StrongRandom _random = new StrongRandom();

        /// <summary>
        /// The available list of engineers.
        /// </summary>
        private readonly Dictionary<string, Engineer> _engineers;

        /// <summary>
        /// The available list of engineers.
        /// </summary>
        private readonly Engineer[] _engineersArray;

        /// <summary>
        /// The available list of inactive engineers.
        /// </summary>
        private readonly Dictionary<string, Engineer> _inactiveEngineers;

        /// <summary>
        /// The available list of queues.
        /// </summary>
        private readonly Dictionary<string, Queue> _queues;

        /// <summary>
        /// The list of queues that existed in the past.
        /// </summary>
        private readonly Dictionary<string, Queue> _inactiveQueues;

        /// <summary>
        /// The time offset from midnight where the rotation starts.
        /// </summary>
        private readonly TimeSpan _rotationStartTime;

        /// <summary>
        /// Initializes a new instance of <see cref="ScheduleConfig"/>.
        /// </summary>
        /// <param name="engineers">An enumerable list of engineers that is used in the schedule.</param>
        /// <param name="queues">An enumerable list of queues that is used in the schedule.</param>
        /// <param name="rotationStartTime">The time offset from midnight where the rotation starts.</param>
        public ScheduleConfig(
            IEnumerable<Engineer> engineers,
            IEnumerable<Queue> queues,
            TimeSpan rotationStartTime)
        {
            _engineers = engineers.ToDictionary(e => e.Alias, StringComparer.OrdinalIgnoreCase);
            _engineersArray = engineers.ToArray();
            _inactiveEngineers = new Dictionary<string, Engineer>();
            _queues = queues.ToDictionary(q => q.Name);
            _inactiveQueues = new Dictionary<string, Queue>();
            _rotationStartTime = rotationStartTime;
        }

        /// <summary>
        /// Get a list of active queues.
        /// </summary>
        /// <returns>The list of active queues participating in the scheduling.</returns>
        public IEnumerable<Queue> ActiveQueues
        {
            get
            {
                return _queues.Values;
            }
        }

        /// <summary>
        /// Get a list of active engineers.
        /// </summary>
        /// <returns>The list of active engineers participating in the scheduling.</returns>
        public IEnumerable<Engineer> ActiveEngineers
        {
            get
            {
                return _engineersArray;
            }
        }

        /// <summary>
        /// Gets the time offset from midnight where the rotation starts.
        /// </summary>
        public TimeSpan RotationStartTime
        {
            get
            {
                return _rotationStartTime;
            }
        }

        /// <summary>
        /// Retrieve an engineer instance given the alias.
        /// </summary>
        /// <param name="alias">The alias of the engineer.</param>
        /// <returns>The instance of the engineer with the given <paramref name="alias"/>, or <c>null</c> if it's not found.</returns>
        public Engineer GetEngineerFromAlias(string alias)
        {
            Engineer engineer;
            _engineers.TryGetValue(alias, out engineer);

            return engineer;
        }

        /// <summary>
        /// Retrieve an engineer instance given the alias.
        /// </summary>
        /// <param name="icmName">The ICM name of the engineer.</param>
        /// <returns>The instance of the engineer with the given <paramref name="icmName"/>, or <c>null</c> if it's not found.</returns>
        public Engineer GetEngineerFromIcmName(string icmName)
        {
            Engineer engineer = _engineers.Values.Concat(_inactiveEngineers.Values).Where(e => e.IcmName == icmName).FirstOrDefault();
            if (engineer == null)
            {
                engineer = new Engineer(icmName, icmName, icmName, null, false);
                _inactiveEngineers.Add(icmName, engineer);
            }

            return engineer;
        }

        /// <summary>
        /// Get an random active engineer.
        /// </summary>
        /// <returns>An random active engineer.</returns>
        public Engineer GetRandomEngineer()
        {
            int idx = _random.Next(0, _engineersArray.Length);
            return _engineersArray[idx];
        }

        /// <summary>
        /// Get a list of active engineers.
        /// </summary>
        /// <returns>The list of active engineers participating in the scheduling.</returns>
        public bool IsActiveEngineer(Engineer engineer)
        {
            return _engineersArray.Any(e => e.Equals(engineer));
        }

        /// <summary>
        /// Retrieve a queue instance given the name.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        /// <returns>The instance of the queue with the given <paramref name="name"/>, or <c>null</c> if it's not found.</returns>
        public Queue GetQueueFromName(string name)
        {
            Queue queue;
            if (!_queues.TryGetValue(name, out queue))
            {
                if (!_inactiveQueues.TryGetValue(name, out queue))
                {
                    queue = new Queue(name);
                    _inactiveQueues.Add(name, queue);
                }
            }

            return queue;
        }
    }
}
