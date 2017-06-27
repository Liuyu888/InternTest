using System;
using System.Collections.Generic;
using System.Linq;

namespace OnCallScheduler.Rules
{
    /// <summary>
    /// The constraint that gives manual override control for specific days.
    /// </summary>
    public class ManualOverrideConstraint : IConstraint
    {
        private readonly List<Assignment> _assignmentOverrides;

        private readonly HashSet<DateTime> _overrideDays;

        /// <summary>
        /// Initializes a new instance of <see cref="ManualOverrideConstraint"/>.
        /// </summary>
        /// <param name="config">The schedule's config.</param>
        public ManualOverrideConstraint(ScheduleConfig config)
        {
            _assignmentOverrides = new List<Assignment>();
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("2/13/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("sakittus")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("2/13/2016"), config.GetQueueFromName("Backup")), config.GetEngineerFromAlias("salonis")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("2/14/2016"), config.GetQueueFromName("Backup")), config.GetEngineerFromAlias("sakittus")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("2/14/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("salonis")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("2/15/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("sakittus")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("2/15/2016"), config.GetQueueFromName("Backup")), config.GetEngineerFromAlias("salonis")));

            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("3/1/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("minetok")));

            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("5/27/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("pratraw")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("5/28/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("adamkr")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("5/28/2016"), config.GetQueueFromName("Backup")), config.GetEngineerFromAlias("pratraw")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("5/29/2016"), config.GetQueueFromName("Backup")), config.GetEngineerFromAlias("adamkr")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("5/29/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("pratraw")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("5/30/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("adamkr")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("5/30/2016"), config.GetQueueFromName("Backup")), config.GetEngineerFromAlias("pratraw")));

            // Morale Event
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("6/23/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("wayneb")));

            // 4th of July Holiday
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("7/2/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("sero")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("7/3/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("seliu")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("7/4/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("sero")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("7/2/2016"), config.GetQueueFromName("Backup")), config.GetEngineerFromAlias("seliu")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("7/3/2016"), config.GetQueueFromName("Backup")), config.GetEngineerFromAlias("sero")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("7/4/2016"), config.GetQueueFromName("Backup")), config.GetEngineerFromAlias("seliu")));

            // Garima's Start Date
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("7/5/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("gaagra")));

            // Yogesh's Start Date
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("7/6/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("yogkul")));

            // Labor Day Holiday
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("9/3/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("alchin")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("9/3/2016"), config.GetQueueFromName("Backup")), config.GetEngineerFromAlias("mprimke")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("9/4/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("mprimke")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("9/4/2016"), config.GetQueueFromName("Backup")), config.GetEngineerFromAlias("alchin")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("9/5/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("alchin")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("9/5/2016"), config.GetQueueFromName("Backup")), config.GetEngineerFromAlias("mprimke")));

            // Wayne's last day on the queue
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("10/2/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("wayneb")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("10/3/2016"), config.GetQueueFromName("Backup")), config.GetEngineerFromAlias("wayneb")));

            // Vaishali's Start Date
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("10/3/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("vanark")));

            // Thanksgiving 2016 (TBD)
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("11/24/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("vanark")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("11/24/2016"), config.GetQueueFromName("Backup")), config.GetEngineerFromAlias("olignat")));

            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("11/25/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("olignat")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("11/26/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("vanark")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("11/27/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("olignat")));

            // Start date for new folks
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("12/2/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("ayeltsov")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("12/4/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("jaredmoo")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("12/6/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("nafan")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("12/8/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("apurvs")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("12/10/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("franktsa")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("12/12/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("dimadhus")));

            // Christmas 2016
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("12/23/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("pratraw")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("12/23/2016"), config.GetQueueFromName("Backup")), config.GetEngineerFromAlias("bcham")));

            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("12/24/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("bcham")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("12/25/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("pratraw")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("12/26/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("bcham")));

            // New Year 2017
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("12/30/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("yogkul")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("12/31/2016"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("gaagra")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("1/1/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("yogkul")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("1/2/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("gaagra")));

            // Martin Luther King 2017
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("1/13/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("pratraw")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("1/14/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("sumesh")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("1/15/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("pratraw")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("1/16/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("sumesh")));

            // President's day 2017
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("2/17/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("sero")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("2/18/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("yogkul")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("2/19/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("sero")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("2/20/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("yogkul")));

            // Temp - Adjusting the new schedule generation
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("2/27/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("franktsa")));

            // Start date for new folks
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("2/28/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("jacky")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("3/2/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("ansinh")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("3/6/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("shatri")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("3/8/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("wezen")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("3/10/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("payi")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("3/13/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("anugar")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("3/15/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("mykolian")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("3/17/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("xliang")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("3/20/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("zhson")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("3/22/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("hshih")));

            // Memorial day 2017
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("5/26/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("apurvs")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("5/27/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("jacky")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("5/28/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("apurvs")));
            _assignmentOverrides.Add(new Assignment(new AssignmentKey(DateTime.Parse("5/29/2017"), config.GetQueueFromName("Primary")), config.GetEngineerFromAlias("jacky")));


            _overrideDays = new HashSet<DateTime>(_assignmentOverrides.Select(a => a.Key.Date).Distinct());
        }

        /// <summary>
        /// Constraint a generated schedule with an always applied rule.
        /// </summary>
        /// <param name="schedule">The schedule to constraint.</param>
        /// <returns>A new schedule with the constraint applied.</returns>
        public Schedule ConstraintSchedule(Schedule schedule)
        {
            List<Assignment> newSchedule = new List<Assignment>();
            foreach (Assignment assignment in schedule)
            {
                if (_overrideDays.Contains(assignment.Key.Date))
                {
                    Assignment overrideAssignment = _assignmentOverrides.SingleOrDefault(a => a.Key.Equals(assignment.Key));
                    if (overrideAssignment != null)
                    {
                        newSchedule.Add(overrideAssignment);
                        continue;
                    }
                }

                newSchedule.Add(assignment);
            }

            return new Schedule(schedule.ScheduleConfig, newSchedule);
        }
    }
}
