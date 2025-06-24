using System;
using System.Timers;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Oraton.Classes
{
    public class Reminder
    {
        private DateTime reminderTime;
        private Timer timer;

        public Reminder(DateTime time, string message)
        {
            reminderTime = time;
            timer = new Timer(1000);
            timer.Elapsed += (s, e) => CheckReminder(message);
            timer.Start();
        }

        private void CheckReminder(string message)
        {
            if (DateTime.Now >= reminderTime)
            {
                ShowNotification("Нагадування", message);
                timer.Stop();
            }
        }

        private void ShowNotification(string title, string message)
        {
            new ToastContentBuilder()
                .AddText(title)
                .AddText(message)
                .Show();
        }
    }
}
