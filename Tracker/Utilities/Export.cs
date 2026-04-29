using ClosedXML.Excel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;


namespace TimeTracker.Utilities
{
    public class Export<T>
    {
        public void InvokeExport(List<T> listToExport, ICollection<string[]> columnHeaders, string title)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "xlsx Files | *.xlsx";
            saveFileDialog.Title = "Save As";
            var dialogResult= saveFileDialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                ExportToExcel(listToExport, columnHeaders, title, saveFileDialog.FileName);
            }
        }

        public IQueryable<T> ListToExport { get; set; }

        #region Constructor
        public Export()
        {
        }
        #endregion

        private void ExportToExcel(List<T> listToExport, ICollection<string[]> columnHeaders, string reportTitle, string fileName)
        {
            using (var wb = new XLWorkbook()) //create workbook
            {
                var ws = wb.Worksheets.Add(reportTitle); //add worksheet to workbook

                var rangeTitle = ws.Cell(1, 1).InsertData(columnHeaders); //insert titles to first row
                rangeTitle.AddToNamed("Titles");
                var titlesStyle = wb.Style;
                titlesStyle.Font.Bold = true; //font must be bold

                wb.NamedRanges.NamedRange("Titles").Ranges.Style = titlesStyle; //attach style to the range

                if (listToExport != null && listToExport.Count() > 0)
                {
                    //insert data to from second row on
                    ws.Cell(2, 1).InsertData(listToExport);
                    ws.Columns().AdjustToContents();
                }
                //save file directly to the destination
                wb.SaveAs(fileName);
            }
        }
    }
}
