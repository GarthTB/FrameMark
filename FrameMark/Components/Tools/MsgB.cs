using System.Windows;

namespace FrameMark.Components.Tools
{
    /// <summary> 为简化代码，将MessageBox封装成一个类 </summary>
    internal static class MsgB
    {
        internal static void OkInfo(string info, string title)
            => _ = MessageBox.Show(info, title, MessageBoxButton.OK, MessageBoxImage.Information);

        internal static bool YNInfo(string info, string title)
            => MessageBox.Show(info, title, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes;

        internal static void OkErr(string info, string title)
            => _ = MessageBox.Show(info, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
