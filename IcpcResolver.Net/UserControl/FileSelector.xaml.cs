using System.Windows;
using System.Windows.Forms;
using Ookii.Dialogs.Wpf;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace IcpcResolver.Net.UserControl
{
    /// <summary>
    /// Interaction logic for FileSelector.xaml
    /// </summary>
    public partial class FileSelector : System.Windows.Controls.UserControl
    {
        public FileSelector()
        {
            InitializeComponent();
        }

        public string LabelName
        {
            get => (string) GetValue(LabelNameProperty);
            set => SetValue(LabelNameProperty, value);
        }

        private static readonly DependencyProperty LabelNameProperty =
            DependencyProperty.Register("LabelName", typeof(string), typeof(FileSelector));

        public string PathSelected
        {
            get => (string) GetValue(PathSelectedProperty);
            set => SetValue(PathSelectedProperty, value);
        }

        private static readonly DependencyProperty PathSelectedProperty =
            DependencyProperty.Register("PathSelected", typeof(string), typeof(FileSelector),
                new PropertyMetadata(""));


        public int LabelWidth
        {
            get => (int) GetValue(LabelWidthProperty);
            set => SetValue(LabelWidthProperty, value);
        }

        private static readonly DependencyProperty LabelWidthProperty =
            DependencyProperty.Register("LabelWidth", typeof(int), typeof(FileSelector), new PropertyMetadata(50));

        public bool SelectFolder
        {
            get => (bool) GetValue(SelectFolderProperty);
            set => SetValue(SelectFolderProperty, value);
        }

        private static readonly DependencyProperty SelectFolderProperty =
            DependencyProperty.Register("SelectFolder", typeof(bool), typeof(FileSelector),
                new PropertyMetadata(false));

        public string FileNameFilter
        {
            get => (string) GetValue(FileNameFilterProperty);
            set => SetValue(FileNameFilterProperty, value);
        }

        private static readonly DependencyProperty FileNameFilterProperty =
            DependencyProperty.Register("FileNameFilter", typeof(string), typeof(FileSelector),
                new PropertyMetadata("All files (*.*)|*.*"));

        private void ButtonSelect_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectFolder)
            {
                var dialog = new VistaFolderBrowserDialog();
                
                if (dialog.ShowDialog() == true)
                {
                    PathSelected = dialog.SelectedPath;
                }
            }
            else
            {
                var dialog = new OpenFileDialog
                {
                    Filter= FileNameFilter,
                };

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    PathSelected = dialog.FileName;
                }
            }
        }
    }
}
