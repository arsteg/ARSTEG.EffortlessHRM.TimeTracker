using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using TimeTracker.Models;
using TimeTracker.Models.Communication;
using TimeTracker.Trace;

namespace TimeTracker.Services.Communication
{
    public class CommunicationWebSocketService : IDisposable
    {
        private SocketIOClient.SocketIO _socket;
        private readonly string _baseUrl;
        private string _userId;
        private bool _isConnecting;
        private int _reconnectAttempts;
        private const int MaxReconnectAttempts = 50;
        private bool _disposed;

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

        public bool IsConnected => _socket?.Connected ?? false;

        public CommunicationWebSocketService()
        {
            _baseUrl = GlobalSetting.apiBaseUrl;
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

                var token = GlobalSetting.Instance.LoginResult?.token;
                if (string.IsNullOrEmpty(token))
                {
                    LogManager.Logger.Warn("No token available for WebSocket connection");
                    _isConnecting = false;
                    ConnectionStateChanged?.Invoke(this, false);
                    return;
                }

                // Create Socket.IO client with authentication
                _socket = new SocketIOClient.SocketIO(_baseUrl, new SocketIOOptions
                {
                    Auth = new { token = token },
                    Reconnection = true,
                    ReconnectionAttempts = MaxReconnectAttempts,
                    ReconnectionDelay = 1000,
                    ReconnectionDelayMax = 30000,
                    ConnectionTimeout = TimeSpan.FromSeconds(20),
                    Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
                });

                SetupEventHandlers();

                LogManager.Logger.Info($"Connecting to Socket.IO at {_baseUrl}");
                await _socket.ConnectAsync();

                LogManager.Logger.Info("Socket.IO connected successfully");
                _reconnectAttempts = 0;
                _isConnecting = false;
                ConnectionStateChanged?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Socket.IO connection error: {ex.Message}");
                _isConnecting = false;
                ConnectionStateChanged?.Invoke(this, false);
                ScheduleReconnect();
            }
        }

        private void SetupEventHandlers()
        {
            _socket.OnConnected += (sender, e) =>
            {
                LogManager.Logger.Info("Socket.IO OnConnected event fired");
                ConnectionStateChanged?.Invoke(this, true);
            };

            _socket.OnDisconnected += (sender, e) =>
            {
                LogManager.Logger.Info($"Socket.IO disconnected: {e}");
                ConnectionStateChanged?.Invoke(this, false);
                if (_userId != null && !_disposed)
                {
                    ScheduleReconnect();
                }
            };

            _socket.OnError += (sender, e) =>
            {
                LogManager.Logger.Error($"Socket.IO error: {e}");
            };

            _socket.OnReconnectAttempt += (sender, attempt) =>
            {
                LogManager.Logger.Info($"Socket.IO reconnect attempt {attempt}");
            };

            _socket.OnReconnected += (sender, attempt) =>
            {
                LogManager.Logger.Info($"Socket.IO reconnected after {attempt} attempts");
                ConnectionStateChanged?.Invoke(this, true);
            };

            _socket.OnReconnectFailed += (sender, e) =>
            {
                LogManager.Logger.Error("Socket.IO reconnect failed");
            };

            // Message events
            _socket.On("new_message", response =>
            {
                try
                {
                    var json = response.GetValue<JObject>();
                    var message = json?["message"]?.ToObject<Message>();
                    if (message != null)
                    {
                        LogManager.Logger.Debug($"Received new_message: {message.Id}");
                        NewMessageReceived?.Invoke(this, message);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Error($"Error handling new_message: {ex.Message}");
                }
            });

            _socket.On("message_updated", response =>
            {
                try
                {
                    var message = response.GetValue<Message>();
                    if (message != null)
                    {
                        MessageUpdated?.Invoke(this, message);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Error($"Error handling message_updated: {ex.Message}");
                }
            });

            _socket.On("message_deleted", response =>
            {
                try
                {
                    var json = response.GetValue<JObject>();
                    var conversationId = json?["conversationId"]?.ToString();
                    var messageId = json?["messageId"]?.ToString();
                    if (!string.IsNullOrEmpty(conversationId) && !string.IsNullOrEmpty(messageId))
                    {
                        MessageDeleted?.Invoke(this, (conversationId, messageId));
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Error($"Error handling message_deleted: {ex.Message}");
                }
            });

            // Typing events
            _socket.On("user_typing", response =>
            {
                try
                {
                    var json = response.GetValue<JObject>();
                    var conversationId = json?["conversationId"]?.ToString();
                    var userId = json?["userId"]?.ToString();
                    var userName = json?["userName"]?.ToString();
                    if (!string.IsNullOrEmpty(conversationId) && !string.IsNullOrEmpty(userId))
                    {
                        UserTyping?.Invoke(this, (conversationId, userId, userName ?? ""));
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Error($"Error handling user_typing: {ex.Message}");
                }
            });

            _socket.On("user_stopped_typing", response =>
            {
                try
                {
                    var json = response.GetValue<JObject>();
                    var conversationId = json?["conversationId"]?.ToString();
                    var userId = json?["userId"]?.ToString();
                    if (!string.IsNullOrEmpty(conversationId) && !string.IsNullOrEmpty(userId))
                    {
                        UserStoppedTyping?.Invoke(this, (conversationId, userId));
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Error($"Error handling user_stopped_typing: {ex.Message}");
                }
            });

            // Presence events
            _socket.On("presence_update", response =>
            {
                try
                {
                    var json = response.GetValue<JObject>();
                    var userId = json?["userId"]?.ToString();
                    var status = json?["status"]?.ToString();
                    if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(status))
                    {
                        PresenceUpdated?.Invoke(this, (userId, status));
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Error($"Error handling presence_update: {ex.Message}");
                }
            });

            // Call events
            _socket.On("incoming_call", response =>
            {
                try
                {
                    var call = response.GetValue<CallSession>();
                    if (call != null)
                    {
                        IncomingCall?.Invoke(this, call);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Error($"Error handling incoming_call: {ex.Message}");
                }
            });

            _socket.On("call_answered", response =>
            {
                try
                {
                    var json = response.GetValue<JObject>();
                    var callId = json?["callId"]?.ToString();
                    if (!string.IsNullOrEmpty(callId))
                    {
                        CallAnswered?.Invoke(this, callId);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Error($"Error handling call_answered: {ex.Message}");
                }
            });

            _socket.On("call_declined", response =>
            {
                try
                {
                    var json = response.GetValue<JObject>();
                    var callId = json?["callId"]?.ToString();
                    if (!string.IsNullOrEmpty(callId))
                    {
                        CallDeclined?.Invoke(this, callId);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Error($"Error handling call_declined: {ex.Message}");
                }
            });

            _socket.On("call_ended", response =>
            {
                try
                {
                    var json = response.GetValue<JObject>();
                    var callId = json?["callId"]?.ToString();
                    if (!string.IsNullOrEmpty(callId))
                    {
                        CallEnded?.Invoke(this, callId);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Error($"Error handling call_ended: {ex.Message}");
                }
            });

            // Conversation events
            _socket.On("conversation_created", response =>
            {
                try
                {
                    var conversation = response.GetValue<Conversation>();
                    if (conversation != null)
                    {
                        ConversationCreated?.Invoke(this, conversation);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Error($"Error handling conversation_created: {ex.Message}");
                }
            });

            _socket.On("conversation_updated", response =>
            {
                try
                {
                    var conversation = response.GetValue<Conversation>();
                    if (conversation != null)
                    {
                        ConversationUpdated?.Invoke(this, conversation);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Error($"Error handling conversation_updated: {ex.Message}");
                }
            });
        }

        public async Task DisconnectAsync()
        {
            _userId = null;
            await DisconnectInternalAsync();
            ConnectionStateChanged?.Invoke(this, false);
        }

        private async Task DisconnectInternalAsync()
        {
            if (_socket != null)
            {
                try
                {
                    if (_socket.Connected)
                    {
                        await _socket.DisconnectAsync();
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Error($"Error disconnecting Socket.IO: {ex.Message}");
                }
                finally
                {
                    _socket.Dispose();
                    _socket = null;
                }
            }
        }

        #region Send Methods

        public async Task JoinConversationAsync(string conversationId)
        {
            if (!IsConnected) return;
            try
            {
                await _socket.EmitAsync("join_conversation", new { conversationId });
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error joining conversation: {ex.Message}");
            }
        }

        public async Task LeaveConversationAsync(string conversationId)
        {
            if (!IsConnected) return;
            try
            {
                await _socket.EmitAsync("leave_conversation", new { conversationId });
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error leaving conversation: {ex.Message}");
            }
        }

        public async Task JoinCallRoomAsync(string callId)
        {
            if (!IsConnected) return;
            try
            {
                await _socket.EmitAsync("join_call", new { callId });
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error joining call room: {ex.Message}");
            }
        }

        public async Task LeaveCallRoomAsync(string callId)
        {
            if (!IsConnected) return;
            try
            {
                await _socket.EmitAsync("leave_call", new { callId });
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error leaving call room: {ex.Message}");
            }
        }

        public async Task StartTypingAsync(string conversationId)
        {
            if (!IsConnected) return;
            try
            {
                await _socket.EmitAsync("typing_start", new { conversationId });
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error sending typing start: {ex.Message}");
            }
        }

        public async Task StopTypingAsync(string conversationId)
        {
            if (!IsConnected) return;
            try
            {
                await _socket.EmitAsync("typing_stop", new { conversationId });
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error sending typing stop: {ex.Message}");
            }
        }

        public async Task UpdatePresenceAsync(string status, string customMessage = null)
        {
            if (!IsConnected) return;
            try
            {
                await _socket.EmitAsync("presence_update", new { status, customMessage });
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error updating presence: {ex.Message}");
            }
        }

        #endregion

        #region Reconnection

        private void ScheduleReconnect()
        {
            if (_reconnectAttempts >= MaxReconnectAttempts || _userId == null || _disposed) return;

            var delay = Math.Min(1000 * Math.Pow(2, _reconnectAttempts), 30000);
            _reconnectAttempts++;

            LogManager.Logger.Info($"Scheduling Socket.IO reconnect in {delay}ms (attempt {_reconnectAttempts})");

            Task.Delay((int)delay).ContinueWith(async _ =>
            {
                if (_userId != null && !_disposed)
                {
                    await ConnectAsync(_userId);
                }
            });
        }

        #endregion

        public void Dispose()
        {
            _disposed = true;
            DisconnectInternalAsync().Wait(TimeSpan.FromSeconds(5));
        }
    }
}
