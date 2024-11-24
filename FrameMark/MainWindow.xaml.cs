using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FrameMark
{
    /// <summary> 主窗口的交互逻辑 </summary>
    public partial class MainWindow : Window
    {
        #region 加载和快捷键

        public MainWindow() => InitializeComponent();

        private void MW_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
                Components.Help.Show();
        }

        #endregion

        #region 边框设置

        private void TBFrameT_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(TBFrameT.Text, out double frameT)
                && (frameT > 100
                || frameT <= 0))
                TBFrameT.Text = "12";
        }

        private void TBFrameB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(TBFrameB.Text, out double frameB)
                && (frameB > 100
                || frameB <= 0))
                TBFrameB.Text = "16";
        }

        private void TBFrameL_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(TBFrameL.Text, out double frameL)
                && (frameL > 100
                || frameL <= 0))
                TBFrameL.Text = "14";
        }

        private void TBFrameR_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(TBFrameR.Text, out double frameR)
                && (frameR > 100
                || frameR <= 0))
                TBFrameR.Text = "14";
        }

        #endregion

        #region 圆角设置

        private void TBRoundCorner_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(TBRoundCorner.Text, out double roundCorner)
                && (roundCorner > 100
                || roundCorner <= 0))
                TBRoundCorner.Text = "16";
        }

        #endregion

        #region 模糊半径设置

        private void TBBlurRadius_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse(TBBlurRadius.Text, out double blurRadius)
                && (blurRadius > 100
                || blurRadius <= 0))
                TBBlurRadius.Text = "1";
        }

        #endregion

        #region 选取水印图标

        private void BtSelectWaterMark_Click(object sender, RoutedEventArgs e)
        {
            if (Components.Tools.File.Pick("选择水印图标", out var path))
                if (Components.Tools.File.IsImage(path))
                    TBWaterMark.Text = path;
                else Components.Tools.MsgB.OkInfo("选择的文件不是图片，未使用。", "提示");
        }

        #endregion

        private void BtAddFile_Click(object sender, RoutedEventArgs e)
        {
            if (Components.Tools.File.MultiPick("选择要处理的图片文件", out var paths))
            {
                var imagePaths = Components.Tools.File.FilterImages(paths);
                foreach (var path in imagePaths)
                    _ = LBFiles.Items.Add(path);
            }
        }
    }
}
