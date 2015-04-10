using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace TaskScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            /*ITrigger trigger1 = new Trigger(DateTime.Now.AddSeconds(1), TimeSpan.FromSeconds(2));
            //Trigger trigger2 = new Trigger(false);
            Job job1 = new Job();

            SchedulerTask schedulerTask = new SchedulerTask();
            schedulerTask.AddJob(job1);
            schedulerTask.AddAction(()=>Console.WriteLine(Guid.NewGuid()));
            schedulerTask.AddTrigger(trigger1);
            //schedulerTask.AddTrigger(trigger2);
            schedulerTask.Start();
            Console.ReadKey();*/

            MulticastDelegate obj = new Action<int>(Console.WriteLine);
            Action action = obj as Action;
            Action<int> actionInt = obj as Action<int>;
            if (action!= null)
            {
                action();
            }

            if (actionInt != null) actionInt(8);
            Console.ReadKey();
        }
    }
}
