using System.Collections.Generic;

namespace OnCallScheduler
{
    /// <summary>
    /// This class implements the comparer for an <see cref="AssignmentKey"/>.
    /// </summary>
    public class AssignmentKeyComparer : IEqualityComparer<AssignmentKey>
    {
        /// <summary>
        /// Stores the singleton instance.
        /// </summary>
        private static readonly AssignmentKeyComparer _comparer =
            new AssignmentKeyComparer();

        /// <summary>
        /// Initializes a new instace of <see cref="AssignmentKeyComparer"/>.
        /// Marking constructor private to force the use of singleton instance.
        /// </summary>
        private AssignmentKeyComparer()
        {
        }

        /// <summary>
        /// Gets the singleton instance for this comparer.
        /// </summary>
        public static AssignmentKeyComparer Instance
        {
            get
            {
                return _comparer;
            }
        }

        /// <summary>
        /// Determines whether the specified keys are equal.
        /// </summary>
        /// <param name="x">The first key.</param>
        /// <param name="y">The second key.</param>
        /// <returns><c>true</c> iff the keys are equal.</returns>
        public bool Equals(AssignmentKey x, AssignmentKey y)
        {
            if (x == null ^ y == null)
            {
                return false;
            }

            if (x.Date != y.Date ||
                x.Queue != y.Queue)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the hash code for the given key.
        /// </summary>
        /// <param name="obj">The object to hash.</param>
        /// <returns>The hash code for the given key.</returns>
        public int GetHashCode(AssignmentKey obj)
        {
            return obj.Date.GetHashCode() +
                (obj.Queue != null ? obj.Queue.GetHashCode() : 0);
        }
    }
}
