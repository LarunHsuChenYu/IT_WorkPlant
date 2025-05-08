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

namespace IT_WorkPlant.Pages
{
    public partial class PMC_WO_HeadUpdate : System.Web.UI.Page
    {
        private static DataTable PendingRows;
        private static int CurrentRowIndex;

        private readonly ExcelHelper _excelHelper = new ExcelHelper();
        private readonly WO_UpdateService _updateService;

        public PMC_WO_HeadUpdate()
        {
            string connString = ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString;
            _updateService = new WO_UpdateService(connString);
        }

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
            }

            string deptName = Session["DeptName"]?.ToString();
            if (deptName != "PC" && deptName != "IT")
            {
                ShowAlertAndRedirect(
                    "You Didn't Allow to Visit this Function. \n Please check your access by IT!!",
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
                        string filePath = _excelHelper.SaveUploadedFile(FileUpload1.PostedFile, Server);
                        DataTable dtExcel = _excelHelper.ReadExcelData(filePath);
                        AddStatusColumn(dtExcel);

                        PendingRows = dtExcel;
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

        // Timer tick event handling database update and progress display
        protected void Timer1_Tick(object sender, EventArgs e)
        {
            if (PendingRows == null || CurrentRowIndex >= PendingRows.Rows.Count)
            {
                _Timer1.Enabled = false;
                _lblStatus.Text = "Processing completed.";
                ShowAlert("Processing completed successfully!");
                return;
            }

            DataRow row = PendingRows.Rows[CurrentRowIndex];

            try
            {
                _updateService.UpdateDatabase(row);
                row["Status"] = "Success";
            }
            catch (Exception ex)
            {
                row["Status"] = "Error: " + ex.Message;
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

        private void AddStatusColumn(DataTable dt)
        {
            if (!dt.Columns.Contains("Status"))
            {
                dt.Columns.Add("Status", typeof(string));
            }

            foreach (DataRow row in dt.Rows)
            {
                row["Status"] = "Pending";
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