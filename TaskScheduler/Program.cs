using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            ITrigger trigger1 = new Trigger(DateTime.Now.AddSeconds(1), TimeSpan.FromSeconds(1),5){Exclusive = false};
            ITrigger trigger2 = new Trigger(DateTime.Now.AddSeconds(6));
            Job job1 = new Job{Concurrent = true};
            Job job2 = new Job();
            SchedulerTask schedulerTask = new SchedulerTask();
            //schedulerTask.AddJob(job1);
            schedulerTask.AddJob(job2);
            schedulerTask.AddTrigger(trigger2).AddJob(job1).AddJob(job1).Start();
            //schedulerTask.AddTrigger(trigger1,() => { Console.WriteLine(Guid.NewGuid()); /*Thread.Sleep(3000);*/ Console.WriteLine(Thread.CurrentThread.ManagedThreadId); });
            schedulerTask.AddTrigger(trigger1).Start();
            //schedulerTask.AddTrigger(trigger2);
            //schedulerTask.Start();
            Console.ReadKey();

        }
    }
}
