using System;

namespace TaskScheduler
{
    interface ITask
    {
        string Id { get; set; }
        string Name { get; set; }
        bool Enabled { get; set; }
        bool IsRunning { get; set; }
        bool RunIfMissed { get; set; }
        bool AllowConcurrent { get; set; }

        ITask AddTrigger(ITrigger trigger);
        ITask AddAction(Action action);
        ITask AddJob(IJob job);
        void Start();
        void Stop();
        
    }

    interface ITask<out T> : ITask
    {
        void AddAction(Action<T> action);
    }
}
