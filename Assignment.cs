using System;

namespace OnCallScheduler
{
    /// <summary>
    /// This class represents an assignment.
    /// </summary>
    public class Assignment
    {
        /// <summary>
        /// Stores the date of the assignment.
        /// </summary>
        private readonly AssignmentKey _key;

        /// <summary>
        /// Stores the engineer that is being assigned.
        /// </summary>
        private readonly Engineer _engineer;

        /// <summary>
        /// Initializes a new instance of <see cref="Assignment"/>.
        /// </summary>
        /// <param name="key">The date and queue for which the engineer is scheduled for.</param>
        /// <param name="engineer">The engineer that is being assigned.</param>
        public Assignment(AssignmentKey key, Engineer engineer)
        {
            _key = key;
            _engineer = engineer;
        }

        /// <summary>
        /// The date and queue for which the engineer is scheduled for.
        /// </summary>
        public AssignmentKey Key
        {
            get
            {
                return _key;
            }
        }

        /// <summary>
        /// The engineer that is being assigned.
        /// </summary>
        public Engineer Engineer
        {
            get
            {
                return _engineer;
            }
        }
    }
}
