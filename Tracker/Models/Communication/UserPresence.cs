using System;
using System.Collections.Generic;

namespace TimeTracker.Models.Communication
{
    public class UserPresence
    {
        public string UserId { get; set; }
        public string Status { get; set; } // "online", "away", "busy", "dnd", "offline", "invisible"
        public string CustomMessage { get; set; }
        public DateTime? LastSeen { get; set; }
        public List<DeviceInfo> Devices { get; set; } = new List<DeviceInfo>();

        public string StatusLabel
        {
            get
            {
                return Status switch
                {
                    "online" => "Online",
                    "away" => "Away",
                    "busy" => "Busy",
                    "dnd" => "Do Not Disturb",
                    "invisible" => "Invisible",
                    _ => "Offline"
                };
            }
        }

        public string StatusColor
        {
            get
            {
                return Status switch
                {
                    "online" => "#4CAF50",
                    "away" => "#FF9800",
                    "busy" or "dnd" => "#F44336",
                    _ => "#9E9E9E"
                };
            }
        }
    }

    public class DeviceInfo
    {
        public string DeviceId { get; set; }
        public string DeviceType { get; set; } // "mobile", "desktop", "web", "tablet"
        public string Platform { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastActive { get; set; }
    }

    public class UpdatePresenceRequest
    {
        public string Status { get; set; }
        public string CustomMessage { get; set; }
    }

    public class RegisterDeviceRequest
    {
        public string DeviceId { get; set; }
        public string DeviceType { get; set; }
        public string Platform { get; set; }
        public string PushToken { get; set; }
    }

    public class BulkPresenceRequest
    {
        public List<string> UserIds { get; set; }
    }
}
