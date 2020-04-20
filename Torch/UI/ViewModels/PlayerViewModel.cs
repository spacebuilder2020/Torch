using Sandbox.Engine.Multiplayer;
using Sandbox.Game.World;
using VRage.Game.ModAPI;
using VRage.Replication;

namespace Torch.UI.ViewModels
{
    public class PlayerViewModel : ViewModel, IPlayer
    {
        private ConnectionState _state;

        public PlayerViewModel(ulong steamId, string name = null)
        {
            SteamId = steamId;
            Name = name ?? ((MyDedicatedServerBase)MyMultiplayerMinimalBase.Instance).GetMemberName(steamId);
        }

        public MyPromoteLevel PromoteLevel => MySession.Static.GetUserPromoteLevel(SteamId);

        public string PromotedName
        {
            get
            {
                var p = PromoteLevel;
                if (p <= MyPromoteLevel.None)
                    return Name;
                else
                    return $"{Name} ({p})";
            }
        }

        public ulong SteamId { get; }
        public string Name { get; }

        public ConnectionState State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChanged();
            }
        }
    }
}