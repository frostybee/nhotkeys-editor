using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NHotkeysEditor.Wpf.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Key> ExcludedKeys { get; private set; } = new List<Key>();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            PopulateExcludedKeys();
        }

        private void PopulateExcludedKeys()
        {
            // A list of keys to be excluded.
            ExcludedKeys.AddRange(
                new Key[] {
                   Key.Up,
                   Key.Down,
                   Key.Left,
                   Key.Right,
                   Key.PageDown,
                   Key.PageUp,
                });
        }
    }
}