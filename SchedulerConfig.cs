using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OnCallScheduler
{
    /// <summary>
    /// A class for holding all the configurations used by the scheduler.
    /// </summary>
    public class SchedulerConfig : IRule, IConstraint
    {
        /// <summary>
        /// The configuration for the schedule.
        /// </summary>
        private readonly ScheduleConfig _scheduleConfig;

        /// <summary>
        /// The list of global rules.
        /// </summary>
        private readonly IList<IRule> _globalRules;

        /// <summary>
        /// The list of global constraints.
        /// </summary>
        private readonly IList<IConstraint> _globalConstraints;

        /// <summary>
        /// The list of rules per engineer.
        /// </summary>
        private readonly Dictionary<Engineer, IList<IRule>> _engineerRules;

        /// <summary>
        /// When to start the schedule.
        /// </summary>
        private readonly DateTime _startDate;

        /// <summary>
        /// When to end the schedule.
        /// </summary>
        private readonly DateTime _endDate;

        /// <summary>
        /// The population of the pool.
        /// </summary>
        private readonly int _population;

        /// <summary>
        /// The tournament selection size
        /// </summary>
        private readonly int _tournamentSize;

        /// <summary>
        /// The size of the elites that will be brought forward each evolution
        /// </summary>
        private readonly int _eliteSize;

        /// <summary>
        /// Initializes a new instance of <see cref="SchedulerConfig"/>.
        /// </summary>
        /// <param name="scheduleConfig">The configuration for the schedule.</param>
        public SchedulerConfig(
            ScheduleConfig scheduleConfig,
            DateTime startDate,
            DateTime endDate,
            int population)
        {
            _scheduleConfig = scheduleConfig;
            _globalRules = new List<IRule>();
            _globalConstraints = new List<IConstraint>();
            _engineerRules = new Dictionary<Engineer, IList<IRule>>();
            _startDate = startDate.NormalizeDate();
            _endDate = endDate.NormalizeDate();
            _population = population;
            _tournamentSize = 5;
            _eliteSize = (int)(population * 0.05);
        }

        /// <summary>
        /// Gets the configuration for the schedule.
        /// </summary>
        public ScheduleConfig ScheduleConfig
        {
            get
            {
                return _scheduleConfig;
            }
        }

        /// <summary>
        /// Gets the date on which the scheduler should start scheduling.
        /// </summary>
        public DateTime StartDate
        {
            get
            {
                return _startDate;
            }
        }

        /// <summary>
        /// Gets the date on which the scheduler should end scheduling.
        /// </summary>
        public DateTime EndDate
        {
            get
            {
                return _endDate;
            }
        }

        /// <summary>
        /// Gets the population of the pool.
        /// </summary>
        public int Population
        {
            get
            {
                return _population;
            }
        }

        /// <summary>
        /// Gets the tournament population size.
        /// </summary>
        public int TournamentSize
        {
            get
            {
                return _tournamentSize;
            }
        }

        /// <summary>
        /// Gets size of the elites that will be brought forward each evolution.
        /// </summary>
        public int EliteSize
        {
            get
            {
                return _eliteSize;
            }
        }

        /// <summary>
        /// Add a rule that will be applied for all engineers.
        /// </summary>
        /// <param name="rule">The rule to add.</param>
        public void AddGlobalRule(IRule rule)
        {
            _globalRules.Add(rule);
        }

        /// <summary>
        /// Add a rule that will be applied for all engineers.
        /// </summary>
        /// <param name="rule">The rule to add.</param>
        public void AddGlobalConstraint(IConstraint constraint)
        {
            _globalConstraints.Add(constraint);
        }

        /// <summary>
        /// Constraint a generated schedule with an always applied rule.
        /// </summary>
        /// <param name="schedule">The schedule to constraint.</param>
        /// <returns>A new schedule with the constraint applied.</returns>
        public Schedule ConstraintSchedule(Schedule schedule)
        {
            foreach (IConstraint constraint in _globalConstraints)
            {
                schedule = constraint.ConstraintSchedule(schedule);
            }

            return schedule;
        }

        /// <summary>
        /// Computes the fitness of the given <paramref name="schedule"/>.
        /// </summary>
        /// <param name="schedule">The schedule to compute.</param>
        /// <returns>A fitness score of the schedule.</returns>
        public double ComputeFitness(Schedule schedule)
        {
            double fitness = 0;

            foreach (IRule rule in _globalRules)
            {
                fitness += rule.ComputeFitness(schedule);
            }

            return fitness;
        }

        /// <summary>
        /// Add a rule that will be applied for a specific engineer.
        /// </summary>
        /// <param name="rule">The rule to add.</param>
        /// <param name="engineer">The engineer where the rule will be applied.</param>
        public void AddRuleForEngineer(IRule rule, Engineer engineer)
        {
            IList<IRule> rules;
            if (!_engineerRules.TryGetValue(engineer, out rules))
            {
                rules = new List<IRule>();
                _engineerRules.Add(engineer, rules);
            }

            rules.Add(rule);
        }

        /// <summary>
        /// Get all rules that should be applied for an engineer.
        /// </summary>
        /// <param name="engineer">The engineer for which to retrieve the rules.</param>
        /// <returns>All the rules that should be applied for the given engineer.</returns>
        public IEnumerable<IRule> GetRulesForEngineer(Engineer engineer)
        {
            // Return all the global rules
            IEnumerable<IRule> results = _globalRules;

            // Combine with per engineer rules
            IList<IRule> rules;
            if (_engineerRules.TryGetValue(engineer, out rules))
            {
                results = results.Concat(rules);
            }

            return results;
        }

        /// <summary>
        /// Dump the statistics for the given schedule.
        /// </summary>
        /// <param name="schedule">The schedule to evaluate.</param>
        /// <param name="writer">The output for the statistics.</param>
        public void DumpStats(Schedule schedule, TextWriter writer)
        {
            foreach (IRule rule in _globalRules)
            {
                rule.DumpStats(schedule, writer);
            }
        }
    }
}
