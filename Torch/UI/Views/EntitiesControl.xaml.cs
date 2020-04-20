using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NLog;
using Torch.UI.ViewModels;
using Torch.UI.ViewModels.Entities;
using Torch.UI.ViewModels.Entities.Blocks;
using Torch.UI.Views.Entities;
using Torch.UI.Views.Entities.Blocks;
using VRage.Game.ModAPI;
using GridView = Torch.UI.Views.Entities.GridView;

namespace Torch.UI.Views
{
    /// <summary>
    ///     Interaction logic for EntitiesControl.xaml
    /// </summary>
    public partial class EntitiesControl : UserControl
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public EntitiesControl()
        {
            InitializeComponent();
            Entities = new EntityTreeViewModel(this);
            DataContext = Entities;
            Entities.Init();
            SortCombo.ItemsSource = Enum.GetNames(typeof(EntityTreeViewModel.SortEnum));
        }

        public EntityTreeViewModel Entities { get; set; }

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is EntityViewModel vm)
            {
                Entities.CurrentEntity = vm;
                if (e.NewValue is GridViewModel gvm)
                    EditorFrame.Content = new GridView {DataContext = gvm};
                if (e.NewValue is BlockViewModel bvm)
                    EditorFrame.Content = new BlockView {DataContext = bvm};
                if (e.NewValue is VoxelMapViewModel vvm)
                    EditorFrame.Content = new VoxelMapView {DataContext = vvm};
                if (e.NewValue is CharacterViewModel cvm)
                    EditorFrame.Content = new CharacterView {DataContext = cvm};
            }
            else
            {
                Entities.CurrentEntity = null;
                EditorFrame.Content = null;
            }
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            if (Entities.CurrentEntity?.Entity is IMyCharacter)
                return;

            TorchBase.Instance.Invoke(() => Entities.CurrentEntity?.Delete());
        }

        private void Stop_OnClick(object sender, RoutedEventArgs e)
        {
            TorchBase.Instance.Invoke(() => Entities.CurrentEntity?.Entity.Physics?.ClearSpeed());
        }

        private void TreeViewItem_OnExpanded(object sender, RoutedEventArgs e)
        {
            //Exact item that was expanded.
            var item = (TreeViewItem)e.OriginalSource;
            if (item.DataContext is ILazyLoad l)
                l.Load();
        }

        private void SortCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sort = (EntityTreeViewModel.SortEnum)SortCombo.SelectedIndex;

            var comparer = new EntityViewModel.Comparer(sort);

            var sortTasks = new Task[4];

            Entities.CurrentSort = sort;
            Entities.SortedCharacters.SetComparer(comparer);
            Entities.SortedFloatingObjects.SetComparer(comparer);
            Entities.SortedGrids.SetComparer(comparer);
            Entities.SortedVoxelMaps.SetComparer(comparer);

            foreach (var i in Entities.SortedCharacters)
                i.DescriptiveName = i.GetSortedName(sort);
            foreach (var i in Entities.SortedFloatingObjects)
                i.DescriptiveName = i.GetSortedName(sort);
            foreach (var i in Entities.SortedGrids)
                i.DescriptiveName = i.GetSortedName(sort);
            foreach (var i in Entities.SortedVoxelMaps)
                i.DescriptiveName = i.GetSortedName(sort);
        }
    }
}