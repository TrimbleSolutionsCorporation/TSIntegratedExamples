namespace SpreadsheetReinforcement.Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using Data;
    using Excel;

    /// <summary>
    /// Service class to read spreadsheet and parse values
    /// </summary>
    public static class SpreadsheetReaderService
    {
        private const string HeaderCommentRowKeyWord = "Mark";

        /// <summary>
        /// Opens spreadsheet and gets values from cells
        /// </summary>
        /// <param name="file">File to read in</param>
        /// <param name="options">Settings</param>
        /// <returns></returns>
        public static ObservableCollection<SpreadsheetResultData> ReadFile(FileInfo file, SavedSetting options)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (options == null) throw new ArgumentNullException(nameof(options));

            var result = new ObservableCollection<SpreadsheetResultData>();
            Application excel = null;
            Workbook wkb = null;

            try
            {
                excel = new Application();
                wkb = OpenBook(excel, file.FullName, false, true, false);
                var sheet = wkb.Sheets.Item[1] as Worksheet;
                if (sheet == null) return result;

                var range = sheet.UsedRange;
                var rowCount = range.Rows.Count;
                var colCount = range.Columns.Count;

                //enumerate each row
                for (var i = 1; i <= rowCount; i++)
                {
                    //Create array of every cell in current row
                    var cellList = new List<string>();
                    for (var j = 1; j <= colCount; j++)
                    {
                        var cellRange = range.Cells[i, j] as Range;
                        if (cellRange == null) continue;
                        if (cellRange.Value2 == null) continue;
                        var cellValue = cellRange.Value2.ToString();

                        if(string.IsNullOrEmpty(cellValue)) continue; //skip blank row
                        if(cellValue == HeaderCommentRowKeyWord) continue; //skip comment row
                        cellList.Add(cellValue);
                    }

                    //Check current row if valid data to use
                    Trace.WriteLine(string.Join(" ", cellList.ToArray()));
                    if (cellList.Count == 8)
                    {
                        result.Add(new SpreadsheetResultData(cellList, options));
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.InnerException + ex.Message + ex.StackTrace);
                return new ObservableCollection<SpreadsheetResultData>();
            }
            finally
            {
                if (wkb != null)
                {
                    wkb.Close();
                    ReleaseRcm(wkb);
                }

                if (excel != null)
                {
                    excel.Quit();
                    ReleaseRcm(excel);
                }
            }
        }

        /// <summary>
        /// Release com object to garbage collection
        /// </summary>
        /// <param name="o">Object to release</param>
        private static void ReleaseRcm(object o)
        {
            try
            {
                Marshal.ReleaseComObject(o);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                o = null;
            }
        }

        /// <summary>
        /// Method to open spreadsheet from file name
        /// </summary>
        /// <param name="excelInstance"></param>
        /// <param name="fileName"></param>
        /// <param name="readOnly"></param>
        /// <param name="editable"></param>
        /// <param name="updateLinks"></param>
        /// <returns></returns>
        private static Workbook OpenBook(_Application excelInstance, string fileName,
            bool readOnly, bool editable, bool updateLinks)
        {
            var book = excelInstance.Workbooks.Open(fileName, updateLinks, readOnly, Type.Missing, Type.Missing,
                Type.Missing, Type.Missing, Type.Missing, Type.Missing, editable, Type.Missing,
                Type.Missing, Type.Missing);
            return book;
        }
    }
}