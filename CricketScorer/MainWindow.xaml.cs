using System.Diagnostics;
using System.Windows;

namespace CricketScorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainViewModel mvm;
        public MainWindow()
        {
            InitializeComponent();
            mvm = new MainViewModel();
            DataContext = mvm;
            Trace.WriteLine("View model assigned.");
        }
    }
}
