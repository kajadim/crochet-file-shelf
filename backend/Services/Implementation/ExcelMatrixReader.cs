using backend.Exceptions;
using ClosedXML.Excel;

namespace backend.Services.Implementation
{
    public sealed record ExcelCell(int Row, int Column, string Hex);

    public sealed record ExcelWarning(string Cell, string Reason);

    public sealed record ExcelMatrix(
        int Width,
        int Height,
        List<ExcelCell> Cells,
        List<ExcelWarning> Warnings,
        int SkippedCells);

    public static class ExcelMatrixReader
    {
        public const int MaxSize = 200;
        private const int MaxWarnings = 50;

        public static ExcelMatrix Read(Stream stream)
        {
            XLWorkbook workbook;
            try
            {
                workbook = new XLWorkbook(stream);
            }
            catch (Exception)
            {
                throw new BadRequestException(ErrorCode.ExcelFileInvalid);
            }

            using (workbook)
            {
                var sheet = workbook.Worksheets.FirstOrDefault()
                    ?? throw new BadRequestException(ErrorCode.ExcelFileInvalid);

                return ReadSheet(sheet, workbook.Theme);
            }
        }

        private static ExcelMatrix ReadSheet(IXLWorksheet sheet, IXLTheme theme)
        {
            var cells = new List<ExcelCell>();
            var warnings = new List<ExcelWarning>();
            var skipped = 0;
            var maxRow = 0;
            var maxColumn = 0;

            foreach (var cell in sheet.CellsUsed(XLCellsUsedOptions.All))
            {
                var fill = cell.Style.Fill;
                if (fill.PatternType == XLFillPatternValues.None)
                {
                    continue;
                }

                var address = cell.Address;
                if (address.RowNumber > MaxSize * 5 || address.ColumnNumber > MaxSize * 5)
                {
                    continue;
                }

                var hex = fill.PatternType == XLFillPatternValues.Solid ? ResolveHex(fill.BackgroundColor, theme) : null;
                if (hex is null)
                {
                    skipped++;
                    if (warnings.Count < MaxWarnings)
                    {
                        var reason = fill.PatternType == XLFillPatternValues.Solid ? "unresolvedColor" : "unsupportedFill";
                        warnings.Add(new ExcelWarning(address.ToStringRelative(false), reason));
                    }
                    continue;
                }

                if (hex == "#FFFFFF")
                {
                    continue;
                }

                cells.Add(new ExcelCell(address.RowNumber - 1, address.ColumnNumber - 1, hex));
                maxRow = Math.Max(maxRow, address.RowNumber);
                maxColumn = Math.Max(maxColumn, address.ColumnNumber);
            }

            if (cells.Count == 0)
            {
                throw new BadRequestException(ErrorCode.ExcelEmptyMatrix);
            }

            if (maxRow > MaxSize || maxColumn > MaxSize)
            {
                throw new BadRequestException(ErrorCode.ExcelMatrixTooLarge);
            }

            var lastUsed = sheet.LastCellUsed(XLCellsUsedOptions.All);
            var width = maxColumn;
            var height = maxRow;
            if (lastUsed is not null
                && lastUsed.Address.RowNumber <= MaxSize
                && lastUsed.Address.ColumnNumber <= MaxSize)
            {
                width = Math.Max(width, lastUsed.Address.ColumnNumber);
                height = Math.Max(height, lastUsed.Address.RowNumber);
            }

            return new ExcelMatrix(width, height, cells, warnings, skipped);
        }

        private static string? ResolveHex(XLColor color, IXLTheme theme)
        {
            try
            {
                if (!color.HasValue)
                {
                    return null;
                }

                var rgb = color.ColorType == XLColorType.Theme ? ResolveTheme(color, theme) : color.Color;
                if (rgb.A == 0)
                {
                    return null;
                }

                return $"#{rgb.R:X2}{rgb.G:X2}{rgb.B:X2}";
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static System.Drawing.Color ResolveTheme(XLColor color, IXLTheme theme)
        {
            var source = color.ThemeColor switch
            {
                XLThemeColor.Background1 => theme.Background1,
                XLThemeColor.Text1 => theme.Text1,
                XLThemeColor.Background2 => theme.Background2,
                XLThemeColor.Text2 => theme.Text2,
                XLThemeColor.Accent1 => theme.Accent1,
                XLThemeColor.Accent2 => theme.Accent2,
                XLThemeColor.Accent3 => theme.Accent3,
                XLThemeColor.Accent4 => theme.Accent4,
                XLThemeColor.Accent5 => theme.Accent5,
                XLThemeColor.Accent6 => theme.Accent6,
                _ => throw new InvalidOperationException("Unsupported theme color"),
            };

            return ApplyTint(source.Color, color.ThemeTint);
        }

        private static System.Drawing.Color ApplyTint(System.Drawing.Color source, double tint)
        {
            if (tint == 0)
            {
                return source;
            }

            var hue = source.GetHue() / 360.0;
            double saturation = source.GetSaturation();
            double lightness = source.GetBrightness();

            lightness = tint < 0 ? lightness * (1 + tint) : lightness * (1 - tint) + tint;
            lightness = Math.Clamp(lightness, 0, 1);

            return FromHsl(hue, saturation, lightness);
        }

        private static System.Drawing.Color FromHsl(double hue, double saturation, double lightness)
        {
            if (saturation == 0)
            {
                var gray = (int)Math.Round(lightness * 255);
                return System.Drawing.Color.FromArgb(gray, gray, gray);
            }

            var q = lightness < 0.5 ? lightness * (1 + saturation) : lightness + saturation - lightness * saturation;
            var p = 2 * lightness - q;

            return System.Drawing.Color.FromArgb(
                (int)Math.Round(Channel(p, q, hue + 1.0 / 3) * 255),
                (int)Math.Round(Channel(p, q, hue) * 255),
                (int)Math.Round(Channel(p, q, hue - 1.0 / 3) * 255));
        }

        private static double Channel(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2) return q;
            if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
            return p;
        }
    }
}
