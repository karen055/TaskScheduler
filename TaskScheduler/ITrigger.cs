using System;

namespace TaskScheduler
{
    interface ITrigger
    {
        string Id { get; set; }
        DateTimeOffset StartTime { get; set; }
        DateTimeOffset ExpireTime { get; set; }
        TimeSpan RepeatInterval { get; set; }
        int RepeatCount { get; set; }
        bool IsRepeating { get; set; }
        bool Enabled { get; set; }
        bool Exclusive { get; set; }
    }
}
