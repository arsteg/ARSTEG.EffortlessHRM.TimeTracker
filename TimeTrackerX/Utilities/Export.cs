using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using ClosedXML.Excel;

namespace TimeTrackerX.Utilities
{
    public class Export<T>
    {
        public async Task InvokeExport(
            List<T> listToExport,
            ICollection<string[]> columnHeaders,
            string title,
            Window parentWindow
        )
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter { Name = "Excel Files", Extensions = { "xlsx" } }
                },
                DefaultExtension = "xlsx",
                Title = "Save As"
            };

            var fileName = await saveFileDialog.ShowAsync(parentWindow);
            if (!string.IsNullOrEmpty(fileName))
            {
                ExportToExcel(listToExport, columnHeaders, title, fileName);
            }
        }

        public IQueryable<T> ListToExport { get; set; }

        #region Constructor
        public Export() { }
        #endregion

        private void CopyStream(Stream stream, string destPath)
        {
            using (var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }

        private void ExportToExcel(
            List<T> listToExport,
            ICollection<string[]> columnHeaders,
            string reportTitle,
            string fileName
        )
        {
            var wb = new XLWorkbook(); // Create workbook
            var ws = wb.Worksheets.Add(reportTitle); // Add worksheet to workbook

            var rangeTitle = ws.Cell(1, 1).InsertData(columnHeaders); // Insert titles to first row
            rangeTitle.AddToNamed("Titles");
            var titlesStyle = wb.Style;
            titlesStyle.Font.Bold = true; // Font must be bold

            wb.NamedRanges.NamedRange("Titles").Ranges.Style = titlesStyle; // Attach style to the range

            if (listToExport != null && listToExport.Any())
            {
                // Insert data from second row on
                ws.Cell(2, 1).InsertData(listToExport);
                ws.Columns().AdjustToContents();
            }

            // Save file to memory stream and write to disk
            using (var ms = new MemoryStream())
            {
                wb.SaveAs(ms);
                CopyStream(new MemoryStream(ms.ToArray()), fileName);
            }
        }
    }
}
