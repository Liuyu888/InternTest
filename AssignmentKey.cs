using System;
using System.Collections.Generic;

namespace OnCallScheduler
{
    /// <summary>
    /// This class represents an assignment key.
    /// </summary>
    public class AssignmentKey
    {
        /// <summary>
        /// Stores the date of the assignment.
        /// </summary>
        private readonly DateTime _date;

        /// <summary>
        /// Stores the queue for which the engineer is scheduled for.
        /// </summary>
        private readonly Queue _queue;

        /// <summary>
        /// Initializes a new instance of <see cref="AssignmentKey"/>.
        /// </summary>
        /// <param name="date">The date of the assignment.</param>
        /// <param name="queue">The queue for which the engineer is scheduled for.</param>
        /// <param name="engineer">The engineer that is being assigned.</param>
        public AssignmentKey(DateTime date, Queue queue)
        {
            _date = date;
            _queue = queue;
        }

        /// <summary>
        /// The date of the assignment.
        /// </summary>
        public DateTime Date
        {
            get
            {
                return _date;
            }
        }

        /// <summary>
        /// The queue for which the engineer is scheduled for.
        /// </summary>
        public Queue Queue
        {
            get
            {
                return _queue;
            }
        }

        /// <summary>
        /// Gets the hash code for this key.
        /// </summary>
        /// <returns>The hash code for this key.</returns>
        public override int GetHashCode()
        {
            return AssignmentKeyComparer.Instance.GetHashCode(this);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            AssignmentKey other = obj as AssignmentKey;
            return AssignmentKeyComparer.Instance.Equals(this, other);
        }
    }
}
