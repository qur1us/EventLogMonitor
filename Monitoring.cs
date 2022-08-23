﻿using System;
using System.Windows.Forms;
using System.Diagnostics.Eventing.Reader;

namespace EventLogMonitor
{
    internal class Monitoring
    {
        NotifyIcon trayIcon;
        string popUpMessage = String.Empty;
        long? lastRecordID = 0;

        public EventLogWatcher? Watcher { get; set; }

        private string PowerShellLogName = "Microsoft-Windows-PowerShell/Operational";

        private string msExecutedSignature = "Microsoft Corporation. All rights reserved.";

        public Monitoring(NotifyIcon trayIcon)
        {
            this.trayIcon = trayIcon;
            MonitorPowershell();
        }

        public void MonitorPowershell()
        {
            EventLogQuery query = new EventLogQuery(PowerShellLogName, PathType.LogName);
            Watcher = new EventLogWatcher(query);

            Watcher.EventRecordWritten += new EventHandler<EventRecordWrittenEventArgs>(PowerShellEventLogUpdateHandler);
            Watcher.Enabled = true;
        }

        private void PowerShellEventLogUpdateHandler(object? sender, EventRecordWrittenEventArgs e)
        {
            int suspiciousEventID = 4104;

            if (e.EventRecord.Id != suspiciousEventID)
            {
                return;
            }

            LogEntry? entry = LogEntry.CreateObj(e.EventRecord);

            if (entry == null)
            {
                return;
            }

            lastRecordID = entry.RecordID;

            if (!entry.Message.Contains(msExecutedSignature))
            {
                if (entry.Message.Contains("Creating Scriptblock text (1 of") && !entry.Message.Contains(msExecutedSignature))
                {
                    popUpMessage = entry.Message;
                    SuspiciousActivityDetected();
                }
            }
        }

        private void SuspiciousActivityDetected()
        {
            trayIcon.ShowBalloonTip(3000, "Suspicious behaviour detected", "Click here to show more information", ToolTipIcon.Warning);
            trayIcon.BalloonTipClicked += new EventHandler(PopMessageBox);
        }

        private void PopMessageBox(object? sender, EventArgs e)
        {
            MessageBox.Show(popUpMessage);
            trayIcon.BalloonTipClicked -= PopMessageBox;
        }
    }
}
