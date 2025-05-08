using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using IT_WorkPlant.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Word = DocumentFormat.OpenXml.Wordprocessing;


namespace IT_WorkPlant.Pages
{
    public partial class IT_EmailRequest : System.Web.UI.Page
    {
        private readonly MssqlDatabaseHelper _dbHelper = new MssqlDatabaseHelper();
        private readonly UserInfo _ui = new UserInfo();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["UserEmpID"] == null)
                {
                    Response.Redirect("../Login.aspx");
                }

                requestDate.Text = DateTime.Now.ToString("yyyy/MM/dd");
                requesterName.Text = Session["UserName"]?.ToString();
                department.Text = Session["DeptName"]?.ToString();

                ViewState["RequestEmailTable_RowCount"] = 1;
                AddRow(1, "RequestEmailTable");
            }
            else
            {
                RebuildTableRows("RequestEmailTable");
            }
        }

        private void ShowAlert(string message)
        {
            string safeMessage = HttpUtility.JavaScriptStringEncode(message);
            string script = $"alert('{safeMessage}');";
            if (!ClientScript.IsStartupScriptRegistered("alert"))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", script, true);
            }
        }

        protected void AddRow_Click(object sender, EventArgs e)
        {
            int currentRowCount = (int)ViewState["RequestEmailTable_RowCount"];

            if (currentRowCount >= 5)
            {
                ShowAlert("You can add a maximum of 5 rows.");
                return;
            }

            // 验证是否已填写
            for (int i = 1; i < RequestEmailTable.Rows.Count; i++)
            {
                HtmlTableRow row = RequestEmailTable.Rows[i] as HtmlTableRow;
                string firstName = GetTextBoxValueFromHtmlCell(row.Cells[1]);
                string lastName = GetTextBoxValueFromHtmlCell(row.Cells[2]);
                string employeeID = GetTextBoxValueFromHtmlCell(row.Cells[3]);
                string department = GetTextBoxValueFromHtmlCell(row.Cells[4]);

                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(employeeID))
                {
                    ShowAlert($"Row {i}: First Name, Last Name, and Employee ID are required!");
                    return;
                }
            }

            AddRow(currentRowCount + 1, "RequestEmailTable");
        }

        private void AddRow(int rowNumber, string tableType)
        {
            if (rowNumber > 5)
            {
                ShowAlert("You can add a maximum of 5 rows.");
                return;
            }

            HtmlTableRow newRow = new HtmlTableRow();

            if (tableType == "RequestEmailTable")
            {
                newRow.Cells.Add(CreateStaticCell(rowNumber.ToString()));
                newRow.Cells.Add(CreateInputCell("RequestEmailTable", "FirstName", rowNumber));
                newRow.Cells.Add(CreateInputCell("RequestEmailTable", "LastName", rowNumber));
                newRow.Cells.Add(CreateInputCell("RequestEmailTable", "EmployeeID", rowNumber));
                newRow.Cells.Add(CreateInputCell("RequestEmailTable", "Department", rowNumber));
                RequestEmailTable.Rows.Add(newRow);
            }

            ViewState[$"{tableType}_RowCount"] = rowNumber;
        }

        private void RebuildTableRows(string tableType)
        {
            int rowCount = ViewState[$"{tableType}_RowCount"] != null ? (int)ViewState[$"{tableType}_RowCount"] : 0;
            for (int i = RequestEmailTable.Rows.Count - 1; i < rowCount; i++)
            {
                AddRow(i + 1, tableType);
            }
        }


        private HtmlTableCell CreateInputCell(string tableType, string columnName, int rowNumber)
        {
            HtmlTableCell cell = new HtmlTableCell();
            TextBox inputBox = new TextBox
            {
                ID = $"{tableType}_{columnName}_{rowNumber}",
                CssClass = "form-control"
            };

            // ลองดึงค่าจาก ViewState ถ้ามี
            string savedValue = ViewState[$"{tableType}_{columnName}_{rowNumber}"] as string;
            if (!string.IsNullOrEmpty(savedValue))
            {
                inputBox.Text = savedValue;
            }

            cell.Controls.Add(inputBox);
            return cell;
        }

        private HtmlTableCell CreateStaticCell(string sRowNumber)
        {
            var cell = new HtmlTableCell { InnerText = sRowNumber };
            return cell;
        }

        private string GetTextBoxValueFromHtmlCell(HtmlTableCell cell)
        {
            var textBox = cell.Controls.OfType<TextBox>().FirstOrDefault();
            return textBox != null ? textBox.Text.Trim() : string.Empty;
        }

        protected async void btnRequestEmailSubmit_Click(object sender, EventArgs e)
        {
            string userName = Session["UserName"]?.ToString();
            string userEmpID = Session["UserEmpID"]?.ToString();

            int? requestUserID = _ui.GetRequestUserID(userName, userEmpID);

            if (requestUserID == null)
            {
                ShowAlert("RequestUserID could not be determined.");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"Total rows in RequestEmailTable: {RequestEmailTable.Rows.Count}");
            System.Diagnostics.Debug.WriteLine($"Expecting row count from ViewState: {ViewState["RequestEmailTable_RowCount"]}");

            try
            {
                // สร้างข้อความแจ้งเตือน
                StringBuilder lineNotifyMessageBuilder = new StringBuilder();
                lineNotifyMessageBuilder.AppendLine("📧 *Email Account Request*");
                lineNotifyMessageBuilder.AppendLine($"👤 Requester: {userName}");
                lineNotifyMessageBuilder.AppendLine($"🏢 Department: {Session["DeptName"]}");
                lineNotifyMessageBuilder.AppendLine($"📅 Request Date: {DateTime.Now:yyyy/MM/dd}");
                lineNotifyMessageBuilder.AppendLine("");
                lineNotifyMessageBuilder.AppendLine("📝 *Requested Accounts:*");



                // ✅ Insert ข้อมูลเข้า DB
                for (int i = 1; i <= (int)ViewState["RequestEmailTable_RowCount"]; i++)
                {
                    int tableRowIndex = i;
                    if (RequestEmailTable.Rows.Count <= tableRowIndex) break;

                    HtmlTableRow webRow = RequestEmailTable.Rows[tableRowIndex] as HtmlTableRow;
                    if (webRow == null) continue;

                    string firstName = GetTextBoxValueFromHtmlCell(webRow.Cells[1]);
                    string lastName = GetTextBoxValueFromHtmlCell(webRow.Cells[2]);
                    string employeeID = GetTextBoxValueFromHtmlCell(webRow.Cells[3]);
                    string sDepartment = GetTextBoxValueFromHtmlCell(webRow.Cells[4]);
                    lineNotifyMessageBuilder.AppendLine($"▶️ Row {i}: {firstName} {lastName} (Dept: {sDepartment})");

                    if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(employeeID))
                    {
                        ShowAlert($"Row {i}: First Name, Last Name, and Employee ID are required!");
                        return;
                    }

                    var columnValues = new Dictionary<string, object>
            {
                { "IssueDate", DateTime.Now },
                { "DeptNameID", Session["DeptName"].ToString() },
                { "CompanyID", "ENR" },
                { "RequestUserID", requestUserID },
                { "IssueDetails", "Request EMAIL Account: " + firstName + "." + lastName },
                { "IssueTypeID", 5 },
                { "Status", false },
                { "LastUpdateDate", DateTime.Now },
                { "DRI_UserID", DBNull.Value },
                { "Solution", DBNull.Value },
                { "FinishedDate", DBNull.Value },
                { "Remark", DBNull.Value }
            };

                    _dbHelper.InsertData("IT_RequestList", columnValues);
                }

                // 🟢 ส่งแจ้งเตือน LINE
                try
                {
                    var notifier = new LineNotificationModel();
                    string lineGroupId = ConfigurationManager.AppSettings["LineGroupID"];
                    await notifier.SendLineGroupMessageAsync(lineGroupId, lineNotifyMessageBuilder.ToString());
                }
                catch (Exception ex)
                {
                    ShowAlert("LINE notify failed: " + ex.Message);
                }

                // ⏬ จากนี้คือส่วนเดิม MEMO ไม่แตะไม่ยุ่ง
                string deptName = Session["DeptName"]?.ToString();
                string todayDate = DateTime.Now.ToString("yyyy/MM/dd");
                string templatePath = Server.MapPath("~/App_Data/MEMO-Rquest_Email.docx");

                using (MemoryStream mem = new MemoryStream())
                {
                    using (FileStream fileStream = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
                    {
                        fileStream.CopyTo(mem);
                    }
                    mem.Position = 0;

                    using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(mem, true))
                    {
                        var body = wordDoc.MainDocumentPart.Document.Body;

                        ReplaceAllPlaceholders(body, new Dictionary<string, string>
                {
                    { "[DATE]", DateTime.Now.ToString("yyyy/MM/dd") },
                    { "[Department]", Session["DeptName"]?.ToString() ?? "N/A" }
                });

                        var table = body.Descendants<Word.Table>().FirstOrDefault();
                        if (table != null)
                        {
                            var rows = table.Elements<Word.TableRow>().ToList();
                            int rowOffset = 1;

                            for (int i = 0; i < (int)ViewState["RequestEmailTable_RowCount"]; i++)
                            {
                                int tableRowIndex = i + 1;
                                if (RequestEmailTable.Rows.Count <= tableRowIndex) break;

                                HtmlTableRow webRow = RequestEmailTable.Rows[tableRowIndex] as HtmlTableRow;
                                if (webRow == null) continue;

                                string firstName = GetTextBoxValueFromHtmlCell(webRow.Cells[1]);
                                string lastName = GetTextBoxValueFromHtmlCell(webRow.Cells[2]);
                                string employeeID = GetTextBoxValueFromHtmlCell(webRow.Cells[3]);
                                string sDepartment = GetTextBoxValueFromHtmlCell(webRow.Cells[4]);

                                while (rows.Count <= rowOffset + i)
                                {
                                    Word.TableRow templateRow = rows[1];
                                    var newRow = (Word.TableRow)templateRow.CloneNode(true);
                                    table.AppendChild(newRow);
                                    rows = table.Elements<Word.TableRow>().ToList();
                                }

                                Word.TableRow dataRow = rows[rowOffset + i];
                                var cells = dataRow.Elements<Word.TableCell>().ToList();

                                while (cells.Count < 5)
                                {
                                    dataRow.AppendChild(new Word.TableCell(new Word.Paragraph(new Word.Run(new Word.Text("")))));
                                    cells = dataRow.Elements<Word.TableCell>().ToList();
                                }
                                
                                ClearCell(cells[0]); cells[0].Append(CreateCenteredParagraph((i + 1).ToString()));
                                ClearCell(cells[1]); cells[1].Append(CreateCenteredParagraph(firstName));
                                ClearCell(cells[2]); cells[2].Append(CreateCenteredParagraph(lastName));
                                ClearCell(cells[3]); cells[3].Append(CreateCenteredParagraph(employeeID));
                                ClearCell(cells[4]); cells[4].Append(CreateCenteredParagraph(sDepartment));

                                wordDoc.MainDocumentPart.Document.Save();
                            }
                            ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('📥 MEMO กำลังถูกดาวน์โหลด กรุณารอ...');", true);

                            Response.Clear();
                            Response.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                            Response.AddHeader("Content-Disposition", "attachment; filename=MEMO-Request_Email.docx");
                            Response.BinaryWrite(mem.ToArray());
                            Response.Flush();
                            HttpContext.Current.ApplicationInstance.CompleteRequest(); // ใช้แทน Response.End()

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowAlert($"Error: {ex.Message}");
            }
        }

        private void ReplaceAllPlaceholders(Body body, Dictionary<string, string> replacements)
        {
            foreach (var text in body.Descendants<Text>())
            {
                foreach (var pair in replacements)
                {
                    if (text.Text.Contains(pair.Key))
                    {
                        text.Text = text.Text.Replace(pair.Key, pair.Value);
                    }
                }
            }
        }

        private void ClearCell(DocumentFormat.OpenXml.Wordprocessing.TableCell cell)
        {
            cell.RemoveAllChildren<DocumentFormat.OpenXml.Wordprocessing.Paragraph>();
            cell.RemoveAllChildren<DocumentFormat.OpenXml.Wordprocessing.Run>();
            cell.RemoveAllChildren<DocumentFormat.OpenXml.Wordprocessing.Text>();
        }

        private void ReplaceTextInWord(Body body, string placeholder, string newText)
        {
            foreach (var text in body.Descendants<Text>())
            {
                if (text.Text.Contains(placeholder))
                {
                    text.Text = text.Text.Replace(placeholder, newText);
                }
            }
        }
        private Paragraph CreateCenteredParagraph(string text)
        {
            return new Paragraph(
                new ParagraphProperties(
                    new Justification() { Val = JustificationValues.Center },
                    new SpacingBetweenLines() { After = "0" } 
                ),
                new Run(new Text(text ?? ""))
            );
        }

        private void UpdateCellText(DocumentFormat.OpenXml.Wordprocessing.TableCell cell, string text)
        {
            cell.RemoveAllChildren<DocumentFormat.OpenXml.Wordprocessing.Paragraph>();
            cell.Append(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(
                new DocumentFormat.OpenXml.Wordprocessing.Run(
                    new DocumentFormat.OpenXml.Wordprocessing.Text(text ?? "")
                )
            ));
        }






    }
}
