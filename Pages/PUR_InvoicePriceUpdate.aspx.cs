using System;
using System.Data;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using IT_WorkPlant.Models;
using System.IO;
using System.Web;
using MathNet.Numerics;
using Oracle.ManagedDataAccess.Client;
using DocumentFormat.OpenXml.Spreadsheet;
using OfficeOpenXml;
using System.Collections.Generic;

namespace IT_WorkPlant.Pages
{
	public partial class PUR_InvoicePriceUpdate : System.Web.UI.Page
	{
        private static DataTable PendingRows;
        private static int CurrentRowIndex;
        private readonly OracleDatabaseHelper _dbHelper;
        private readonly ExcelHelper _excelHelper = new ExcelHelper();

        // Access other controls using underscore-prefixed properties
        private Label _lblStatus => (Label)Master.FindControl("MainContent").FindControl("lblStatus");
        private Label _lblProgress => (Label)Master.FindControl("MainContent").FindControl("lblProgress");
        private HtmlGenericControl _progressBar => (HtmlGenericControl)Master.FindControl("MainContent").FindControl("progressBar");
        private GridView _GridView1 => (GridView)Master.FindControl("MainContent").FindControl("GridView1");
        private Timer _Timer1 => (Timer)Master.FindControl("MainContent").FindControl("Timer1");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserEmpID"] == null)
            {
                Response.Redirect("../Login.aspx");
                return;
            }

            string deptName = Session["DeptName"]?.ToString();
            if (deptName != "PU" && deptName != "IT")
            {
                string message = "You didn't have permission to visit this function.\\nPlease contact IT.";
                string script = $"alert('{message}'); window.location='../Default.aspx';";

                if (!ClientScript.IsStartupScriptRegistered("permissionAlert"))
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "permissionAlert", script, true);
                }

                return;
            }
        }
      protected void btnUpload_Click(object sender, EventArgs e)
        {
            if (FileUpload1.HasFile)
            {
                ExcelPackage.License.SetNonCommercialPersonal("Enrich_Testing");
                string fileExtension = Path.GetExtension(FileUpload1.FileName).ToLower();
                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    try
                    {
                        string filePath = _excelHelper.SaveUploadedFile(FileUpload1.PostedFile, Server);

                        PendingRows = AddStatusColumn(_excelHelper.ReadExcelData(filePath));

                        CurrentRowIndex = 0;

                        _GridView1.DataSource = PendingRows.Clone();
                        _GridView1.DataBind();

                        _lblStatus.Text = "Start processing...";
                        _Timer1.Enabled = true;
                    }
                    catch (Exception ex)
                    {
                        _lblStatus.Text = "Error: " + ex.Message;
                    }
                }
                else
                {
                    _lblStatus.Text = "Only Excel files (.xls, .xlsx) are allowed.";
                }
            }
            else
            {
                _lblStatus.Text = "Please select a file to upload.";
            }
        }

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            if (PendingRows == null || CurrentRowIndex >= PendingRows.Rows.Count)
            {
                _Timer1.Enabled = false;
                _lblStatus.Text = "Processing completed.";
                btnConfirmSubmit.Visible = true;
                ShowAlert("Processing completed successfully!");
                return;
            }

            DataRow row = PendingRows.Rows[CurrentRowIndex];

            string apbSql = @"
                UPDATE DS5.APB_FILE SET 
                    apb23 = :apb23,
                    apb24 = :apb24,
                    apb08 = :apb08,
                    apb081 = :apb081,
                    apb10 = :apb10,
                    apb101 = :apb101
                WHERE apb21 = :apb21 AND apb22 = :apb22";

            var paraApb = new[]
            {
                new OracleParameter("apb23", row[2]),
                new OracleParameter("apb24", row[3]),
                new OracleParameter("apb08", row[4]),
                new OracleParameter("apb081",row[4]),
                new OracleParameter("apb10", row[5]),
                new OracleParameter("apb101",row[5]),
                new OracleParameter("apb21", row[0]),
                new OracleParameter("apb22", row[1])
            };

            try
            {
                
                
                OracleDatabaseHelper _dbHelper = new OracleDatabaseHelper(ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString);

                int apbRows = _dbHelper.ExecuteNonQuery(apbSql, paraApb);
                

                row["Status"] = (apbRows>0) ? "Success" : "No Match Found";
            }
            catch (Exception ex)
            {
                // 如果發生例外，記錄失敗狀態和錯誤訊息
                row["Status"] = "Error: " + ex.Message;
                row["Error"] = ex.Message; // 記錄詳細錯誤訊息
            }


            DataTable currentData = PendingRows.Clone();
            for (int i = 0; i <= CurrentRowIndex; i++)
            {
                currentData.ImportRow(PendingRows.Rows[i]);
            }

            _GridView1.DataSource = currentData;
            _GridView1.DataBind();

            int percent = (int)(((double)(CurrentRowIndex + 1) / PendingRows.Rows.Count) * 100);
            _lblProgress.Text = $"Progress: {CurrentRowIndex + 1} / {PendingRows.Rows.Count} ({percent}%)";
            _progressBar.Style["width"] = percent + "%";

            CurrentRowIndex++;
        }

        private DataTable AddStatusColumn(DataTable dt)
        {
            DataTable filteredDt = dt.DefaultView.ToTable(false,
                dt.Columns[3].ColumnName,  //D 
                dt.Columns[4].ColumnName,  //E 
                dt.Columns[17].ColumnName, //R 
                dt.Columns[18].ColumnName, //S
                dt.Columns[19].ColumnName, //T 
                dt.Columns[20].ColumnName);//U

            // 在新表格中添加 Status 欄位
            filteredDt.Columns.Add("Status", typeof(string));

            // 將原 DataTable 中的每一行數據填入 filteredDt 並添加狀態
            foreach (DataRow row in filteredDt.Rows)
            {
                // 填充狀態欄位為 "Pending"
                row["Status"] = "Pending";
            }

            return filteredDt;
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
        protected void btnConfirmSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                string userName = Session["UserName"]?.ToString();
                string userEmpID = Session["UserEmpID"]?.ToString();
                string deptName = Session["DeptName"]?.ToString();

                var ui = new IT_WorkPlant.Models.UserInfo();
                int? requestUserID = ui.GetRequestUserID(userName, userEmpID);
                if (requestUserID == null)
                {
                    ShowAlert("❌ Error: requestUserID not found", "error", null);
                    return;
                }

                int driUserID = 26;

                var dbHelper = new MssqlDatabaseHelper();
                var columnValues = new Dictionary<string, object>
        {
            { "IssueDate", DateTime.Now },
            { "DeptNameID", deptName },
            { "CompanyID", "ENR" },
            { "RequestUserID", requestUserID },
            { "IssueDetails", "PUR Invoice Price Update" },
            { "IssueTypeID", 3 },
            { "Status", true },
            { "LastUpdateDate", DateTime.Now },
            { "DRI_UserID", driUserID },
            { "Solution", DBNull.Value },
            { "FinishedDate", DateTime.Now },
            { "Remark", DBNull.Value }
        };

                dbHelper.InsertData("IT_RequestList", columnValues);
                ShowAlert("✅Your price update has been submitted successfully!", "success", "/Default.aspx");
            }
            catch (Exception ex)
            {
                ShowAlert("❌ Error: " + ex.Message, "error", null);
            }
        }
        private void ShowAlert(string message, string icon, string redirectUrl)
        {
            string safeMessage = HttpUtility.JavaScriptStringEncode(message);
            string script = $@"
Swal.fire({{
    icon: '{icon}',
    title: '{(icon == "success" ? "Submitted!" : icon == "warning" ? "Warning!" : "Error!")}',
    text: '{safeMessage}',
    confirmButtonText: 'Close',
    customClass: {{
        confirmButton: 'btn btn-primary'
    }},
    buttonsStyling: false
}}).then((result) => {{
    {(redirectUrl != null ? $"window.location = '{redirectUrl}';" : "")}
}});";

            ScriptManager.RegisterStartupScript(this, this.GetType(), "sweetalert", script, true);
        }

    }
}