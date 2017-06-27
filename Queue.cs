using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OnCallScheduler
{
    /// <summary>
    /// Represents a queue that will participate in a schedule.
    /// </summary>
    public class Queue
    {
        /// <summary>
        /// Stores the name of the queue.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// Initializes a new instance of <see cref="Engineer"/>.
        /// </summary>
        /// <param name="name">The name of the queue.</param>
        internal Queue(string name)
        {
            _name = name;
        }

        /// <summary>
        /// The name of the queue.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Load a list of queues from a csv file.
        /// </summary>
        /// <param name="fileName">The name of the file to load.</param>
        /// <returns>An enumerable list of queues from that file.</returns>
        public static IEnumerable<Queue> Load(string fileName)
        {
            return File.ReadLines(fileName)
                .Select(l => Queue.Parse(l));
        }

        /// <summary>
        /// Parses a line from the queue definition csv file.
        /// </summary>
        /// <param name="line">The line to parse.</param>
        /// <returns>An instance of <see cref="Queue"/> with the information in the given <paramref name="line"/>.</returns>
        private static Queue Parse(string line)
        {
            return new Queue(line);
        }

        /// <summary>
        /// Gets the hash code for this queue.
        /// </summary>
        /// <returns>The hash code for this queue.</returns>
        public override int GetHashCode()
        {
            return _name.GetHashCode();
        }

        /// <summary>
        /// Formats the given queue to a csv line.
        /// </summary>
        /// <returns>The csv representation for this queue.</returns>
        public override string ToString()
        {
            return string.Format("{0}", _name);
        }
    }
}
