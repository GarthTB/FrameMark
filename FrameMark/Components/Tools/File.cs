using Microsoft.Win32;

namespace FrameMark.Components.Tools
{
    /// <summary> 涉及文件的操作 </summary>
    internal static class File
    {
        /// <summary> 选择一个指定类型的文件 </summary>
        /// <param name="filter"> 文件类型 </param>
        /// <param name="path"> 文件路径 </param>
        /// <returns> 是否成功 </returns>
        internal static bool Pick(string filter, out string path)
        {
            OpenFileDialog ofd = new()
            {
                Filter = filter,
                Multiselect = false,
                Title = "选择文件",
            };

            path = ofd.ShowDialog() == true ? ofd.FileName : "";

            return path.Length > 0;
        }
    }
}
