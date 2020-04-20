using System.Collections.Generic;
using Torch.Collections;
using Torch.Managers;
using Torch.Plugins;

namespace Torch.UI.ViewModels
{
    public class PluginManagerViewModel : ViewModel
    {
        private PluginViewModel _selectedPlugin;

        public PluginManagerViewModel() { }

        public PluginManagerViewModel(IPluginManager pluginManager)
        {
            foreach (var plugin in pluginManager)
                Plugins.Add(new PluginViewModel(plugin));
            pluginManager.PluginsLoaded += PluginManager_PluginsLoaded;
        }

        public MtObservableList<PluginViewModel> Plugins { get; } = new MtObservableList<PluginViewModel>();

        public PluginViewModel SelectedPlugin
        {
            get => _selectedPlugin;
            set
            {
                _selectedPlugin = value;
                OnPropertyChanged(nameof(SelectedPlugin));
            }
        }

        private void PluginManager_PluginsLoaded(IReadOnlyCollection<ITorchPlugin> obj)
        {
            Plugins.Clear();
            foreach (var plugin in obj)
            {
                Plugins.Add(new PluginViewModel(plugin));
            }
        }
    }
}