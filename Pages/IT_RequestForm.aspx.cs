using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
                if (string.IsNullOrEmpty(UserEmpID))
                {
                    Response.Redirect("../Login.aspx");
                }

                txtDate.Text = DateTime.Now.ToString("yyyy/MM/dd");
                txtName.Text = UserName;
                txtDept.Text = DeptName;

                BindCategoryDropDown();
            }
        }

        protected async void SubmitForm(object sender, EventArgs e)
        {
            int? requestUserID = _ui.GetRequestUserID(UserName, UserEmpID);
            if (requestUserID == null)
            {
                ShowAlert("RequestUserID could not be determined.");
                return;
            }
            

            if (string.IsNullOrEmpty(ddlCategory.SelectedValue))
            {
                ShowAlert("กรุณาเลือกประเภทปัญหา (Issue Type) ก่อนส่งฟอร์ม");
                return;
            }

            string imagePath = null;

            // ✅ Check and save the image to the UploadedImages folder // ตรวจสอบและบันทึกภาพลงในโฟลเดอร์ UploadedImages
            if (fileUploadImage.HasFile)
            {
                string fileExtension = System.IO.Path.GetExtension(fileUploadImage.FileName).ToLower();

                // ✅ .jpg, .jpeg, .png only !
                if (fileExtension != ".jpg" && fileExtension != ".jpeg" && fileExtension != ".png")
                {
                    ShowAlert("อนุญาตเฉพาะไฟล์รูปภาพ .jpg, .jpeg หรือ .png เท่านั้น");
                    return;
                }

                // ✅ Create a new filename by generating a GUID.// สร้างชื่อไฟล์ใหม่ด้วย GUID
                string fileName = Guid.NewGuid().ToString() + fileExtension;
                fileName = System.IO.Path.GetFileName(fileName); // กัน path แฝง

                // ✅ Prepare the destination path // เตรียม path ปลายทาง
                string uploadFolder = Server.MapPath("~/App_Temp/");
                string serverPath = System.IO.Path.Combine(uploadFolder, fileName);
                System.Diagnostics.Debug.WriteLine("📁 Image saved to: " + serverPath);
                try
                {
                    // ✅ Create the directory if it does not already exist // สร้างโฟลเดอร์ถ้ายังไม่มี
                    if (!System.IO.Directory.Exists(uploadFolder))
                    {
                        System.IO.Directory.CreateDirectory(uploadFolder);
                    }

                    // ✅ Save the file // บันทึกไฟล์
                    fileUploadImage.SaveAs(serverPath);
                    imagePath = fileName;
                }
                catch (Exception ex)
                {
                    ShowAlert("เกิดข้อผิดพลาดในการบันทึกรูปภาพ: " + ex.Message);
                    return;
                }
            }

            // ✅ Prepare the data to be stored in the database // เตรียมข้อมูลบันทึกลงฐานข้อมูล
            var columnValues = new Dictionary<string, object>
            {
                { "IssueDate", DateTime.Parse(txtDate.Text) },
                { "DeptNameID", DeptName },
                { "CompanyID", "ENR" },
                { "RequestUserID", requestUserID },
                { "IssueDetails", HttpUtility.HtmlEncode(txtDescription.Text) },
                { "IssueTypeID", ddlCategory.SelectedValue },
                { "Status", false },
                { "LastUpdateDate", DateTime.Now },
                { "DRI_UserID", DBNull.Value },
                { "Solution", DBNull.Value },
                { "FinishedDate", DBNull.Value },
                { "Remark", DBNull.Value },
                { "ImagePath", imagePath ?? (object)DBNull.Value }
            };

            try
            {
                int rowsInserted = _dbHelper.InsertData("IT_RequestList", columnValues);
                ShowAlert(rowsInserted > 0 ? "Request added successfully." : "Failed to add request.");
            }
            catch (Exception ex)
            {
                ShowAlert($"Error: {ex.Message}");
                return;
            }

            // ✅ Send text-only via LINE without image attachment // ส่งLINEเฉพาะข้อความ ไม่แนบรูป
            string sNotifyMsg =
                $"Date: {DateTime.Now:yyyy/MM/dd}\n" +
                $"Department: {DeptName}\n" +
                $"Name: {UserName}\n" +
                $"Category: {ddlCategory.SelectedItem.Text}\n" +
                $"Description: {txtDescription.Text}";

            try
            {
                var notifier = new LineNotificationModel();
                string lineGroupId = ConfigurationManager.AppSettings["LineGroupID"];
                await notifier.SendLineGroupMessageAsync(lineGroupId, sNotifyMsg);
                ShowAlert("Successfully sent the notification to the LINE group 🎉");
            }
            catch (Exception ex)
            {
                ShowAlert($"Failed to send the LINE notification: {ex.Message}");
            }

            Response.Redirect("~/Default.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        private void BindCategoryDropDown()
        {
            string query = "SELECT IssueTypeID, IssueTypeCode, IssueTypeName FROM IssueType WHERE IssueTypeID <= 4 ORDER BY IssueTypeID";
            DataTable dt = _dbHelper.ExecuteQuery(query, null);

            dt.Columns.Add("DisplayText", typeof(string), "IssueTypeCode + ' ' + IssueTypeName");
            ddlCategory.DataSource = dt;
            ddlCategory.DataTextField = "DisplayText";
            ddlCategory.DataValueField = "IssueTypeID";
            ddlCategory.DataBind();
            ddlCategory.Items.Insert(0, new ListItem("Select Issue Type", ""));
        }

        protected void CancelForm(object sender, EventArgs e)
        {
            ShowAlertAndRedirect("Request cancelled.", "~/Default.aspx");
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
    }
}
