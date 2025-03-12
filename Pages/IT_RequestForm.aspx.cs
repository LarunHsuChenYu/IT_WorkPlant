using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;
using IT_WorkPlant.Models;

namespace IT_WorkPlant.Pages
{
    public partial class IT_RequestForm : System.Web.UI.Page
    {
        private readonly MssqlDatabaseHelper _dbHelper = new MssqlDatabaseHelper();
        private readonly UserInfo _ui = new UserInfo();
        protected void Page_Load(object sender, EventArgs e)
        {
            
            if (!IsPostBack)
            {

                // 檢查是否已登入
                if (Session["UserEmpID"] == null)
                {
                    // 未登入，重定向至登入頁面
                    Response.Redirect("../Login.aspx");
                }
                
                txtDate.Text = DateTime.Now.ToString("yyyy/MM/dd");
                txtName.Text = Session["UserName"].ToString();
                txtDept.Text = Session["DeptName"].ToString();
                BindCategoryDropDown();
            }
        }

        protected async void SubmitForm(object sender, EventArgs e)
        {
            string userName = Session["UserName"]?.ToString();
            string userEmpID = Session["UserEmpID"]?.ToString();

            int? requestUserID = _ui.GetRequestUserID(userName, userEmpID);

            if (requestUserID == null)
            {
                ShowAlert("RequestUserID could not be determined.");
                return;
            }

            // ตรวจสอบไฟล์อัปโหลด
            if (fileUploadImage.HasFile)
            {
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                string fileExtension = Path.GetExtension(fileUploadImage.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    ShowAlert("Only image files (.jpg, .jpeg, .png, .gif, .bmp, .webp) are allowed.");
                    return;
                }

                // ตรวจสอบประเภท MIME เพื่อป้องกันไฟล์ปลอม
                string[] allowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp" };
                if (!allowedMimeTypes.Contains(fileUploadImage.PostedFile.ContentType))
                {
                    ShowAlert("Invalid file type. Please upload an image.");
                    return;
                }
            }

            var columnValues = new Dictionary<string, object>
            {
                { "IssueDate", DateTime.Parse(txtDate.Text) },         // 必需：問題日期
                { "DeptNameID", Session["DeptName"].ToString() },      // 必需：部門名稱 ID
                { "CompanyID", "ENR" },                                // 必需：公司 ID
                { "RequestUserID", requestUserID },                    // 必需：請求者的用戶 ID
                { "IssueDetails", txtDescription.Text },               // 必需：問題描述
                { "IssueTypeID", ddlCategory.SelectedValue },          // 必需：問題類型 ID
                { "Status", false },                                   // 必需：狀態，初始為未完成 (false)
                { "LastUpdateDate", DateTime.Now },                    // 必需：最後更新日期
                { "DRI_UserID", DBNull.Value },                        // 可選：負責人的用戶 ID
                { "Solution", DBNull.Value },                          // 可選：解決方案
                { "FinishedDate", DBNull.Value },                      // 可選：完成日期
                { "Remark", DBNull.Value }                             // 可選：備註
            };

            try
            {
                int rowsInserted = _dbHelper.InsertData("IT_RequestList", columnValues);

                if (rowsInserted > 0)
                {
                    ShowAlert("Request added successfully.");
                }
                else
                {
                    ShowAlert("Failed to add request.");
                }
            }
            catch (Exception ex)
            {
                ShowAlert($"Error: {ex.Message}");
                return;
            }

            // 構建通知訊息
            string sNotifyMsg =
                $"\nDate: {DateTime.Now:yyyy/MM/dd}\n" +
                $"Department: {Session["DeptName"]}\n" +
                $"Name: {Session["UserName"]}\n" +
                $"Category: {ddlCategory.SelectedItem.Text}\n" +
                $"Description: {txtDescription.Text}";

            HttpPostedFile uploadedImage = fileUploadImage.HasFile ? fileUploadImage.PostedFile : null;

            // 從配置中讀取 Line Notify AccessToken
            string accessToken = System.Configuration.ConfigurationManager.AppSettings["LineNotifyAccessToken"];

            if (string.IsNullOrEmpty(accessToken))
            {
                ShowAlert("AccessToken not configured.");
                return;
            }

            // 發送通知
            try
            {
                var notifier = new LineNotificationModel();

                if (uploadedImage != null)
                {
                    await notifier.SendLineNotifyAsync(sNotifyMsg, uploadedImage);
                }
                else
                {
                    await notifier.SendLineNotifyAsync(sNotifyMsg);
                }

                ShowAlert("Notification sent successfully.");
            }
            catch (Exception ex)
            {
                ShowAlert($"Error sending notification: {ex.Message}");
            }

            // 跳轉回主頁面
            Response.Redirect("~/Default.aspx");
        }



        private void SendNotificationEmail(string name, string department, string category, string description)
        {
            string to = "it-support@example.com";
            string subject = "New IT Service Request Submitted";
            string body = $"Name: {name}\nDepartment: {department}\nCategory: {category}\nDescription: {description}\nDate: {DateTime.Now.ToString("yyyy-MM-dd")}";

            using (MailMessage mail = new MailMessage("no-reply@example.com", to))
            {
                mail.Subject = subject;
                mail.Body = body;

                using (SmtpClient client = new SmtpClient())
                {
                    client.Host = "smtp.example.com";
                    client.Port = 587;
                    client.Credentials = new System.Net.NetworkCredential("username", "password");
                    client.EnableSsl = true;
                    client.Send(mail);
                }
            }
        }

        
        private void BindCategoryDropDown()
        {
            // 從資料庫中查詢所有類型
            string query = "SELECT IssueTypeID, IssueTypeCode, IssueTypeName FROM IssueType ORDER BY IssueTypeID";
            DataTable dt = _dbHelper.ExecuteQuery(query, null);

            // 過濾前四項
            DataView filteredView = new DataView(dt)
            {
                RowFilter = "IssueTypeID <= 4" // 僅包含 IssueTypeID <= 4 的項目
            };

            // 添加計算列來合併 IssueTypeCode 和 IssueTypeName
            DataTable filteredTable = filteredView.ToTable();
            filteredTable.Columns.Add("DisplayText", typeof(string), "IssueTypeCode + ' ' + IssueTypeName");

            // 綁定下拉選單
            ddlCategory.DataSource = filteredTable;
            ddlCategory.DataTextField = "DisplayText";
            ddlCategory.DataValueField = "IssueTypeID";
            ddlCategory.DataBind();

            // 添加默認選項
            ddlCategory.Items.Insert(0, new ListItem("Select Issue Type", ""));
        }

        protected void CancelForm(object sender, EventArgs e)
        {
            // 取消操作，重定向到上一頁或首頁
            Response.Redirect("~/Default.aspx");
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
    }
}