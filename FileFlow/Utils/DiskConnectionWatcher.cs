using System;
using System.Management;

namespace FileFlow.ViewModels
{
    public class DiskConnectionWatcher
    {
        private ManagementEventWatcher watcher;
        private Action callback;

#pragma warning disable CA1416 // Validate platform compatibility
        public DiskConnectionWatcher(Action callback)
        {
            WqlEventQuery query = new("SELECT * FROM __InstanceOperationEvent WITHIN 1 WHERE TargetInstance ISA 'Win32_LogicalDisk'");

            watcher = new ManagementEventWatcher(query);

            this.callback = callback;

            watcher.EventArrived += DiskConnectionEventArrived;
            watcher.Start();

        }

        ~DiskConnectionWatcher()
        {
            watcher.Stop();
            watcher.Dispose();
        }

        private void DiskConnectionEventArrived(object sender, EventArrivedEventArgs e)
        {
            // Обрабатываем событие подключения или отключения диска
            ManagementBaseObject targetInstance = (ManagementBaseObject)e.NewEvent["TargetInstance"];
            string driveName = (string)targetInstance["DeviceID"];
            string eventType = (string)e.NewEvent["__CLASS"];

            if (eventType.Equals("__InstanceCreationEvent") || eventType.Equals("__InstanceDeletionEvent"))
            {
                callback();
            }
        }
#pragma warning restore CA1416 // Validate platform compatibility
    }
}
