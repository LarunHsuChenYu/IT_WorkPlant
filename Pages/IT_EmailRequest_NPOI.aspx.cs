using IT_WorkPlant.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

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
            // 驗證現有行的必填欄位
            for (int i = 1; i < RequestEmailTable.Rows.Count; i++)
            {
                HtmlTableRow row = RequestEmailTable.Rows[i] as HtmlTableRow;
                string firstName = GetTextBoxValueFromHtmlCell(row.Cells[1]);
                string lastName = GetTextBoxValueFromHtmlCell(row.Cells[2]);
                string employeeID = GetTextBoxValueFromHtmlCell(row.Cells[3]);
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
            for (int i = RequestEmailTable.Rows.Count; i <= rowCount; i++)
            {
                AddRow(i, tableType);
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
            cell.Controls.Add(inputBox);
            return cell;
        }

        private HtmlTableCell CreateStaticCell(string sRowNumber)
        {
            return new HtmlTableCell { InnerText = sRowNumber };
        }

        private string GetTextBoxValueFromHtmlCell(HtmlTableCell cell)
        {
            var textBox = cell.Controls.OfType<TextBox>().FirstOrDefault();
            return textBox != null ? textBox.Text.Trim() : string.Empty;
        }

        private EmailRequestSubmissionModel BuildSubmissionModel()
        {
            var submission = new EmailRequestSubmissionModel
            {
                RequestDate = DateTime.Now,
                RequesterName = Session["UserName"]?.ToString(),
                DeptName = Session["DeptName"]?.ToString()
            };

            int totalRows = (int)ViewState["RequestEmailTable_RowCount"];
            for (int i = 1; i <= totalRows; i++)
            {
                HtmlTableRow row = RequestEmailTable.Rows[i] as HtmlTableRow;
                string firstName = GetTextBoxValueFromHtmlCell(row.Cells[1]);
                string lastName = GetTextBoxValueFromHtmlCell(row.Cells[2]);
                string employeeID = GetTextBoxValueFromHtmlCell(row.Cells[3]);
                string rowDept = GetTextBoxValueFromHtmlCell(row.Cells[4]);

                if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(employeeID))
                {
                    ShowAlert($"Row {i}: First Name, Last Name, and Employee ID are required!");
                    return null;
                }

                submission.Requests.Add(new EmailRequestModel
                {
                    FirstName = firstName,
                    LastName = lastName,
                    EmployeeID = employeeID,
                    Department = rowDept
                });
            }
            return submission;
        }

        protected async void btnRequestEmailSubmit_Click(object sender, EventArgs e)
        {
            var submissionModel = BuildSubmissionModel();
            if (submissionModel == null)
            {
                // 資料驗證失敗，BuildSubmissionModel 已顯示錯誤訊息
                return;
            }

            string userName = Session["UserName"]?.ToString();
            string userEmpID = Session["UserEmpID"]?.ToString();
            int? requestUserID = _ui.GetRequestUserID(userName, userEmpID);
            if (requestUserID == null)
            {
                ShowAlert("RequestUserID could not be determined.");
                return;
            }

            try
            {
                StringBuilder lineNotifyMessageBuilder = new StringBuilder();
                lineNotifyMessageBuilder.AppendLine($"Requester: {submissionModel.RequesterName}");
                lineNotifyMessageBuilder.AppendLine($"Department: {submissionModel.DeptName}");
                lineNotifyMessageBuilder.AppendLine($"Request Date: {submissionModel.RequestDate:yyyy/MM/dd}");
                lineNotifyMessageBuilder.AppendLine("Email Account Requests:");

                int rowIndex = 1;
                foreach (var req in submissionModel.Requests)
                {
                    var columnValues = new Dictionary<string, object>
                    {
                        { "IssueDate", DateTime.Now },
                        { "DeptNameID", submissionModel.DeptName },
                        { "CompanyID", "ENR" },
                        { "RequestUserID", requestUserID },
                        { "IssueDetails", $"Request EMAIL Account: {req.FirstName}.{req.LastName}" },
                        { "IssueTypeID", 5 },
                        { "Status", false },
                        { "LastUpdateDate", DateTime.Now },
                        { "DRI_UserID", DBNull.Value },
                        { "Solution", DBNull.Value },
                        { "FinishedDate", DBNull.Value },
                        { "Remark", DBNull.Value }
                    };

                    _dbHelper.InsertData("IT_RequestList", columnValues);

                    lineNotifyMessageBuilder.AppendLine($"- Row {rowIndex}:");
                    lineNotifyMessageBuilder.AppendLine($"  Department: {req.Department}");
                    lineNotifyMessageBuilder.AppendLine($"  Full Name: {req.FirstName} {req.LastName}");
                    rowIndex++;
                }

                var notifier = new LineNotificationModel();
                await notifier.SendLineNotifyAsync(lineNotifyMessageBuilder.ToString());

                ShowAlert("EMail Account Request submitted successfully!");
                Response.Redirect("~/Default.aspx");
            }
            catch (Exception ex)
            {
                ShowAlert($"Error: {ex.Message}");
            }
        }

        protected void btnGenerateWord_Click(object sender, EventArgs e)
        {
            var submission = BuildSubmissionModel();
            if (submission == null)
            {
                // 若資料有誤，BuildSubmissionModel 會顯示錯誤訊息
                return;
            }

            WordHelper exportModel = new WordHelper();
            string templatePath = Server.MapPath("~/App_Data/MEMO-Rquest_EMail.docx");
            MemoryStream generatedDoc = exportModel.GenerateWordDocumentFromSubmission(templatePath, submission);
            if (generatedDoc != null)
            {
                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                Response.AddHeader("Content-Disposition", "attachment; filename=MEMO-Request_Email_Output.docx");
                Response.BinaryWrite(generatedDoc.ToArray());
                Response.End();
            }
            else
            {
                ShowAlert("Generate Word Fail...");
            }
        }
    }
}
