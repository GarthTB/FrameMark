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
                || roundCorner < 0))
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
                else Components.Tools.MsgB.OkInfo("选择的文件不是支持的图片，未使用。", "提示");
        }

        #endregion

        #region 添加、选取和移除文件

        private void BtAddFile_Click(object sender, RoutedEventArgs e)
        {
            if (Components.Tools.File.MultiPick("选择要处理的图片文件", out var paths))
            {
                var imagePaths = Components.Tools.File.FilterImages(paths);
                if (imagePaths.Length == 0) return;
                foreach (var path in imagePaths)
                    _ = LBFiles.Items.Add(path);
                if (paths.Length > imagePaths.Length)
                    Components.Tools.MsgB.OkInfo($"选择的{paths.Length}个文件中包含{paths.Length - imagePaths.Length}个非图片文件，已忽略。", "提示");
                BtRun.IsEnabled = true;
            }
        }

        private void LBFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => BtRemoveFile.IsEnabled = LBFiles.SelectedItems.Count > 0;

        private void BtRemoveFile_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in LBFiles.SelectedItems)
                LBFiles.Items.Remove(item);
            BtRun.IsEnabled = LBFiles.Items.Count > 0;
        }

        #endregion

        #region 执行操作

        private void BtRun_Click(object sender, RoutedEventArgs e)
        {
            Components.ImageEditor editor = new(
                double.Parse(TBFrameT.Text),
                double.Parse(TBFrameB.Text),
                double.Parse(TBFrameL.Text),
                double.Parse(TBFrameR.Text),
                double.Parse(TBRoundCorner.Text),
                double.Parse(TBBlurRadius.Text),
                TBWaterMark.Text,
                TBShutter.Text,
                TBAperture.Text,
                TBISO.Text,
                TBFocalLen.Text,
                CBOutputType.Text ?? "WEBP",
                LBFiles.Items.Cast<string>().ToArray()
                );
            editor.Run();
            BtRun.IsEnabled = false;
        }

        #endregion
    }
}
