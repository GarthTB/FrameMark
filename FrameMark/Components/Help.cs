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
            var help = sb.AppendLine("欢迎使用图片边框与水印工具！\n")
                         .AppendLine("本程序会对照片进行以下操作：")
                         .AppendLine("1. 按需要放大、模糊，并压暗，作为边框。")
                         .AppendLine("2. 将图片本身切去圆角，放在其上。")
                         .AppendLine("3. 在下边框列出图标，以及快门、光圈、ISO、\n  焦距。若无法自动获取，则填入缺省值。")
                         .AppendLine("4. 转存为指定的格式。")
                         .AppendLine("详见README.md。\n")
                         .AppendLine($"版本号：{version}")
                         .AppendLine("作者：GarthTB\n")
                         .ToString();
            Tools.MsgB.OkInfo(help, "帮助");
        }
    }
}
