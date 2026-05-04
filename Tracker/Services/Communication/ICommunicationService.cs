using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimeTracker.Models.Communication;

namespace TimeTracker.Services.Communication
{
    public interface ICommunicationService
    {
        // Conversations
        Task<List<Conversation>> GetConversationsAsync(int page = 1, int limit = 50, string type = null, bool archived = false);
        Task<Conversation> GetConversationAsync(string conversationId);
        Task<Conversation> CreateConversationAsync(CreateConversationRequest request);
        Task<Conversation> UpdateConversationAsync(string conversationId, string name = null, ConversationSettings settings = null);
        Task<Conversation> ArchiveConversationAsync(string conversationId);
        Task LeaveConversationAsync(string conversationId);
        Task<Conversation> AddParticipantsAsync(string conversationId, List<string> userIds);
        Task<Conversation> RemoveParticipantAsync(string conversationId, string userId);

        // Messages
        Task<List<Message>> GetMessagesAsync(string conversationId, string before = null, string after = null, int limit = 50);
        Task<Message> SendMessageAsync(string conversationId, SendMessageRequest request);
        Task<Message> EditMessageAsync(string messageId, string text);
        Task DeleteMessageAsync(string messageId);
        Task<Message> AddReactionAsync(string messageId, string emoji);
        Task<Message> RemoveReactionAsync(string messageId, string emoji);
        Task<List<Message>> SearchMessagesAsync(string conversationId, string query);
        Task MarkAsReadAsync(string conversationId, string messageId = null);

        // Calls
        Task<CallSession> InitiateCallAsync(InitiateCallRequest request);
        Task<CallSession> AnswerCallAsync(string callId);
        Task<CallSession> DeclineCallAsync(string callId);
        Task<CallSession> EndCallAsync(string callId);
        Task UpdateParticipantStateAsync(string callId, UpdateParticipantStateRequest request);
        Task<List<CallSession>> GetCallHistoryAsync(int page = 1, int limit = 20);

        // Presence
        Task<UserPresence> UpdatePresenceAsync(UpdatePresenceRequest request);
        Task<UserPresence> GetMyPresenceAsync();
        Task<UserPresence> GetUserPresenceAsync(string userId);
        Task<List<UserPresence>> GetBulkPresenceAsync(List<string> userIds);
        Task RegisterDeviceAsync(RegisterDeviceRequest request);
        Task UnregisterDeviceAsync(string deviceId);

        // Users
        Task<List<ChatUser>> SearchUsersAsync(string query);
        Task<List<ChatUser>> GetOrganizationUsersAsync();
    }
}
