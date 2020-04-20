using System;
using System.Collections.Concurrent;
using System.Linq;
using NLog;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.World;
using Torch.Collections;
using Torch.UI.ViewModels;
using Torch.Utils;
using Torch.Utils.Reflected;
using VRage.Game.ModAPI;
using VRage.GameServices;

namespace Torch.Managers
{
    public abstract class MultiplayerManagerBase : Manager, IMultiplayerManagerBase
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

#pragma warning disable 649
        [ReflectedGetter(Name = "m_players")]
        private static Func<MyPlayerCollection, ConcurrentDictionary<MyPlayer.PlayerId, MyPlayer>> _onlinePlayers;
#pragma warning restore 649

        protected MultiplayerManagerBase(ITorchBase torch) : base(torch) { }

        public MtObservableSortedDictionary<ulong, PlayerViewModel> Players { get; } = new MtObservableSortedDictionary<ulong, PlayerViewModel>();

        /// <inheritdoc />
        public event Action<IPlayer> PlayerJoined;

        /// <inheritdoc />
        public event Action<IPlayer> PlayerLeft;

        /// <inheritdoc />
        public override void Attach()
        {
            MyMultiplayer.Static.ClientLeft += OnClientLeft;
        }

        /// <inheritdoc />
        public override void Detach()
        {
            if (MyMultiplayer.Static != null)
                MyMultiplayer.Static.ClientLeft -= OnClientLeft;
        }

        /// <inheritdoc />
        public IMyPlayer GetPlayerByName(string name)
        {
            return _onlinePlayers.Invoke(MySession.Static.Players).FirstOrDefault(x => x.Value.DisplayName == name).Value;
        }

        /// <inheritdoc />
        public IMyPlayer GetPlayerBySteamId(ulong steamId)
        {
            _onlinePlayers.Invoke(MySession.Static.Players).TryGetValue(new MyPlayer.PlayerId(steamId), out var p);
            return p;
        }

        /// <inheritdoc />
        public string GetSteamUsername(ulong steamId)
        {
            return MyMultiplayer.Static.GetMemberName(steamId);
        }

        public ulong GetSteamId(long identityId)
        {
            foreach (var kv in _onlinePlayers.Invoke(MySession.Static.Players))
            {
                if (kv.Value.Identity.IdentityId == identityId)
                    return kv.Key.SteamId;
            }

            return 0;
        }

        private void OnClientLeft(ulong steamId, MyChatMemberStateChangeEnum stateChange)
        {
            Players.TryGetValue(steamId, out var vm);
            if (vm == null)
                vm = new PlayerViewModel(steamId);
            _log.Info($"{vm.Name} ({vm.SteamId}) {(ConnectionState)stateChange}.");
            PlayerLeft?.Invoke(vm);
            Players.Remove(steamId);
        }

        protected void RaiseClientJoined(ulong steamId)
        {
            var vm = new PlayerViewModel(steamId) {State = ConnectionState.Connected};
            _log.Info($"Player {vm.Name} joined ({vm.SteamId})");
            Players.Add(steamId, vm);
            PlayerJoined?.Invoke(vm);
        }
    }
}