using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace OnCallScheduler
{
    public class Scheduler
    {
        /// <summary>
        /// Stores the instance of random that will be used.
        /// </summary>
        private static readonly StrongRandom _random = new StrongRandom();

        /// <summary>
        /// Stores the configuration for the scheduler.
        /// </summary>
        private readonly SchedulerConfig _config;

        /// <summary>
        /// Stores the starting schedule.
        /// </summary>
        private readonly Schedule _startingSchedule;

        /// <summary>
        /// Stores the starting schedule as a dictionary for quicker lookup.
        /// </summary>
        private readonly Dictionary<AssignmentKey, Assignment> _startingScheduleDictionary;

        /// <summary>
        /// Initialize a new instance of <see cref="Schedule"/>.
        /// </summary>
        /// <param name="config">The configuration for the schedule.</param>
        /// <param name="startingSchedule">The starting state of the schedule.</param>
        public Scheduler(SchedulerConfig config, Schedule startingSchedule)
        {
            _config = config;
            _startingSchedule = startingSchedule;
            _startingScheduleDictionary = _startingSchedule.ToDictionary(a => a.Key, AssignmentKeyComparer.Instance);
        }

        /// <summary>
        /// Runs the scheduling algorithm.
        /// </summary>
        public Schedule Run(int generations, Schedule lastBest, CancellationToken token)
        {
            int maxMutationSize = 45;

            // 1. randomly initialize population(t)
            Schedule[] population = new Schedule[_config.Population];
            for (int i = 0; i < population.Length; i++)
            {
                population[i] = _config.ConstraintSchedule(
                    Schedule.MakeRandom(
                        _config.ScheduleConfig,
                        _config.StartDate,
                        _config.EndDate));
            }

            if (lastBest != null)
            {
                population[0] = lastBest;
            }

            // 2. determine fitness of population(t)
            Tuple<int, double>[] populationFitness;
            populationFitness = AppendIndex(population)
                .AsParallel()
                .WithDegreeOfParallelism(10)
                .Select(s => new Tuple<int, double>(s.Item1, _config.ComputeFitness(s.Item2)))
                .OrderByDescending(p => p.Item2)
                .ToArray();

            for (int generation = 0; generation < generations; generation++)
            {
                Console.WriteLine("Gen={0}, Best={1}, Best2={2}, Worst={3}", generation, populationFitness[0].Item2, populationFitness[1].Item2, populationFitness.Last().Item2);
                if (token.IsCancellationRequested)
                {
                    break;
                }

                // Copy the elites
                for (int i = 0; i < _config.EliteSize; i++)
                {
                    population[i] = population[populationFitness[i].Item1];
                }

                // 3.1 select parents from population(t)
                // 3.2 perform crossover on parents creating population(t+1)
                Tuple<int, double>[] parentFitness = populationFitness;
                Schedule[] parents = (Schedule[])population.Clone();
                for (int i = _config.EliteSize; i < population.Length; i++)
                {
                    int parent1 = TournamentSelection(parentFitness);
                    int parent2 = TournamentSelection(parentFitness);

                    population[i] = _config.ConstraintSchedule(parents[parent1].Cross(parents[parent2]));
                }

                // 3.3 perform mutation of population(t+1)
                for (int i = _config.EliteSize; i < _config.Population; i++)
                {
                    population[i] = _config.ConstraintSchedule(population[i].Mutate(_random.Next(0, maxMutationSize)));
                }

                // 3.4. determine fitness of full population(t+1)
                populationFitness = AppendIndex(population)
                    .AsParallel()
                    .WithDegreeOfParallelism(10)
                    .Select(s => new Tuple<int, double>(s.Item1, _config.ComputeFitness(s.Item2)))
                    .OrderByDescending(s => s.Item2)
                    .ToArray();

                // Save the best chromosome
                lastBest = population[populationFitness[0].Item1];
            }

            return lastBest;
        }

        /// <summary>
        /// Select individuals for crossover.
        /// </summary>
        /// <param name="populationFitness">The fitness of the population to choose from.</param>
        /// <returns>The individual chosen.</returns>
        private int TournamentSelection(Tuple<int, double>[] populationFitness)
        {
            // Create a tournament population
            Tuple<int, double>[] tournamentFitness = new Tuple<int, double>[_config.TournamentSize];

            // For each place in the tournament get a random individual
            for (int i = 0; i < tournamentFitness.Length; i++)
            {
                int randomId = _random.Next(0, populationFitness.Length);
                tournamentFitness[i] = populationFitness[randomId];
            }

            // Get the fittest
            int fittestIdx = tournamentFitness
                .OrderByDescending(s => s.Item2)
                .First()
                .Item1;
            return fittestIdx;
        }

        /// <summary>
        /// Compute the fitness for the whole population.
        /// </summary>
        /// <param name="population">The population to compute.</param>
        /// <returns>An enumerable tuple of the chromosome index and its fitness.</returns>
        private IEnumerable<Tuple<int, Schedule>> AppendIndex(Schedule[] population)
        {
            for (int i = 0; i < population.Length; i++)
            {
                yield return new Tuple<int, Schedule>(i, population[i]);
            }
        }
    }
}
