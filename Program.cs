using OnCallScheduler.Rules;
using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace OnCallScheduler
{
    class Program
    {
        private static readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();

        static void Main(string[] args)
        {
            Console.CancelKeyPress += Console_CancelKeyPress;

            ScheduleConfig scheduleConfig = new ScheduleConfig(
                Engineer.Load(@"Schedules\OnCallEngineers.csv"),
                Queue.Load(@"Schedules\Queues.csv"),
                TimeSpan.FromHours(9));

            Schedule currentSchedule = Schedule.Load(scheduleConfig, @"Schedules\CurrentSchedule.csv");
            currentSchedule.Save(@"Schedules\CurrentSchedule2.csv");

            SchedulerConfig config = new SchedulerConfig(
                scheduleConfig,
                new DateTime(2017, 5, 31),
                new DateTime(2017, 8, 1).AddMonths(1).AddDays(-1), // Last day of month
                300);
            config.AddGlobalConstraint(new ManualOverrideConstraint(scheduleConfig));
            config.AddGlobalConstraint(new RotateByQueueConstraint(currentSchedule));
            config.AddGlobalRule(new CannotBeInMultipleQueueOnSameDayRule());
            config.AddGlobalRule(new VacationRule(config, @"Schedules\Vacations.csv"));
            //config.AddGlobalRule(new PaidHolidayRule(@"Schedules\PaidHolidays.csv", currentSchedule));
            config.AddGlobalRule(new EveryoneOncallSameNumberOfTimesRule(currentSchedule));
            config.AddGlobalRule(new ClusteringRule(config, currentSchedule));
            config.AddGlobalRule(new IncidentManagerRule());
            config.AddGlobalRule(new EveryoneParticipatesRule(scheduleConfig));

            Schedule best = null;

            // Load previous best if exists
            if (File.Exists("best.csv"))
            {
                best = Schedule.Load(scheduleConfig, @"best.csv");

                // Trim the best to match the current schedule
                best = new Schedule(scheduleConfig, best
                    .Where(a => a.Key.Date >= config.StartDate)
                    .Where(a => a.Key.Date < config.EndDate));

                // Stretch the schedule up to the configured end date
                best = Schedule.MakeRandom(scheduleConfig, config.StartDate, config.EndDate).Combine(best);

                config.DumpStats(best, Console.Out);
                using (FileStream stream = File.OpenWrite("stats.txt"))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    config.DumpStats(best, writer);
                }
            }

            while (!_cancellationSource.IsCancellationRequested)
            {
                Scheduler scheduler = new Scheduler(config, currentSchedule);
                best = scheduler.Run(4000, best, _cancellationSource.Token);
                best.Save("best.csv");

                config.DumpStats(best, Console.Out);
                using (FileStream stream = File.OpenWrite("stats.txt"))
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    config.DumpStats(best, writer);
                }
            }

            Console.ReadLine();
        }

        /// <summary>
        /// Process Ctrl+C from console, stop execution, save best schedule and dump stats.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The argu</param>
        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _cancellationSource.Cancel();
            e.Cancel = true;
        }
    }
}
