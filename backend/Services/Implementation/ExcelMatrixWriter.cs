using backend.Models;
using ClosedXML.Excel;

namespace backend.Services.Implementation
{
    public static class ExcelMatrixWriter
    {
        private const double CellWidth = 3.0;
        private const double CellHeight = 18.0;

        public static byte[] Write(string workName, int width, int height, IReadOnlyCollection<PatternCell> cells)
        {
            using var workbook = new XLWorkbook();

            WriteMatrix(workbook.Worksheets.Add("Matrix"), width, height, cells);
            WriteLegend(workbook.Worksheets.Add("Legend"), workName, width, height, cells);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static void WriteMatrix(IXLWorksheet sheet, int width, int height, IReadOnlyCollection<PatternCell> cells)
        {
            sheet.Columns(1, width).Width = CellWidth;
            sheet.Rows(1, height).Height = CellHeight;

            var grid = sheet.Range(1, 1, height, width);
            grid.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            grid.Style.Border.OutsideBorderColor = XLColor.LightGray;
            grid.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            grid.Style.Border.InsideBorderColor = XLColor.LightGray;

            foreach (var cell in cells)
            {
                sheet.Cell(cell.RowIndex + 1, cell.ColumnIndex + 1).Style.Fill.BackgroundColor =
                    XLColor.FromHtml(cell.YarnColor.HexValue);
            }
        }

        private static void WriteLegend(
            IXLWorksheet sheet,
            string workName,
            int width,
            int height,
            IReadOnlyCollection<PatternCell> cells)
        {
            sheet.Cell(1, 1).Value = "Work";
            sheet.Cell(1, 2).Value = workName;
            sheet.Cell(2, 1).Value = "Width";
            sheet.Cell(2, 2).Value = width;
            sheet.Cell(3, 1).Value = "Height";
            sheet.Cell(3, 2).Value = height;
            sheet.Range(1, 1, 3, 1).Style.Font.Bold = true;

            sheet.Cell(5, 1).Value = "Color";
            sheet.Cell(5, 2).Value = "Code";
            sheet.Cell(5, 3).Value = "Name";
            sheet.Cell(5, 4).Value = "Cells";
            sheet.Range(5, 1, 5, 4).Style.Font.Bold = true;

            var row = 6;
            var groups = cells
                .GroupBy(c => c.YarnColorId)
                .Select(g => (Color: g.First().YarnColor, Count: g.Count()))
                .OrderByDescending(g => g.Count);

            foreach (var (color, count) in groups)
            {
                sheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml(color.HexValue);
                sheet.Cell(row, 2).Value = color.HexValue;
                sheet.Cell(row, 3).Value = color.Name;
                sheet.Cell(row, 4).Value = count;
                row++;
            }

            sheet.Column(1).Width = 10;
            sheet.Column(2).AdjustToContents();
            sheet.Column(3).AdjustToContents();
            sheet.Column(4).AdjustToContents();
        }
    }
}
