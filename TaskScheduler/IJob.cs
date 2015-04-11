using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskScheduler
{
    interface IJob
    {
        string Id { get; set; }
        bool Concurrent { get; set; }
        void Run();
    }

    class Job : IJob
    {
        public string Id { get; set; }

        public bool Concurrent { get; set; }

        public void Run()
        {
            //Console.WriteLine(DateTime.Now);
            //Thread.Sleep(2000);
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            Console.WriteLine(GetHashCode());
        }
    }
}
