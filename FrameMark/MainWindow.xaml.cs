using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FrameMark
{
    /// <summary> 主窗口的交互逻辑 </summary>
    public partial class MainWindow : Window
    {
        #region 加载、帮助和快捷键

        public MainWindow() => InitializeComponent();

        private void MW_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F1)
                ShowHelp();
        }

        private static void ShowHelp()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
            Core.Tools.MsgB.OkInfo(new StringBuilder()
                .AppendLine("欢迎使用图片边框与水印工具！\n")
                .AppendLine("本程序会对照片进行以下操作：")
                .AppendLine("1. 按需要放大、模糊，并压暗，作为边框。")
                .AppendLine("2. 将图片本身切去圆角，放在其上。")
                .AppendLine("3. 在下边框列出图标，以及快门、光圈、ISO、")
                .AppendLine("   焦距。若无法自动获取，则填入缺省值。")
                .AppendLine("4. 转存为指定的格式。")
                .AppendLine("详见README.md。\n")
                .AppendLine($"版本号：{version ?? "未知"}")
                .AppendLine("作者：GarthTB\n")
                .ToString(), "帮助");
        }

        #endregion

        #region 边框设置

        private void TBFrameT_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!double.TryParse(TBFrameT.Text, out double frameT)
                || frameT < 0)
                TBFrameT.Text = "12";
        }

        private void TBFrameB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!double.TryParse(TBFrameB.Text, out double frameB)
                || frameB <= 0)
                TBFrameB.Text = "18";
        }

        private void TBFrameL_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!double.TryParse(TBFrameL.Text, out double frameL)
                || frameL < 0)
                TBFrameL.Text = "12";
        }

        private void TBFrameR_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!double.TryParse(TBFrameR.Text, out double frameR)
                || frameR < 0)
                TBFrameR.Text = "12";
        }

        #endregion

        #region 圆角设置

        private void TBRoundCorner_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!double.TryParse(TBRoundCorner.Text, out double roundCorner)
                || roundCorner > 100
                || roundCorner < 0)
                TBRoundCorner.Text = "18";
        }

        #endregion

        #region 模糊程度设置

        private void TBBlurRatio_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!double.TryParse(TBBlurRatio.Text, out double blurRatio)
                || blurRatio > 1
                || blurRatio <= 0)
                TBBlurRatio.Text = "0.01";
        }

        #endregion

        #region 选取水印图标

        private void BtSelectWaterMark_Click(object sender, RoutedEventArgs e)
        {
            if (Core.Tools.File.Pick("选择水印图标", out var path))
                if (Core.Tools.File.IsImage(path))
                    TBWaterMark.Text = path;
                else Core.Tools.MsgB.OkInfo("选择的文件不是支持的图片，未使用。", "提示");
        }

        #endregion

        #region 添加、选取和移除文件

        private void BtAddFile_Click(object sender, RoutedEventArgs e)
        {
            if (Core.Tools.File.MultiPick("选择要处理的图片文件", out var paths))
                AddFiles2List(paths);
        }

        private void LBFiles_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                AddFiles2List((string[])e.Data.GetData(DataFormats.FileDrop));
        }

        private void AddFiles2List(string[] paths)
        {
            var imagePaths = Core.Tools.File.FilterImages(paths);
            foreach (var path in imagePaths)
                _ = LBFiles.Items.Add(path);
            if (paths.Length > imagePaths.Length)
                Core.Tools.MsgB.OkInfo($"选择的{paths.Length}个文件中包含{paths.Length - imagePaths.Length}个非图片文件，已忽略。", "提示");
            BtRun.IsEnabled = true;
        }

        private void LBFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
            => BtRemoveFile.IsEnabled = LBFiles.SelectedItems.Count > 0;

        private void BtRemoveFile_Click(object sender, RoutedEventArgs e)
        {
            var items = LBFiles.SelectedItems.Cast<string>().ToArray();
            foreach (var item in items)
                LBFiles.Items.Remove(item);
            BtRun.IsEnabled = LBFiles.Items.Count > 0;
        }

        #endregion

        #region 执行操作

        private void BtRun_Click(object sender, RoutedEventArgs e)
        {
            MW.Title = "边框与水印工具  处理中，请勿关闭...";
            new Core.ImageEditor(
                double.Parse(TBFrameT.Text),
                double.Parse(TBFrameB.Text),
                double.Parse(TBFrameL.Text),
                double.Parse(TBFrameR.Text),
                double.Parse(TBRoundCorner.Text),
                double.Parse(TBBlurRatio.Text),
                TBWaterMark.Text,
                TBShutter.Text,
                TBAperture.Text,
                TBISO.Text,
                TBFocalLen.Text,
                CBOutputType.Text ?? "无损WEBP",
                [.. LBFiles.Items.Cast<string>()]
                ).Run();
            MW.Title = "边框与水印工具";
        }

        #endregion
    }
}
