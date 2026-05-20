using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TimeTracker.Models.Communication
{
    public class UserPresence
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } // "online", "away", "busy", "dnd", "offline", "invisible"

        [JsonProperty("customStatus")]
        public CustomStatus CustomStatus { get; set; }

        [JsonProperty("customMessage")]
        public string CustomMessage { get; set; }

        [JsonProperty("lastSeenAt")]
        public DateTime? LastSeen { get; set; }

        [JsonProperty("activeDevices")]
        public List<DeviceInfo> Devices { get; set; } = new List<DeviceInfo>();

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonIgnore]
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

        [JsonIgnore]
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

        [JsonIgnore]
        public bool IsOnline => Status == "online" || Status == "away" || Status == "busy" || Status == "dnd";
    }

    public class CustomStatus
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("emoji")]
        public string Emoji { get; set; }

        [JsonProperty("expiresAt")]
        public DateTime? ExpiresAt { get; set; }
    }

    public class DeviceInfo
    {
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; } // "web", "desktop", "mobile", "tablet"

        [JsonProperty("os")]
        public string Os { get; set; }

        [JsonProperty("browser")]
        public string Browser { get; set; }

        [JsonProperty("appVersion")]
        public string AppVersion { get; set; }

        [JsonProperty("isActive")]
        public bool IsActive { get; set; }

        [JsonProperty("lastActiveAt")]
        public DateTime LastActive { get; set; }

        [JsonProperty("pushToken")]
        public string PushToken { get; set; }

        [JsonProperty("pushProvider")]
        public string PushProvider { get; set; }
    }

    public class UpdatePresenceRequest
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("duration")]
        public int? Duration { get; set; } // Duration in milliseconds
    }

    public class RegisterDeviceRequest
    {
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("platform")]
        public string Platform { get; set; } // "web", "desktop", "mobile", "tablet"

        [JsonProperty("os")]
        public string Os { get; set; }

        [JsonProperty("browser")]
        public string Browser { get; set; }

        [JsonProperty("appVersion")]
        public string AppVersion { get; set; }

        [JsonProperty("pushToken")]
        public string PushToken { get; set; }

        [JsonProperty("pushProvider")]
        public string PushProvider { get; set; } // "fcm", "apns", "web"
    }

    public class BulkPresenceRequest
    {
        [JsonProperty("userIds")]
        public List<string> UserIds { get; set; }
    }
}
