using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskScheduler
{
    class Program
    {
        static void Main(string[] args)
        {
            Trigger trigger = new Trigger(DateTime.Now.AddSeconds(-9),TimeSpan.FromSeconds(2),5);
            //Trigger trigger = new Trigger();
            //trigger.AddJob(new Job());
            //trigger.Start();
            Console.ReadKey();
        }
    }
}
