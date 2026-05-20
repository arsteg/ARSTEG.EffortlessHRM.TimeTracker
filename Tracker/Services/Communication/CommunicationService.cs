using System;
using System.Collections.Generic;
using System.Linq;
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
                // API uses skip/limit not page
                var skip = (page - 1) * limit;
                var url = $"{_baseUrl}/conversations?limit={limit}&skip={skip}&archived={archived}";
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
                LogManager.Logger.Info($"CreateConversationAsync called - Type: {request.Type}, Participants: {string.Join(",", request.Participants ?? new List<string>())}");

                if (request.Type == "direct" && request.Participants?.Count == 1)
                {
                    // Use the direct conversation endpoint
                    var targetUserId = request.Participants[0];
                    LogManager.Logger.Info($"Creating direct conversation with targetUserId: {targetUserId}");

                    var directRequest = new { targetUserId };
                    var url = $"{_baseUrl}/conversations/direct";
                    LogManager.Logger.Info($"Calling API: POST {url}");

                    var result = await _httpProvider.PostWithTokenAsync<ApiResponse<Conversation>, object>(url, directRequest, Token);

                    LogManager.Logger.Info($"API Response - Status: {result?.Status}, HasData: {result?.Data != null}, ConversationId: {result?.Data?.Id}");
                    return result?.Data;
                }
                else if (request.Type == "group")
                {
                    // Use the group conversation endpoint - API expects participantIds not Participants
                    var groupRequest = new { name = request.Name, participantIds = request.Participants };
                    var result = await _httpProvider.PostWithTokenAsync<ApiResponse<Conversation>, object>(
                        $"{_baseUrl}/conversations/group", groupRequest, Token);
                    LogManager.Logger.Info($"Group conversation created: {result?.Data?.Id}");
                    return result?.Data;
                }
                else
                {
                    // Fallback - try direct endpoint with first participant
                    LogManager.Logger.Info("Using fallback direct conversation creation");
                    var directRequest = new { targetUserId = request.Participants?.FirstOrDefault() };
                    var result = await _httpProvider.PostWithTokenAsync<ApiResponse<Conversation>, object>(
                        $"{_baseUrl}/conversations/direct", directRequest, Token);
                    return result?.Data;
                }
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error creating conversation: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }

        public async Task<Conversation> UpdateConversationAsync(string conversationId, string name = null, ConversationSettings settings = null)
        {
            try
            {
                var data = new { name, settings };
                // API uses PATCH for updates
                var result = await _httpProvider.PatchWithTokenAsync<ApiResponse<Conversation>, object>(
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
                // API expects participantIds, not UserIds
                var data = new { participantIds = userIds };
                var result = await _httpProvider.PostWithTokenAsync<ApiResponse<Conversation>, object>(
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
                // FIXED: API endpoint is /messages/conversation/{conversationId}
                var url = $"{_baseUrl}/messages/conversation/{conversationId}?limit={limit}";
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
                // FIXED: API expects POST to /messages/ with conversationId in body
                var data = new
                {
                    conversationId = conversationId,
                    content = new { text = request.Text },
                    type = request.Type ?? "text",
                    attachments = request.Attachments,
                    replyTo = request.ReplyTo
                };
                var result = await _httpProvider.PostWithTokenAsync<ApiResponse<Message>, object>(
                    $"{_baseUrl}/messages", data, Token);
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
                // FIXED: API uses PATCH for editing messages
                var data = new { text };
                var result = await _httpProvider.PatchWithTokenAsync<ApiResponse<Message>, object>(
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
                var data = new { emoji };
                var result = await _httpProvider.PostWithTokenAsync<ApiResponse<Message>, object>(
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
                // FIXED: API uses DELETE with body, not URL parameter
                var result = await _httpProvider.DeleteWithTokenAsync<ApiResponse<Message>>(
                    $"{_baseUrl}/messages/{messageId}/reactions", Token);
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
                // FIXED: API endpoint is /messages/search with query params
                var url = $"{_baseUrl}/messages/search?q={Uri.EscapeDataString(query)}";
                if (!string.IsNullOrEmpty(conversationId))
                    url += $"&conversationId={conversationId}";

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
                // FIXED: Endpoint is on conversations, not messages
                var data = new { messageId };
                await _httpProvider.PostWithTokenAsync<ApiResponse<object>, object>(
                    $"{_baseUrl}/conversations/{conversationId}/read", data, Token);
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
                // FIXED: API endpoint is just /calls/ not /calls/initiate
                // API expects targetUserId for 1:1 calls
                var data = new
                {
                    targetUserId = request.Participants?.FirstOrDefault(),
                    conversationId = request.ConversationId,
                    type = request.Type ?? "voice"
                };
                var result = await _httpProvider.PostWithTokenAsync<ApiResponse<CallResponseData>, object>(
                    $"{_baseUrl}/calls", data, Token);
                return result?.Data?.CallSession;
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
                    $"{_baseUrl}/calls/{callId}/answer", new { hasAudio = true, hasVideo = false }, Token);
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
                // FIXED: API endpoint is /reject not /decline
                var result = await _httpProvider.PostWithTokenAsync<ApiResponse<CallSession>, object>(
                    $"{_baseUrl}/calls/{callId}/reject", new { }, Token);
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
                    $"{_baseUrl}/calls/{callId}/end", new { reason = "completed" }, Token);
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
                // FIXED: API uses PATCH and endpoint is /state not /participant-state
                await _httpProvider.PatchWithTokenAsync<ApiResponse<object>, UpdateParticipantStateRequest>(
                    $"{_baseUrl}/calls/{callId}/state", request, Token);
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
                var skip = (page - 1) * limit;
                var url = $"{_baseUrl}/calls/history?limit={limit}&skip={skip}";
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
                // FIXED: API uses PATCH not PUT
                var result = await _httpProvider.PatchWithTokenAsync<ApiResponse<UserPresence>, UpdatePresenceRequest>(
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
                // FIXED: API endpoint is /presence/ not /presence/me
                var result = await _httpProvider.GetWithTokenAsync<ApiResponse<UserPresence>>($"{_baseUrl}/presence", Token);
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
                // FIXED: API endpoint is /presence/user/{userId}
                var result = await _httpProvider.GetWithTokenAsync<ApiResponse<UserPresence>>($"{_baseUrl}/presence/user/{userId}", Token);
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
                var data = new { userIds };
                var result = await _httpProvider.PostWithTokenAsync<ApiResponse<List<UserPresence>>, object>(
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

        public async Task SendHeartbeatAsync(string deviceId, bool isActive)
        {
            try
            {
                var data = new { deviceId, isActive };
                await _httpProvider.PostWithTokenAsync<ApiResponse<object>, object>(
                    $"{_baseUrl}/presence/heartbeat", data, Token);
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error sending heartbeat: {ex.Message}");
            }
        }

        #endregion

        #region Users

        public async Task<List<ChatUser>> SearchUsersAsync(string query)
        {
            try
            {
                // Get all users and filter client-side
                var allUsers = await GetOrganizationUsersAsync();

                if (string.IsNullOrWhiteSpace(query))
                    return allUsers;

                var lowerQuery = query.ToLower();
                return allUsers.Where(u =>
                    (u.FullName?.ToLower().Contains(lowerQuery) ?? false) ||
                    (u.Email?.ToLower().Contains(lowerQuery) ?? false)
                ).ToList();
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
                // Use existing endpoint: /api/v1/users/getUsersByCompany/:companyId
                var companyId = GlobalSetting.Instance.LoginResult?.data?.user?.company?.id;
                if (string.IsNullOrEmpty(companyId))
                {
                    LogManager.Logger.Warn("Company ID is null - cannot fetch users");
                    return new List<ChatUser>();
                }

                var url = $"{GlobalSetting.apiBaseUrl}/api/v1/users/getUsersByCompany/{companyId}";
                LogManager.Logger.Info($"Fetching users from: {url}");

                var result = await _httpProvider.GetWithTokenAsync<UsersApiResponse>(url, Token);
                var users = result?.Data?.Users ?? new List<ChatUser>();

                // Log user IDs for debugging
                LogManager.Logger.Info($"GetOrganizationUsersAsync returned {users.Count} users");
                foreach (var user in users.Take(5)) // Log first 5 for debugging
                {
                    LogManager.Logger.Debug($"  User: {user.FullName}, Id: '{user.Id}'");
                }

                return users;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error getting organization users: {ex.Message}");
                return new List<ChatUser>();
            }
        }

        #endregion

        #region Response Classes

        private class ApiResponse<T>
        {
            // API returns "status": "success" as string
            public string Status { get; set; }
            // Some endpoints might return "success": true as boolean
            public bool Success { get; set; }
            public T Data { get; set; }
            public string Message { get; set; }
            // Additional fields from API
            public int? Results { get; set; }
            public int? Total { get; set; }
            public bool? Existing { get; set; }

            // Helper property to check if successful (handles both formats)
            public bool IsSuccess => Success || Status?.ToLower() == "success";
        }

        // Response class for /api/v1/users/getUsersByCompany endpoint
        private class UsersApiResponse
        {
            public string Status { get; set; }
            public UsersData Data { get; set; }
        }

        private class UsersData
        {
            public List<ChatUser> Users { get; set; }
        }

        private class CallResponseData
        {
            public CallSession CallSession { get; set; }
            public List<IceServer> IceServers { get; set; }
        }

        private class IceServer
        {
            public List<string> Urls { get; set; }
            public string Username { get; set; }
            public string Credential { get; set; }
        }

        #endregion
    }
}
