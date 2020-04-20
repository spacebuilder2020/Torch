using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Sandbox.Engine.Utils;
using Torch.Collections;
using Torch.Managers;
using Torch.Utils.SteamWorkshopTools;
using VRage.Game;

namespace Torch.UI.ViewModels
{
    public class ConfigDedicatedViewModel : ViewModel
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private MtObservableList<ModItemInfo> _mods = new MtObservableList<ModItemInfo>();

        //this is a damn server password. I don't care if this is insecure. Bite me.
        private string _password;
        private WorldViewModel _selectedWorld;

        private SessionSettingsViewModel _sessionSettings;

        public ConfigDedicatedViewModel() : this(new MyConfigDedicated<MyObjectBuilder_SessionSettings>("")) { }

        public ConfigDedicatedViewModel(MyConfigDedicated<MyObjectBuilder_SessionSettings> configDedicated)
        {
            Model = configDedicated;
            //_config.IgnoreLastSession = true;
            SessionSettings = new SessionSettingsViewModel(Model.SessionSettings);
            Task.Run(() => UpdateAllModInfosAsync());
        }

        public MyConfigDedicated<MyObjectBuilder_SessionSettings> Model { get; }

        public SessionSettingsViewModel SessionSettings
        {
            get => _sessionSettings;
            set
            {
                _sessionSettings = value;
                OnPropertyChanged();
            }
        }

        public MtObservableList<WorldViewModel> Worlds { get; } = new MtObservableList<WorldViewModel>();

        public WorldViewModel SelectedWorld
        {
            get => _selectedWorld;
            set
            {
                SetValue(ref _selectedWorld, value);
                LoadWorld = _selectedWorld?.WorldPath;
            }
        }

        public List<string> Administrators { get => Model.Administrators; set => SetValue(x => Model.Administrators = x, value); }

        public List<ulong> Banned { get => Model.Banned; set => SetValue(x => Model.Banned = x, value); }

        public MtObservableList<ModItemInfo> Mods
        {
            get => _mods;
            set
            {
                SetValue(x => _mods = x, value);
                Task.Run(() => UpdateAllModInfosAsync());
            }
        }

        public List<ulong> Reserved { get => Model.Reserved; set => SetValue(x => Model.Reserved = x, value); }

        public int AsteroidAmount { get => Model.AsteroidAmount; set => SetValue(x => Model.AsteroidAmount = x, value); }

        public ulong GroupId { get => Model.GroupID; set => SetValue(x => Model.GroupID = x, value); }

        public string IP { get => Model.IP; set => SetValue(x => Model.IP = x, value); }

        public int Port { get => Model.ServerPort; set => SetValue(x => Model.ServerPort = x, value); }

        public string ServerName { get => Model.ServerName; set => SetValue(x => Model.ServerName = x, value); }

        public string ServerDescription { get => Model.ServerDescription; set => SetValue(x => Model.ServerDescription = x, value); }

        public bool PauseGameWhenEmpty { get => Model.PauseGameWhenEmpty; set => SetValue(x => Model.PauseGameWhenEmpty = x, value); }

        public bool AutodetectDependencies { get => Model.AutodetectDependencies; set => SetValue(x => Model.AutodetectDependencies = x, value); }

        public string PremadeCheckpointPath { get => Model.PremadeCheckpointPath; set => SetValue(x => Model.PremadeCheckpointPath = x, value); }

        public string LoadWorld { get => Model.LoadWorld; set => SetValue(x => Model.LoadWorld = x, value); }

        public int SteamPort { get => Model.SteamPort; set => SetValue(x => Model.SteamPort = x, value); }

        public string WorldName { get => Model.WorldName; set => SetValue(x => Model.WorldName = x, value); }

        public string Password
        {
            get
            {
                if (string.IsNullOrEmpty(_password))
                {
                    if (string.IsNullOrEmpty(Model.ServerPasswordHash))
                        return string.Empty;

                    return "**********";
                }

                return _password;
            }
            set
            {
                _password = value;
                if (!string.IsNullOrEmpty(value))
                    Model.SetPassword(value);
                else
                {
                    Model.ServerPasswordHash = null;
                    Model.ServerPasswordSalt = null;
                }
            }
        }

        public void Save(string path = null)
        {
            Validate();

            Model.SessionSettings = _sessionSettings;
            // Never ever
            //_config.IgnoreLastSession = true;
            Model.Save(path);
        }

        public bool Validate()
        {
            if (SelectedWorld == null)
            {
                Log.Warn($"{nameof(SelectedWorld)} == null");
                return false;
            }

            if (LoadWorld == null)
            {
                Log.Warn($"{nameof(LoadWorld)} == null");
                return false;
            }

            return true;
        }

        public async Task UpdateAllModInfosAsync(Action<string> messageHandler = null)
        {
            if (Mods.Count() == 0)
                return;

            var ids = Mods.Select(m => m.PublishedFileId);
            var workshopService = WebAPI.Instance;
            Dictionary<ulong, PublishedItemDetails> modInfos = null;

            try
            {
                modInfos = await workshopService.GetPublishedFileDetails(ids.ToArray());
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                return;
            }

            Log.Info("Mods Info successfully retrieved!");

            foreach (var mod in Mods)
            {
                if (!modInfos.ContainsKey(mod.PublishedFileId) || modInfos[mod.PublishedFileId] == null)
                {
                    Log.Error($"Failed to retrieve info for mod with workshop id '{mod.PublishedFileId}'!");
                }
                //else if (!modInfo.Tags.Contains(""))
                else
                {
                    mod.FriendlyName = modInfos[mod.PublishedFileId].Title;
                    mod.Description = modInfos[mod.PublishedFileId].Description;
                    //mod.Name = modInfos[mod.PublishedFileId].FileName;
                }
            }
        }
    }
}