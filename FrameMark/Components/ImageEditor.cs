using ImageMagick;

namespace FrameMark.Components
{
    internal class ImageEditor(double frameT, double frameB, double frameL, double frameR,
                               double rcRadius, double blurRadius, string wmPath, string shutter,
                               string apertrue, string iso, string focalLen, string outType, string[] filePaths)
    {
        internal void Run()
        {
            try
            {
                _ = Parallel.ForEach(filePaths, path =>
                {
                    using var image = new MagickImage(path);
                    var exif = image.GetExifProfile();
                    var s = exif?.GetValue(ExifTag.ExposureTime)?.Value.ToString() ?? shutter;
                    var a = exif?.GetValue(ExifTag.FNumber)?.Value.ToString() ?? apertrue;
                    var i = exif?.GetValue(ExifTag.ISOSpeedRatings)?.Value.ToString() ?? iso;
                    var f = exif?.GetValue(ExifTag.FocalLength)?.Value.ToString() ?? focalLen;
                    var bkg = GenerateBackground(image, frameT, frameB, frameL, frameR, blurRadius);
                    var fg = GenerateForeground(image, rcRadius);
                    var wm = GenerateWatermark(wmPath, s, a, i, f, frameB, image.Height);
                    var combined = Combine(bkg, fg, wm);
                    Output(combined, path, outType);
                });
            }
            catch (Exception e)
            {
                Tools.MsgB.OkErr($"运行出错：{e.Message}", "异常中止");
            }
        }
    }
}
