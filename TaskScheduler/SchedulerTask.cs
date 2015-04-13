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
        private readonly ISet<IJob> _commonJobs;
        private readonly ISet<Action> _commonActions;
        private readonly IDictionary<ITrigger, Timer> _timers;
        private readonly IDictionary<ITrigger, IList<IJob>> _exclusiveJobs;
        private readonly IDictionary<ITrigger, IList<Action>> _exclusiveActions;
        public IList<IJob> Jobs
        {
            get { return _exclusiveJobs.Values.SelectMany(jobs => jobs).Concat(_commonJobs).ToList(); }
        }

        public IList<Action> Actions
        {
            get { return _exclusiveActions.Values.SelectMany(actions => actions).Concat(_commonActions).ToList(); }
        }

        public SchedulerTask()
        {
            _exclusiveJobs = new Dictionary<ITrigger, IList<IJob>>();
            _exclusiveActions = new Dictionary<ITrigger, IList<Action>>();
            _triggers = new HashSet<ITrigger>();
            _commonJobs = new HashSet<IJob>();
            _timers = new Dictionary<ITrigger, Timer>();
            _commonActions = new HashSet<Action>();
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public bool Enabled { get; set; }

        public bool IsRunning { get; set; }

        public bool RunIfMissed { get; set; }

        public bool AllowConcurrent { get; set; }

        public ITask AddTrigger(ITrigger trigger)
        {
            _triggers.Add(trigger);
            return this;
        }
        public ITask AddTrigger(ITrigger trigger, IJob job)
        {
            _triggers.Add(trigger);
            if (_exclusiveJobs.ContainsKey(trigger))
                _exclusiveJobs[trigger].Add(job);
            else
                _exclusiveJobs.Add(trigger, new[] { job });
            return this;
        }
        public ITask AddTrigger(ITrigger trigger, Action action)
        {
            _triggers.Add(trigger);
            if (_exclusiveActions.ContainsKey(trigger))
                _exclusiveActions[trigger].Add(action);
            else
                _exclusiveActions.Add(trigger, new[] { action });
            return this;
        }
        public ITask AddAction(Action action)
        {
            _commonActions.Add(action);
            return this;
        }

        public ITask AddJob(IJob job)
        {
            _commonJobs.Add(job);
            return this;
        }

        public void Stop()
        {
            foreach (ITrigger trigger in _triggers)
            {
                _timers[trigger].Change(Timeout.Infinite, Timeout.Infinite);
            }
            IsRunning = false;
        }

        public void Start()
        {
            if (IsRunning) return;
            foreach (ITrigger trigger in _triggers)
            {
                DateTimeOffset dateTimeNow = DateTimeOffset.Now;
                TimeSpan dueTime;
                if (trigger.ToStartNow)
                {
                    dueTime = TimeSpan.FromMilliseconds(0);
                    trigger.StartTime = dateTimeNow;
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
                Timer timer = new Timer(obj => Callback(triggerLocal, GetJobs(triggerLocal)));
                _timers.Add(trigger, timer);
                timer.Change(dueTime, trigger.RepeatInterval);
            }
            IsRunning = true;
        }

        private void Callback(ITrigger trigger, IEnumerable<IJob> jobs)
        {
            if (jobs != null)
            {
                foreach (var job in jobs)
                {
                    if (job.Concurrent)
                        Task.Run(new Action(job.Run));
                    else
                        job.Run();
                }
            }

            if (trigger.RepeatCount != -1)
            {
                if (trigger.IsRepeating)
                    trigger.RepeatCount--;
                if (trigger.RepeatCount == 0)
                    _timers[trigger].Change(Timeout.Infinite, Timeout.Infinite);
                if (!trigger.IsRepeating) trigger.IsRepeating = true;
            }
        }

        private IEnumerable<IJob> GetJobs(ITrigger trigger)
        {
            if (trigger.Exclusive)
            {
                if (_exclusiveJobs.ContainsKey(trigger))
                    return _exclusiveJobs[trigger];
                return null;
            }
            if (_exclusiveJobs.ContainsKey(trigger))
                return _exclusiveJobs[trigger].Concat(_commonJobs);
            return _commonJobs;
        }
    }
}
