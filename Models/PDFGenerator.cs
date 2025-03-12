using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;

namespace IT_WorkPlant.Models
{
    public class PDFConverter
    {
        /// <summary>
        /// 將 Excel 文件轉換為 PDF。
        /// </summary>
        /// <param name="excelPath">Excel 文件路徑。</param>
        /// <param name="sheetName">需要轉換的 Sheet 名稱。</param>
        /// <param name="outputPdfPath">生成的 PDF 文件路徑。</param>
        public void ConvertExcelToPdf(string excelPath, string sheetName, string pdfPath)
        {
            if (!File.Exists(excelPath))
            {
                throw new FileNotFoundException("Excel file not found.", excelPath);
            }

            using (var fs = new FileStream(pdfPath, FileMode.Create, FileAccess.Write))
            {
                Document pdfDoc = new Document(PageSize.A4);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, fs);
                pdfDoc.Open();

                using (var package = new ExcelPackage(new FileInfo(excelPath)))
                {
                    var worksheet = package.Workbook.Worksheets[sheetName];
                    if (worksheet == null)
                    {
                        throw new ArgumentException($"Sheet '{sheetName}' not found in Excel file.");
                    }

                    PdfPTable table = new PdfPTable(worksheet.Dimension.End.Column);

                    // 添加表頭 (第一行內容)
                    for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                    {
                        string header = worksheet.Cells[1, col].Text.Trim();
                        PdfPCell cell = new PdfPCell(new Phrase(header))
                        {
                            BackgroundColor = BaseColor.LIGHT_GRAY, // 表頭背景色
                            HorizontalAlignment = Element.ALIGN_CENTER
                        };
                        table.AddCell(cell);
                    }

                    // 添加數據行
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                        {
                            string cellValue = worksheet.Cells[row, col].Text.Trim();
                            PdfPCell cell = new PdfPCell(new Phrase(cellValue))
                            {
                                HorizontalAlignment = Element.ALIGN_CENTER // 文本居中
                            };
                            table.AddCell(cell);
                        }
                    }

                    table.WidthPercentage = 100; // 表格寬度設置為100%
                    pdfDoc.Add(table);
                }

                pdfDoc.Close();
            }
        }


    }
}
