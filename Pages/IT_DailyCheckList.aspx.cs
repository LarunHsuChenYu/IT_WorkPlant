using IT_WorkPlant.Models;
using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IT_WorkPlant.Pages
{
    public partial class IT_DailyCheckList : System.Web.UI.Page
    {
        private readonly MssqlDatabaseHelper _dbHelper = new MssqlDatabaseHelper();
        private readonly UserInfo _ui = new UserInfo();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["UserEmpID"] == null)
                {
                    // 未登入，重定向至登入頁面
                    Response.Redirect("../Login.aspx");
                }

                BindGridView();
            }
        }

        private void BindGridView()
        {
            var varServerCheckList = new List<CheckItem>
            {
                new CheckItem { Id = 1, Item = "AIS Fiber optic storage box", Details = "-", IsNumericInput = false },
                new CheckItem { Id = 2, Item = "UIH Fiber optic storage box", Details = "-", IsNumericInput = false },
                new CheckItem { Id = 3, Item = "AIS Router", Details = "110.49.28.184 /29", IsNumericInput = false },
                new CheckItem { Id = 4, Item = "UIH Router (Disable)", Details = "-", IsNumericInput = false },
                new CheckItem { Id = 5, Item = "SDWAN routing", Details = "192.168.120.2", IsNumericInput = false },
                new CheckItem { Id = 6, Item = "SDWAN Switch", Details = "192.168.120.20", IsNumericInput = false },
                new CheckItem { Id = 7, Item = "H3C AP controller", Details = "172.10.10.5", IsNumericInput = false },
                new CheckItem { Id = 8, Item = "H3C switch AP", Details = "172.10.10.7", IsNumericInput = false },
                new CheckItem { Id = 9, Item = "H3C switch", Details = "172.10.10.8", IsNumericInput = false },
                new CheckItem { Id = 10, Item = "H3C switch", Details = "172.10.10.9", IsNumericInput = false },
                new CheckItem { Id = 11, Item = "H3C switch", Details = "172.10.10.10", IsNumericInput = false },
                new CheckItem { Id = 12, Item = "AVAYA IP phone central control", Details = "192.168.30.237", IsNumericInput = false },
                new CheckItem { Id = 13, Item = "FH3C F1000-AI-55 Firewall", Details = "172.10.10.4", IsNumericInput = false },
                new CheckItem { Id = 14, Item = "H3C Internet Behavior Management F1000-AI-35", Details = "172.10.10.3", IsNumericInput = false },
                new CheckItem { Id = 15, Item = "H3C WEB application protection F1000-AI-25", Details = "172.10.10.2", IsNumericInput = false },
                new CheckItem { Id = 16, Item = "H3C 4700 G5 (IMC management)", Details = "192.168.30.20", IsNumericInput = false },
                new CheckItem { Id = 17, Item = "H3C R4900 G5 Proxy", Details = "192.168.32.109", IsNumericInput = false },
                new CheckItem { Id = 18, Item = "H3C R4900 G5 VM", Details = "192.168.32.111", IsNumericInput = false },
                new CheckItem { Id = 19, Item = "Lenovo Thinksystem SR550 VM VM IP audit", Details = "192.168.30.100, 192.168.32.101", IsNumericInput = false },
                new CheckItem { Id = 20, Item = "Synology RS1221RP+ Enrich Nas", Details = "192.168.30.167", IsNumericInput = false },
                new CheckItem { Id = 21, Item = "H3C S6520X-26C-SI switch", Details = "172.10.10.11", IsNumericInput = false },
                new CheckItem { Id = 22, Item = "H3C S7503E-M core switch", Details = "172.10.10.1", IsNumericInput = false },
                new CheckItem { Id = 23, Item = "Core switch and Fiber optic to Factory (Jetstor)", Details = "-", IsNumericInput = false },
                new CheckItem { Id = 24, Item = "KVM", Details = "-", IsNumericInput = false },
                new CheckItem { Id = 25, Item = "HR AP server", Details = "192.168.30.238", IsNumericInput = false },
                new CheckItem { Id = 26, Item = "HR DB server", Details = "192.168.30.239", IsNumericInput = false },
                new CheckItem { Id = 27, Item = "Central CMS hosting monitoring and management", Details = "192.168.31.245", IsNumericInput = false },
                new CheckItem { Id = 28, Item = "Backup server", Details = "192.168.31.244", IsNumericInput = false },
                new CheckItem { Id = 29, Item = "CCTV NVR1", Details = "192.168.31.241", IsNumericInput = false },
                new CheckItem { Id = 30, Item = "CCTV NVR2", Details = "192.168.31.242", IsNumericInput = false },
                new CheckItem { Id = 31, Item = "CCTV NVR3", Details = "192.168.31.243", IsNumericInput = false },
                new CheckItem { Id = 32, Item = "UPS 40KV controller", Details = "-", IsNumericInput = false },
                new CheckItem { Id = 33, Item = "硬盘录像机_DVR", Details = "192.168.32.211", IsNumericInput = false },
                new CheckItem { Id = 34, Item = "动环采集主机_Dynamic environment acquisition host", Details = "192.168.32.213", IsNumericInput = false },
                new CheckItem { Id = 35, Item = "动环服务器_Dynamic environment server", Details = "192.168.32.212", IsNumericInput = false },
                new CheckItem { Id = 36, Item = "电池管理主机_Battery management host", Details = "192.168.32.214", IsNumericInput = false },
                new CheckItem { Id = 37, Item = "电池屏_Battery screen", Details = "192.168.32.215", IsNumericInput = false }
            };


            var varUPSCheckList = new List<CheckItem>
            {
                // Air Conditioner Check List
                new CheckItem { Id = 1, Item = "Air Conditioner Check List", Details = "Temperature (°C)", IsNumericInput = true },
                new CheckItem { Id = 2, Item = "Air Conditioner Check List", Details = "Humidity (%)", IsNumericInput = true },
                new CheckItem { Id = 3, Item = "Air Conditioner Check List", Details = "Air conditioner status", IsNumericInput = false },

                // Electricity Check List
                new CheckItem { Id = 4, Item = "Electricity Check List", Details = "Electricity frequency (Hz)", IsNumericInput = true },
                new CheckItem { Id = 5, Item = "Electricity Check List", Details = "A-Phase (V)", IsNumericInput = true },
                new CheckItem { Id = 6, Item = "Electricity Check List", Details = "B-Phase (V)", IsNumericInput = true },
                new CheckItem { Id = 7, Item = "Electricity Check List", Details = "C-Phase (V)", IsNumericInput = true },
                new CheckItem { Id = 8, Item = "Electricity Check List", Details = "Electricity status", IsNumericInput = false },

                // UPS Check List
                new CheckItem { Id = 9, Item = "UPS Check List", Details = "A-Phase Input voltage (V)", IsNumericInput = true },
                new CheckItem { Id = 10, Item = "UPS Check List", Details = "A-Phase Output voltage (V)", IsNumericInput = true },
                new CheckItem { Id = 11, Item = "UPS Check List", Details = "Battery Backup Time (min)", IsNumericInput = true },
                new CheckItem { Id = 12, Item = "UPS Check List", Details = "Output frequency (Hz)", IsNumericInput = true },
                new CheckItem { Id = 13, Item = "UPS Check List", Details = "UPS status", IsNumericInput = false },
            };


            gvServerRoomCheck.DataSource = varServerCheckList;
            gvServerRoomCheck.DataBind();

            gvUPSCheck.DataSource = varUPSCheckList;
            gvUPSCheck.DataBind();
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            bool allStatusFilled = true; // 用於檢查狀態是否都有填寫

            string userName = Session["UserName"]?.ToString();
            string userEmpID = Session["UserEmpID"]?.ToString();

            int? requestUserID = _ui.GetRequestUserID(userName, userEmpID);

            // 檢查 gvServerRoomCheck 和 gvUPSCheck
            allStatusFilled = CheckGridViewStatus(gvServerRoomCheck) && CheckGridViewStatus(gvUPSCheck);

            if (!allStatusFilled)
            {
                ShowAlert("Please ensure that all Check-Items has been filled in.\r\n請確保所有檢查項目的狀態都已填寫。");
                return; // 結束執行
            }

            SaveGridViewData(gvServerRoomCheck, requestUserID, "ENR");
            SaveGridViewData(gvUPSCheck, requestUserID, "ENR");

            ShowAlert("Results saved successfully.");
        }

        private bool CheckGridViewStatus(GridView gridView)
        {
            foreach (GridViewRow row in gridView.Rows)
            {
                var phTextBox = row.FindControl("phTextBox") as PlaceHolder;
                var phRadioButtonList = row.FindControl("phRadioButtonList") as PlaceHolder;

                if (phTextBox != null && phTextBox.Visible)
                {
                    var txtNumericInput = phTextBox.FindControl("txtNumericInput") as TextBox;
                    if (txtNumericInput == null || string.IsNullOrEmpty(txtNumericInput.Text.Trim()))
                    {
                        return false;
                    }

                    if (!decimal.TryParse(txtNumericInput.Text.Trim(), out _))
                    {
                        return false;
                    }
                }
                else if (phRadioButtonList != null && phRadioButtonList.Visible)
                {
                    var rblStatus = phRadioButtonList.FindControl("rblStatus") as RadioButtonList;
                    if (rblStatus == null || string.IsNullOrEmpty(rblStatus.SelectedValue))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private void SaveGridViewData(GridView gridView, int? checkerId, string factoryId)
        {
            if (!CheckTodayRecord())
            {
                ShowAlert("Today's Daily Check has been completed. Please do not execute it again.\r\n今日例行檢查已完成，請勿重複執行。");
                return; // 結束執行
            }
            foreach (GridViewRow row in gridView.Rows)
            {
                string item = HttpUtility.HtmlDecode(row.Cells[1].Text);
                string details = HttpUtility.HtmlDecode(row.Cells[2].Text);
                var inputType = row.Cells[3].Text;

                // 查詢 ItemID
                string sQuery = "SELECT ItemID FROM IT_ServerRoom_CheckItems WHERE Item_Name=@Item_Name AND Item_Detail=@Item_Detail";
                SqlParameter[] parameters =
                {
                    new SqlParameter("@Item_Name", item),
                    new SqlParameter("@Item_Detail", details)
                };
                object oFindItemID = _dbHelper.ExecuteScalar(sQuery, parameters);

                string itemValue; // 用於儲存 Item_Value 的變數

                // 找到數值輸入和狀態輸入的 PlaceHolder
                var phTextBox = row.FindControl("phTextBox") as PlaceHolder;
                var phRadioButtonList = row.FindControl("phRadioButtonList") as PlaceHolder;

                // 如果數值輸入 PlaceHolder 可見
                if (phTextBox != null && phTextBox.Visible)
                {
                    var textBox = phTextBox.FindControl("txtNumericInput") as TextBox;
                    itemValue = textBox?.Text ?? string.Empty;

                    if (string.IsNullOrEmpty(itemValue) || !decimal.TryParse(itemValue, out _))
                    {
                        throw new Exception($"Item-{details} input value is incorrect!");
                    }
                }
                else if (phRadioButtonList != null && phRadioButtonList.Visible)
                {
                    var radioButtonList = phRadioButtonList.FindControl("rblStatus") as RadioButtonList;
                    if (radioButtonList == null || string.IsNullOrEmpty(radioButtonList.SelectedValue))
                    {
                        throw new Exception($"Item-{details} statu is not selected!");
                    }
                    itemValue = radioButtonList.SelectedValue == "1" ? "PASS" : "FAIL";
                }
                else
                {
                    throw new Exception($"未知的輸入類型或未找到有效輸入控制項: {details}");
                }

                var tbRemark = row.FindControl("txtRemarks") as TextBox;
                var remarks = tbRemark?.Text ?? string.Empty;

                // 插入檢查記錄到資料庫
                var columnValues = new Dictionary<string, object>
                {
                    { "Check_Date", DateTime.Now.ToString("yyyy/MM/dd") },
                    { "FactoryID", factoryId },
                    { "ItemID", oFindItemID.ToString() },
                    { "Item_Value", itemValue},
                    { "Item_Checker", checkerId },
                    { "Remarks", remarks }
                };

                _dbHelper.InsertData("IT_DailyCheck_records", columnValues);
            }
        }

        protected bool CheckTodayRecord()
        {
            // 檢查今天是否已有紀錄
            string checkQuery = "SELECT COUNT(*) FROM IT_DailyCheck_records WHERE Check_Date = @Check_Date";
            SqlParameter[] checkParams =
            {
                new SqlParameter("@Check_Date", DateTime.Now.ToString("yyyy/MM/dd")),
            };

            int existingCount = Convert.ToInt32(_dbHelper.ExecuteScalar(checkQuery, checkParams));
            if ((existingCount > 0 && existingCount==50 ) || existingCount > 50)
            {
                return false; 
            }
            return true;
        }


        private void DownloadExcelFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found for download.", filePath);
            }

            string fileName = Path.GetFileName(filePath);

            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AppendHeader("Content-Disposition", $"attachment; filename={Path.GetFileName(filePath)}");
            Response.TransmitFile(filePath);
            Response.End();
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

    public class CheckItem
    {
        public int Id { get; set; }
        public string Item { get; set; }
        public string Details { get; set; }
        public bool IsNumericInput { get; set; }
    }

}