using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KurguWebsite.Infrastructure.Services
{

    public interface IBackgroundJobService
    {
        void Schedule(Func<Task> job, TimeSpan delay);
        void ScheduleRecurring(Func<Task> job, TimeSpan interval);
        Task<string> EnqueueAsync(Func<Task> job);
    }

    public class BackgroundJobService : IBackgroundJobService
    {
        // In production, use Hangfire or similar
        public void Schedule(Func<Task> job, TimeSpan delay)
        {
            Task.Run(async () =>
            {
                await Task.Delay(delay);
                await job();
            });
        }

        public void ScheduleRecurring(Func<Task> job, TimeSpan interval)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(interval);
                    await job();
                }
            });
        }

        public Task<string> EnqueueAsync(Func<Task> job)
        {
            var jobId = Guid.NewGuid().ToString();
            Task.Run(job);
            return Task.FromResult(jobId);
        }
    }
}