using System;
using System.Collections.Generic;

namespace TimeTracker.Models.Communication
{
    public class CallSession
    {
        public string Id { get; set; }
        public string Type { get; set; } // "audio", "video"
        public string Status { get; set; } // "ringing", "connecting", "active", "ended", "missed", "declined"
        public string InitiatorId { get; set; }
        public ChatUser Initiator { get; set; }
        public List<CallParticipant> Participants { get; set; } = new List<CallParticipant>();
        public DateTime? StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public List<IceServer> IceServers { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public TimeSpan? Duration
        {
            get
            {
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
        public string UserId { get; set; }
        public ChatUser User { get; set; }
        public string Status { get; set; } // "invited", "ringing", "connecting", "connected", "left", "declined"
        public DateTime? JoinedAt { get; set; }
        public DateTime? LeftAt { get; set; }
        public bool HasAudio { get; set; }
        public bool HasVideo { get; set; }
        public bool IsScreenSharing { get; set; }
    }

    public class IceServer
    {
        public string Urls { get; set; }
        public string Username { get; set; }
        public string Credential { get; set; }
    }

    public class InitiateCallRequest
    {
        public string Type { get; set; }
        public List<string> Participants { get; set; }
        public string ConversationId { get; set; }
    }

    public class UpdateParticipantStateRequest
    {
        public bool? HasAudio { get; set; }
        public bool? HasVideo { get; set; }
        public bool? IsScreenSharing { get; set; }
    }
}
