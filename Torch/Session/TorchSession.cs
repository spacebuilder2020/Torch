using NLog;
using Sandbox.Game.World;
using Torch.Managers;

namespace Torch.Session
{
    public class TorchSession : ITorchSession
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private TorchSessionState _state = TorchSessionState.Loading;

        public TorchSession(ITorchBase torch, MySession keenSession)
        {
            Torch = torch;
            KeenSession = keenSession;
            Managers = new DependencyManager(torch.Managers);
        }

        /// <summary>
        ///     The Torch instance this session is bound to
        /// </summary>
        public ITorchBase Torch { get; }

        /// <summary>
        ///     The Space Engineers game session this session is bound to.
        /// </summary>
        public MySession KeenSession { get; }

        /// <inheritdoc cref="IDependencyManager" />
        public IDependencyManager Managers { get; }

        /// <inheritdoc />
        public TorchSessionState State
        {
            get => _state;
            internal set
            {
                _state = value;
                StateChanged?.Invoke(this, _state);
            }
        }

        /// <inheritdoc />
        public event TorchSessionStateChangedDel StateChanged;

        internal void Attach()
        {
            Managers.Attach();
        }

        internal void Detach()
        {
            Managers.Detach();
        }
    }
}