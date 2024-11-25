using System.Reflection;
using System.Text;

namespace FrameMark.Components
{
    internal static class Help
    {
        internal static void Show()
        {
            var version = Assembly.GetExecutingAssembly()
                                  .GetName()
                                  .Version?
                                  .ToString()
                                  ?? "未知";
            var sb = new StringBuilder();
            var help = sb.AppendLine("欢迎使用图片加边框水印工具！")
                         .AppendLine("本程序会对照片进行以下操作：")
                         .AppendLine("1. 按需要放大、模糊，并压暗，作为边框。\n  模糊半径为图像长边的指定百分比。")
                         .AppendLine("2. 将图片本身切去圆角，放在其上。\n  圆角半径为图像短边的指定百分比。")
                         .AppendLine("3. 在下边框列出图标，以及快门、光圈、ISO、\n  焦距等信息。若无法获取，则填入缺省值。")
                         .AppendLine("4. 转存为指定的格式。")
                         .AppendLine("详见README.md。\n")
                         .AppendLine($"版本号：{version}")
                         .AppendLine("作者：GarthTB\n")
                         .ToString();
            Tools.MsgB.OkInfo(help, "帮助");
        }
    }
}
