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

namespace IT_WorkPlant.Pages
{
	public partial class PUR_Vanguard_Price_Update : System.Web.UI.Page
	{

        private static DataTable PendingRows;
        private static int CurrentRowIndex;
        private readonly OracleDatabaseHelper _dbHelper;
        private readonly ExcelHelper _excelHelper = new ExcelHelper();

        // Access other controls using underscore-prefixed properties

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
                ShowAlertAndRedirect(
                    "You Didn't Allow to Visit this Function. \n Please check your access by IT!!",
                    "../Default.aspx"
                );
                return;
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
                        string filePath = _excelHelper.SaveUploadedFile(FileUpload1.PostedFile, Server);
                        
                        PendingRows = AddStatusColumn(_excelHelper.ReadExcelData(filePath)); 

                        CurrentRowIndex = 0;

                        GridView1.DataSource = PendingRows.Clone();
                        GridView1.DataBind();

                        lblStatus.Text = "Start processing...";
                        Timer1.Enabled = true;
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

        protected void Timer1_Tick(object sender, EventArgs e)
        {
            if (PendingRows == null || CurrentRowIndex >= PendingRows.Rows.Count)
            {
                Timer1.Enabled = false;
                lblStatus.Text = "Processing completed.";
                ShowAlert("Processing completed successfully!");
                return;
            }

            DataRow row = PendingRows.Rows[CurrentRowIndex];

            string rvbSql = "UPDATE DS5.rvb_file SET rvb10=:rvb10, rvb10t=:rvb10t WHERE rvb01=:rvb01 AND rvb02=:rvb02";
            string rvvSql = "UPDATE DS5.rvv_file SET rvv38=:rvv38, rvv38t=:rvv38t, rvv39=:rvv39 WHERE rvv04=:rvv04 AND rvv05=:rvv05";
            string pmnSql = "UPDATE DS5.pmn_file SET pmn31=:pmn31, pmn31t=:pmn31t WHERE pmn01=:pmn01 AND pmn02=:pmn02";
                        
            var parametersRvb = new[]
            {
                new OracleParameter("rvb10", row[5]),
                new OracleParameter("rvb10t", row[5]),
                new OracleParameter("rvb01", row[0]),
                new OracleParameter("rvb02", row[1])
            };

            var parametersRvv = new[]
            {
                new OracleParameter("rvv38", row[5]),
                new OracleParameter("rvv38t", row[5]),
                new OracleParameter("rvv39", Convert.ToDecimal(row[5]) * Convert.ToDecimal(row[4]) ),
                new OracleParameter("rvv04", row[0]),
                new OracleParameter("rvv05", row[1])
            };

            var parametersPmn = new[]
            {
                new OracleParameter("pmn31", row[5]),
                new OracleParameter("pmn31t",row[5]),
                new OracleParameter("pmn01", row[2]),
                new OracleParameter("pmn02", row[3])
            };

            try
            {
                int rvbRows = 0;
                int rvvRows = 0;
                int pmnRows = 0;
                OracleDatabaseHelper _dbHelper = new OracleDatabaseHelper(ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString);

                rvbRows = _dbHelper.ExecuteNonQuery(rvbSql, parametersRvb);
                rvvRows = _dbHelper.ExecuteNonQuery(rvvSql, parametersRvv);
                pmnRows = _dbHelper.ExecuteNonQuery(pmnSql, parametersPmn);

                
                row["Status"] = (rvbRows > 0 && rvvRows > 0 && pmnRows > 0) ? "Success" : "No Match Found";
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

            GridView1.DataSource = currentData;
           GridView1.DataBind();

            int percent = (int)(((double)(CurrentRowIndex + 1) / PendingRows.Rows.Count) * 100);
            progressBar.Style["width"] = percent + "%";

            CurrentRowIndex++;
        }

        private DataTable AddStatusColumn(DataTable dt)
        {
            // 確保只提取出您需要的欄位：D、E、F、G、T、AG
            DataTable filteredDt = dt.DefaultView.ToTable(false, 
                dt.Columns[3].ColumnName,  //D 
                dt.Columns[4].ColumnName,  //E 
                dt.Columns[5].ColumnName,  //F 
                dt.Columns[6].ColumnName,  //G
                dt.Columns[19].ColumnName, //T 
                dt.Columns[32].ColumnName);//AG

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
        private void ShowAlertAndRedirect(string message, string redirectUrl)
        {
            string safeMessage = HttpUtility.JavaScriptStringEncode(message);
            string script = $@"
        alert('{safeMessage}');
        window.location.href = '{redirectUrl}';
    ";

            if (!ClientScript.IsStartupScriptRegistered("redirectScript"))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "redirectScript", script, true);
            }
        }

    }
}