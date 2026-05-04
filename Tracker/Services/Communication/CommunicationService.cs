using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimeTracker.Models;
using TimeTracker.Models.Communication;
using TimeTracker.Services.Interfaces;
using TimeTracker.Trace;

namespace TimeTracker.Services.Communication
{
    public class CommunicationService : ICommunicationService
    {
        private readonly IHttpProvider _httpProvider;
        private readonly string _baseUrl;

        public CommunicationService(IHttpProvider httpProvider)
        {
            _httpProvider = httpProvider;
            _baseUrl = GlobalSetting.apiBaseUrl + "/api/v1/communication";
        }

        private string Token => GlobalSetting.Instance.LoginResult?.token ?? "";

        #region Conversations

        public async Task<List<Conversation>> GetConversationsAsync(int page = 1, int limit = 50, string type = null, bool archived = false)
        {
            try
            {
                var url = $"{_baseUrl}/conversations?page={page}&limit={limit}&archived={archived}";
                if (!string.IsNullOrEmpty(type))
                    url += $"&type={type}";

                var result = await _httpProvider.GetWithTokenAsync<ApiResponse<List<Conversation>>>(url, Token);
                return result?.Data ?? new List<Conversation>();
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error getting conversations: {ex.Message}");
                return new List<Conversation>();
            }
        }

        public async Task<Conversation> GetConversationAsync(string conversationId)
        {
            try
            {
                var result = await _httpProvider.GetWithTokenAsync<ApiResponse<Conversation>>($"{_baseUrl}/conversations/{conversationId}", Token);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error getting conversation: {ex.Message}");
                return null;
            }
        }

        public async Task<Conversation> CreateConversationAsync(CreateConversationRequest request)
        {
            try
            {
                var result = await _httpProvider.PostWithTokenAsync<ApiResponse<Conversation>, CreateConversationRequest>(
                    $"{_baseUrl}/conversations", request, Token);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error creating conversation: {ex.Message}");
                return null;
            }
        }

        public async Task<Conversation> UpdateConversationAsync(string conversationId, string name = null, ConversationSettings settings = null)
        {
            try
            {
                var data = new UpdateConversationRequest { Name = name, Settings = settings };
                var result = await _httpProvider.PutWithTokenAsync<ApiResponse<Conversation>, UpdateConversationRequest>(
                    $"{_baseUrl}/conversations/{conversationId}", data, Token);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error updating conversation: {ex.Message}");
                return null;
            }
        }

        public async Task<Conversation> ArchiveConversationAsync(string conversationId)
        {
            try
            {
                var result = await _httpProvider.PostWithTokenAsync<ApiResponse<Conversation>, object>(
                    $"{_baseUrl}/conversations/{conversationId}/archive", new { }, Token);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error archiving conversation: {ex.Message}");
                return null;
            }
        }

        public async Task LeaveConversationAsync(string conversationId)
        {
            try
            {
                await _httpProvider.PostWithTokenAsync<ApiResponse<object>, object>(
                    $"{_baseUrl}/conversations/{conversationId}/leave", new { }, Token);
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error leaving conversation: {ex.Message}");
            }
        }

        public async Task<Conversation> AddParticipantsAsync(string conversationId, List<string> userIds)
        {
            try
            {
                var data = new AddParticipantsRequest { UserIds = userIds };
                var result = await _httpProvider.PostWithTokenAsync<ApiResponse<Conversation>, AddParticipantsRequest>(
                    $"{_baseUrl}/conversations/{conversationId}/participants", data, Token);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error adding participants: {ex.Message}");
                return null;
            }
        }

        public async Task<Conversation> RemoveParticipantAsync(string conversationId, string userId)
        {
            try
            {
                var result = await _httpProvider.DeleteWithTokenAsync<ApiResponse<Conversation>>(
                    $"{_baseUrl}/conversations/{conversationId}/participants/{userId}", Token);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error removing participant: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Messages

        public async Task<List<Message>> GetMessagesAsync(string conversationId, string before = null, string after = null, int limit = 50)
        {
            try
            {
                var url = $"{_baseUrl}/messages/{conversationId}?limit={limit}";
                if (!string.IsNullOrEmpty(before))
                    url += $"&before={before}";
                if (!string.IsNullOrEmpty(after))
                    url += $"&after={after}";

                var result = await _httpProvider.GetWithTokenAsync<ApiResponse<List<Message>>>(url, Token);
                return result?.Data ?? new List<Message>();
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error getting messages: {ex.Message}");
                return new List<Message>();
            }
        }

        public async Task<Message> SendMessageAsync(string conversationId, SendMessageRequest request)
        {
            try
            {
                var result = await _httpProvider.PostWithTokenAsync<ApiResponse<Message>, SendMessageRequest>(
                    $"{_baseUrl}/messages/{conversationId}", request, Token);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error sending message: {ex.Message}");
                return null;
            }
        }

        public async Task<Message> EditMessageAsync(string messageId, string text)
        {
            try
            {
                var data = new EditMessageRequest { Text = text };
                var result = await _httpProvider.PutWithTokenAsync<ApiResponse<Message>, EditMessageRequest>(
                    $"{_baseUrl}/messages/{messageId}", data, Token);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error editing message: {ex.Message}");
                return null;
            }
        }

        public async Task DeleteMessageAsync(string messageId)
        {
            try
            {
                await _httpProvider.DeleteWithTokenAsync<ApiResponse<object>>($"{_baseUrl}/messages/{messageId}", Token);
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error deleting message: {ex.Message}");
            }
        }

        public async Task<Message> AddReactionAsync(string messageId, string emoji)
        {
            try
            {
                var data = new ReactionRequest { Emoji = emoji };
                var result = await _httpProvider.PostWithTokenAsync<ApiResponse<Message>, ReactionRequest>(
                    $"{_baseUrl}/messages/{messageId}/reactions", data, Token);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error adding reaction: {ex.Message}");
                return null;
            }
        }

        public async Task<Message> RemoveReactionAsync(string messageId, string emoji)
        {
            try
            {
                var result = await _httpProvider.DeleteWithTokenAsync<ApiResponse<Message>>(
                    $"{_baseUrl}/messages/{messageId}/reactions/{Uri.EscapeDataString(emoji)}", Token);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error removing reaction: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Message>> SearchMessagesAsync(string conversationId, string query)
        {
            try
            {
                var url = $"{_baseUrl}/messages/{conversationId}/search?q={Uri.EscapeDataString(query)}";
                var result = await _httpProvider.GetWithTokenAsync<ApiResponse<List<Message>>>(url, Token);
                return result?.Data ?? new List<Message>();
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error searching messages: {ex.Message}");
                return new List<Message>();
            }
        }

        public async Task MarkAsReadAsync(string conversationId, string messageId = null)
        {
            try
            {
                var data = new MarkAsReadRequest { MessageId = messageId };
                await _httpProvider.PostWithTokenAsync<ApiResponse<object>, MarkAsReadRequest>(
                    $"{_baseUrl}/messages/{conversationId}/read", data, Token);
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error marking as read: {ex.Message}");
            }
        }

        #endregion

        #region Calls

        public async Task<CallSession> InitiateCallAsync(InitiateCallRequest request)
        {
            try
            {
                var result = await _httpProvider.PostWithTokenAsync<ApiResponse<CallSession>, InitiateCallRequest>(
                    $"{_baseUrl}/calls/initiate", request, Token);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error initiating call: {ex.Message}");
                return null;
            }
        }

        public async Task<CallSession> AnswerCallAsync(string callId)
        {
            try
            {
                var result = await _httpProvider.PostWithTokenAsync<ApiResponse<CallSession>, object>(
                    $"{_baseUrl}/calls/{callId}/answer", new { }, Token);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error answering call: {ex.Message}");
                return null;
            }
        }

        public async Task<CallSession> DeclineCallAsync(string callId)
        {
            try
            {
                var result = await _httpProvider.PostWithTokenAsync<ApiResponse<CallSession>, object>(
                    $"{_baseUrl}/calls/{callId}/decline", new { }, Token);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error declining call: {ex.Message}");
                return null;
            }
        }

        public async Task<CallSession> EndCallAsync(string callId)
        {
            try
            {
                var result = await _httpProvider.PostWithTokenAsync<ApiResponse<CallSession>, object>(
                    $"{_baseUrl}/calls/{callId}/end", new { }, Token);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error ending call: {ex.Message}");
                return null;
            }
        }

        public async Task UpdateParticipantStateAsync(string callId, UpdateParticipantStateRequest request)
        {
            try
            {
                await _httpProvider.PutWithTokenAsync<ApiResponse<object>, UpdateParticipantStateRequest>(
                    $"{_baseUrl}/calls/{callId}/participant-state", request, Token);
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error updating participant state: {ex.Message}");
            }
        }

        public async Task<List<CallSession>> GetCallHistoryAsync(int page = 1, int limit = 20)
        {
            try
            {
                var url = $"{_baseUrl}/calls/history?page={page}&limit={limit}";
                var result = await _httpProvider.GetWithTokenAsync<ApiResponse<List<CallSession>>>(url, Token);
                return result?.Data ?? new List<CallSession>();
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error getting call history: {ex.Message}");
                return new List<CallSession>();
            }
        }

        #endregion

        #region Presence

        public async Task<UserPresence> UpdatePresenceAsync(UpdatePresenceRequest request)
        {
            try
            {
                var result = await _httpProvider.PutWithTokenAsync<ApiResponse<UserPresence>, UpdatePresenceRequest>(
                    $"{_baseUrl}/presence/status", request, Token);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error updating presence: {ex.Message}");
                return null;
            }
        }

        public async Task<UserPresence> GetMyPresenceAsync()
        {
            try
            {
                var result = await _httpProvider.GetWithTokenAsync<ApiResponse<UserPresence>>($"{_baseUrl}/presence/me", Token);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error getting my presence: {ex.Message}");
                return null;
            }
        }

        public async Task<UserPresence> GetUserPresenceAsync(string userId)
        {
            try
            {
                var result = await _httpProvider.GetWithTokenAsync<ApiResponse<UserPresence>>($"{_baseUrl}/presence/{userId}", Token);
                return result?.Data;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error getting user presence: {ex.Message}");
                return null;
            }
        }

        public async Task<List<UserPresence>> GetBulkPresenceAsync(List<string> userIds)
        {
            try
            {
                var data = new BulkPresenceRequest { UserIds = userIds };
                var result = await _httpProvider.PostWithTokenAsync<ApiResponse<List<UserPresence>>, BulkPresenceRequest>(
                    $"{_baseUrl}/presence/bulk", data, Token);
                return result?.Data ?? new List<UserPresence>();
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error getting bulk presence: {ex.Message}");
                return new List<UserPresence>();
            }
        }

        public async Task RegisterDeviceAsync(RegisterDeviceRequest request)
        {
            try
            {
                await _httpProvider.PostWithTokenAsync<ApiResponse<object>, RegisterDeviceRequest>(
                    $"{_baseUrl}/presence/device", request, Token);
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error registering device: {ex.Message}");
            }
        }

        public async Task UnregisterDeviceAsync(string deviceId)
        {
            try
            {
                await _httpProvider.DeleteWithTokenAsync<ApiResponse<object>>($"{_baseUrl}/presence/device/{deviceId}", Token);
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error unregistering device: {ex.Message}");
            }
        }

        #endregion

        #region Users

        public async Task<List<ChatUser>> SearchUsersAsync(string query)
        {
            try
            {
                var url = $"{_baseUrl}/users/search?q={Uri.EscapeDataString(query)}";
                var result = await _httpProvider.GetWithTokenAsync<ApiResponse<List<ChatUser>>>(url, Token);
                return result?.Data ?? new List<ChatUser>();
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error searching users: {ex.Message}");
                return new List<ChatUser>();
            }
        }

        public async Task<List<ChatUser>> GetOrganizationUsersAsync()
        {
            try
            {
                var result = await _httpProvider.GetWithTokenAsync<ApiResponse<List<ChatUser>>>($"{_baseUrl}/users", Token);
                return result?.Data ?? new List<ChatUser>();
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error getting organization users: {ex.Message}");
                return new List<ChatUser>();
            }
        }

        #endregion

        private class ApiResponse<T>
        {
            public bool Success { get; set; }
            public T Data { get; set; }
            public string Message { get; set; }
        }
    }
}
