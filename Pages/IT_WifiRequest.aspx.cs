using System;
using System.Web.Services;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using IT_WorkPlant.Models;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;

namespace IT_WorkPlant.Pages
{
    public partial class IT_WifiRequest : Page
    {
        private readonly MssqlDatabaseHelper _dbHelper = new MssqlDatabaseHelper();
        private readonly UserInfo _ui = new UserInfo();
        private readonly IT_RequestModel _model = new IT_RequestModel();

        #region ── Constants ────────────────────────────────────────
        // Flow IDs for Wi-Fi Access Requests
        private const int FLOW_ID_NEW_EMP_WIFI = 4;    // NewEmp. WIFI Acc. Request
        private const int FLOW_ID_VISITOR_WIFI = 5;    // Visitor WIFI Acc. Request
        private const int FLOW_ID_BIZ_TRIP_WIFI = 6;   // BizTrip WIFI Acc. Request
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["lang"] == null)
            {
                Session["lang"] = "en"; // ตั้งค่าภาษาเริ่มต้น
            }

            if (!IsPostBack)
            {
#if !DEBUG
                if (Session["UserEmpID"] == null)
                {
                    Response.Redirect("../Login.aspx");
                }
                
                
#else
                Session["UserEmpID"] = "061230010";
                Session["UserName"] = "Kanyarat.t";
                Session["DeptName"] = "AD";
#endif
                if (!string.IsNullOrEmpty(hfActiveTab.Value))
                {
                    ShowTab(hfActiveTab.Value);
                }

                requestDate.Text = DateTime.Now.ToString("yyyy/MM/dd");
                requesterName.Text = Session["UserName"].ToString();
                department.Text = Session["DeptName"].ToString();

                ViewState["VisitorTable_RowCount"] = 1;
                ViewState["OnboardTable_RowCount"] = 1;
                ViewState["BizTripTable_RowCount"] = 1;

                AddRow(1, "VisitorTable");
                AddRow(1, "BizTripTable");
                AddRow(1, "OnboardTable");

                Page.DataBind(); // ⭐ สำคัญสุด
            }
            else
            {
                hfActiveTab.Value = "VisitorDiv";
                ShowTab("VisitorDiv");

                RebuildTableRows("VisitorTable");
                RebuildTableRows("BizTripTable");
                RebuildTableRows("OnboardTable");
            }
        }

        private void ShowTab(string tabId)
        {
            // 添加 CSS active 類別到對應的 div
            ScriptManager.RegisterStartupScript(this, GetType(), "ActivateTab",
                $"document.querySelectorAll('.tab-content').forEach(tab => tab.classList.remove('active'));" +
                $"document.getElementById('{tabId}').classList.add('active');", true);
        }

        private void ShowAlert(string message)
        {
            // 確保消息正確轉義
            string safeMessage = HttpUtility.JavaScriptStringEncode(message);
            string script = $"alert('{safeMessage}');";

            // 注入腳本到頁面
            if (!ClientScript.IsStartupScriptRegistered("alert"))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", script, true);
            }
        }

        protected void AddRow_Click(object sender, EventArgs e)
        {
            string activeTab = hfActiveTab.Value;
            int currentRowCount = (int)ViewState["VisitorTable_RowCount"];

            // 檢查是否達到最大行數
            if (currentRowCount >= 5)
            {
                ShowAlert("You can add a maximum of 5 rows.");
                return;
            }

            // 驗證現有行是否正確填寫
            for (int i = 1; i < VisitorTable.Rows.Count; i++) // 從第一行開始
            {
                HtmlTableRow row = VisitorTable.Rows[i] as HtmlTableRow;

                string description = GetTextBoxValueFromHtmlCell(row.Cells[1]);
                string dateTime = GetTextBoxValueFromHtmlCell(row.Cells[2]);
                string macAddress = GetTextBoxValueFromHtmlCell(row.Cells[3]);

                if (string.IsNullOrEmpty(description) || string.IsNullOrEmpty(dateTime) || string.IsNullOrEmpty(macAddress))
                {
                    ShowAlert($"Row {i}: Description, Date & Time, and IP/WLAN Mac Address are required!");
                    return;
                }
            }

            // 添加新行
            AddRow(currentRowCount + 1, "VisitorTable");
            ShowTab("VisitorDiv");
        }

        private void AddRow(int rowNumber, string tableType)
        {
            if (rowNumber > 5)
            {
                ShowAlert("You can add a maximum of 5 rows.");
                return;
            }

            HtmlTableRow newRow = new HtmlTableRow();

            switch (tableType)
            {
                case "VisitorTable":
                    newRow.Cells.Add(CreateStaticCell(rowNumber.ToString()));
                    newRow.Cells.Add(CreateInputCell("VisitorTable", "FullName", rowNumber));
                    newRow.Cells.Add(CreateInputCell("VisitorTable", "Company", rowNumber));
                    newRow.Cells.Add(CreateDateCell("VisitorTable", "StartDate", rowNumber));
                    newRow.Cells.Add(CreateDateCell("VisitorTable", "EndDate", rowNumber));
                    newRow.Cells.Add(CreateInputCell("VisitorTable", "Description", rowNumber));
                    VisitorTable.Rows.Add(newRow);
                    break;

                case "BizTripTable":
                    newRow.Cells.Add(CreateStaticCell(rowNumber.ToString()));
                    newRow.Cells.Add(CreateInputCell("BizTripTable", "FullName", rowNumber));
                    newRow.Cells.Add(CreateInputCell("BizTripTable", "EmployeeID", rowNumber));
                    newRow.Cells.Add(CreateInputCell("BizTripTable", "Department", rowNumber));
                    newRow.Cells.Add(CreateInputCell("BizTripTable", "DeviceType", rowNumber));
                    newRow.Cells.Add(CreateInputCell("BizTripTable", "DeviceMACAddress", rowNumber));
                    newRow.Cells.Add(CreateDateCell("BizTripTable", "StartDate", rowNumber));
                    newRow.Cells.Add(CreateDateCell("BizTripTable", "EndDate", rowNumber));
                    BizTripTable.Rows.Add(newRow);
                    break;

                case "OnboardTable":
                    newRow.Cells.Add(CreateStaticCell(rowNumber.ToString()));
                    newRow.Cells.Add(CreateInputCell("OnboardTable", "FullName", rowNumber));
                    newRow.Cells.Add(CreateInputCell("OnboardTable", "EmployeeID", rowNumber));
                    newRow.Cells.Add(CreateInputCell("OnboardTable", "Department", rowNumber));
                    newRow.Cells.Add(CreateInputCell("OnboardTable", "Email", rowNumber));
                    newRow.Cells.Add(CreateInputCell("OnboardTable", "DeviceType", rowNumber));
                    newRow.Cells.Add(CreateInputCell("OnboardTable", "MACAddress", rowNumber));
                    newRow.Cells.Add(CreateInputCell("OnboardTable", "Description", rowNumber));
                    OnboardTable.Rows.Add(newRow);
                    break;

                default:
                    throw new ArgumentException("Invalid table type");
            }


            ViewState[$"{tableType}_RowCount"] = rowNumber;
        }


        private void RebuildTableRows(string tableType)
        {
            // 獲取 ViewState 中保存的行數
            int rowCount = ViewState[$"{tableType}_RowCount"] != null
                ? (int)ViewState[$"{tableType}_RowCount"]
                : 0;

            // 確保表格中已存在的行不會重複創建
            switch (tableType)
            {
                case "VisitorTable":
                    while (VisitorTable.Rows.Count - 1 < rowCount)
                    {
                        AddRow(VisitorTable.Rows.Count, tableType);
                    }
                    break;
                case "BizTripTable":
                    while (BizTripTable.Rows.Count - 1 < rowCount)
                    {
                        AddRow(BizTripTable.Rows.Count, tableType);
                    }
                    break;
                case "OnboardTable":
                    while (OnboardTable.Rows.Count - 1 < rowCount)
                    {
                        AddRow(OnboardTable.Rows.Count, tableType);
                    }
                    break;
            }

        }

        private HtmlTableCell CreateCellBasedOnType(string dataType, int rowNumber, string columnName)
        {
            HtmlTableCell cell = new HtmlTableCell();

            TextBox inputBox = new TextBox
            {
                ID = $"{columnName}{rowNumber}",
                CssClass = "form-control"
            };

            // 設定輸入框的型別
            switch (dataType.ToLower())
            {
                case "string":
                    inputBox.TextMode = TextBoxMode.SingleLine;
                    break;
                case "text":
                    inputBox.TextMode = TextBoxMode.MultiLine;
                    inputBox.Rows = 2;
                    break;
                case "date":
                    inputBox.TextMode = TextBoxMode.Date;
                    break;
                case "datetime":
                    inputBox.TextMode = TextBoxMode.DateTimeLocal;
                    break;
                case "number":
                    inputBox.TextMode = TextBoxMode.Number;
                    break;
                default:
                    inputBox.TextMode = TextBoxMode.SingleLine;
                    break;
            }

            cell.Controls.Add(inputBox);
            return cell;
        }

        private HtmlTableCell CreateInputCell(string tableType, string columnName, int rowNumber)
        {
            HtmlTableCell cell = new HtmlTableCell();

            if (columnName == "Department")
            {
                DropDownList ddl = new DropDownList
                {
                    ID = $"{tableType}_{columnName}_{rowNumber}",
                    CssClass = "form-control"
                };

                // Get departments from database
                DataTable dtDepartments = _model.GetDepartments();
                ddl.DataSource = dtDepartments;
                ddl.DataTextField = "DeptName_en";
                ddl.DataValueField = "DeptNameID";
                ddl.DataBind();

                cell.Controls.Add(ddl);
            }
            else
            {
                TextBox inputBox = new TextBox
                {
                    ID = $"{tableType}_{columnName}_{rowNumber}",
                    CssClass = "form-control"
                };

                cell.Controls.Add(inputBox);
            }

            return cell;
        }

        private HtmlTableCell CreateDateCell(string tableType, string columnName, int rowNumber)
        {
            HtmlTableCell cell = new HtmlTableCell();

            // 创建一个日期类型的 TextBox
            TextBox dateBox = new TextBox
            {
                ID = $"{tableType}_{columnName}_{rowNumber}", // 确保 ID 唯一
                CssClass = "form-control",
                TextMode = TextBoxMode.Date // 设置输入模式为日期
            };

            cell.Controls.Add(dateBox);
            return cell;
        }

        private HtmlTableCell CreateStaticCell(string sRowNumber)
        {
            var cell = new HtmlTableCell { InnerText = sRowNumber };
            return cell;
        }

        private string GetTextBoxValueFromHtmlCell(HtmlTableCell cell)
        {
            // Check for DropDownList first
            var dropDown = cell.Controls.OfType<DropDownList>().FirstOrDefault();
            if (dropDown != null)
            {
                return dropDown.SelectedValue;
            }

            // Then check for TextBox
            var textBox = cell.Controls.OfType<TextBox>().FirstOrDefault();
            return textBox != null ? textBox.Text.Trim() : string.Empty;
        }

        protected void btnBizTripAddRow_Click(object sender, EventArgs e)
        {
            int currentRowCount = (int)ViewState["BizTripTable_RowCount"];

            // 檢查是否達到最大行數
            if (currentRowCount >= 5)
            {
                ShowAlert("You can add a maximum of 5 rows for Business Trip.");
                return;
            }

            // 驗證現有行是否正確填寫
            for (int i = 1; i < BizTripTable.Rows.Count; i++) // 從第一行開始
            {
                HtmlTableRow row = BizTripTable.Rows[i] as HtmlTableRow;

                string fullName = GetTextBoxValueFromHtmlCell(row.Cells[1]);
                string employeeID = GetTextBoxValueFromHtmlCell(row.Cells[2]);
                string department = GetTextBoxValueFromHtmlCell(row.Cells[3]);
                string deviceType = GetTextBoxValueFromHtmlCell(row.Cells[4]);
                string deviceMacAddress = GetTextBoxValueFromHtmlCell(row.Cells[5]);

                if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(employeeID) || string.IsNullOrEmpty(department) ||
                    string.IsNullOrEmpty(deviceType) || string.IsNullOrEmpty(deviceMacAddress))
                {
                    ShowAlert($"Row {i}: Full Name, Employee ID, Department, Device Type, and MAC Address are required!");
                    return;
                }
            }

            // 添加新行
            AddRow(currentRowCount + 1, "BizTripTable");
            ShowTab("BizTripDiv");
        }

        protected void btnOnboardAddRow_Click(object sender, EventArgs e)
        {
            int currentRowCount = (int)ViewState["OnboardTable_RowCount"];

            // 檢查是否達到最大行數
            if (currentRowCount >= 5)
            {
                ShowAlert("You can add a maximum of 5 rows for Onboard.");
                return;
            }

            // 驗證現有行是否正確填寫
            for (int i = 1; i < OnboardTable.Rows.Count; i++) // 從第一行開始
            {
                HtmlTableRow row = OnboardTable.Rows[i] as HtmlTableRow;

                string fullName = GetTextBoxValueFromHtmlCell(row.Cells[1]);
                string employeeID = GetTextBoxValueFromHtmlCell(row.Cells[2]);
                string department = GetTextBoxValueFromHtmlCell(row.Cells[3]);
                string email = GetTextBoxValueFromHtmlCell(row.Cells[4]);
                string deviceType = GetTextBoxValueFromHtmlCell(row.Cells[5]);
                string macAddress = GetTextBoxValueFromHtmlCell(row.Cells[6]);

                if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(employeeID) || string.IsNullOrEmpty(department) ||
                    string.IsNullOrEmpty(email) || string.IsNullOrEmpty(deviceType) || string.IsNullOrEmpty(macAddress))
                {
                    ShowAlert($"Row {i}: Full Name, Employee ID, Department, Email, Device Type, and MAC Address are required!");
                    return;
                }
            }

            // 添加新行
            AddRow(currentRowCount + 1, "OnboardTable");
            ShowTab("OnboardDiv");
        }
        protected async void SubmitForm_Click(object sender, EventArgs e)
        {
            string userName = Session["UserName"]?.ToString();
            string userEmpID = Session["UserEmpID"]?.ToString();

            int? requestUserID = _ui.GetRequestUserID(userName, userEmpID);

            if (requestUserID == null)
            {
                ShowAlert("RequestUserID could not be determined.");
                return;
            }

            var transaction = _dbHelper.BeginTransaction();
            try
            {
                StringBuilder lineNotifyMessageBuilder = new StringBuilder();
                lineNotifyMessageBuilder.AppendLine($"Requester: {userName}");
                lineNotifyMessageBuilder.AppendLine($"Department: {Session["DeptName"]}");
                lineNotifyMessageBuilder.AppendLine($"Request Date: {DateTime.Now:yyyy/MM/dd}");
                lineNotifyMessageBuilder.AppendLine("Wi-Fi Requests:");

                // Build detailed information
                StringBuilder detailsBuilder = new StringBuilder();
                detailsBuilder.AppendLine("Visitor Wi-Fi Requests:");
                for (int i = 1; i <= (int)ViewState["VisitorTable_RowCount"]; i++)
                {
                    HtmlTableRow row = VisitorTable.Rows[i] as HtmlTableRow;

                    string fullName = GetTextBoxValueFromHtmlCell(row.Cells[1]);
                    string company = GetTextBoxValueFromHtmlCell(row.Cells[2]);
                    string startDate = GetTextBoxValueFromHtmlCell(row.Cells[3]);
                    string endDate = GetTextBoxValueFromHtmlCell(row.Cells[4]);
                    string description = GetTextBoxValueFromHtmlCell(row.Cells[5]);

                    if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(company) || string.IsNullOrEmpty(startDate))
                    {
                        _dbHelper.RollbackTransaction();
                        ShowAlert($"Row {i}: Full Name, Company, and Start Date are required!");
                        return;
                    }

                    detailsBuilder.AppendLine($"Request {i}:");
                    detailsBuilder.AppendLine($"- Full Name: {fullName}");
                    detailsBuilder.AppendLine($"- Company: {company}");
                    detailsBuilder.AppendLine($"- Start Date: {startDate}");
                    detailsBuilder.AppendLine($"- End Date: {endDate}");
                    if (!string.IsNullOrEmpty(description))
                    {
                        detailsBuilder.AppendLine($"- Description: {description}");
                    }
                    detailsBuilder.AppendLine();

                    lineNotifyMessageBuilder.AppendLine($"- Row {i}:");
                    lineNotifyMessageBuilder.AppendLine($"  Full Name: {fullName}");
                    lineNotifyMessageBuilder.AppendLine($"  Company: {company}");
                    lineNotifyMessageBuilder.AppendLine($"  Start Date: {startDate}");
                    lineNotifyMessageBuilder.AppendLine($"  End Date: {endDate}");
                    if (!string.IsNullOrEmpty(description))
                    {
                        lineNotifyMessageBuilder.AppendLine($"  Description: {description}");
                    }
                }

                // Create single request with all details
                var requestValues = new Dictionary<string, object>
                {
                    { "IssueDate", DateTime.Now },
                    { "DeptNameID", Session["DeptName"].ToString() },
                    { "CompanyID", "ENR" },
                    { "RequestUserID", requestUserID },
                    { "IssueDetails", detailsBuilder.ToString() },
                    { "IssueTypeID", 5 },
                    { "Status", false },
                    { "LastUpdateDate", DateTime.Now },
                    { "DRI_UserID", DBNull.Value },
                    { "Solution", DBNull.Value },
                    { "FinishedDate", DBNull.Value },
                    { "Remark", DBNull.Value }
                };

                int reportID = _dbHelper.InsertDataReturnId("IT_RequestList", requestValues, transaction);

                if (reportID <= 0)
                {
                    throw new Exception("Insert IT_RequestList failed, no ID returned.");
                }

                // Submit workflow
                var wfRepo = new WF_Repository(_dbHelper);
                wfRepo.SubmitWorkflow(reportID, FLOW_ID_VISITOR_WIFI, "Visitor Wi-Fi request submitted", transaction);

                _dbHelper.CommitTransaction();
#if !DEBUG
                string lineGroupId = ConfigurationManager.AppSettings["LineGroupID"];
                var notifier = new LineNotificationModel();
                await notifier.SendLineGroupMessageAsync(lineGroupId, lineNotifyMessageBuilder.ToString());
#else
                await Task.CompletedTask; // Suppress CS1998 warning in DEBUG
#endif  
                ShowAlert("Wi-Fi Usage Request submitted successfully!");
                Response.Redirect("~/Default.aspx");
            }
            catch (Exception ex)
            {
                _dbHelper.RollbackTransaction();
                ShowAlert($"Error: {ex.Message}");
            }
        }

        protected async void btnOnboardSubmit_Click(object sender, EventArgs e)
        {
            string userName = Session["UserName"]?.ToString();
            string userEmpID = Session["UserEmpID"]?.ToString();

            int? requestUserID = _ui.GetRequestUserID(userName, userEmpID);

            if (requestUserID == null)
            {
                ShowAlert("RequestUserID could not be determined.");
                return;
            }

            var transaction = _dbHelper.BeginTransaction();
            try
            {
                StringBuilder lineNotifyMessageBuilder = new StringBuilder();
                lineNotifyMessageBuilder.AppendLine($"Requester: {userName}");
                lineNotifyMessageBuilder.AppendLine($"Department: {Session["DeptName"]}");
                lineNotifyMessageBuilder.AppendLine($"Request Date: {DateTime.Now:yyyy/MM/dd}");
                lineNotifyMessageBuilder.AppendLine("Onboard Wi-Fi Requests:");

                // Build detailed information
                StringBuilder detailsBuilder = new StringBuilder();
                detailsBuilder.AppendLine("New Employee Wi-Fi Requests:");
                for (int i = 1; i <= (int)ViewState["OnboardTable_RowCount"]; i++)
                {
                    HtmlTableRow row = OnboardTable.Rows[i] as HtmlTableRow;

                    string fullName = GetTextBoxValueFromHtmlCell(row.Cells[1]);
                    string employeeID = GetTextBoxValueFromHtmlCell(row.Cells[2]);
                    string department = GetTextBoxValueFromHtmlCell(row.Cells[3]);
                    string email = GetTextBoxValueFromHtmlCell(row.Cells[4]);
                    string deviceType = GetTextBoxValueFromHtmlCell(row.Cells[5]);
                    string macAddress = GetTextBoxValueFromHtmlCell(row.Cells[6]);
                    string description = GetTextBoxValueFromHtmlCell(row.Cells[7]);

                    if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(employeeID) || string.IsNullOrEmpty(macAddress))
                    {
                        _dbHelper.RollbackTransaction();
                        ShowAlert($"Row {i}: Full Name, Employee ID, and MAC Address are required!");
                        return;
                    }

                    detailsBuilder.AppendLine($"Request {i}:");
                    detailsBuilder.AppendLine($"- Full Name: {fullName}");
                    detailsBuilder.AppendLine($"- Employee ID: {employeeID}");
                    detailsBuilder.AppendLine($"- Department: {department}");
                    detailsBuilder.AppendLine($"- Email: {email}");
                    detailsBuilder.AppendLine($"- Device Type: {deviceType}");
                    detailsBuilder.AppendLine($"- MAC Address: {macAddress}");
                    if (!string.IsNullOrEmpty(description))
                    {
                        detailsBuilder.AppendLine($"- Description: {description}");
                    }
                    detailsBuilder.AppendLine();

                    lineNotifyMessageBuilder.AppendLine($"- Row {i}:");
                    lineNotifyMessageBuilder.AppendLine($"  Full Name: {fullName}");
                    lineNotifyMessageBuilder.AppendLine($"  Employee ID: {employeeID}");
                    lineNotifyMessageBuilder.AppendLine($"  Department: {department}");
                    lineNotifyMessageBuilder.AppendLine($"  Email: {email}");
                    lineNotifyMessageBuilder.AppendLine($"  Device Type: {deviceType}");
                    lineNotifyMessageBuilder.AppendLine($"  MAC Address: {macAddress}");
                    if (!string.IsNullOrEmpty(description))
                    {
                        lineNotifyMessageBuilder.AppendLine($"  Description: {description}");
                    }
                }

                // Create single request with all details
                var requestValues = new Dictionary<string, object>
                {
                    { "IssueDate", DateTime.Now },
                    { "DeptNameID", Session["DeptName"].ToString() },
                    { "CompanyID", "ENR" },
                    { "RequestUserID", requestUserID },
                    { "IssueDetails", detailsBuilder.ToString() },
                    { "IssueTypeID", FLOW_ID_NEW_EMP_WIFI },
                    { "Status", false },
                    { "LastUpdateDate", DateTime.Now },
                    { "DRI_UserID", DBNull.Value },
                    { "Solution", DBNull.Value },
                    { "FinishedDate", DBNull.Value },
                    { "Remark", DBNull.Value }
                };

                int reportID = _dbHelper.InsertDataReturnId("IT_RequestList", requestValues, transaction);

                if (reportID <= 0)
                {
                    throw new Exception("Insert IT_RequestList failed, no ID returned.");
                }

                // Submit workflow
                var wfRepo = new WF_Repository(_dbHelper);
                wfRepo.SubmitWorkflow(reportID, FLOW_ID_NEW_EMP_WIFI, "New employee Wi-Fi request submitted", transaction);

                _dbHelper.CommitTransaction();

                string lineGroupId = ConfigurationManager.AppSettings["LineGroupID"];
                var notifier = new LineNotificationModel();
                await notifier.SendLineGroupMessageAsync(lineGroupId, lineNotifyMessageBuilder.ToString());

                ShowAlert("Onboard Wi-Fi Usage Request submitted successfully!");
                Response.Redirect("~/Default.aspx");
            }
            catch (Exception ex)
            {
                _dbHelper.RollbackTransaction();
                ShowAlert($"Error: {ex.Message}");
            }
        }

        protected async void btnBizTripSubmit_Click(object sender, EventArgs e)
        {
            string userName = Session["UserName"]?.ToString();
            string userEmpID = Session["UserEmpID"]?.ToString();

            int? requestUserID = _ui.GetRequestUserID(userName, userEmpID);

            if (requestUserID == null)
            {
                ShowAlert("RequestUserID could not be determined.");
                return;
            }

            var transaction = _dbHelper.BeginTransaction();
            try
            {
                StringBuilder lineNotifyMessageBuilder = new StringBuilder();
                lineNotifyMessageBuilder.AppendLine($"Requester: {userName}");
                lineNotifyMessageBuilder.AppendLine($"Department: {Session["DeptName"]}");
                lineNotifyMessageBuilder.AppendLine($"Request Date: {DateTime.Now:yyyy/MM/dd}");
                lineNotifyMessageBuilder.AppendLine("Business Trip Wi-Fi Requests:");

                // Build detailed information
                StringBuilder detailsBuilder = new StringBuilder();
                detailsBuilder.AppendLine("Business Trip Wi-Fi Requests:");
                for (int i = 1; i <= (int)ViewState["BizTripTable_RowCount"]; i++)
                {
                    HtmlTableRow row = BizTripTable.Rows[i] as HtmlTableRow;

                    string fullName = GetTextBoxValueFromHtmlCell(row.Cells[1]);
                    string employeeID = GetTextBoxValueFromHtmlCell(row.Cells[2]);
                    string department = GetTextBoxValueFromHtmlCell(row.Cells[3]);
                    string deviceType = GetTextBoxValueFromHtmlCell(row.Cells[4]);
                    string macAddress = GetTextBoxValueFromHtmlCell(row.Cells[5]);
                    string startDate = GetTextBoxValueFromHtmlCell(row.Cells[6]);
                    string endDate = GetTextBoxValueFromHtmlCell(row.Cells[7]);

                    if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(employeeID) || string.IsNullOrEmpty(startDate))
                    {
                        _dbHelper.RollbackTransaction();
                        ShowAlert($"Row {i}: Full Name, Employee ID, and Start Date are required!");
                        return;
                    }

                    detailsBuilder.AppendLine($"Request {i}:");
                    detailsBuilder.AppendLine($"- Full Name: {fullName}");
                    detailsBuilder.AppendLine($"- Employee ID: {employeeID}");
                    detailsBuilder.AppendLine($"- Department: {department}");
                    detailsBuilder.AppendLine($"- Device Type: {deviceType}");
                    detailsBuilder.AppendLine($"- MAC Address: {macAddress}");
                    detailsBuilder.AppendLine($"- Start Date: {startDate}");
                    detailsBuilder.AppendLine($"- End Date: {endDate}");
                    detailsBuilder.AppendLine();

                    lineNotifyMessageBuilder.AppendLine($"- Row {i}:");
                    lineNotifyMessageBuilder.AppendLine($"  Full Name: {fullName}");
                    lineNotifyMessageBuilder.AppendLine($"  Employee ID: {employeeID}");
                    lineNotifyMessageBuilder.AppendLine($"  Department: {department}");
                    lineNotifyMessageBuilder.AppendLine($"  Device Type: {deviceType}");
                    lineNotifyMessageBuilder.AppendLine($"  MAC Address: {macAddress}");
                    lineNotifyMessageBuilder.AppendLine($"  Start Date: {startDate}");
                    lineNotifyMessageBuilder.AppendLine($"  End Date: {endDate}");
                }

                // Create single request with all details
                var requestValues = new Dictionary<string, object>
                {
                    { "IssueDate", DateTime.Now },
                    { "DeptNameID", Session["DeptName"].ToString() },
                    { "CompanyID", "ENR" },
                    { "RequestUserID", requestUserID },
                    { "IssueDetails", detailsBuilder.ToString() },
                    { "IssueTypeID", FLOW_ID_BIZ_TRIP_WIFI },
                    { "Status", false },
                    { "LastUpdateDate", DateTime.Now },
                    { "DRI_UserID", DBNull.Value },
                    { "Solution", DBNull.Value },
                    { "FinishedDate", DBNull.Value },
                    { "Remark", DBNull.Value }
                };

                int reportID = _dbHelper.InsertDataReturnId("IT_RequestList", requestValues, transaction);

                if (reportID <= 0)
                {
                    throw new Exception("Insert IT_RequestList failed, no ID returned.");
                }

                // Submit workflow
                var wfRepo = new WF_Repository(_dbHelper);
                wfRepo.SubmitWorkflow(reportID, FLOW_ID_BIZ_TRIP_WIFI, "Business trip Wi-Fi request submitted", transaction);

                _dbHelper.CommitTransaction();

                string lineGroupId = ConfigurationManager.AppSettings["LineGroupID"];
                var notifier = new LineNotificationModel();
                await notifier.SendLineGroupMessageAsync(lineGroupId, lineNotifyMessageBuilder.ToString());

                ShowAlert("Business Trip Wi-Fi Usage Request submitted successfully!");
                Response.Redirect("~/Default.aspx");
            }
            catch (Exception ex)
            {
                _dbHelper.RollbackTransaction();
                ShowAlert($"Error: {ex.Message}");
            }
        }
        protected string GetLabel(string key)
        {
            string lang = Session["lang"]?.ToString() ?? "th";

            var th = new Dictionary<string, string> {
        { "title", "แบบฟอร์มขอใช้งาน Wi-Fi" },
        { "requester", "ชื่อผู้ขอ" },
        { "department", "แผนก" },
        { "issuedate", "วันที่ขอ" },
        { "visitor", "ผู้มาติดต่อ" },
        { "company", "บริษัท" },
        { "businesstrip", "ออกงานนอกสถานที่" },
        { "newemployee", "พนักงานใหม่" },
        { "visitorrequest", "คำขอสำหรับผู้มาติดต่อ" },
        { "biztriprequest", "คำขอสำหรับออกงานนอกสถานที่" },
        { "onboardrequest", "คำขอสำหรับพนักงานใหม่" },
        { "no", "ลำดับ" },
        { "fullname", "ชื่อ - นามสกุล" },
        { "empid", "รหัสพนักงาน" },
        { "email", "อีเมล" },
        { "devicetype", "ประเภทอุปกรณ์" },
        { "mac", "MAC Address" },
        { "description", "รายละเอียด" },
        { "addrow", "+ เพิ่มแถว" },
        { "submit", "ส่งคำขอ" },
        { "startdate", "วันที่เริ่มต้น" },
        { "enddate", "วันที่สิ้นสุด" },
    };

            var en = new Dictionary<string, string> {
        { "title", "Wi-Fi Usage Request Form" },
        { "requester", "Requester Name" },
        { "department", "Department" },
        { "issuedate", "Request Date" },
        { "visitor", "Visitor Request" },
        { "company", "Company" },
        { "businesstrip", "Business Trip" },
        { "newemployee", "New Employee" },
        { "visitorrequest", "Visitor Request" },
        { "biztriprequest", "Business Trip Request" },
        { "onboardrequest", "New Employee Request" },
        { "no", "No." },
        { "fullname", "Full Name" },
        { "empid", "Employee ID" },
        { "email", "Email" },
        { "devicetype", "Device Type" },
        { "mac", "MAC Address" },
        { "description", "Description" },
        { "addrow", "+ Add Row" },
        { "submit", "Submit Request" },
        { "startdate", "Start Date" },
        { "enddate", "End Date" }
    };

            var zh = new Dictionary<string, string> {
        { "title", "無線網路使用申請單" },
        { "requester", "申請者姓名" },
        { "department", "部門" },
        { "issuedate", "申請日期" },
        { "visitor", "訪客需求" },
        { "company", "公司" },
        { "businesstrip", "出差" },
        { "newemployee", "新進員工" },
        { "visitorrequest", "訪客需求" },
        { "biztriprequest", "出差申請" },
        { "onboardrequest", "新進員工需求" },
        { "no", "編號" },
        { "fullname", "全名" },
        { "empid", "工號" },
        { "email", "電子信箱" },
        { "devicetype", "設備類別" },
        { "mac", "硬體位址" },
        { "description", "描述" },
        { "addrow", "新增" },
        { "submit", "送出" },
        { "startdate", "開始日期" },
        { "enddate", "結束日期" },
    };

            Dictionary<string, string> dict;
            if (lang == "zh") dict = zh;
            else if (lang == "th") dict = th;
            else dict = en;

            return dict.ContainsKey(key) ? dict[key] : key;
        }

    }
}