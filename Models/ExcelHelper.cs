using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using OfficeOpenXml;

namespace IT_WorkPlant.Models
{
    public class ExcelHelper
    {
        public string SaveUploadedFile(HttpPostedFile fileUpload, HttpServerUtility server)
        {
            string directoryPath = server.MapPath("~/App_Uploads/");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(fileUpload.FileName);
            string filePath = Path.Combine(directoryPath, uniqueFileName);
            fileUpload.SaveAs(filePath);
            return filePath;
        }

        public DataTable ReadExcelData(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            {
                throw new FileNotFoundException("Excel file not found at the specified path.");
            }

            DataTable dataTable = new DataTable();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                if (worksheet == null)
                {
                    throw new Exception("The Excel file is empty or has no valid worksheet.");
                }

                foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                {
                    string columnName = firstRowCell.Text?.Trim(); // 去除多餘空格
                    if (!string.IsNullOrEmpty(columnName)) // 僅新增非空欄位
                    {
                        dataTable.Columns.Add(columnName);
                    }
                }

                // 填充數據行，處理跨欄置中
                for (int rowNum = 2; rowNum <= worksheet.Dimension.End.Row; rowNum++)
                {
                    var row = dataTable.NewRow();
                    for (int colNum = 1; colNum <= dataTable.Columns.Count; colNum++)
                    {
                        var cell = worksheet.Cells[rowNum, colNum];
                        string cellValue = cell.Text;

                        // 如果當前單元格為空，檢查它是否屬於合併範圍
                        if (string.IsNullOrWhiteSpace(cellValue))
                        {
                            cellValue = GetMergedCellValue(worksheet, rowNum, colNum);
                        }

                        row[colNum - 1] = cellValue;
                    }
                    dataTable.Rows.Add(row);
                }
            }

            return dataTable;
        }

        private string GetMergedCellValue(ExcelWorksheet worksheet, int row, int column)
        {
            // 檢查 MergedCells 集合，確認當前單元格是否屬於某個合併範圍
            foreach (var mergedAddress in worksheet.MergedCells)
            {
                var mergedRange = worksheet.Cells[mergedAddress];
                if (mergedRange.Start.Row <= row && row <= mergedRange.End.Row &&
                    mergedRange.Start.Column <= column && column <= mergedRange.End.Column)
                {
                    // 返回合併範圍的起始值
                    return worksheet.Cells[mergedRange.Start.Row, mergedRange.Start.Column].Text;
                }
            }

            return string.Empty; // 如果當前單元格不屬於任何合併範圍，返回空
        }

        public void FillExcelTemplate(string templatePath, string[] sInvHeadInfo, DataTable data, int iStartRow, string outputPath, string sheetName)
        {
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException("Template file not found.", templatePath);
            }

            // 複製模板文件到輸出路徑
            File.Copy(templatePath, outputPath, true);

            using (var package = new ExcelPackage(new FileInfo(outputPath)))
            {
                // 獲取指定的工作表
                var worksheet = package.Workbook.Worksheets[sheetName];
                if (worksheet == null)
                {
                    throw new ArgumentException($"Sheet '{sheetName}' not found in template.");
                }

                // 刪除不需要的工作表，只保留指定的工作表
                foreach (var ws in package.Workbook.Worksheets.ToList())
                {
                    if (ws.Name != sheetName)
                    {
                        package.Workbook.Worksheets.Delete(ws);
                    }
                }

                // 插入行以適應數據量
                if (data.Rows.Count > 17)
                {
                    worksheet.InsertRow(iStartRow, data.Rows.Count - 17, iStartRow);
                }

                for (int i = 0; i < sInvHeadInfo.Length; i++)
                {
                    worksheet.Cells[10 + i, 11].Value = sInvHeadInfo[i];
                }

                int startRow = iStartRow;

                // 填充數據
                foreach (DataRow row in data.Rows)
                {
                    for (int col = 0; col < data.Columns.Count; col++)
                    {
                        switch (col)
                        {
                            case int n when n >= 0 && n <= 4:
                                worksheet.Cells[startRow, col + 2].Value = ConvertToNumeric(row[col]);
                                break;
                            case int n when n > 4:
                                worksheet.Cells[startRow, col * 2 - 3].Value = ConvertToNumeric(row[col]);
                                break;
                        }
                    }
                    startRow++;
                }

                // 添加公式
                if (startRow > 34)
                {
                    string sFormulaG_Col = $"SUM(G18:G{startRow - 1})";
                    string sFormulaK_Col = $"SUM(K18:K{startRow - 1})";

                    worksheet.Cells[startRow, 7].Formula = sFormulaG_Col;
                    worksheet.Cells[startRow, 11].Formula = sFormulaK_Col;

                    if (sheetName == "PLS")
                    {
                        string sFormulaI_Col = $"SUM(I18:I{startRow - 1})";
                        string sFormulaM_Col = $"SUM(M18:M{startRow - 1})";

                        worksheet.Cells[startRow, 9].Formula = sFormulaI_Col;
                        worksheet.Cells[startRow, 13].Formula = sFormulaM_Col;
                    }
                }

                // 計算公式
                worksheet.Calculate();

                // 保存文件
                package.Save();
            }
        }


        private object ConvertToNumeric(object value)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return null; // 空值不填充
            }

            if (double.TryParse(value.ToString(), out double result))
            {
                return result; // 成功轉換為數字
            }

            return value.ToString(); // 如果不是數字，保持為文本
        }

    }
}
