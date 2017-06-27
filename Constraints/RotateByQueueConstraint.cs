using System;
using System.Collections.Generic;
using System.Linq;

namespace OnCallScheduler.Rules
{
    /// <summary>
    /// The constraint that will rotate primary engineers as backup the next day.
    /// </summary>
    public class RotateByQueueConstraint : IConstraint
    {
        /// <summary>
        /// Stores the old schedule.
        /// </summary>
        private readonly Schedule _oldSchedule;

        /// <summary>
        /// Initializes a new instance of <see cref="RotateByQueueConstraint"/>.
        /// </summary>
        /// <param name="oldSchedule">The original schedule, used to fill the start of the schedule with.</param>
        public RotateByQueueConstraint(Schedule oldSchedule)
        {
            _oldSchedule = oldSchedule;
        }

        /// <summary>
        /// Constraint a generated schedule with an always applied rule.
        /// </summary>
        /// <param name="schedule">The schedule to constraint.</param>
        /// <returns>A new schedule with the constraint applied.</returns>
        public Schedule ConstraintSchedule(Schedule schedule)
        {
            Queue[] queueOrder = schedule.ScheduleConfig.ActiveQueues.ToArray();
            Queue primaryQueue = queueOrder[0];

            DateTime startDate = schedule[0].Key.Date;
            DateTime maxDate = schedule.Max(s => s.Key.Date);

            List<Assignment> newSchedule = new List<Assignment>();
            foreach (Assignment assignment in schedule.Where(s => s.Key.Queue == primaryQueue))
            {
                newSchedule.Add(assignment);
                DateTime currentDate = assignment.Key.Date;
                for (int i = 1; i < queueOrder.Length; i++)
                {
                    currentDate += TimeSpan.FromDays(1);
                    if (currentDate <= maxDate)
                    {
                        newSchedule.Add(new Assignment(new AssignmentKey(currentDate, queueOrder[i]), assignment.Engineer));
                    }
                }

                if (currentDate < startDate)
                {
                    startDate = currentDate;
                }
            }

            // Fill in the first few days
            for (int day = 1; day < queueOrder.Length; day++)
            {
                DateTime currentDate = startDate - TimeSpan.FromDays(day);

                // Get the primary assignment on that day, or a random engineer if not available.
                Assignment assignment = _oldSchedule
                    .Where(s => s.Key.Queue == primaryQueue && s.Key.Date == currentDate)
                    .FirstOrDefault();
                assignment = assignment ?? new Assignment(new AssignmentKey(currentDate, primaryQueue), schedule.ScheduleConfig.GetRandomEngineer());
                for (int i = 1; i < queueOrder.Length; i++)
                {
                    currentDate += TimeSpan.FromDays(1);
                    newSchedule.Add(new Assignment(new AssignmentKey(currentDate, queueOrder[i]), assignment.Engineer));
                }
            }

            return new Schedule(schedule.ScheduleConfig, newSchedule);
        }
    }
}
