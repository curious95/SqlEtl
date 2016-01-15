using System;
using System.Collections.Generic;

namespace SqlEtl.Entities
{
    public class BulkCopyResponse
    {
        public BulkCopyResponse(DateTime startTime, DateTime endTime, uint changesTotal = 0, uint changesApplied = 0,
            uint changesFailed = 0, Dictionary<string, Dictionary<string, string>> status = null)
        {
            ChangesTotal = changesTotal;
            ChangesFailed = changesFailed;
            ChangesApplied = changesApplied;
            StartTime = startTime;
            EndTime = endTime;
            Status = status;
        }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; internal set; }
        public Dictionary<string, Dictionary<string, string>> Status { get; internal set; }
        public uint ChangesTotal { get; private set; }
        public uint ChangesFailed { get; private set; }
        public uint ChangesApplied { get; private set; }
    }
}