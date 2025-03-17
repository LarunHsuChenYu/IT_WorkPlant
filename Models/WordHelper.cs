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

namespace IT_WorkPlant.Models
{
    public class WordHelper
    {
        public MemoryStream GenerateWordDocumentFromSubmission(string templatePath, EmailRequestSubmissionModel submission)
        {
            MemoryStream memStream = new MemoryStream();
            using (FileStream fs = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
            {
                fs.CopyTo(memStream);
            }
            memStream.Position = 0;

            // 以可寫入模式開啟 Word 文件
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(memStream, true))
            {
                MainDocumentPart mainPart = wordDoc.MainDocumentPart;
                Document doc = mainPart.Document;
                Body body = doc.Body;

                // 假設模板中已存在一個目標表格，通常取第一個表格
                Table targetTable = body.Elements<Table>().FirstOrDefault();
                if (targetTable == null)
                {
                    // 若找不到表格，則建立一個新的（格式可能不符合預期）
                    targetTable = new Table();
                    body.Append(targetTable);
                }
                else
                {
                    // 移除除表頭以外的所有列（假設第一列為表頭）
                    var rows = targetTable.Elements<TableRow>().ToList();
                    if (rows.Count > 1)
                    {
                        for (int i = 1; i < rows.Count; i++)
                        {
                            rows[i].Remove();
                        }
                    }
                }

                // 根據 submission 逐筆加入資料列
                foreach (var req in submission.Requests)
                {
                    TableRow newRow = new TableRow();

                    // 依據模板的欄位順序，例如：
                    // 第一欄：Full Name (FirstName + LastName)
                    // 第二欄：Employee ID
                    // 第三欄：Department
                    TableCell cell1 = CreateTextCell($"{req.FirstName} {req.LastName}");
                    TableCell cell2 = CreateTextCell(req.EmployeeID);
                    TableCell cell3 = CreateTextCell(req.Department);

                    newRow.Append(cell1, cell2, cell3);
                    targetTable.Append(newRow);
                }

                doc.Save();
            }

            memStream.Position = 0;
            return memStream;
        }

        /// <summary>
        /// 輔助方法：建立一個只包含單一段落文字的儲存格
        /// </summary>
        /// <param name="text">儲存格內的文字</param>
        /// <returns>TableCell 物件</returns>
        private TableCell CreateTextCell(string text)
        {
            TableCell cell = new TableCell();
            // 建立一個段落、Run 與 Text，並加入儲存格
            Paragraph para = new Paragraph(new Run(new Text(text)));
            cell.Append(para);
            return cell;
        }
    }
}
