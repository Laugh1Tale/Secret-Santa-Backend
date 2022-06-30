using Quartz;
using Quartz.Impl;

namespace SecretSanta_Backend.Jobs
{
    public class EventNotificationScheduler
    {
        public static async void Start()
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            await scheduler.Start();
            

            IJobDetail job = JobBuilder.Create<WorkOfEndOfEvent>().Build();

            ITrigger trigger = TriggerBuilder.Create() 
                .WithIdentity("trigger1", "group1")   
                .StartAt(DateBuilder.TodayAt(12, 0, 0))
                .WithSimpleSchedule(x => x
                    .WithIntervalInHours(12)
                    .RepeatForever())                  
                .Build();                             

            await scheduler.ScheduleJob(job, trigger);       
        }
    }
}