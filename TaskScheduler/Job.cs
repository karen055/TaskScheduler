using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskScheduler
{
    class Job
    {
        public virtual void Execute()
        {
            Console.WriteLine(DateTime.Now);
        }
    }
}
