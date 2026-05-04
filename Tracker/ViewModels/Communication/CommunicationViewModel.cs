using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using TimeTracker.Models;
using TimeTracker.Models.Communication;
using TimeTracker.Services.Communication;
using TimeTracker.Trace;

namespace TimeTracker.ViewModels.Communication
{
    public class CommunicationViewModel : ViewModelBase
    {
        private readonly ICommunicationService _communicationService;
        private readonly CommunicationWebSocketService _webSocketService;

        #region Properties

        private ObservableCollection<Conversation> _conversations;
        public ObservableCollection<Conversation> Conversations
        {
            get => _conversations;
            set { _conversations = value; OnPropertyChanged(); }
        }

        private Conversation _selectedConversation;
        public Conversation SelectedConversation
        {
            get => _selectedConversation;
            set
            {
                _selectedConversation = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedConversation));
                if (value != null)
                {
                    _ = LoadMessagesAsync(value.Id);
                }
            }
        }

        public bool HasSelectedConversation => SelectedConversation != null;

        private ObservableCollection<Message> _messages;
        public ObservableCollection<Message> Messages
        {
            get => _messages;
            set { _messages = value; OnPropertyChanged(); }
        }

        private string _messageText;
        public string MessageText
        {
            get => _messageText;
            set { _messageText = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanSendMessage)); }
        }

        public bool CanSendMessage => !string.IsNullOrWhiteSpace(MessageText) && SelectedConversation != null;

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set { _searchQuery = value; OnPropertyChanged(); FilterConversations(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set { _isConnected = value; OnPropertyChanged(); }
        }

        private Dictionary<string, string> _presenceMap = new Dictionary<string, string>();
        private Dictionary<string, List<string>> _typingUsers = new Dictionary<string, List<string>>();

        private string _currentUserId;
        public string CurrentUserId
        {
            get => _currentUserId;
            set { _currentUserId = value; OnPropertyChanged(); }
        }

        private int _totalUnread;
        public int TotalUnread
        {
            get => _totalUnread;
            set { _totalUnread = value; OnPropertyChanged(); }
        }

        private CallSession _activeCall;
        public CallSession ActiveCall
        {
            get => _activeCall;
            set { _activeCall = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasActiveCall)); }
        }

        public bool HasActiveCall => ActiveCall != null;

        private CallSession _incomingCall;
        public CallSession IncomingCall
        {
            get => _incomingCall;
            set { _incomingCall = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasIncomingCall)); }
        }

        public bool HasIncomingCall => IncomingCall != null;

        private List<Conversation> _allConversations = new List<Conversation>();

        private ObservableCollection<ChatUser> _availableUsers;
        public ObservableCollection<ChatUser> AvailableUsers
        {
            get => _availableUsers;
            set { _availableUsers = value; OnPropertyChanged(); }
        }

        private bool _showNewConversationPanel;
        public bool ShowNewConversationPanel
        {
            get => _showNewConversationPanel;
            set { _showNewConversationPanel = value; OnPropertyChanged(); }
        }

        private ChatUser _selectedUser;
        public ChatUser SelectedUser
        {
            get => _selectedUser;
            set { _selectedUser = value; OnPropertyChanged(); }
        }

        private string _userSearchQuery;
        public string UserSearchQuery
        {
            get => _userSearchQuery;
            set { _userSearchQuery = value; OnPropertyChanged(); FilterUsers(); }
        }

        private List<ChatUser> _allUsers = new List<ChatUser>();

        #endregion

        #region Commands

        public ICommand LoadConversationsCommand { get; }
        public ICommand SendMessageCommand { get; }
        public ICommand CreateConversationCommand { get; }
        public ICommand StartCallCommand { get; }
        public ICommand AnswerCallCommand { get; }
        public ICommand DeclineCallCommand { get; }
        public ICommand EndCallCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ShowNewConversationCommand { get; }
        public ICommand StartConversationWithUserCommand { get; }
        public ICommand CloseNewConversationPanelCommand { get; }

        #endregion

        public CommunicationViewModel(ICommunicationService communicationService, CommunicationWebSocketService webSocketService)
        {
            _communicationService = communicationService;
            _webSocketService = webSocketService;

            Conversations = new ObservableCollection<Conversation>();
            Messages = new ObservableCollection<Message>();
            AvailableUsers = new ObservableCollection<ChatUser>();

            LoadConversationsCommand = new RelayCommand(async () => await LoadConversationsAsync());
            SendMessageCommand = new RelayCommand(async () => await SendMessageAsync(), () => CanSendMessage);
            CreateConversationCommand = new RelayCommand<object>(async (param) => await CreateConversationAsync(param));
            StartCallCommand = new RelayCommand<string>(async (type) => await StartCallAsync(type));
            AnswerCallCommand = new RelayCommand(async () => await AnswerCallAsync());
            DeclineCallCommand = new RelayCommand(async () => await DeclineCallAsync());
            EndCallCommand = new RelayCommand(async () => await EndCallAsync());
            RefreshCommand = new RelayCommand(async () => await LoadConversationsAsync());
            ShowNewConversationCommand = new RelayCommand(async () => await ShowNewConversationPanelAsync());
            StartConversationWithUserCommand = new RelayCommand<ChatUser>(async (user) => await StartConversationWithUserAsync(user));
            CloseNewConversationPanelCommand = new RelayCommand(() => ShowNewConversationPanel = false);

            SetupWebSocketHandlers();
        }

        public async Task InitializeAsync()
        {
            CurrentUserId = GlobalSetting.Instance.LoginResult?.data?.user?.id;

            if (!string.IsNullOrEmpty(CurrentUserId))
            {
                await _webSocketService.ConnectAsync(CurrentUserId);
                await LoadConversationsAsync();
            }
        }

        private void SetupWebSocketHandlers()
        {
            _webSocketService.ConnectionStateChanged += (s, connected) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsConnected = connected;
                });
            };

            _webSocketService.NewMessageReceived += (s, message) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    HandleNewMessage(message);
                });
            };

            _webSocketService.MessageUpdated += (s, message) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var existing = Messages.FirstOrDefault(m => m.Id == message.Id);
                    if (existing != null)
                    {
                        var index = Messages.IndexOf(existing);
                        Messages[index] = message;
                    }
                });
            };

            _webSocketService.MessageDeleted += (s, data) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var message = Messages.FirstOrDefault(m => m.Id == data.MessageId);
                    if (message != null)
                    {
                        Messages.Remove(message);
                    }
                });
            };

            _webSocketService.UserTyping += (s, data) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!_typingUsers.ContainsKey(data.ConversationId))
                        _typingUsers[data.ConversationId] = new List<string>();

                    if (!_typingUsers[data.ConversationId].Contains(data.UserId))
                        _typingUsers[data.ConversationId].Add(data.UserId);

                    OnPropertyChanged(nameof(TypingIndicator));
                });
            };

            _webSocketService.UserStoppedTyping += (s, data) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (_typingUsers.ContainsKey(data.ConversationId))
                    {
                        _typingUsers[data.ConversationId].Remove(data.UserId);
                        OnPropertyChanged(nameof(TypingIndicator));
                    }
                });
            };

            _webSocketService.PresenceUpdated += (s, data) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _presenceMap[data.UserId] = data.Status;
                    OnPropertyChanged(nameof(Conversations));
                });
            };

            _webSocketService.IncomingCall += (s, call) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IncomingCall = call;
                });
            };

            _webSocketService.CallAnswered += (s, callId) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (ActiveCall?.Id == callId)
                        ActiveCall.Status = "active";
                });
            };

            _webSocketService.CallEnded += (s, callId) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (ActiveCall?.Id == callId)
                        ActiveCall = null;
                    if (IncomingCall?.Id == callId)
                        IncomingCall = null;
                });
            };

            _webSocketService.ConversationCreated += (s, conversation) =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (!Conversations.Any(c => c.Id == conversation.Id))
                    {
                        Conversations.Insert(0, conversation);
                        _allConversations.Insert(0, conversation);
                    }
                });
            };
        }

        public string TypingIndicator
        {
            get
            {
                if (SelectedConversation == null) return null;

                if (_typingUsers.TryGetValue(SelectedConversation.Id, out var users) && users.Count > 0)
                {
                    if (users.Count == 1)
                        return "Someone is typing...";
                    return $"{users.Count} people are typing...";
                }
                return null;
            }
        }

        #region Methods

        private async Task LoadConversationsAsync()
        {
            IsLoading = true;
            try
            {
                var conversations = await _communicationService.GetConversationsAsync();
                _allConversations = conversations;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Conversations.Clear();
                    foreach (var conv in conversations.OrderByDescending(c => c.LastActivity))
                    {
                        Conversations.Add(conv);
                    }
                    TotalUnread = conversations.Sum(c => c.UnreadCount);
                });
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error loading conversations: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadMessagesAsync(string conversationId)
        {
            IsLoading = true;
            try
            {
                var messages = await _communicationService.GetMessagesAsync(conversationId);
                await _webSocketService.JoinConversationAsync(conversationId);
                await _communicationService.MarkAsReadAsync(conversationId);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Clear();
                    foreach (var msg in messages.OrderBy(m => m.CreatedAt))
                    {
                        msg.IsOwn = msg.SenderId == CurrentUserId;
                        Messages.Add(msg);
                    }

                    // Update unread count
                    var conv = Conversations.FirstOrDefault(c => c.Id == conversationId);
                    if (conv != null)
                    {
                        TotalUnread -= conv.UnreadCount;
                        conv.UnreadCount = 0;
                    }
                });
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error loading messages: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SendMessageAsync()
        {
            if (!CanSendMessage) return;

            var text = MessageText.Trim();
            MessageText = string.Empty;

            // Add optimistic message
            var tempMessage = new Message
            {
                Id = Guid.NewGuid().ToString(),
                ConversationId = SelectedConversation.Id,
                SenderId = CurrentUserId,
                Type = "text",
                Content = new MessageContent { Text = text },
                CreatedAt = DateTime.UtcNow,
                IsSending = true,
                IsOwn = true
            };

            Application.Current.Dispatcher.Invoke(() =>
            {
                Messages.Add(tempMessage);
            });

            try
            {
                var message = await _communicationService.SendMessageAsync(SelectedConversation.Id, new SendMessageRequest { Text = text });

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var index = Messages.IndexOf(tempMessage);
                    if (index >= 0 && message != null)
                    {
                        Messages[index] = message;
                    }
                });
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error sending message: {ex.Message}");
                Application.Current.Dispatcher.Invoke(() =>
                {
                    tempMessage.IsSending = false;
                    tempMessage.HasFailed = true;
                });
            }
        }

        private async Task CreateConversationAsync(object param)
        {
            // This would typically show a dialog to select users
            // For now, we'll implement the basic logic
            if (param is CreateConversationRequest request)
            {
                try
                {
                    var conversation = await _communicationService.CreateConversationAsync(request);
                    if (conversation != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (!Conversations.Any(c => c.Id == conversation.Id))
                            {
                                Conversations.Insert(0, conversation);
                            }
                            SelectedConversation = conversation;
                        });
                    }
                }
                catch (Exception ex)
                {
                    LogManager.Logger.Error($"Error creating conversation: {ex.Message}");
                }
            }
        }

        private async Task StartCallAsync(string type)
        {
            if (SelectedConversation == null) return;

            try
            {
                var participants = SelectedConversation.Participants
                    .Select(p => p.UserId)
                    .Where(id => id != CurrentUserId)
                    .ToList();

                var call = await _communicationService.InitiateCallAsync(new InitiateCallRequest
                {
                    Type = type,
                    Participants = participants,
                    ConversationId = SelectedConversation.Id
                });

                if (call != null)
                {
                    ActiveCall = call;
                    await _webSocketService.JoinCallRoomAsync(call.Id);
                }
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error starting call: {ex.Message}");
            }
        }

        private async Task AnswerCallAsync()
        {
            if (IncomingCall == null) return;

            try
            {
                var call = await _communicationService.AnswerCallAsync(IncomingCall.Id);
                if (call != null)
                {
                    ActiveCall = call;
                    await _webSocketService.JoinCallRoomAsync(call.Id);
                }
                IncomingCall = null;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error answering call: {ex.Message}");
            }
        }

        private async Task DeclineCallAsync()
        {
            if (IncomingCall == null) return;

            try
            {
                await _communicationService.DeclineCallAsync(IncomingCall.Id);
                IncomingCall = null;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error declining call: {ex.Message}");
            }
        }

        private async Task EndCallAsync()
        {
            if (ActiveCall == null) return;

            try
            {
                await _communicationService.EndCallAsync(ActiveCall.Id);
                await _webSocketService.LeaveCallRoomAsync(ActiveCall.Id);
                ActiveCall = null;
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error ending call: {ex.Message}");
            }
        }

        private void HandleNewMessage(Message message)
        {
            if (SelectedConversation?.Id == message.ConversationId)
            {
                if (!Messages.Any(m => m.Id == message.Id))
                {
                    Messages.Add(message);
                }
            }

            // Update conversation list
            var conv = Conversations.FirstOrDefault(c => c.Id == message.ConversationId);
            if (conv != null)
            {
                conv.LastMessage = message;
                conv.LastActivity = message.CreatedAt;

                if (SelectedConversation?.Id != message.ConversationId && message.SenderId != CurrentUserId)
                {
                    conv.UnreadCount++;
                    TotalUnread++;
                }

                // Move to top
                var index = Conversations.IndexOf(conv);
                if (index > 0)
                {
                    Conversations.Move(index, 0);
                }
            }
        }

        private void FilterConversations()
        {
            if (string.IsNullOrWhiteSpace(SearchQuery))
            {
                Conversations.Clear();
                foreach (var conv in _allConversations.OrderByDescending(c => c.LastActivity))
                {
                    Conversations.Add(conv);
                }
            }
            else
            {
                var query = SearchQuery.ToLower();
                var filtered = _allConversations
                    .Where(c => GetConversationName(c).ToLower().Contains(query))
                    .OrderByDescending(c => c.LastActivity)
                    .ToList();

                Conversations.Clear();
                foreach (var conv in filtered)
                {
                    Conversations.Add(conv);
                }
            }
        }

        public string GetConversationName(Conversation conversation)
        {
            if (!string.IsNullOrEmpty(conversation.Name))
                return conversation.Name;

            if (conversation.Type == "direct")
            {
                var otherParticipant = conversation.Participants
                    .FirstOrDefault(p => p.UserId != CurrentUserId);
                if (otherParticipant?.User != null)
                    return otherParticipant.User.FullName;
            }

            return "Unnamed Conversation";
        }

        public string GetPresenceStatus(string userId)
        {
            return _presenceMap.TryGetValue(userId, out var status) ? status : "offline";
        }

        public bool IsOwnMessage(Message message)
        {
            return message.SenderId == CurrentUserId;
        }

        private async Task ShowNewConversationPanelAsync()
        {
            try
            {
                IsLoading = true;
                var users = await _communicationService.GetOrganizationUsersAsync();
                _allUsers = users.Where(u => u.Id != CurrentUserId).ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    AvailableUsers.Clear();
                    foreach (var user in _allUsers)
                    {
                        AvailableUsers.Add(user);
                    }
                    ShowNewConversationPanel = true;
                });
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error loading users: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task StartConversationWithUserAsync(ChatUser user)
        {
            if (user == null) return;

            try
            {
                // Check if a direct conversation already exists with this user
                var existingConv = Conversations.FirstOrDefault(c =>
                    c.Type == "direct" &&
                    c.Participants.Any(p => p.UserId == user.Id));

                if (existingConv != null)
                {
                    SelectedConversation = existingConv;
                    ShowNewConversationPanel = false;
                    return;
                }

                // Create new conversation
                var request = new CreateConversationRequest
                {
                    Type = "direct",
                    Participants = new List<string> { user.Id }
                };

                var conversation = await _communicationService.CreateConversationAsync(request);
                if (conversation != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (!Conversations.Any(c => c.Id == conversation.Id))
                        {
                            Conversations.Insert(0, conversation);
                        }
                        SelectedConversation = conversation;
                        ShowNewConversationPanel = false;
                    });
                }
            }
            catch (Exception ex)
            {
                LogManager.Logger.Error($"Error starting conversation: {ex.Message}");
            }
        }

        private void FilterUsers()
        {
            if (string.IsNullOrWhiteSpace(UserSearchQuery))
            {
                AvailableUsers.Clear();
                foreach (var user in _allUsers)
                {
                    AvailableUsers.Add(user);
                }
            }
            else
            {
                var query = UserSearchQuery.ToLower();
                var filtered = _allUsers.Where(u =>
                    u.FullName.ToLower().Contains(query) ||
                    (u.Email?.ToLower().Contains(query) ?? false))
                    .ToList();

                AvailableUsers.Clear();
                foreach (var user in filtered)
                {
                    AvailableUsers.Add(user);
                }
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _webSocketService?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
