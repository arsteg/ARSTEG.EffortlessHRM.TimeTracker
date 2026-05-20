using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TimeTracker.Models.Communication
{
    public class Message
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("conversationId")]
        public string ConversationId { get; set; }

        [JsonProperty("senderId")]
        public string SenderId { get; set; }

        [JsonProperty("sender")]
        public ChatUser Sender { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; } // "text", "image", "file", "audio", "video", "call", "system"

        [JsonProperty("content")]
        public MessageContent Content { get; set; }

        [JsonProperty("attachments")]
        public List<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();

        [JsonProperty("replyTo")]
        public string ReplyToId { get; set; }

        [JsonProperty("replyToMessage")]
        public Message ReplyToMessage { get; set; }

        [JsonProperty("reactions")]
        public List<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();

        [JsonProperty("threadId")]
        public string ThreadId { get; set; }

        [JsonProperty("threadCount")]
        public int ThreadCount { get; set; }

        [JsonProperty("readBy")]
        public List<ReadReceipt> ReadBy { get; set; } = new List<ReadReceipt>();

        [JsonProperty("deliveredTo")]
        public List<DeliveryReceipt> DeliveredTo { get; set; } = new List<DeliveryReceipt>();

        [JsonProperty("metadata")]
        public Dictionary<string, object> Metadata { get; set; }

        [JsonProperty("isEdited")]
        public bool IsEdited { get; set; }

        [JsonProperty("editedAt")]
        public DateTime? EditedAt { get; set; }

        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }

        [JsonProperty("deletedAt")]
        public DateTime? DeletedAt { get; set; }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt { get; set; }

        [JsonProperty("updatedAt")]
        public DateTime UpdatedAt { get; set; }

        // UI state (not from API)
        [JsonIgnore]
        public bool IsSending { get; set; }

        [JsonIgnore]
        public bool HasFailed { get; set; }

        [JsonIgnore]
        public bool IsOwn { get; set; }

        // Helper to get display text
        [JsonIgnore]
        public string DisplayText => Content?.Text ?? "";

        // Helper to check if message has been read by anyone
        [JsonIgnore]
        public bool IsRead => ReadBy?.Count > 0;

        // Helper to check if message has been delivered to anyone
        [JsonIgnore]
        public bool IsDelivered => DeliveredTo?.Count > 0;
    }

    public class MessageContent
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("mentions")]
        public List<string> Mentions { get; set; }

        [JsonProperty("links")]
        public List<MessageLink> Links { get; set; }
    }

    public class MessageLink
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }
    }

    public class MessageAttachment
    {
        [JsonProperty("type")]
        public string Type { get; set; } // "image", "video", "audio", "file"

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("mimeType")]
        public string MimeType { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }

        [JsonProperty("duration")]
        public int? Duration { get; set; }

        [JsonProperty("width")]
        public int? Width { get; set; }

        [JsonProperty("height")]
        public int? Height { get; set; }
    }

    public class MessageReaction
    {
        [JsonProperty("emoji")]
        public string Emoji { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("users")]
        public List<string> Users { get; set; } = new List<string>();
    }

    public class ReadReceipt
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("readAt")]
        public DateTime ReadAt { get; set; }
    }

    public class DeliveryReceipt
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("deliveredAt")]
        public DateTime DeliveredAt { get; set; }
    }

    public class SendMessageRequest
    {
        public string Text { get; set; }
        public string Type { get; set; } = "text";
        public List<MessageAttachment> Attachments { get; set; }
        public string ReplyTo { get; set; }
    }

    public class EditMessageRequest
    {
        public string Text { get; set; }
    }

    public class ReactionRequest
    {
        public string Emoji { get; set; }
    }

    public class MarkAsReadRequest
    {
        public string MessageId { get; set; }
    }
}
