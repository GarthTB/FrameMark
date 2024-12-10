using ImageMagick;
using Microsoft.Win32;

namespace FrameMark.Core.Tools
{
    /// <summary> 涉及文件的操作 </summary>
    internal static class File
    {
        /// <summary> 选择一个指定类型的文件 </summary>
        /// <param name="title"> 标题 </param>
        /// <param name="path"> 文件路径 </param>
        /// <returns> 是否成功 </returns>
        internal static bool Pick(string title, out string path)
        {
            OpenFileDialog ofd = new()
            {
                Multiselect = false,
                Title = title,
            };
            path = ofd.ShowDialog() == true ? ofd.FileName : "";
            return path.Length > 0;
        }

        /// <summary> 选择多个指定类型的文件 </summary>
        /// <param name="title"> 标题 </param>
        /// <param name="paths"> 所有文件路径 </param>
        /// <returns> 是否成功 </returns>
        internal static bool MultiPick(string title, out string[] paths)
        {
            OpenFileDialog ofd = new()
            {
                Multiselect = true,
                Title = title,
            };
            paths = ofd.ShowDialog() == true ? ofd.FileNames : [];
            return paths.Length > 0;
        }

        /// <summary> 试加载以判断一个文件是否为图片 </summary>
        /// <param name="path"> 文件路径 </param>
        /// <returns> 是否为图片 </returns>
        internal static bool IsImage(string path)
        {
            try { return new MagickImage(path).Width > 0; }
            catch { return false; }
            finally { GC.Collect(); }
        }

        /// <summary> 过滤掉所有不是图片的文件 </summary>
        /// <param name="paths"> 文件路径 </param>
        /// <returns> 所有有效的图片文件路径 </returns>
        internal static string[] FilterImages(string[] paths)
            => [.. paths.AsParallel().Where(IsImage)];
    }
}
