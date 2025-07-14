using System;
using System.Data;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using IT_WorkPlant.Models;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Linq;

namespace IT_WorkPlant.Pages
{
    public partial class OPD_TLF_Statics : Page
    {
        private OracleDatabaseHelper _dbHelper;

        protected void Page_Load(object sender, EventArgs e)
        {

            if (Session["UserEmpID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            _dbHelper = new OracleDatabaseHelper(ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString);

            if (!IsPostBack)
            {
                
                DateTime today = DateTime.Today;
                DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                txtStartDate.Text = firstDayOfMonth.ToString("yyyy-MM-dd");
                txtEndDate.Text = lastDayOfMonth.ToString("yyyy-MM-dd");

                LoadChartData();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadChartData();
        }

        private void LoadChartData()
        {
#if DEBUG
            string startDate = "20250601";
            string endDate = "20250630";
#else
            string startDate = txtStartDate.Text;
            string endDate = txtEndDate.Text;
#endif

            
            string dailyQuery = @"
                SELECT
                    TO_CHAR(tlf07, 'YYYY-MM-DD') AS STAT_DATE,
                    COUNT(*) AS CNT
                FROM ds5.tlf_file
                WHERE tlf07 BETWEEN TO_DATE(:startDate, 'yyyy-mm-dd') AND TO_DATE(:endDate, 'yyyy-mm-dd')
                    AND (tlf13 LIKE 'axmt%' OR tlf13 LIKE 'aomt%')
                GROUP BY TO_CHAR(tlf07, 'YYYY-MM-DD')
                ORDER BY STAT_DATE";

            var dailyParameters = new[]
            {
                new OracleParameter("startDate", startDate),
                new OracleParameter("endDate", endDate)
            };

            DataTable dailyData = _dbHelper.ExecuteQuery(dailyQuery, dailyParameters);

           
            var chartData = new
            {
                daily = new
                {
                    labels = dailyData.AsEnumerable().Select(r => r["STAT_DATE"].ToString()).ToList(),
                    data = dailyData.AsEnumerable().Select(r => Convert.ToInt32(r["CNT"])).ToList()
                }
            };

            
            hfChartData.Value = new JavaScriptSerializer().Serialize(chartData);
        }
    }
} 