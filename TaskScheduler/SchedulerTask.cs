using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskScheduler
{
    class SchedulerTask : ITask
    {
        private readonly ISet<ITrigger> _triggers;
        private readonly ISet<IJob> _jobs;
        private readonly IList<Action> _actions;
        private readonly IDictionary<ITrigger, Timer> _timers;
        private readonly IDictionary<ITrigger, List<object>> _triggerJobs;

        public SchedulerTask()
        {
            _triggerJobs = new Dictionary<ITrigger, List<object>>();
            _triggers = new HashSet<ITrigger>();
            _jobs = new HashSet<IJob>();
            _timers = new Dictionary<ITrigger, Timer>();
            _actions = new List<Action>();
        }

        public string Id
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool Enabled
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool RunIfMissed
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool AllowConcurrent
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public SchedulerTask AddTrigger(ITrigger trigger)
        {
            _triggers.Add(trigger);
            return this;
        }
        public void AddTrigger(ITrigger trigger, IJob job)
        {
            _triggers.Add(trigger);
            _jobs.Add(job);
            if (_triggerJobs.ContainsKey(trigger))
                _triggerJobs[trigger].Add(job);
            else
                _triggerJobs.Add(trigger, new List<object> { job });
        }
        public void AddTrigger(ITrigger trigger, Action action)
        {
            _triggers.Add(trigger);
            _actions.Add(action);
            if (_triggerJobs.ContainsKey(trigger))
                _triggerJobs[trigger].Add(action);
            else
                _triggerJobs.Add(trigger, new List<object> { action });
        }
        public void AddAction(Action action)
        {
            _actions.Add(action);
        }

        public SchedulerTask AddJob(IJob job)
        {
            _jobs.Add(job);
            return this;
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            foreach (ITrigger trigger in _triggers)
            {
                bool startNow = false;
                DateTimeOffset dateTimeNow = DateTimeOffset.Now;
                TimeSpan dueTime;
                if (startNow)
                {
                    dueTime = TimeSpan.FromMilliseconds(0);
                }
                else if (trigger.StartTime < dateTimeNow)
                {
                    if (trigger.RepeatInterval == Timeout.InfiniteTimeSpan)
                        dueTime = Timeout.InfiniteTimeSpan;
                    else
                    {
                        long timePassedTicks = dateTimeNow.Ticks - trigger.StartTime.Ticks;
                        dueTime = TimeSpan.FromTicks(trigger.RepeatInterval.Ticks - timePassedTicks % trigger.RepeatInterval.Ticks);
                        if (trigger.RepeatCount > 0)
                        {
                            double fullTimeTicks = trigger.RepeatInterval.Ticks * trigger.RepeatCount;
                            if (fullTimeTicks > timePassedTicks)
                            {
                                long repeatsPassed = timePassedTicks / trigger.RepeatInterval.Ticks;
                                Console.WriteLine(repeatsPassed);
                                trigger.RepeatCount -= (int)repeatsPassed;
                                trigger.IsRepeating = true;
                            }
                            else
                            {
                                dueTime = Timeout.InfiniteTimeSpan;
                            }
                        }
                    }
                }
                else
                {
                    dueTime = trigger.StartTime - dateTimeNow;
                }
                ITrigger triggerLocal = trigger;
                Timer timer = new Timer(obj => Callback(triggerLocal, obj as Timer));
                timer.Change(dueTime, trigger.RepeatInterval);
                _timers.Add(trigger, timer);
            }
        }

        private void Callback(ITrigger trigger, Timer timer)
        {
            List<object> jobs;
            if (_triggerJobs.ContainsKey(trigger))
            {
                if (trigger.Exclusive)
                    jobs = _triggerJobs[trigger];
                else
                {
                    jobs = _jobs.Concat(_triggerJobs[trigger].ToList()).ToList();
                }
            }
            else jobs = _jobs.ToList<object>();

            foreach (var job in jobs)
            {
                Console.WriteLine(job.GetType());
                if (job is IJob)
                {
                    IJob j = (IJob)job;
                    if (j.Concurrent)
                        Task.Run(new Action(j.Run));
                    else
                        ((IJob)job).Run();
                }
                if (job is Action)
                {
                    ((Action)job)();
                }

                //Task.Run(() => job.Run());
            }

            /*foreach (Action action in _actions)
            {
                action();
            }*/
            //Parallel.ForEach(_actions, action => action());
            if (trigger.RepeatCount != -1)
            {
                if (trigger.IsRepeating)
                    trigger.RepeatCount--;
                if (trigger.RepeatCount == 0)
                    timer.Change(Timeout.Infinite, Timeout.Infinite);
                if (!trigger.IsRepeating) trigger.IsRepeating = true;
            }
        }
    }
}
