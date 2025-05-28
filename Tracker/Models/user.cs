using System;
using System.Collections.Generic;
using System.Text;

namespace TimeTracker.Models
{
    public class LoginResult
    {
        public string status { get; set; }
        public string token { get; set; }
        public LoginresultData data { get; set; }        
    }

    public class LoginresultData
    {
        public Login user { get; set; }
    }

    public class user
    {
        public string _Id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string role { get; set; }
    }

    public class EventData
    {
        public string EventName { get; set; }
        public string UserId { get; set; }
    }

    public class UserPreferencesKey
    {
        public string TrackerSelectedProject { get; } = "Tracker.SelectedProject";
        public string ScreenshotSoundDisabled { get; } = "Tracker.ScreenshotSoundDisabled_explicit";
        public string ScreenshotNotificationDisabled { get; } = "Tracker.ScreenshotNotificationDisabled_explicit";
        public string ScreenshotBlur { get; } = "Tracker.ScreenshotBlur_explicit";
        public string WeeklyHoursLimit { get; } = "Tracker.WeeklyHoursLimit_explicit";
        public string MonthlyHoursLimit { get; } = "Tracker.MonthlyHoursLimit_explicit";
    }
}
