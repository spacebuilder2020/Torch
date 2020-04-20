using System.Windows;
using System.Windows.Controls;

namespace Torch.UI.Views.Entities
{
    /// <summary>
    ///     Interaction logic for VoxelMapView.xaml
    /// </summary>
    public partial class VoxelMapView : UserControl
    {
        public VoxelMapView()
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