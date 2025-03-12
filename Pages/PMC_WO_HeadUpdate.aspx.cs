using System;
using System.Data;
using System.Configuration;
using System.Web.UI;
using IT_WorkPlant.Models;
using System.IO;
using System.Web;

namespace IT_WorkPlant.Pages
{
    public partial class PMC_WO_HeadUpdate : System.Web.UI.Page
    {
        private readonly ExcelHelper _excelHelper = new ExcelHelper();
        private readonly WO_UpdateService _updateService;

        public PMC_WO_HeadUpdate()
        {
            string connString = ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString;
            _updateService = new WO_UpdateService(connString);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserEmpID"] == null)
            {
                Response.Redirect("../Login.aspx");
            }

            string deptName = Session["DeptName"]?.ToString();
            if (deptName != "PC" && deptName != "IT")
            {
                ShowAlertAndRedirect(
                    "You Didn't Allow to Visit this Function. \r\n Please check your access by IT!!",
                    "../Default.aspx"
                );
            }
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            if (FileUpload1.HasFile)
            {
                string fileExtension = Path.GetExtension(FileUpload1.FileName).ToLower();

                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    try
                    {
                        // 使用 ExcelHelper 儲存文件
                        string filePath = _excelHelper.SaveUploadedFile(FileUpload1.PostedFile, Server);

                        // 使用 ExcelHelper 解析文件
                        DataTable dtExcel = _excelHelper.ReadExcelData(filePath);

                        // 執行頁面特定邏輯
                        AddStatusColumn(dtExcel);

                        // 更新資料庫
                        foreach (DataRow row in dtExcel.Rows)
                        {
                            _updateService.UpdateDatabase(row);
                        }
                        
                        // 綁定數據到 GridView
                        GridView1.DataSource = dtExcel;
                        GridView1.DataBind();

                        lblStatus.Text = "File uploaded and processed successfully."+ dtExcel.Rows.Count.ToString();
                        ShowAlert(lblStatus.Text);

                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = "Error: " + ex.Message;
                    }
                }
                else
                {
                    lblStatus.Text = "Only Excel files (.xls, .xlsx) are allowed.";
                }
            }
            else
            {
                lblStatus.Text = "Please select a file to upload.";
            }
        }

        private void AddStatusColumn(DataTable dt)
        {
            if (!dt.Columns.Contains("Status"))
            {
                dt.Columns.Add("Status", typeof(string));
            }

            foreach (DataRow row in dt.Rows)
            {
                row["Status"] = "Pending"; // 默認值
            }
        }

        private void ShowAlertAndRedirect(string message, string redirectUrl = null)
        {
            string script = string.IsNullOrEmpty(redirectUrl)
                ? $"alert('{message.Replace("\r\n", "\\n").Replace("'", "\\'")}');"
                : $@"alert('{message.Replace("\r\n", "\\n").Replace("'", "\\'")}'); window.location = '{redirectUrl}';";

            ScriptManager.RegisterStartupScript(this, this.GetType(), "alertRedirect", script, true);
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

    }
}