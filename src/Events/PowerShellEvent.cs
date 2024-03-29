﻿using System.Diagnostics.Eventing.Reader;

namespace EventLogMonitor.Events
{
    internal class PowerShellEvent : BaseEvent
    {
        private static readonly string msExecutedSignature = "Microsoft Corporation. All rights reserved.";

        public PowerShellEvent() { }

        public override bool IsSuspicious(EventRecordWrittenEventArgs e)
        {
            LogEntry? entry = LogEntry.CreateObj(e.EventRecord);

            if (entry == null)
            {
                return false;
            }

            if (ExecuteRemoteCommand(entry))
            {
                return true;
            }

            return false;
        }

        public static bool ExecuteRemoteCommand(LogEntry e)
        {
            int suspiciousEventID = 4104;

            if (e.EventID != suspiciousEventID)
            {
                return false;
            }

            if (!e.Message.Contains(msExecutedSignature))
            {
                if (e.Message.Contains("Creating Scriptblock text (1 of") && !e.Message.Contains(msExecutedSignature))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
