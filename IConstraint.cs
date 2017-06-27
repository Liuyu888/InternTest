namespace OnCallScheduler
{
    /// <summary>
    /// The interface for a generic constraint.
    /// </summary>
    public interface IConstraint
    {
        /// <summary>
        /// Constraint a generated schedule with an always applied rule.
        /// </summary>
        /// <param name="schedule">The schedule to constraint.</param>
        /// <returns>A new schedule with the constraint applied.</returns>
        Schedule ConstraintSchedule(Schedule schedule);
    }
}
