using System;
using System.Threading.Tasks;
using NLog;
using Torch.Utils.SteamWorkshopTools;
using VRage.Game;

namespace Torch.UI.ViewModels
{
    /// <summary>
    ///     Wrapper around VRage.Game.Objectbuilder_Checkpoint.ModItem
    ///     that holds additional meta information
    ///     (e.g. workshop description)
    /// </summary>
    public class ModItemInfo : ViewModel
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private string _description;
        MyObjectBuilder_Checkpoint.ModItem _modItem;

        /// <summary>
        ///     Constructor, returns a new ModItemInfo instance
        /// </summary>
        /// <param name="mod">The wrapped mod</param>
        public ModItemInfo(MyObjectBuilder_Checkpoint.ModItem mod)
        {
            _modItem = mod;
        }

        /// <summary>
        ///     Human friendly name of the mod
        /// </summary>
        public string FriendlyName { get { return _modItem.FriendlyName; } set { SetValue(ref _modItem.FriendlyName, value); } }

        /// <summary>
        ///     Workshop ID of the mod
        /// </summary>
        public ulong PublishedFileId { get { return _modItem.PublishedFileId; } set { SetValue(ref _modItem.PublishedFileId, value); } }

        /// <summary>
        ///     Local filename of the mod
        /// </summary>
        public string Name { get { return _modItem.Name; } set { SetValue(ref _modItem.FriendlyName, value); } }

        /// <summary>
        ///     Whether or not the mod was added
        ///     because another mod depends on it
        /// </summary>
        public bool IsDependency { get { return _modItem.IsDependency; } set { SetValue(ref _modItem.IsDependency, value); } }

        /// <summary>
        ///     Workshop description of the mod
        /// </summary>
        public string Description { get { return _description; } set { SetValue(ref _description, value); } }

        /// <summary>
        ///     Retrieve information about the
        ///     wrapped mod from the workhop asynchronously
        ///     via the Steam web API.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> UpdateModInfoAsync()
        {
            var workshopService = WebAPI.Instance;
            PublishedItemDetails modInfo = null;
            try
            {
                modInfo = (await workshopService.GetPublishedFileDetails(new ulong[] {PublishedFileId}))?[PublishedFileId];
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
            }

            if (modInfo == null)
            {
                Log.Error($"Failed to retrieve mod with workshop id '{PublishedFileId}'!");
                return false;
            }
            //else if (!modInfo.Tags.Contains(""))
            else
            {
                Log.Info("Mod Info successfully retrieved!");
                FriendlyName = modInfo.Title;
                Description = modInfo.Description;
                //Name = modInfo.FileName;
                return true;
            }
        }
    }
}