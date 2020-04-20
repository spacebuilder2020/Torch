using VRage.Game;

namespace Torch.UI.ViewModels
{
    public class ModViewModel
    {
        public ModViewModel(MyObjectBuilder_Checkpoint.ModItem item, string description = "")
        {
            ModItem = item;
            Description = description;
        }

        public MyObjectBuilder_Checkpoint.ModItem ModItem { get; }
        public string Name => ModItem.Name;
        public string FriendlyName => ModItem.FriendlyName;
        public ulong PublishedFileId => ModItem.PublishedFileId;
        public string Description { get; }

        public static implicit operator MyObjectBuilder_Checkpoint.ModItem(ModViewModel item)
        {
            return item.ModItem;
        }
    }
}