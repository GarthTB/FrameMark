﻿using ImageMagick;
using ImageMagick.Drawing;
using System.IO;

namespace FrameMark.Core
{
    internal static class ImageHelpers
    {
        /// <summary> 生成模糊且变暗的背景图 </summary>
        internal static IMagickImage GenBkg(this MagickImage image, double frameT, double frameB, double frameL, double frameR, double blurRatio)
        {
            var bkg = image.Clone();
            var geometry = new MagickGeometry() { IgnoreAspectRatio = true };
            bkg.FilterType = FilterType.CubicSpline;
            if (blurRatio != 1)
            {
                geometry.Width = (uint)(blurRatio * image.Width);
                geometry.Height = (uint)(blurRatio * image.Height);
                if (geometry.Width * geometry.Height == 0)
                    geometry.Width = geometry.Height = 1;
                bkg.Resize(geometry);
            }

            geometry.Width = (uint)((frameL + frameR + 100) / 100 * image.Width);
            geometry.Height = (uint)((frameT + frameB + 100) / 100 * image.Height);
            bkg.Resize(geometry);

            bkg.Evaluate(Channels.All, EvaluateOperator.Multiply, 0.618);
            return bkg;
        }

        /// <summary> 给图像切去圆角 </summary>
        internal static void RoundCorner(MagickImage image, double rcRadius)
        {
            if (rcRadius == 0) return;
            var r = rcRadius / 100 * Math.Min(image.Width, image.Height);
            var mask = new MagickImage(new MagickColor(0, 0, 0, 0), image.Width, image.Height);
            mask.Settings.FillColor = MagickColors.White;
            mask.Draw(new DrawableRoundRectangle(0, 0, image.Width, image.Height, r, r));
            mask.GaussianBlur(0.5, 0.5); // 去锯齿
            image.Alpha(AlphaOption.Set);
            image.Composite(mask, CompositeOperator.CopyAlpha);
        }

        /// <summary> 收集照片信息 </summary>
        internal static string CollectInfo(MagickImage image, string shutter, string apertrue, string iso, string focalLen)
        {
            var exif = image.GetExifProfile();
            var s = Normalize(exif?.GetValue(ExifTag.ExposureTime)?.Value.ToString()) ?? shutter;
            var a = ConvertFraction(exif?.GetValue(ExifTag.FNumber)?.Value.ToString()) ?? apertrue;
            var i = exif?.GetValue(ExifTag.ISOSpeed)?.Value.ToString() ?? iso;
            var f = ConvertFraction(exif?.GetValue(ExifTag.FocalLengthIn35mmFilm)?.ToString())
                ?? ConvertFraction(exif?.GetValue(ExifTag.FocalLength)?.Value.ToString())
                ?? focalLen;

            List<string> info = new(4);
            if (s.Length > 0) info.Add($"{s} s");
            if (a.Length > 0) info.Add($"f/{a}");
            if (i.Length > 0) info.Add($"ISO {i}");
            if (f.Length > 0) info.Add($"{f} mm");
            return info.Aggregate((a, b) => $"{a} | {b}");

            static string? Normalize(string? value)
            {
                if (value == null) return null;
                var parts = value.Split('/');
                return parts.Length != 2
                    || parts[0] == "1"
                    || !uint.TryParse(parts[0], out var up)
                    || !uint.TryParse(parts[1], out var down)
                        ? value // 如果不为分数或者已通分，则原样返回
                        : up > down
                            ? $"{(double)up / down:#.#}"
                            : $"1/{(double)down / up:#.#}";
            }

            static string? ConvertFraction(string? value)
            {
                if (value == null) return null;
                var parts = value.Split('/');
                if (parts.Length != 2
                    || !uint.TryParse(parts[0], out var up)
                    || !uint.TryParse(parts[1], out var down))
                    return value;
                if (up > down) // 如果分子大，则直接插入小数点
                {
                    var dotIndex = parts[0].Length - parts[1].Length + 1;
                    return $"{parts[0][..dotIndex]}.{parts[0][dotIndex..]}";
                }
                else // 如果分母大，则按精度保留指定长度
                {
                    var result = (double)up / down;
                    return result.ToString()[..(parts[1].Length + 1)];
                }
            }
        }

        /// <summary> 在背景图上添加水印 </summary>
        internal static void AddWm(this IMagickImage bkg, MagickImage image, string wmPath, string text, double frameB)
        {
            // 字体像素高度为底框高度的0.24倍
            var fontPoint = (uint)(0.24 * frameB / 100 * image.Height);
            image.Settings.FontPointsize = fontPoint;
            var metrics = image.FontTypeMetrics(text);
            var textWidth = metrics?.TextWidth ?? throw new Exception("获取文本宽度失败");
            var textHeight = metrics?.TextHeight ?? throw new Exception("获取文本高度失败");

            // 水印高度与字体高度一致
            MagickImage? wm = null;
            if (wmPath.Length > 0)
            {
                wm = new MagickImage(wmPath);
                wm.Resize((uint)(wm.Width * textHeight / wm.Height), (uint)textHeight);
            }

            // 整体偏移
            var totalWidth = (wm?.Width ?? 0) + textHeight * 0.618 + textWidth;
            var xOffset = (bkg.Width - totalWidth) / 2;
            var yOffset = bkg.Height - frameB / 200 * image.Height - textHeight / 2;

            // 合成
            if (wm is not null)
                bkg.Composite(wm, (int)xOffset, (int)yOffset, CompositeOperator.Over);
            var drawables = new Drawables().FillColor(new MagickColor(62720, 62720, 62720))
                .FontPointSize(fontPoint)
                .StrokeColor(new MagickColor(60000, 60000, 60000))
                .StrokeWidth(fontPoint * 0.008)
                .Text(xOffset + (wm?.Width ?? 0) + textHeight * 0.618, yOffset + textHeight * 0.8, text);
            _ = drawables.Draw((IMagickImage<float>)bkg);
        }

        /// <summary> 前景与背景融合 </summary>
        internal static void Mix(this IMagickImage bkg, MagickImage fg, double frameT, double frameL)
        {
            var x = frameL / 100 * fg.Width;
            var y = frameT / 100 * fg.Height;
            bkg.Composite(fg, (int)x, (int)y, CompositeOperator.Over);
        }

        /// <summary> 输出为指定格式 </summary>
        internal static void Output(this IMagickImage image, string path, string format)
        {
            image.Quality = 100;
            image.Write(Reformat(image, format));

            string Reformat(IMagickImage image, string format)
            {
                switch (format)
                {
                    case "无损WEBP":
                        image.Format = MagickFormat.WebP;
                        return Rename("webp");
                    case "满质量JPG":
                        image.Format = MagickFormat.Jpeg;
                        return Rename("jpg");
                    case "PNG":
                        image.Format = MagickFormat.Png;
                        return Rename("png");
                    case "TIF":
                        image.Format = MagickFormat.Tiff;
                        return Rename("tif");
                    default:
                        throw new Exception("不支持的格式");
                }
            }

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
