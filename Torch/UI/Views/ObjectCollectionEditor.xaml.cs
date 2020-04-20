using System.Collections;
using System.Windows;

namespace Torch.UI.Views
{
    /// <summary>
    ///     Interaction logic for ObjectCollectionEditor.xaml
    /// </summary>
    public partial class ObjectCollectionEditor : Window
    {
        public ObjectCollectionEditor()
        {
            InitializeComponent();
        }

        public void Edit(ICollection collection, string title)
        {
            Editor.Edit(collection);
            Title = title;

            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ShowDialog();
        }
    }
}