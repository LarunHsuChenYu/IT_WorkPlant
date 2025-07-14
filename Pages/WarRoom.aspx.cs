using System;
using System.Data;
using System.Web.Script.Serialization;
using System.Web.UI;
using IT_WorkPlant.Models;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Linq;
using System.Collections.Generic;
using System.Web.Services;
using System.IO;

namespace IT_WorkPlant.Pages
{
    public partial class WarRoom : Page
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
                txtStartDate.Text = new DateTime(today.Year, today.Month, 1).ToString("yyyy-MM-dd");
                txtEndDate.Text = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month)).ToString("yyyy-MM-dd");
                LoadDashboardData(txtStartDate.Text, txtEndDate.Text);
            }
        }

        protected void btnQuery_Click(object sender, EventArgs e)
        {
            LoadDashboardData(txtStartDate.Text, txtEndDate.Text);
        }

        private void LoadDashboardData(string startDate, string endDate)
        {
            string json = GetDashboardData(startDate, endDate);
            hfDashboardData.Value = json;
        }

        [WebMethod]
        public static string GetDashboardData(string startDate, string endDate)
        {
            var dbHelper = new OracleDatabaseHelper(System.Configuration.ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString);
            string defaultStart = startDate;
            string defaultEnd = endDate;

            // 1. Top 5 Product Orders (with STO/SPO breakdown)
            var salesModel = new OPD_vwModelOrderSale();

            // Calculate date ranges
            DateTime startDateTime = DateTime.Parse(defaultStart);
            DateTime endDateTime = DateTime.Parse(defaultEnd);
            string prevMonthStart = startDateTime.AddMonths(-1).ToString("yyyy-MM-dd");
            string prevMonthEnd = endDateTime.AddMonths(-1).ToString("yyyy-MM-dd");

            // Query data for SPO and STO for both periods, getting Top 5 for each category
            DataTable dtCurrentSPO = salesModel.GetTopProductSalesByType(defaultStart, defaultEnd, "SPO", 5);
            DataTable dtCurrentSTO = salesModel.GetTopProductSalesByType(defaultStart, defaultEnd, "STO", 5);
            DataTable dtPrevSPO = salesModel.GetTopProductSalesByType(prevMonthStart, prevMonthEnd, "SPO", 5);
            DataTable dtPrevSTO = salesModel.GetTopProductSalesByType(prevMonthStart, prevMonthEnd, "STO", 5);

            // Prepare two separate label sets for SPO and STO
            var spoLabels = dtCurrentSPO.AsEnumerable().Select(r => r["product_code"]?.ToString())
                .Union(dtPrevSPO.AsEnumerable().Select(r => r["product_code"]?.ToString()))
                .Where(l => !string.IsNullOrEmpty(l))
                .Distinct()
                .ToList();

            var stoLabels = dtCurrentSTO.AsEnumerable().Select(r => r["product_code"]?.ToString())
                .Union(dtPrevSTO.AsEnumerable().Select(r => r["product_code"]?.ToString()))
                .Where(l => !string.IsNullOrEmpty(l))
                .Distinct()
                .ToList();

            // Prepare datasets for both SPO and STO
            var currentStoData = new List<long>();
            var currentSpoData = new List<long>();
            var prevStoData = new List<long>();
            var prevSpoData = new List<long>();

            // Helper function to get quantity
            long GetQty(DataTable dt, string pcode)
            {
                var row = dt.AsEnumerable().FirstOrDefault(r => r["product_code"]?.ToString() == pcode);
                return long.TryParse(row?["sales_qty"]?.ToString(), out var v) ? v : 0;
            }

            // Prepare SPO data
            var spoCurrentData = spoLabels.Select(label => GetQty(dtCurrentSPO, label)).ToList();
            var spoPrevData = spoLabels.Select(label => GetQty(dtPrevSPO, label)).ToList();

            // Prepare STO data
            var stoCurrentData = stoLabels.Select(label => GetQty(dtCurrentSTO, label)).ToList();
            var stoPrevData = stoLabels.Select(label => GetQty(dtPrevSTO, label)).ToList();

            var orderModel = new
            {
                spoModel = new
                {
                    labels = spoLabels,
                    datasets = new[]
                    {
                        new { label = "本月 SPO", data = spoCurrentData, backgroundColor = "rgba(133, 193, 233, 1)", stack = "本月" },
                        new { label = "上個月 SPO", data = spoPrevData, backgroundColor = "rgba(241, 148, 138, 1)", stack = "上個月" }
                    }
                },
                stoModel = new
                {
                    labels = stoLabels,
                    datasets = new[]
                    {
                        new { label = "本月 STO", data = stoCurrentData, backgroundColor = "rgba(52, 152, 219, 1)", stack = "本月" },
                        new { label = "上個月 STO", data = stoPrevData, backgroundColor = "rgba(231, 76, 60, 1)", stack = "上個月" }
                    }
                }
            };

            // 2. Trial Order Completion Rate
            string sql2 = $@"
SELECT sfb01 AS ""TrialBatchNo"", 
       sfb05 AS ""ProductNo"", 
       ima02 AS ""ProductName"",
       sfb08 AS ""PlannedQty"",
       sfb09 AS ""CompletedQty"",
       ROUND((NVL(sfb09,0)/NULLIF(sfb08,0))*100, 2) AS ""CompletionRate""
FROM DS5.sfb_file sfb
JOIN DS5.ima_file ima ON sfb.sfb05 = ima.ima01
WHERE sfb.sfb02 = 15
ORDER BY sfb01
";
            DataTable dt2 = dbHelper.ExecuteQuery(sql2);
            var trialOrder = dt2.AsEnumerable().Select(r => new {
                TrialBatchNo = r["TrialBatchNo"]?.ToString() ?? "",
                ProductNo = r["ProductNo"]?.ToString() ?? "",
                ProductName = r["ProductName"]?.ToString() ?? "",
                PlannedQty = r["PlannedQty"]?.ToString() ?? "",
                CompletedQty = r["CompletedQty"]?.ToString() ?? "",
                CompletionRate = r["CompletionRate"]?.ToString() ?? ""
            }).ToList();

            // 3. REPLACED: Top 5 Product Shipment
            var shipmentModel = new OPD_vwModelShipmentSale();
            DataTable dtCurrentSPO_Ship = shipmentModel.GetTopProductSalesByType(defaultStart, defaultEnd, "SPO", 5);
            DataTable dtCurrentSTO_Ship = shipmentModel.GetTopProductSalesByType(defaultStart, defaultEnd, "STO", 5);
            DataTable dtPrevSPO_Ship = shipmentModel.GetTopProductSalesByType(prevMonthStart, prevMonthEnd, "SPO", 5);
            DataTable dtPrevSTO_Ship = shipmentModel.GetTopProductSalesByType(prevMonthStart, prevMonthEnd, "STO", 5);

            var spoLabels_Ship = dtCurrentSPO_Ship.AsEnumerable().Select(r => r["product_code"]?.ToString())
                .Union(dtPrevSPO_Ship.AsEnumerable().Select(r => r["product_code"]?.ToString()))
                .Where(l => !string.IsNullOrEmpty(l))
                .Distinct()
                .ToList();
            var stoLabels_Ship = dtCurrentSTO_Ship.AsEnumerable().Select(r => r["product_code"]?.ToString())
                .Union(dtPrevSTO_Ship.AsEnumerable().Select(r => r["product_code"]?.ToString()))
                .Where(l => !string.IsNullOrEmpty(l))
                .Distinct()
                .ToList();
            long GetQtyShip(DataTable dt, string pcode)
            {
                var row = dt.AsEnumerable().FirstOrDefault(r => r["product_code"]?.ToString() == pcode);
                return long.TryParse(row?["shipping_qty"]?.ToString(), out var v) ? v : 0;
            }
            var spoCurrentData_Ship = spoLabels_Ship.Select(label => GetQtyShip(dtCurrentSPO_Ship, label)).ToList();
            var spoPrevData_Ship = spoLabels_Ship.Select(label => GetQtyShip(dtPrevSPO_Ship, label)).ToList();
            var stoCurrentData_Ship = stoLabels_Ship.Select(label => GetQtyShip(dtCurrentSTO_Ship, label)).ToList();
            var stoPrevData_Ship = stoLabels_Ship.Select(label => GetQtyShip(dtPrevSTO_Ship, label)).ToList();
            var booking = new
            {
                spoModel = new
                {
                    labels = spoLabels_Ship,
                    datasets = new[]
                    {
                        new { label = "本月 SPO 出貨", data = spoCurrentData_Ship, backgroundColor = "rgba(133, 193, 233, 1)", stack = "本月" },
                        new { label = "上個月 SPO 出貨", data = spoPrevData_Ship, backgroundColor = "rgba(241, 148, 138, 1)", stack = "上個月" }
                    }
                },
                stoModel = new
                {
                    labels = stoLabels_Ship,
                    datasets = new[]
                    {
                        new { label = "本月 STO 出貨", data = stoCurrentData_Ship, backgroundColor = "rgba(52, 152, 219, 1)", stack = "本月" },
                        new { label = "上個月 STO 出貨", data = stoPrevData_Ship, backgroundColor = "rgba(231, 76, 60, 1)", stack = "上個月" }
                    }
                }
            };

            // 4. Department Capacity Statistics
            string sql4 = $@"
SELECT 
    smymemo3 AS ""WO_TYPE"",
    sfb82 AS ""DEPT_CODE"",
    gem.gem02 AS ""DEPT_NAME"", 
    COUNT(DISTINCT sfb01) AS ""WO_COUNT"",
    SUM(sfb08) AS ""WO_QTY"", 
    SUM(sfb09) AS ""FINISH_QTY""
FROM DS5.sfb_file sfb
LEFT JOIN DS5.gem_file gem ON sfb.sfb82 = gem.gem01
LEFT JOIN DS5.smy_file smy ON SUBSTR(sfb.sfb01,1,3) = smy.smyslip
WHERE sfb87='Y' AND sfb.sfb81 BETWEEN TO_DATE('250601', 'YYMMDD') AND TO_DATE('250630', 'YYMMDD')
GROUP BY smymemo3,sfb82, gem.gem02
ORDER BY sfb82";
            DataTable dt4 = dbHelper.ExecuteQuery(sql4);
            var deptCapacity = new
            {
                woType = dt4.AsEnumerable().Select(r => r["WO_TYPE"]?.ToString() ?? "").ToList(),
                deptCode = dt4.AsEnumerable().Select(r => r["DEPT_CODE"]?.ToString() ?? "").ToList(),
                deptName = dt4.AsEnumerable().Select(r => r["DEPT_NAME"]?.ToString() ?? "").ToList(),
                woCount = dt4.AsEnumerable().Select(r => int.TryParse(r["WO_COUNT"]?.ToString(), out var v) ? v : 0).ToList(),
                woQty = dt4.AsEnumerable().Select(r => int.TryParse(r["WO_QTY"]?.ToString(), out var v) ? v : 0).ToList(),
                finishQty = dt4.AsEnumerable().Select(r => int.TryParse(r["FINISH_QTY"]?.ToString(), out var v) ? v : 0).ToList()
            };

            // 5. OPD TLF Daily Statistics
            string sql5 = $@"
SELECT TO_CHAR(tlf07, 'YYYY-MM-DD') AS ""STAT_DATE"", COUNT(*) AS ""CNT""
FROM ds5.tlf_file
WHERE tlf07 BETWEEN TO_DATE('{defaultStart}', 'yyyy-mm-dd') AND TO_DATE('{defaultEnd}', 'yyyy-mm-dd')
    AND (tlf13 LIKE 'axmt%' OR tlf13 LIKE 'aomt%')
GROUP BY TO_CHAR(tlf07, 'YYYY-MM-DD')
ORDER BY STAT_DATE";
            DataTable dt5 = dbHelper.ExecuteQuery(sql5);
            var opdTlfDaily = new
            {
                labels = dt5.AsEnumerable().Select(r => r["STAT_DATE"].ToString()).ToList(),
                data = dt5.AsEnumerable().Select(r => Convert.ToInt32(r["CNT"])).ToList()
            };

            var dashboardData = new
            {
                orderModel,
                trialOrder,
                booking,
                deptCapacity,
                opdTlfDaily
            };
            return new JavaScriptSerializer().Serialize(dashboardData);
        }
    }
} 