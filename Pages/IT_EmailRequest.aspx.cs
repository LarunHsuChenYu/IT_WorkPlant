using IT_WorkPlant.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
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

            try
            {
                // สร้างข้อความแจ้งเตือน
                StringBuilder lineNotifyMessageBuilder = new StringBuilder();
                lineNotifyMessageBuilder.AppendLine($"Requester: {userName}");
                lineNotifyMessageBuilder.AppendLine($"Department: {Session["DeptName"]}");
                lineNotifyMessageBuilder.AppendLine($"Request Date: {DateTime.Now:yyyy/MM/dd}");
                lineNotifyMessageBuilder.AppendLine("Email Account Requests:");

                for (int i = 1; i <= (int)ViewState["RequestEmailTable_RowCount"]; i++)
                {
                    HtmlTableRow row = RequestEmailTable.Rows[i] as HtmlTableRow;

                    string firstName = GetTextBoxValueFromHtmlCell(row.Cells[1]);
                    string lastName = GetTextBoxValueFromHtmlCell(row.Cells[2]);
                    string employeeID = GetTextBoxValueFromHtmlCell(row.Cells[3]);
                    string sDepartment = GetTextBoxValueFromHtmlCell(row.Cells[4]);

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

                    lineNotifyMessageBuilder.AppendLine($"- Row {i}:");
                    lineNotifyMessageBuilder.AppendLine($"  Department: {sDepartment}");
                    lineNotifyMessageBuilder.AppendLine($"  Full Name: {firstName} {lastName}");
                }

                // ✅ ส่งเข้า LINE Group แทน user เฉพาะเจาะจง
                string lineGroupId = ConfigurationManager.AppSettings["LineGroupID"];
                var notifier = new LineNotificationModel();
                await notifier.SendLineGroupMessageAsync(lineGroupId, lineNotifyMessageBuilder.ToString());

                ShowAlert("Email Account Request submitted successfully!");
                Response.Redirect("~/Default.aspx");
            }
            catch (Exception ex)
            {
                ShowAlert($"Error: {ex.Message}");
            }
        }

    }
}
