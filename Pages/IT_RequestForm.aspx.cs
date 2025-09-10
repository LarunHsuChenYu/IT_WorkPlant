using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using IT_WorkPlant.Models;

namespace IT_WorkPlant.Pages
{
    public partial class IT_RequestForm : System.Web.UI.Page
    {
        private readonly MssqlDatabaseHelper _dbHelper = new MssqlDatabaseHelper();
        private readonly UserInfo _ui = new UserInfo();

        private string UserName => Session["UserName"]?.ToString() ?? "";
        private string UserEmpID => Session["UserEmpID"]?.ToString() ?? "";
        private string DeptName => Session["DeptName"]?.ToString() ?? "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblTitle.Text = GetLabel("title");
                lblName.Text = GetLabel("requestorname");
                lblDept.Text = GetLabel("department");
                lblDate.Text = GetLabel("issuedate");
                lblTitle.Text = GetLabel("title");
                lblName.Text = GetLabel("requestorname");
                lblDept.Text = GetLabel("department");
                lblDate.Text = GetLabel("issuedate");
                btnSubmit.Text = GetLabel("submit");
                btnCancel.Text = GetLabel("cancel");
                if (string.IsNullOrEmpty(UserEmpID))
                {
                    Response.Redirect("../Login.aspx");
                }

                txtDate.Text = DateTime.Now.ToString("yyyy/MM/dd");
                txtName.Text = UserName;
                txtDept.Text = DeptName;

                BindEmptyRows(); // เติมแถวว่าง 5 แถว
            }
        }

        private void BindEmptyRows()
        {
            var emptyRows = new List<object>();
            for (int i = 0; i < 5; i++)
                emptyRows.Add(new object());

            rptRequestItems.DataSource = emptyRows;
            rptRequestItems.DataBind();
        }

        protected void rptRequestItems_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var ddl = e.Item.FindControl("ddlIssueType") as DropDownList;
                if (ddl != null)
                {
                    string query = "SELECT IssueTypeID, IssueTypeCode, IssueTypeName FROM IssueType WHERE IssueTypeID <= 4 ORDER BY IssueTypeID";
                    DataTable dt = _dbHelper.ExecuteQuery(query, null);
                    dt.Columns.Add("DisplayText", typeof(string), "IssueTypeCode + ' ' + IssueTypeName");

                    ddl.DataSource = dt;
                    ddl.DataTextField = "DisplayText";
                    ddl.DataValueField = "IssueTypeID";
                    ddl.DataBind();
                    ddl.Items.Insert(0, new ListItem(GetLabel("selectissuetype"), ""));
                }
            }
        }

        protected async void SubmitForm(object sender, EventArgs e)
        {
            int? requestUserID = _ui.GetRequestUserID(UserName, UserEmpID);
            if (requestUserID == null)
            {
                ShowAlert("Unable to identify user");
                return;
            }

            var validItems = new List<(string issueTypeText, string issueTypeID, string description, string imagePath)>();

            // ✅ เพิ่มตัวแปรลำดับไฟล์ (เปลี่ยนแค่ชื่อไฟล์เท่านั้น ส่วนอื่นคงเดิม)
            int seq = 1;

            foreach (RepeaterItem item in rptRequestItems.Items)
            {
                var ddl = item.FindControl("ddlIssueType") as DropDownList;
                var txt = item.FindControl("txtDescription") as TextBox;
                var fileUpload = item.FindControl("fileUploadImage") as FileUpload;

                if (ddl == null || txt == null)
                    continue;

                if (string.IsNullOrWhiteSpace(txt.Text) && string.IsNullOrEmpty(ddl.SelectedValue))
                    continue;
                string imagePath = null;
                if (fileUpload != null && fileUpload.HasFile)
                {
                    string ext = Path.GetExtension(fileUpload.FileName).ToLower();
                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                    {
                        // ตั้งชื่อแบบ ชม.นาที + ลำดับ
                        string baseName = $"{DateTime.Now:yyyyMMdd_HHmm}-S{seq:D2}";
                        string fileName = baseName + ext;

                        // โฟลเดอร์เดิม App_Temp (ต้องมีตัวแปร folder + เช็กสร้าง)
                        string folder = Server.MapPath("~/App_Temp/");
                        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                        // กันชื่อชน: ถ้ามีอยู่แล้ว เติม -x2, -x3 ...
                        string savePath = Path.Combine(folder, fileName);
                        int i = 2;
                        while (File.Exists(savePath))
                        {
                            fileName = $"{baseName}-x{i}{ext}";
                            savePath = Path.Combine(folder, fileName);
                            i++;
                        }

                        fileUpload.SaveAs(savePath);
                        imagePath = fileName;  // เก็บเฉพาะชื่อไฟล์เหมือนเดิม

                        seq++; // ไฟล์ถัดไปเพิ่มลำดับ
                    }
                }


                validItems.Add((ddl.SelectedItem.Text, ddl.SelectedValue, txt.Text, imagePath));
            }

            if (validItems.Count == 0)
            {
                ShowAlert("Please enter at least 1 issue");
                return;
            }

            foreach (var item in validItems)
            {
                var columnValues = new Dictionary<string, object>
                {
                    { "IssueDate", DateTime.Parse(txtDate.Text) },
                    { "DeptNameID", DeptName },
                    { "CompanyID", "ENR" },
                    { "RequestUserID", requestUserID },
                    { "IssueDetails", HttpUtility.HtmlEncode(item.description) },
                    { "IssueTypeID", item.issueTypeID },
                    { "Status", false },
                    { "LastUpdateDate", DateTime.Now },
                    { "DRI_UserID", DBNull.Value },
                    { "Solution", DBNull.Value },
                    { "FinishedDate", DBNull.Value },
                    { "Remark", DBNull.Value },
                    { "ImagePath", item.imagePath ?? (object)DBNull.Value }
                };

                try
                {
                    _dbHelper.InsertData("IT_RequestList", columnValues);
                }
                catch (Exception ex)
                {
                    ShowAlert($"Error saving data: {ex.Message}");
                    return;
                }
            }

            string sNotifyMsg = $"Date: {DateTime.Now:yyyy/MM/dd}\nDepartment: {DeptName}\nName: {UserName}";
            for (int i = 0; i < validItems.Count; i++)
            {
                sNotifyMsg += $"\n\n#{i + 1}\nCategory: {validItems[i].issueTypeText}\nDescription: {validItems[i].description}";
            }

            try
            {
                var notifier = new LineNotificationModel();
                string lineGroupId = ConfigurationManager.AppSettings["LineGroupID"];
                await notifier.SendLineGroupMessageAsync(lineGroupId, sNotifyMsg);
            }
            catch (Exception ex)
            {
                ShowAlert($"Failed to send LINE notification: {ex.Message}");
            }

            string script = $@"
    Swal.fire({{
        icon: 'success',
        title: 'Issue Submitted Successfully!',
        text: 'Your issue has been recorded. Thank you for your submission.',
        confirmButtonText: 'OK',
        confirmButtonColor: '#6c5ce7'
    }}).then((result) => {{
        if (result.isConfirmed) {{
            window.location.href = '{ResolveUrl("~/Pages/IT_RequestsList.aspx")}';
        }}
    }});
";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "swalSuccess", script, true);

        }

        protected void CancelForm(object sender, EventArgs e)
        {
            ShowAlertAndRedirect("Request cancelled.", ResolveUrl("~/Pages/IT_RequestsList.aspx"));
        }

        private void ShowAlertAndRedirect(string message, string redirectUrl)
        {
            string safeMessage = HttpUtility.JavaScriptStringEncode(message);
            string script = $@"
                alert('{safeMessage}');
                window.location.href = '{redirectUrl}';
            ";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alertRedirect", script, true);
        }

        private void ShowAlert(string message)
        {
            string safeMessage = HttpUtility.JavaScriptStringEncode(message);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", $"alert('{safeMessage}');", true);
        }
        protected string GetLabel(string key)
        {
            string lang = Session["lang"]?.ToString() ?? "en";

            var th = new Dictionary<string, string>
            {
                ["title"] = "แบบฟอร์มคำร้อง IT",
                ["requestorname"] = "ชื่อผู้ขอ",
                ["department"] = "แผนก",
                ["issuedate"] = "วันที่",
                ["no"] = "ลำดับ",
                ["issuetype"] = "ประเภทปัญหา",
                ["description"] = "รายละเอียด",
                ["attachment"] = "แนบรูปภาพ",
                ["submit"] = "ส่งคำขอ",
                ["cancel"] = "ยกเลิก",
                ["selectissuetype"] = "เลือกประเภทปัญหา",
                ["choosefile"] = "เลือกไฟล์"
            };

            var en = new Dictionary<string, string>
            {
                ["title"] = "IT Request Form",
                ["requestorname"] = "Requestor Name",
                ["department"] = "Department",
                ["issuedate"] = "Issue Date",
                ["no"] = "No.",
                ["issuetype"] = "Issue Type",
                ["description"] = "Issue Description",
                ["attachment"] = "Attachment (Image)",
                ["submit"] = "Submit Request",
                ["cancel"] = "Cancel",
                ["selectissuetype"] = "Select Issue Type",
                ["choosefile"] = "Choose File"
            };

            var zh = new Dictionary<string, string>
            {
                ["title"] = "資訊相關申請",
                ["requestorname"] = "申請者姓名",
                ["department"] = "部門",
                ["issuedate"] = "事件發生日",
                ["no"] = "編號",
                ["issuetype"] = "事件類型",
                ["description"] = "事件描述",
                ["attachment"] = "圖片附件",
                ["submit"] = "確認需求",
                ["cancel"] = "取消",
                ["selectissuetype"] = "選擇需求類型",
                ["choosefile"] = "選擇檔案"
            };

            Dictionary<string, string> dict;
            if (lang == "th") dict = th;
            else if (lang == "zh") dict = zh;
            else dict = en;

            return dict.ContainsKey(key) ? dict[key] : key;
        }
    }
}
