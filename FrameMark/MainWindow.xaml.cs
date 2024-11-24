using System.Windows;
using System.Windows.Input;

namespace FrameMark
{
    /// <summary> 主窗口的交互逻辑 </summary>
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();

        private void MW_KeyDown(object sender, KeyEventArgs e)
            => Components.Help.Show();
    }
}