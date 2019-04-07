using Hangfire;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HangfireExample.Pages
{
    public class IndexModel : PageModel
    {
        public static List<JobHistory> jobs = new List<JobHistory>();

        public void OnGet()
        {
            if (!string.IsNullOrEmpty(Request.Query["option"]))
            {
                var job = new JobHistory();
                job.Id = Guid.NewGuid();
                job.ScheduledAt = DateTime.Now;
                job.Name = Request.Query["option"];
                jobs.Add(job);

                switch (Request.Query["option"])
                {
                    case "fire-forget":
                        BackgroundJob.Enqueue(() => RunJob(job.Id));
                        break;
                    case "delayed":
                        BackgroundJob.Schedule(() => RunJob(job.Id), TimeSpan.FromSeconds(30));
                        break;
                    case "recurring":
                        RecurringJob.AddOrUpdate(() => RunJob(job.Id), "* * * * *");
                        break;
                    default: 
                        // not found
                        jobs.Remove(job);
                        break;
                }
            }
        }

        public static void RunJob(Guid id)
        {
            var job = jobs.FirstOrDefault(p => p.Id == id);

            if (job != null)
            {
                job.RunAt = DateTime.Now;
                System.Diagnostics.Debug.WriteLine(job);
            }
        }
    }

    public class JobHistory
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime ScheduledAt { get; set; }
        public DateTime RunAt { get; set; }

        public override string ToString()
        {
            return string.Format("{0} | {1}: [{2} - {3}]", 
                this.ScheduledAt.ToString("yyy-MM-dd HH:mm:ss"),
                this.RunAt.ToString("yyy-MM-dd HH:mm:ss"),
                this.Id,
                this.Name);
        }
    }
}
