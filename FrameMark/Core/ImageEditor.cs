using ImageMagick;
using System.IO;

namespace FrameMark.Core
{
    internal class ImageEditor(
        double frameT,
        double frameB,
        double frameL,
        double frameR,
        double rcRadius,
        double blurRatio,
        string wmPath,
        string shutter,
        string apertrue,
        string iso,
        string focalLen,
        string outType,
        string[] filePaths)
    {
        internal void Run()
        {
            try
            {
                var successCount = 0;
                _ = Parallel.ForEach(filePaths, path =>
                {
                    if (!File.Exists(path))
                        return;
                    using var image = new MagickImage(path);
                    var info = ImageHelpers.CollectInfo(image, shutter, apertrue, iso, focalLen);
                    var bkg = ImageHelpers.GenBkg(image, frameT, frameB, frameL, frameR, blurRatio);
                    var fg = rcRadius == 0 ? image : ImageHelpers.GenFg(image, rcRadius);
                    var bkgWithWm = ImageHelpers.AddWm(image, bkg, wmPath, info, frameB);
                    var mixed = ImageHelpers.Mix(bkgWithWm, fg, frameT, frameL);
                    ImageHelpers.Output(mixed, path, outType);
                    _ = Interlocked.Increment(ref successCount);
                });
                if (successCount == filePaths.Length)
                    Tools.MsgB.OkInfo($"{filePaths.Length}张图片全部处理完成", "提示");
                else Tools.MsgB.OkInfo($"{successCount}张图片处理完成，{filePaths.Length - successCount}张图片未处理", "提示");
            }
            catch (Exception e)
            {
                Tools.MsgB.OkErr($"运行出错：{e.Message}", "异常中止");
            }
        }
    }
}
