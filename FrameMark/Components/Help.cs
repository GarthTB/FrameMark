using System.Reflection;
using System.Text;

namespace FrameMark.Components
{
    internal static class Help
    {
        public static void Show()
        {
            var version = Assembly.GetExecutingAssembly()
                                  .GetName()
                                  .Version?
                                  .ToString()
                                  ?? "未知";
            var sb = new StringBuilder();
            var help = sb.AppendLine("欢迎使用图片加边框水印工具！")
                         .AppendLine("本程序会对照片进行以下操作：")
                         .AppendLine("1. 放大为适合的大小，进行高斯模糊。其半径为图像长边的指定百分比，作为背景。")
                         .AppendLine("2. 将图片本身置于背景上，并改为圆角，圆角的半径为图像短边的指定百分比。")
                         .AppendLine("3. 在背景的下框写上图像的快门、光圈、ISO、焦距信息。若无法获取，则填入缺省值。")
                         .AppendLine("4. 转存为指定的格式。")
                         .AppendLine("详见README.md。")
                         .AppendLine($"版本号：{version}")
                         .AppendLine("作者：GarthTB\n")
                         .ToString();
            Tools.MsgB.OkInfo(help, "帮助");
        }
    }
}
