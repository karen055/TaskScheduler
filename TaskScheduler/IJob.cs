using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskScheduler
{
    interface IJob
    {
        void Run();
    }

    class Job : IJob
    {
        public void Run()
        {
            Console.WriteLine(DateTime.Now);
        }
    }
}
