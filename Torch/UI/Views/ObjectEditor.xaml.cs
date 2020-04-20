using System.Windows;

namespace Torch.UI.Views
{
    /// <summary>
    ///     Interaction logic for ObjectEditor.xaml
    /// </summary>
    public partial class ObjectEditor : Window
    {
        public ObjectEditor()
        {
            InitializeComponent();
        }

        public void Edit(object o, string title = "Edit Object")
        {
            PGrid.DataContext = o;
            Title = title;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ShowDialog();
        }
    }
}