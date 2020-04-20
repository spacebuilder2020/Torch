using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using NLog;
using Sandbox.Game.Gui;
using Sandbox.Game.World;
using Torch.Managers;
using Torch.Session;
using Torch.Utils;
using CommandManager = Torch.Commands.CommandManager;

namespace Torch.UI.Views
{
    /// <summary>
    ///     Interaction logic for ChatControl.xaml
    /// </summary>
    public partial class ChatControl : UserControl
    {
        private static Logger _log = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<string, Brush> _brushes = new Dictionary<string, Brush>();
        private ITorchServer _server;

        public ChatControl()
        {
            InitializeComponent();
            IsVisibleChanged += OnIsVisibleChanged;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                //I hate this and I hate myself. You should hate me too
                Task.Run(() =>
                {
                    Thread.Sleep(100);

                    Dispatcher.Invoke(() =>
                    {
                        Message.Focus();
                        Keyboard.Focus(Message);
                    });
                });
            }
        }

        public void BindServer(ITorchServer server)
        {
            _server = server;

            server.Initialized += Server_Initialized;
        }

        private void Server_Initialized(ITorchServer obj)
        {
            Dispatcher.InvokeAsync(() => { ChatItems.Inlines.Clear(); });

            var sessionManager = _server.Managers.GetManager<ITorchSessionManager>();
            if (sessionManager != null)
                sessionManager.SessionStateChanged += SessionStateChanged;
        }

        private void SessionStateChanged(ITorchSession session, TorchSessionState state)
        {
            switch (state)
            {
                case TorchSessionState.Loading:
                    Dispatcher.InvokeAsync(() => ChatItems.Inlines.Clear());
                    break;
                case TorchSessionState.Loaded:
                {
                    var chatMgr = session.Managers.GetManager<IChatManagerClient>();
                    if (chatMgr != null)
                        chatMgr.MessageRecieved += OnMessageRecieved;
                }
                    break;
                case TorchSessionState.Unloading:
                {
                    var chatMgr = session.Managers.GetManager<IChatManagerClient>();
                    if (chatMgr != null)
                        chatMgr.MessageRecieved -= OnMessageRecieved;
                }
                    break;
                case TorchSessionState.Unloaded:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void OnMessageRecieved(TorchChatMessage msg, ref bool consumed)
        {
            InsertMessage(msg);
        }

        private static Brush LookupBrush(string font)
        {
            if (_brushes.TryGetValue(font, out var result))
                return result;

            var brush = typeof(Brushes).GetField(font, BindingFlags.Static)?.GetValue(null) as Brush ?? Brushes.Blue;
            _brushes.Add(font, brush);
            return brush;
        }

        private void InsertMessage(TorchChatMessage msg)
        {
            if (Dispatcher.CheckAccess())
            {
                var atBottom = ChatScroller.VerticalOffset + 8 > ChatScroller.ScrollableHeight;
                var span = new Span();
                span.Inlines.Add($"{msg.Timestamp} ");
                switch (msg.Channel)
                {
                    case ChatChannel.Faction:
                        span.Inlines.Add(new Run($"[{MySession.Static.Factions.TryGetFactionById(msg.Target)?.Tag ?? "???"}] ") {Foreground = Brushes.Green});
                        break;
                    case ChatChannel.Private:
                        span.Inlines.Add(new Run($"[to {MySession.Static.Players.TryGetIdentity(msg.Target)?.DisplayName ?? "???"}] ") {Foreground = Brushes.DeepPink});
                        break;
                }

                span.Inlines.Add(new Run(msg.Author) {Foreground = LookupBrush(msg.Font)});
                span.Inlines.Add($": {msg.Message}");
                span.Inlines.Add(new LineBreak());
                ChatItems.Inlines.Add(span);
                if (atBottom)
                    ChatScroller.ScrollToBottom();
            }
            else
                Dispatcher.InvokeAsync(() => InsertMessage(msg));
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            OnMessageEntered();
        }

        private void Message_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                OnMessageEntered();
        }

        private void OnMessageEntered()
        {
            //Can't use Message.Text directly because of object ownership in WPF.
            var text = Message.Text;
            if (string.IsNullOrEmpty(text))
                return;

            var commands = _server.CurrentSession?.Managers.GetManager<CommandManager>();
            if (commands != null && commands.IsCommand(text))
            {
                var color = ColorUtils.TranslateColor(TorchBase.Instance.Config.ChatColor);
                InsertMessage(new TorchChatMessage(TorchBase.Instance.Config.ChatName, text, color));
                _server.Invoke(() =>
                {
                    var responses = commands.HandleCommandFromServer(text);
                    if (responses == null)
                    {
                        InsertMessage(new TorchChatMessage(TorchBase.Instance.Config.ChatName, "Invalid command.", color));
                        return;
                    }

                    foreach (var response in responses)
                        InsertMessage(response);
                });
            }
            else
            {
                _server.CurrentSession?.Managers.GetManager<IChatManagerClient>().SendMessageAsSelf(text);
            }

            Message.Text = "";
        }
    }
}