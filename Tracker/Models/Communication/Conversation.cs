using System;
using System.Collections.Generic;
using System.Linq;

namespace TimeTracker.Models.Communication
{
    public class Conversation
    {
        public string Id { get; set; }
        public string Type { get; set; } // "direct", "group", "channel"
        public string Name { get; set; }
        public string Avatar { get; set; }
        public List<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();
        public ConversationSettings Settings { get; set; }
        public Message LastMessage { get; set; }
        public DateTime? LastActivity { get; set; }
        public int UnreadCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ConversationParticipant
    {
        public string UserId { get; set; }
        public ChatUser User { get; set; }
        public string Role { get; set; } // "owner", "admin", "member"
        public DateTime JoinedAt { get; set; }
        public DateTime? LastReadAt { get; set; }
        public string Notifications { get; set; } // "all", "mentions", "none"
    }

    public class ConversationSettings
    {
        public bool IsPinned { get; set; }
        public bool IsMuted { get; set; }
        public DateTime? MuteUntil { get; set; }
        public bool IsArchived { get; set; }
    }

    public class ChatUser
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
        public string Department { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
        public string Initials => $"{FirstName?.FirstOrDefault()}{LastName?.FirstOrDefault()}".ToUpper();
    }

    public class CreateConversationRequest
    {
        public string Type { get; set; }
        public List<string> Participants { get; set; }
        public string Name { get; set; }
        public string InitialMessage { get; set; }
    }

    public class UpdateConversationRequest
    {
        public string Name { get; set; }
        public ConversationSettings Settings { get; set; }
    }

    public class AddParticipantsRequest
    {
        public List<string> UserIds { get; set; }
    }
}
