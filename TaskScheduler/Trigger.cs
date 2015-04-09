using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskScheduler
{
    class Trigger
    {
        private readonly Timer _timer;

        private List<Job> _jobs;
        private DateTimeOffset _startTime;
        private DateTimeOffset lastTime;

        public Trigger(DateTimeOffset startTime, TimeSpan interval, int repeatCount = -1)
        {
            _jobs = new List<Job>();
            _timer = new Timer(TimerCallback);
            _startTime = startTime;
            RepeatInterval = interval;
            RepeatCount = repeatCount;


            Start();
        }

        public Trigger(bool startNow = true)
            : this(DateTimeOffset.Now, Timeout.InfiniteTimeSpan)
        {
            Start(startNow);
        }
        public Trigger(TimeSpan interval, int repeatCount = -1, bool startNow = true)
            : this(DateTimeOffset.Now, Timeout.InfiniteTimeSpan)
        {
            Start(startNow);
        }



        public Trigger(DateTimeOffset startTime)
            : this(startTime, Timeout.InfiniteTimeSpan)
        {

        }

        public DateTimeOffset StartTime
        {
            get { return _startTime; }
            set
            {
                _startTime = value < DateTimeOffset.Now ? DateTimeOffset.Now : value;
            }
        }

        public TimeSpan RepeatInterval { get; set; }
        public int RepeatCount { get; set; }
        public DateTimeOffset ExpireTime { get; set; }

        public void AddJob(Job job)
        {
            _jobs.Add(job);
        }

        public void Start(bool startNow = false)
        {
            DateTimeOffset dateTimeNow = DateTimeOffset.Now;
            TimeSpan dueTime;
            if (startNow)
            {
                dueTime = TimeSpan.FromMilliseconds(0);
            }
            else if (_startTime < dateTimeNow)
            {
                if (RepeatInterval == Timeout.InfiniteTimeSpan)
                    dueTime = Timeout.InfiniteTimeSpan;
                else
                {
                    long timePassedTicks = dateTimeNow.Ticks - _startTime.Ticks;
                    dueTime = TimeSpan.FromTicks(RepeatInterval.Ticks - timePassedTicks % RepeatInterval.Ticks);
                    if (RepeatCount > 0)
                    {
                        double fullTimeTicks = RepeatInterval.Ticks * RepeatCount;
                        if (fullTimeTicks > timePassedTicks)
                        {
                            long repeatsPassed = timePassedTicks / RepeatInterval.Ticks;
                            Console.WriteLine(repeatsPassed);
                            RepeatCount -= (int)repeatsPassed;
                            _isRepeating = true;
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
                dueTime = _startTime - dateTimeNow;
            }

            _timer.Change(dueTime, RepeatInterval);
            lastTime = dateTimeNow;
        }

        public void Reschedule(DateTime startTime, TimeSpan interval, int repeatCount)
        {
            _timer.Change(startTime - DateTime.Now, interval);
        }

        void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private bool _isRepeating;
        private void TimerCallback(object obj)
        {
            foreach (Job job in _jobs)
            {
                //job.Execute();
            }
            if (RepeatCount != -1)
            {
                if(_isRepeating)
                RepeatCount--;
                if (RepeatCount == 0)
                    Stop();
                if (!_isRepeating) _isRepeating = true;
            }
            Console.WriteLine((DateTimeOffset.Now - lastTime).TotalSeconds + " Count: " + RepeatCount);
            lastTime = DateTimeOffset.Now;
        }


        void Dispose()
        {
            _timer.Dispose();
        }
    }
}
