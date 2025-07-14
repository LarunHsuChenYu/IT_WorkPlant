using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI.HtmlControls;
using NPOI.XWPF.UserModel;

namespace IT_WorkPlant.Models
{
    public class WordHelperNPOI
    {
        public MemoryStream GenerateWordDocumentFromSubmission(string templatePath, EmailRequestSubmissionModel submission)
        {
            // 讀取模板檔案到 MemoryStream
            MemoryStream memStream = new MemoryStream();
            using (FileStream fs = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
            {
                fs.CopyTo(memStream);
            }
            memStream.Position = 0;

            // 開啟 Word 文件 (.docx)（使用 NPOI）
            XWPFDocument doc = new XWPFDocument(memStream);

            // 假設模板中已經存在一個目標表格，且表頭已固定好（例如第一列作為標題）
            XWPFTable targetTable = doc.Tables.FirstOrDefault();
            if (targetTable == null)
            {
                // 若模板中找不到表格，您可以考慮建立一個新的（但格式可能與模板不同）
                targetTable = doc.CreateTable();
            }
            else
            {
                // 若已存在表格，通常會保留第一列作為標題，刪除後面的所有資料列
                while (targetTable.NumberOfRows > 1)
                {
                    targetTable.RemoveRow(1);
                }
            }

            // 將 submission 中的每一筆資料，依序加入表格中
            foreach (var req in submission.Requests)
            {
                XWPFTableRow newRow = targetTable.CreateRow();

                while (newRow.GetTableCells().Count > 0)
                {
                    newRow.RemoveCell(0);
                }
                XWPFTableCell cell1 = newRow.CreateCell();
                cell1.SetText($"{req.FirstName} {req.LastName}");

                XWPFTableCell cell2 = newRow.CreateCell();
                cell2.SetText(req.EmployeeID);

                XWPFTableCell cell3 = newRow.CreateCell();
                cell3.SetText(req.Department);

                // 如模板中有更多欄位，可依需求增加更多儲存格
            }

            // 將修改後的文件輸出到 MemoryStream
            MemoryStream outputStream = new MemoryStream();
            doc.Write(outputStream);
            outputStream.Position = 0;
            return outputStream;
        }
    }
}


