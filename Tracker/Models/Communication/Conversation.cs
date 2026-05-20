using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace TimeTracker.Models.Communication
{
    public class Conversation
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } // "direct", "group", "channel"

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("participants")]
        public List<ConversationParticipant> Participants { get; set; } = new List<ConversationParticipant>();

        [JsonProperty("settings")]
        public ConversationSettings Settings { get; set; }

        [JsonProperty("lastMessage")]
        public Message LastMessage { get; set; }

        // API might return lastMessagePreview as string
        [JsonProperty("lastMessagePreview")]
        public string LastMessagePreview { get; set; }

        [JsonProperty("lastMessageSenderId")]
        public string LastMessageSenderId { get; set; }

        [JsonProperty("lastActivity")]
        public DateTime? LastActivity { get; set; }

        [JsonProperty("unreadCount")]
        public int UnreadCount { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("createdBy")]
        public string CreatedBy { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        // Helper to get display message
        public string DisplayLastMessage => LastMessage?.Content?.Text ?? LastMessagePreview ?? "";
    }

    public class ConversationParticipant
    {
        private string _userId;
        private ChatUser _user;

        // Handle userId as either string or populated object from API
        [JsonProperty("userId")]
        public object UserIdRaw
        {
            set
            {
                if (value == null) return;

                if (value is string str)
                {
                    _userId = str;
                }
                else if (value is Newtonsoft.Json.Linq.JObject jObj)
                {
                    // API returned populated user data in userId field
                    _user = jObj.ToObject<ChatUser>();
                    _userId = _user?.Id;
                }
            }
        }

        [JsonIgnore]
        public string UserId
        {
            get => _userId;
            set => _userId = value;
        }

        [JsonProperty("user")]
        public ChatUser User
        {
            get => _user;
            set { if (value != null) _user = value; }
        }

        [JsonProperty("role")]
        public string Role { get; set; } // "owner", "admin", "member"

        [JsonProperty("joinedAt")]
        public DateTime JoinedAt { get; set; }

        [JsonProperty("lastReadAt")]
        public DateTime? LastReadAt { get; set; }

        [JsonProperty("notifications")]
        public string Notifications { get; set; } // "all", "mentions", "none"

        [JsonProperty("notificationPreference")]
        public string NotificationPreference { get; set; }

        [JsonProperty("isMuted")]
        public bool IsMuted { get; set; }

        [JsonProperty("mutedUntil")]
        public DateTime? MutedUntil { get; set; }
    }

    public class ConversationSettings
    {
        [JsonProperty("isPinned")]
        public bool IsPinned { get; set; }

        [JsonProperty("isMuted")]
        public bool IsMuted { get; set; }

        [JsonProperty("muteUntil")]
        public DateTime? MuteUntil { get; set; }

        [JsonProperty("isArchived")]
        public bool IsArchived { get; set; }

        [JsonProperty("notificationPreference")]
        public string NotificationPreference { get; set; }
    }

    public class ChatUser
    {
        private string _id;

        // Handle both "_id" (from MongoDB) and "id" (from our API)
        [JsonProperty("_id")]
        public string MongoId { set { if (!string.IsNullOrEmpty(value)) _id = value; } }

        [JsonProperty("id")]
        public string Id
        {
            get => _id;
            set { if (!string.IsNullOrEmpty(value)) _id = value; }
        }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("profilePicture")]
        public string ProfilePicture { get; set; }

        [JsonProperty("photo")]
        public string Photo { get; set; }

        [JsonProperty("department")]
        public string Department { get; set; }

        [JsonProperty("jobTitle")]
        public string JobTitle { get; set; }

        [JsonIgnore]
        public string FullName => $"{FirstName} {LastName}".Trim();

        [JsonIgnore]
        public string Initials
        {
            get
            {
                var first = FirstName?.FirstOrDefault();
                var last = LastName?.FirstOrDefault();
                if (first == default && last == default)
                    return "?";
                return $"{first}{last}".ToUpper();
            }
        }

        [JsonIgnore]
        public string DisplayPicture => ProfilePicture ?? Photo;
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
        [JsonProperty("participantIds")]
        public List<string> ParticipantIds { get; set; }
    }
}
