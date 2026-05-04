using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TimeTracker.Models;
using TimeTracker.Models.Communication;
using TimeTracker.Trace;

namespace TimeTracker.Services.Communication
{
    public class CommunicationWebSocketService : IDisposable
    {
        private ClientWebSocket _webSocket;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly string _baseWsUrl;
        private string _userId;
        private bool _isConnecting;
        private int _reconnectAttempts;
        private const int MaxReconnectAttempts = 50;
        private Timer _heartbeatTimer;

        // Events
        public event EventHandler<bool> ConnectionStateChanged;
        public event EventHandler<Message> NewMessageReceived;
        public event EventHandler<Message> MessageUpdated;
        public event EventHandler<(string ConversationId, string MessageId)> MessageDeleted;
        public event EventHandler<(string ConversationId, string UserId, string UserName)> UserTyping;
        public event EventHandler<(string ConversationId, string UserId)> UserStoppedTyping;
        public event EventHandler<(string UserId, string Status)> PresenceUpdated;
        public event EventHandler<CallSession> IncomingCall;
        public event EventHandler<string> CallAnswered;
        public event EventHandler<string> CallDeclined;
        public event EventHandler<string> CallEnded;
        public event EventHandler<Conversation> ConversationCreated;
        public event EventHandler<Conversation> ConversationUpdated;

        public bool IsConnected => _webSocket?.State == WebSocketState.Open;

        public CommunicationWebSocketService()
        {
            _baseWsUrl = GlobalSetting.apiBaseUrl.Replace("https://", "wss://").Replace("http://", "ws://");
        }

        public async Task ConnectAsync(string userId)
        {
            if (_isConnecting) return;
            if (IsConnected && _userId == userId) return;

            _userId = userId;
            _isConnecting = true;

            try
            {
                await DisconnectInternalAsync();

                _webSocket = new ClientWebSocket();
                _cancellationTokenSource = new CancellationTokenSource();

                var token = GlobalSetting.Instance.LoginResult?.token;
                var wsUrl = $"{_baseWsUrl}/communication?token={token}";

                await _webSocket.ConnectAsync(new Uri(wsUrl), _cancellationTokenSource.Token);

                LogManager.Logger.Info("Communication WebSocket connected");
                _reconnectAttempts = 0;
                _isConnecting = false;

                ConnectionStateChanged?.Invoke(this, true);
                StartHeartbeat();
                _ = ListenForMessagesAsync();
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"WebSocket connection error: {ex.Message}");
                _isConnecting = false;
                ConnectionStateChanged?.Invoke(this, false);
                ScheduleReconnect();
            }
        }

        public async Task DisconnectAsync()
        {
            _userId = null;
            StopHeartbeat();
            await DisconnectInternalAsync();
            ConnectionStateChanged?.Invoke(this, false);
        }

        private async Task DisconnectInternalAsync()
        {
            if (_webSocket != null)
            {
                try
                {
                    if (_webSocket.State == WebSocketState.Open)
                    {
                        _cancellationTokenSource?.Cancel();
                        await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None);
                    }
                }
                catch { }
                finally
                {
                    _webSocket.Dispose();
                    _webSocket = null;
                }
            }
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        private async Task ListenForMessagesAsync()
        {
            var buffer = new byte[4096];
            var messageBuilder = new StringBuilder();

            try
            {
                while (_webSocket?.State == WebSocketState.Open)
                {
                    var result = await _webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        _cancellationTokenSource.Token
                    );

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        LogManager.Logger.Info("WebSocket closed by server");
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                        if (result.EndOfMessage)
                        {
                            var message = messageBuilder.ToString();
                            messageBuilder.Clear();
                            HandleMessage(message);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when disconnecting
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"WebSocket receive error: {ex.Message}");
            }
            finally
            {
                ConnectionStateChanged?.Invoke(this, false);
                if (_userId != null)
                {
                    ScheduleReconnect();
                }
            }
        }

        private void HandleMessage(string rawMessage)
        {
            try
            {
                var json = JObject.Parse(rawMessage);
                var eventType = json["event"]?.ToString();
                var payload = json["payload"];

                if (string.IsNullOrEmpty(eventType)) return;

                switch (eventType)
                {
                    case "new_message":
                        var newMessage = payload.ToObject<NewMessagePayload>();
                        if (newMessage?.Message != null)
                            NewMessageReceived?.Invoke(this, newMessage.Message);
                        break;

                    case "message_updated":
                        var updatedMessage = payload.ToObject<Message>();
                        if (updatedMessage != null)
                            MessageUpdated?.Invoke(this, updatedMessage);
                        break;

                    case "message_deleted":
                        var deletedData = payload.ToObject<MessageDeletedPayload>();
                        if (deletedData != null)
                            MessageDeleted?.Invoke(this, (deletedData.ConversationId, deletedData.MessageId));
                        break;

                    case "user_typing":
                        var typingData = payload.ToObject<TypingPayload>();
                        if (typingData != null)
                            UserTyping?.Invoke(this, (typingData.ConversationId, typingData.UserId, typingData.UserName));
                        break;

                    case "user_stopped_typing":
                        var stoppedTypingData = payload.ToObject<TypingPayload>();
                        if (stoppedTypingData != null)
                            UserStoppedTyping?.Invoke(this, (stoppedTypingData.ConversationId, stoppedTypingData.UserId));
                        break;

                    case "presence_update":
                        var presenceData = payload.ToObject<PresencePayload>();
                        if (presenceData != null)
                            PresenceUpdated?.Invoke(this, (presenceData.UserId, presenceData.Status));
                        break;

                    case "incoming_call":
                        var incomingCall = payload.ToObject<CallSession>();
                        if (incomingCall != null)
                            IncomingCall?.Invoke(this, incomingCall);
                        break;

                    case "call_answered":
                        var answeredCallId = payload["callId"]?.ToString();
                        if (!string.IsNullOrEmpty(answeredCallId))
                            CallAnswered?.Invoke(this, answeredCallId);
                        break;

                    case "call_declined":
                        var declinedCallId = payload["callId"]?.ToString();
                        if (!string.IsNullOrEmpty(declinedCallId))
                            CallDeclined?.Invoke(this, declinedCallId);
                        break;

                    case "call_ended":
                        var endedCallId = payload["callId"]?.ToString();
                        if (!string.IsNullOrEmpty(endedCallId))
                            CallEnded?.Invoke(this, endedCallId);
                        break;

                    case "conversation_created":
                        var newConversation = payload.ToObject<Conversation>();
                        if (newConversation != null)
                            ConversationCreated?.Invoke(this, newConversation);
                        break;

                    case "conversation_updated":
                        var updatedConversation = payload.ToObject<Conversation>();
                        if (updatedConversation != null)
                            ConversationUpdated?.Invoke(this, updatedConversation);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error handling WebSocket message: {ex.Message}");
            }
        }

        #region Send Methods

        private async Task SendAsync(string eventType, object payload)
        {
            if (_webSocket?.State != WebSocketState.Open) return;

            try
            {
                var message = JsonConvert.SerializeObject(new { @event = eventType, payload });
                var bytes = Encoding.UTF8.GetBytes(message);
                await _webSocket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    _cancellationTokenSource?.Token ?? CancellationToken.None
                );
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error sending WebSocket message: {ex.Message}");
            }
        }

        public Task JoinConversationAsync(string conversationId)
        {
            return SendAsync("join_conversation", new { conversationId });
        }

        public Task LeaveConversationAsync(string conversationId)
        {
            return SendAsync("leave_conversation", new { conversationId });
        }

        public Task JoinCallRoomAsync(string callId)
        {
            return SendAsync("join_call", new { callId });
        }

        public Task LeaveCallRoomAsync(string callId)
        {
            return SendAsync("leave_call", new { callId });
        }

        public Task StartTypingAsync(string conversationId)
        {
            return SendAsync("typing_start", new { conversationId });
        }

        public Task StopTypingAsync(string conversationId)
        {
            return SendAsync("typing_stop", new { conversationId });
        }

        public Task UpdatePresenceAsync(string status, string customMessage = null)
        {
            return SendAsync("presence_update", new { status, customMessage });
        }

        #endregion

        #region Reconnection

        private void ScheduleReconnect()
        {
            if (_reconnectAttempts >= MaxReconnectAttempts || _userId == null) return;

            var delay = Math.Min(1000 * Math.Pow(2, _reconnectAttempts), 30000);
            _reconnectAttempts++;

            LogManager.Logger.Info($"Scheduling reconnect in {delay}ms (attempt {_reconnectAttempts})");

            Task.Delay((int)delay).ContinueWith(async _ =>
            {
                if (_userId != null)
                {
                    await ConnectAsync(_userId);
                }
            });
        }

        #endregion

        #region Heartbeat

        private void StartHeartbeat()
        {
            StopHeartbeat();
            _heartbeatTimer = new Timer(async _ =>
            {
                await SendAsync("ping", new { });
            }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        private void StopHeartbeat()
        {
            _heartbeatTimer?.Dispose();
            _heartbeatTimer = null;
        }

        #endregion

        public void Dispose()
        {
            StopHeartbeat();
            DisconnectInternalAsync().Wait();
        }

        #region Payload Classes

        private class NewMessagePayload
        {
            public string ConversationId { get; set; }
            public Message Message { get; set; }
        }

        private class MessageDeletedPayload
        {
            public string ConversationId { get; set; }
            public string MessageId { get; set; }
        }

        private class TypingPayload
        {
            public string ConversationId { get; set; }
            public string UserId { get; set; }
            public string UserName { get; set; }
        }

        private class PresencePayload
        {
            public string UserId { get; set; }
            public string Status { get; set; }
        }

        #endregion
    }
}
