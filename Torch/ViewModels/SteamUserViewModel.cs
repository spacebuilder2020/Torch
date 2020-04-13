using Steamworks;

namespace Torch.Server.ViewModels
{
    public class SteamUserViewModel : ViewModel
    {
        public SteamUserViewModel(ulong id)
        {
            SteamId = id;
            Name = SteamFriends.GetFriendPersonaName(new CSteamID(id));
        }

        public SteamUserViewModel() : this(0) { }
        public string Name { get; }
        public ulong SteamId { get; }
    }
}