using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iTextSharp.text;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;
using TableCell = DocumentFormat.OpenXml.Wordprocessing.TableCell;
using TableRow = DocumentFormat.OpenXml.Wordprocessing.TableRow;
using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using Document = DocumentFormat.OpenXml.Wordprocessing.Document;
using System.Collections.Generic;
using System;

namespace IT_WorkPlant.Models
{
    public class WordHelper
    {
        public MemoryStream GenerateMemoFromSubmit(string templatePath, EmailRequestSubmissionModel submission, 
            string sDeptName)
        {
            MemoryStream memStream = new MemoryStream();
            using (FileStream fs = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
            {
                fs.CopyTo(memStream);
            }
            memStream.Position = 0;

            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(memStream, true))
            {
                MainDocumentPart mainPart = wordDoc.MainDocumentPart;
                Document doc = mainPart.Document;

                // **透過關鍵字精確定位你要的表格 (如：找到標題含有 NO., FirstName 的表格)**
                Table targetTable = doc.Descendants<Table>()
                    .Where(tbl => tbl.Descendants<TableCell>()
                    .Any(cell => cell.InnerText.Contains("NO.") && cell.InnerText.Contains("FirstName")))
                    .FirstOrDefault();

                if (targetTable == null)
                {
                    throw new Exception("The specified form cannot be found; please verify that the template includes the correct headers (e.g., NO., FirstName, etc.).。");
                }

                var allRows = targetTable.Descendants<TableRow>().ToList();

                if (allRows.Count < 2)
                {
                    throw new Exception("The target form is missing data fields; please ensure the template includes headers and at least one blank row.");
                }

                TableRow headerRow = allRows[0];
                List<TableRow> dataRows = allRows.Skip(2).ToList();

                int totalRequests = submission.Requests.Count;
                int index = 0;

                // **填入現有空白行**
                for (; index < dataRows.Count && index < totalRequests; index++)
                {
                    TableRow currentRow = dataRows[index];
                    var cells = currentRow.Elements<TableCell>().ToList();
                    if (cells.Count < 5)
                    {
                        throw new Exception($"The fields in row {index + 1} are insufficient; please check the template structure.");
                    }

                    SetCellText(cells[0], (index + 1).ToString());
                    SetCellText(cells[1], submission.Requests[index].FirstName);
                    SetCellText(cells[2], submission.Requests[index].LastName);
                    SetCellText(cells[3], submission.Requests[index].EmployeeID);
                    SetCellText(cells[4], submission.Requests[index].Department);
                }

                // **如果還有更多資料，則新增資料行**
                if (index < totalRequests)
                {
                    TableRow templateRow = dataRows.Last();

                    for (int j = index; j < totalRequests; j++)
                    {
                        TableRow newRow = (TableRow)templateRow.CloneNode(true);
                        var newCells = newRow.Elements<TableCell>().ToList();
                        if (newCells.Count < 5)
                        {
                            throw new Exception("The template row fields are insufficient; please check the template structure.");
                        }

                        SetCellText(newCells[0], (j + 1).ToString());
                        SetCellText(newCells[1], submission.Requests[j].FirstName);
                        SetCellText(newCells[2], submission.Requests[j].LastName);
                        SetCellText(newCells[3], submission.Requests[j].EmployeeID);
                        SetCellText(newCells[4], submission.Requests[j].Department);

                        targetTable.Append(newRow);
                    }
                }

                ReplaceTextInSubject(doc, "IT", "【"+sDeptName+"】");

                doc.Save();
            }

            memStream.Position = 0;
            return memStream;
        }

        // 輔助方法：填入儲存格文字
        private void SetCellText(TableCell cell, string text)
        {
            Paragraph firstParagraph = cell.Elements<Paragraph>().FirstOrDefault();
            if (firstParagraph == null)
            {
                firstParagraph = new Paragraph();
                cell.Append(firstParagraph);
            }

            firstParagraph.RemoveAllChildren<Run>();
            Run run = new Run(new Text(text ?? string.Empty));
            firstParagraph.Append(run);
        }

        private void ReplaceTextInSubject(Document doc, string placeholder, string replacementText)
        {
            // 找到包含 "主旨Subject" 的段落
            var subjectParagraph = doc.Descendants<Paragraph>()
                                      .FirstOrDefault(p => p.InnerText.Contains("主旨Subject"));

            if (subjectParagraph == null)
                throw new Exception("Can not Find 【主旨Subject】");

            foreach (var text in subjectParagraph.Descendants<Text>().Where(t => t.Text.Contains(placeholder)))
            {
                text.Text = text.Text.Replace(placeholder, replacementText);
            }
        }
    }
}
