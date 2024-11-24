using ImageMagick;
using ImageMagick.Drawing;
using System.IO;

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
                    var f = exif?.GetValue(ExifTag.FocalLengthIn35mmFilm)?.Value.ToString() ?? focalLen;
                    var bkg = GenerateBackground(image, frameT, frameB, frameL, frameR, blurRadius);
                    var fg = rcRadius == 0 ? image : GenerateForeground(image, rcRadius);
                    var text = AggregateInfo(s, a, i, f);
                    var bkgWithWM = GenerateWatermark(bkg, wmPath, text, frameB);
                    var combined = Combine(bkgWithWM, fg, frameT, frameL);
                    Output(combined, path, outType);
                });
                Tools.MsgB.OkInfo($"{filePaths.Length}张图片全部处理完成", "提示");
            }
            catch (Exception e)
            {
                Tools.MsgB.OkErr($"运行出错：{e.Message}", "异常中止");
            }
        }

        /// <summary> 生成高斯模糊的背景图 </summary>
        private static IMagickImage GenerateBackground(MagickImage image, double frameT, double frameB, double frameL, double frameR, double blurRadius)
        {
            var bkg = image.Clone();
            var newW = (frameL + frameR + 100) / 100 * image.Width;
            var newH = (frameT + frameB + 100) / 100 * image.Height;
            bkg.Resize((uint)newW, (uint)newH);

            var longSide = Math.Max(image.Width, image.Height);
            bkg.GaussianBlur(blurRadius / 100 * longSide);
            return bkg;
        }

        /// <summary> 生成切去圆角的前景图 </summary>
        private static MagickImage GenerateForeground(MagickImage image, double rcRadius)
        {
            var shortSide = Math.Min(image.Width, image.Height);
            var radius = rcRadius / 100 * shortSide;
            var mask = new MagickImage(new MagickColor(0, 0, 0, 0), image.Width, image.Height);
            mask.Settings.FillColor = MagickColors.White;
            mask.Draw(new DrawableRoundRectangle(0, 0, image.Width, image.Height, radius, radius));
            image.Alpha(AlphaOption.Set);
            image.Composite(mask, CompositeOperator.CopyAlpha);
            return image;
        }

        /// <summary> 连接照片信息 </summary>
        private static string AggregateInfo(string s, string a, string i, string f)
        {
            List<string> info = new(4);
            if (s.Length > 0) info.Add($"{s}s");
            if (a.Length > 0) info.Add($"f/{a}");
            if (i.Length > 0) info.Add($"ISO {i}");
            if (f.Length > 0) info.Add($"{f}mm");
            return info.Aggregate((a, b) => $"{a} | {b}");
        }

        /// <summary> 在背景图上添加水印 </summary>
        private static IMagickImage GenerateWatermark(IMagickImage bkg, string wmPath, string text, double frameB)
        {
            var fontHeightPx = (uint)(0.382 * frameB / 100 * bkg.Height);
            using var wm = new MagickImage(wmPath);
            wm.Resize(wm.Width * fontHeightPx / wm.Height, fontHeightPx);

            var drawables = new Drawables().Font("JetBrains Mono")
                                           .FontPointSize(fontHeightPx)
                                           .FillColor(MagickColors.White);
            var metrics = bkg.FontTypeMetrics(text) ?? throw new Exception("获取文本宽度失败");
            var totalWidth = wm.Width + 2 * fontHeightPx + Math.Ceiling(metrics.TextWidth);
            var xOffset = (bkg.Width - totalWidth) / 2; // 水平居中
            var yOffset = (100 - frameB / 100) * bkg.Height - fontHeightPx / 2; // 垂直居中

            bkg.Composite(wm, (int)xOffset, (int)yOffset, CompositeOperator.Over);
            _ = drawables.TextAlignment(TextAlignment.Left)
                         .Text(xOffset + wm.Width + 2 * fontHeightPx, yOffset, text)
                         .Draw((IMagickImage<float>)bkg);
            return bkg;
        }

        /// <summary> 前景与背景融合 </summary>
        private static IMagickImage Combine(IMagickImage bkg, MagickImage fg, double frameT, double frameL)
        {
            var xOffset = frameL / 100 * bkg.Width;
            var yOffset = frameT / 100 * bkg.Height;
            bkg.Composite(fg, (int)xOffset, (int)yOffset, CompositeOperator.Over);
            return bkg;
        }

        /// <summary> 输出为指定格式 </summary>
        private static void Output(IMagickImage image, string path, string format)
        {
            image.Quality = 100;
            string newPath;
            switch (format)
            {
                case "WEBP":
                    newPath = Rename("webp");
                    image.Format = MagickFormat.WebP;
                    break;
                case "TIFF":
                    newPath = Rename("tif");
                    image.Format = MagickFormat.Tiff;
                    break;
                case "PNG":
                    newPath = Rename("png");
                    image.Format = MagickFormat.Png;
                    break;
                case "满质量JPG":
                    newPath = Rename("jpg");
                    image.Format = MagickFormat.Jpeg;
                    break;
                default:
                    throw new Exception("不支持的格式");
            }
            image.Write(newPath);

            string Rename(string ext)
            {
                FileInfo fi = new(path);
                var dir = fi.Directory?.FullName ?? throw new Exception("获取目录失败");
                var name = Path.GetFileNameWithoutExtension(path);
                var newName = $"{name}_水印.{ext}";
                var newPath = Path.Combine(dir, newName);
                for (int i = 2; File.Exists(newPath); i++)
                {
                    newName = $"{name}_水印({i}).{ext}";
                    newPath = Path.Combine(dir, newName);
                }
                return newPath;
            }
        }
    }
}
