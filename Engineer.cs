using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OnCallScheduler
{
    /// <summary>
    /// Represents an engineer that will participate in a schedule.
    /// </summary>
    public class Engineer : IComparable
    {
        /// <summary>
        /// Stores the alias of the engineer.
        /// </summary>
        private readonly string _alias;

        /// <summary>
        /// Stores the ICM name of the engineer.
        /// </summary>
        private readonly string _icmName;

        /// <summary>
        /// Stores the display name of the engineer.
        /// </summary>
        private readonly string _displayName;

        /// <summary>
        /// Stores the date on which the engineer joined the queue.
        /// </summary>
        private readonly DateTime? _startDate;

        /// <summary>
        /// Indicates whether engineer can serve as an Incident Manager for the queue
        /// </summary>
        /// <remarks>
        /// Incident manager role is used for scheduling optimization to try to put at least one of them per rotation
        /// </remarks>
        private bool _isIncidentManager;

        /// <summary>
        /// Initializes a new instance of <see cref="Engineer"/>.
        /// </summary>
        /// <param name="alias">The alias for the engineer.</param>
        /// <param name="icmName">The ICM name of the engineer.</param>
        /// <param name="displayName">The display name of the engineer.</param>
        /// <param name="startDate">The date on which the engineer joined the queue.</param>
        /// <param name="isIncidentManager">Indicates whether engineer is an incident manager</param>
        internal Engineer(string alias, string icmName, string displayName, DateTime? startDate, bool isIncidentManager)
        {
            _alias = alias;
            _icmName = icmName;
            _displayName = displayName;
            _startDate = startDate;
            _isIncidentManager = isIncidentManager;
        }

        /// <summary>
        /// Gets the alias for the engineer.
        /// </summary>
        public string Alias
        {
            get
            {
                return _alias;
            }
        }

        /// <summary>
        /// Gets the ICM name of the engineer.
        /// </summary>
        public string IcmName
        {
            get
            {
                return _icmName;
            }
        }

        /// <summary>
        /// Gets the display name of the engineer.
        /// </summary>
        public string DisplayName
        {
            get
            {
                return _displayName;
            }
        }

        /// <summary>
        /// Gets the date on which the engineer joined the queue.
        /// </summary>
        public DateTime? StartDate
        {
            get
            {
                return _startDate;
            }
        }

        /// <summary>
        /// Gets the flag indicating whether engineer is an Incident Manager for the queue
        /// </summary>
        public bool IsIncidentManager
        {
            get
            {
                return _isIncidentManager;
            }
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            Engineer other = obj as Engineer;
            return Equals(other);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other">The other engineer to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public bool Equals(Engineer other)
        {
            if (other == null)
            {
                return false;
            }

            return Alias.Equals(other.Alias);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Alias.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified objects are equal.
        /// </summary>
        /// <param name="a">First object.</param>
        /// <param name="b">Second object.</param>
        /// <returns><c>true</c> if the specified objects are equal; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Engineer a, Engineer b)
        {
            // If both are null, or both are same instance, return true.
            if (object.ReferenceEquals(a, b))
            {
                return true;
            }

            // If one is null, but not both, return false.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the fields match:
            return a.Alias.Equals(b.Alias);
        }

        /// <summary>
        /// Determines whether the specified objects are not equal.
        /// </summary>
        /// <param name="a">First object.</param>
        /// <param name="b">Second object.</param>
        /// <returns><c>false</c> if the specified objects are equal; otherwise, <c>true</c>.</returns>
        public static bool operator !=(Engineer a, Engineer b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Load a list of engineers from a csv file.
        /// </summary>
        /// <param name="fileName">The name of the file to load.</param>
        /// <returns>An enumerable list of engineers from that file.</returns>
        public static IEnumerable<Engineer> Load(string fileName)
        {
            return File.ReadLines(fileName)
                .Select(l => Engineer.Parse(l));
        }

        /// <summary>
        /// Parses a line from the engineer definition csv file.
        /// </summary>
        /// <param name="line">The line to parse.</param>
        /// <returns>An instance of <see cref="Engineer"/> with the information in the given <paramref name="line"/>.</returns>
        private static Engineer Parse(string line)
        {
            string[] tokens = line.Split(new char[] { ',' },  5);

            DateTime? startDate = null;
            if (tokens.Length > 3 && !string.IsNullOrWhiteSpace(tokens[3]))
            {
                startDate = (DateTime?)DateTime.Parse(tokens[3]);
            }

            bool isIncidentManager = false;
            if (tokens.Length > 4 && !string.IsNullOrWhiteSpace(tokens[4]))
            {
                isIncidentManager = bool.Parse(tokens[4]);
            }

            return new Engineer(tokens[0], tokens[1], tokens[2], startDate, isIncidentManager);
        }

        /// <summary>
        /// Formats the given engineer to a csv line.
        /// </summary>
        /// <returns>The csv representation for this engineer.</returns>
        public override string ToString()
        {
            return string.Format("{0},{1},{2}", _alias, _icmName, _displayName);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer
        /// that indicates whether the current instance precedes, follows, or occurs in the same position
        /// in the sort order as the other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A value that indicates the relative order of the objects being compared.</returns>
        public int CompareTo(object obj)
        {
            Engineer other = obj as Engineer;
            if (other == null)
            {
                return -1;
            }

            return _alias.CompareTo(other._alias);
        }
    }
}
