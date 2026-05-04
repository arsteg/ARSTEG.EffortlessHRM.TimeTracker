using System;
using System.Collections.Generic;

namespace TimeTracker.Models.Communication
{
    public class Message
    {
        public string Id { get; set; }
        public string ConversationId { get; set; }
        public string SenderId { get; set; }
        public ChatUser Sender { get; set; }
        public string Type { get; set; } // "text", "image", "file", "audio", "video", "call", "system"
        public MessageContent Content { get; set; }
        public List<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
        public string ReplyToId { get; set; }
        public Message ReplyTo { get; set; }
        public List<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();
        public int ThreadCount { get; set; }
        public List<ReadReceipt> ReadBy { get; set; } = new List<ReadReceipt>();
        public List<DeliveryReceipt> DeliveredTo { get; set; } = new List<DeliveryReceipt>();
        public Dictionary<string, object> Metadata { get; set; }
        public bool IsEdited { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // UI state
        public bool IsSending { get; set; }
        public bool HasFailed { get; set; }
        public bool IsOwn { get; set; }
    }

    public class MessageContent
    {
        public string Text { get; set; }
    }

    public class MessageAttachment
    {
        public string Type { get; set; } // "image", "video", "audio", "file"
        public string Url { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public string MimeType { get; set; }
        public string Thumbnail { get; set; }
        public int? Duration { get; set; }
    }

    public class MessageReaction
    {
        public string Emoji { get; set; }
        public int Count { get; set; }
        public List<string> Users { get; set; } = new List<string>();
    }

    public class ReadReceipt
    {
        public string UserId { get; set; }
        public DateTime ReadAt { get; set; }
    }

    public class DeliveryReceipt
    {
        public string UserId { get; set; }
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
