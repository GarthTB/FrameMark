using System.Windows;

namespace FrameMark.Components.Tools
{
    /// <summary> 为简化代码，将MessageBox封装成一个类 </summary>
    internal static class MsgB
    {
        /// <summary> 显示一个带有OK按键的MessageBox </summary>
        internal static void OkInfo(string info, string title)
            => _ = MessageBox.Show(info, title, MessageBoxButton.OK, MessageBoxImage.Information);

        /// <summary> 显示一个带有Yes和No按键的MessageBox，并返回选取结果 </summary>
        internal static bool YNInfo(string info, string title)
            => MessageBox.Show(info, title, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes;

        /// <summary> 显示一个带有OK按键和错误图标的MessageBox </summary>
        internal static void OkErr(string info, string title)
            => _ = MessageBox.Show(info, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
