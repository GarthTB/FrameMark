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
                    var text = AggregateInfo(image, shutter, apertrue, iso, focalLen);
                    var bkg = GenerateBackground(image, frameT, frameB, frameL, frameR, blurRadius);
                    var fg = rcRadius == 0 ? image : GenerateForeground(image, rcRadius);
                    var bkgWithWM = GenerateWatermark(image, bkg, wmPath, text, frameB);
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

        /// <summary> 连接照片信息 </summary>
        private static string AggregateInfo(MagickImage image, string shutter, string apertrue, string iso, string focalLen)
        {
            var exif = image.GetExifProfile();
            var s = exif?.GetValue(ExifTag.ExposureTime)?.Value.ToString() ?? shutter;
            var a = ConvertFraction(exif?.GetValue(ExifTag.FNumber)?.Value.ToString()) ?? apertrue;
            var i = exif?.GetValue(ExifTag.ISOSpeed)?.Value.ToString() ?? iso;
            var f = exif?.GetValue(ExifTag.FocalLengthIn35mmFilm)?.ToString()
                ?? ConvertFraction(exif?.GetValue(ExifTag.FocalLength)?.Value.ToString())
                ?? focalLen;

            List<string> info = new(4);
            if (s.Length > 0) info.Add($"{s}s");
            if (a.Length > 0) info.Add($"f/{a}");
            if (i.Length > 0) info.Add($"ISO {i}");
            if (f.Length > 0) info.Add($"{f}mm");
            return info.Aggregate((a, b) => $"{a} | {b}");

            static string? ConvertFraction(string? value)
            {
                if (value == null) return null;
                var parts = value.Split('/');
                if (parts.Length != 2
                    || !uint.TryParse(parts[0], out var up)
                    || !uint.TryParse(parts[1], out var down)) return value;
                if (up > down)
                {
                    var accuracy = parts[1].Length - 1;
                    var upLen = parts[0].Length;
                    var dotIndex = upLen - accuracy;
                    return $"{parts[0][..dotIndex]}.{parts[0][dotIndex..]}";
                }
                else
                {
                    var result = (double)up / down;
                    return result.ToString()[..(parts[1].Length + 1)];
                }
            }
        }

        /// <summary> 生成模糊且变暗的背景图 </summary>
        private static IMagickImage GenerateBackground(MagickImage image, double frameT, double frameB, double frameL, double frameR, double blurRadius)
        {
            var bkg = image.Clone();
            var newW = (frameL + frameR + 100) / 100 * image.Width;
            var newH = (frameT + frameB + 100) / 100 * image.Height;
            var geometry = new MagickGeometry((uint)newW, (uint)newH)
            {
                IgnoreAspectRatio = true
            };
            bkg.Resize(geometry);

            var longSide = Math.Max(image.Width, image.Height);
            var radius = blurRadius / 100 * longSide;
            bkg.Blur(radius, radius * 0.618);

            bkg.Evaluate(Channels.All, EvaluateOperator.Multiply, 0.618);
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
            mask.Blur(1, 1); // 去锯齿
            image.Alpha(AlphaOption.Set);
            image.Composite(mask, CompositeOperator.CopyAlpha);
            return image;
        }

        /// <summary> 在背景图上添加水印 </summary>
        private static IMagickImage GenerateWatermark(MagickImage image, IMagickImage bkg, string wmPath, string text, double frameB)
        {
            // 字体像素高度为底框高度的0.24倍
            var fontPoint = (uint)(0.24 * frameB / 100 * image.Height);
            image.Settings.FontPointsize = fontPoint;
            var metrics = image.FontTypeMetrics(text);
            var textWidth = metrics?.TextWidth ?? throw new Exception("获取文本宽度失败");
            var textHeight = metrics?.TextHeight ?? throw new Exception("获取文本高度失败");

            // 水印高度与字体高度一致
            var wmWidth = 0u;
            MagickImage? wm = null;
            if (wmPath.Length > 0)
            {
                wm = new MagickImage(wmPath);
                wm.Resize((uint)(wm.Width * textHeight / wm.Height), (uint)textHeight);
                wmWidth = wm.Width;
            }

            // 整体偏移
            var totalWidth = wmWidth + textHeight * 0.618 + textWidth;
            var xOffset = (bkg.Width - totalWidth) / 2;
            var yOffset = bkg.Height - frameB / 200 * image.Height - textHeight / 2;

            // 合成
            if (wm is not null)
                bkg.Composite(wm, (int)xOffset, (int)yOffset, CompositeOperator.Over);
            var drawables = new Drawables().FillColor(new MagickColor(62720, 62720, 62720))
                .FontPointSize(fontPoint)
                .StrokeColor(new MagickColor(2560, 2560, 2560))
                .StrokeWidth(fontPoint * 0.025)
                .Text(xOffset + wmWidth + textHeight * 0.618, yOffset + textHeight * 0.8, text);
            _ = drawables.Draw((IMagickImage<float>)bkg);
            return bkg;
        }

        /// <summary> 前景与背景融合 </summary>
        private static IMagickImage Combine(IMagickImage bkg, MagickImage fg, double frameT, double frameL)
        {
            var xOffset = frameL / 100 * fg.Width;
            var yOffset = frameT / 100 * fg.Height;
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
