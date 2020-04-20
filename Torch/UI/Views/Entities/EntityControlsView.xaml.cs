using System.Windows;
using System.Windows.Controls;

namespace Torch.UI.Views.Entities
{
    /// <summary>
    ///     Interaction logic for EntityControlsView.xaml
    /// </summary>
    public partial class EntityControlsView : ItemsControl
    {
        public EntityControlsView()
        {
            InitializeComponent();

            ThemeControl.UpdateDynamicControls += UpdateResourceDict;
            UpdateResourceDict(ThemeControl.currentTheme);
        }

        public void UpdateResourceDict(ResourceDictionary dictionary)
        {
            Resources.MergedDictionaries.Clear();
            Resources.MergedDictionaries.Add(dictionary);
        }
    }
}