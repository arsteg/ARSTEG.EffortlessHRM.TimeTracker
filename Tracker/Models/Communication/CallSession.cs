using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TimeTracker.Models.Communication
{
    public class CallSession
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } // "voice", "video", "screen_share"

        [JsonProperty("mode")]
        public string Mode { get; set; } // "p2p", "sfu", "mcu"

        [JsonProperty("status")]
        public string Status { get; set; } // "initiating", "ringing", "connecting", "active", "on_hold", "reconnecting", "ended", "missed", "rejected", "failed", "busy"

        [JsonProperty("initiator")]
        public string InitiatorId { get; set; }

        [JsonProperty("initiatorUser")]
        public ChatUser Initiator { get; set; }

        [JsonProperty("participants")]
        public List<CallParticipant> Participants { get; set; } = new List<CallParticipant>();

        [JsonProperty("conversationId")]
        public string ConversationId { get; set; }

        [JsonProperty("channelId")]
        public string ChannelId { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("startedAt")]
        public DateTime? StartedAt { get; set; }

        [JsonProperty("endedAt")]
        public DateTime? EndedAt { get; set; }

        [JsonProperty("duration")]
        public int? DurationSeconds { get; set; }

        [JsonProperty("iceServers")]
        public List<IceServer> IceServers { get; set; }

        [JsonProperty("meetingLink")]
        public string MeetingLink { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("recording")]
        public CallRecording Recording { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonIgnore]
        public TimeSpan? Duration
        {
            get
            {
                if (DurationSeconds.HasValue)
                    return TimeSpan.FromSeconds(DurationSeconds.Value);
                if (StartedAt.HasValue && EndedAt.HasValue)
                    return EndedAt.Value - StartedAt.Value;
                if (StartedAt.HasValue)
                    return DateTime.UtcNow - StartedAt.Value;
                return null;
            }
        }
    }

    public class CallParticipant
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("user")]
        public ChatUser User { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; } // "invited", "ringing", "connecting", "connected", "on_hold", "reconnecting", "left", "rejected", "busy", "no_answer"

        [JsonProperty("joinedAt")]
        public DateTime? JoinedAt { get; set; }

        [JsonProperty("leftAt")]
        public DateTime? LeftAt { get; set; }

        [JsonProperty("hasAudio")]
        public bool HasAudio { get; set; }

        [JsonProperty("hasVideo")]
        public bool HasVideo { get; set; }

        [JsonProperty("isScreenSharing")]
        public bool IsScreenSharing { get; set; }

        [JsonProperty("isMuted")]
        public bool IsMuted { get; set; }

        [JsonProperty("isDeafened")]
        public bool IsDeafened { get; set; }

        [JsonProperty("connectionQuality")]
        public string ConnectionQuality { get; set; }
    }

    public class IceServer
    {
        [JsonProperty("urls")]
        public object Urls { get; set; } // Can be string or array of strings

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("credential")]
        public string Credential { get; set; }

        // Helper to get URLs as list
        [JsonIgnore]
        public List<string> UrlList
        {
            get
            {
                if (Urls is string s)
                    return new List<string> { s };
                if (Urls is Newtonsoft.Json.Linq.JArray arr)
                    return arr.ToObject<List<string>>();
                return new List<string>();
            }
        }
    }

    public class CallRecording
    {
        [JsonProperty("isRecording")]
        public bool IsRecording { get; set; }

        [JsonProperty("startedAt")]
        public DateTime? StartedAt { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("size")]
        public long? Size { get; set; }
    }

    public class InitiateCallRequest
    {
        public string Type { get; set; }
        public List<string> Participants { get; set; }
        public string ConversationId { get; set; }
        public string TargetUserId { get; set; }
    }

    public class UpdateParticipantStateRequest
    {
        [JsonProperty("hasAudio")]
        public bool? HasAudio { get; set; }

        [JsonProperty("hasVideo")]
        public bool? HasVideo { get; set; }

        [JsonProperty("isScreenSharing")]
        public bool? IsScreenSharing { get; set; }

        [JsonProperty("isMuted")]
        public bool? IsMuted { get; set; }

        [JsonProperty("isDeafened")]
        public bool? IsDeafened { get; set; }
    }
}
