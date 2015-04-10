using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskScheduler
{
    class SchedulerTask : ITask
    {
        private readonly IList<ITrigger> _triggers;
        private readonly IList<IJob> _jobs;
        private IList<Action> _actions;
        private IList<Timer> _timers;

        public SchedulerTask()
        {
            _triggers = new List<ITrigger>();
            _jobs = new List<IJob>();
            _timers = new List<Timer>();
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

        public void AddTrigger(ITrigger trigger)
        {
            _triggers.Add(trigger);
        }

        public void AddAction(Action action)
        {
            _actions.Add(action);
        }

        public void AddJob(IJob job)
        {
            _jobs.Add(job);
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
                Timer timer = new Timer(obj =>
                {
                    foreach (IJob job in _jobs)
                    {
                        job.Run();
                    }
                    foreach (Action action in _actions)
                    {
                        action();
                    }
                    if (triggerLocal.RepeatCount != -1)
                    {
                        if (triggerLocal.IsRepeating)
                            triggerLocal.RepeatCount--;
                        if (triggerLocal.RepeatCount == 0)
                            (obj as Timer).Change(Timeout.Infinite, Timeout.Infinite);
                        if (!triggerLocal.IsRepeating) triggerLocal.IsRepeating = true;
                    }
                });
                timer.Change(dueTime, trigger.RepeatInterval);
                _timers.Add(timer);
            }
        }

        /*private void TimerCallback(object obj)
        {
            foreach (Job job in _jobs)
            {
                job.Execute();
            }
            if (RepeatCount != -1)
            {
                if (_isRepeating)
                    RepeatCount--;
                if (RepeatCount == 0)
                    Stop();
                if (!_isRepeating) _isRepeating = true;
            }
        }*/
    }
}
