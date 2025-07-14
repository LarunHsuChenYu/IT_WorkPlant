using System;
using System.Data;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using IT_WorkPlant.Models;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace IT_WorkPlant.Pages
{
    public partial class OPD_SalesOrderAnalysis : Page
    {
        private OPD_vwModelOrderSale _salesModel;
        private List<string> _selectedModels;

        protected void Page_Load(object sender, EventArgs e)
        {

            if (Session["UserEmpID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            _salesModel = new OPD_vwModelOrderSale();

            if (!IsPostBack)
            {
                DateTime today = DateTime.Today;
                DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                txtStartDate.Text = firstDayOfMonth.ToString("yyyy-MM-dd");
                txtEndDate.Text = lastDayOfMonth.ToString("yyyy-MM-dd");

                LoadAllData();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            LoadAllData();
        }

        protected void btnClearFilter_Click(object sender, EventArgs e)
        {
            hfSelectedModels.Value = "";
            LoadAllData();
        }

        protected void btnExportCSV_Click(object sender, EventArgs e)
        {
            ExportToCSV();
        }

        private void LoadAllData()
        {
#if DEBUG
            string startDate = "20250601";
            string endDate = "20250630";
#else
            string startDate = txtStartDate.Text;
            string endDate = txtEndDate.Text;
#endif

            try
            {
                
                LoadTop5Data(startDate, endDate);

                
                LoadSoTypeDistribution(startDate, endDate);

                LoadCustomerHeatmap(startDate, endDate);

                LoadSalesDetail(startDate, endDate);

                UpdateFilterDisplay(startDate, endDate);
            }
            catch (Exception ex)
            {

                System.Diagnostics.Debug.WriteLine($"LoadAllData Error: {ex.Message}");
            }
        }

        private void LoadTop5Data(string startDate, string endDate)
        {

            DataTable stoData = _salesModel.GetTopProductSalesByType(startDate, endDate, "STO", 5);
            var stoModel = new
            {
                labels = stoData.AsEnumerable().Select(r => r["product_code"].ToString()).ToList(),
                data = stoData.AsEnumerable().Select(r => Convert.ToInt32(r["sales_qty"])).ToList(),
                fullNames = stoData.AsEnumerable().Select(r => r["full_model_names"].ToString()).ToList()
            };
           
            DataTable spoData = _salesModel.GetTopProductSalesByType(startDate, endDate, "SPO", 5);
            var spoModel = new
            {
                labels = spoData.AsEnumerable().Select(r => r["product_code"].ToString()).ToList(),
                data = spoData.AsEnumerable().Select(r => Convert.ToInt32(r["sales_qty"])).ToList(),
                fullNames = spoData.AsEnumerable().Select(r => r["full_model_names"].ToString()).ToList()
            };
            var chartData = new { stoModel, spoModel };
            hfTop5Data.Value = new JavaScriptSerializer().Serialize(chartData);
        }

        private void LoadSoTypeDistribution(string startDate, string endDate)
        {
            _selectedModels = GetSelectedModels();
            DataTable soTypeData;
            if (_selectedModels != null && _selectedModels.Any())
            {
                soTypeData = _salesModel.GetSoTypeDistribution(startDate, endDate, _selectedModels);
            }
            else
            {
                DataTable top5Data = _salesModel.GetTopProductSales(startDate, endDate, 5);
                var top5Models = top5Data.AsEnumerable().Select(r => r["product_code"].ToString()).ToList();
                soTypeData = _salesModel.GetSoTypeDistribution(startDate, endDate, top5Models);
            }
            var modelGroups = soTypeData.AsEnumerable()
                .GroupBy(r => r["model_name"].ToString())
                .ToList();
            var labels = modelGroups.Select(g => g.Key).ToList();
            var soTypes = soTypeData.AsEnumerable()
                .Select(r => r["so_type"].ToString())
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            var datasets = new List<object>();
            foreach (var soType in soTypes)
            {
                var data = new List<int>();
                foreach (var model in labels)
                {
                    var qty = soTypeData.AsEnumerable()
                        .Where(r => r["model_name"].ToString() == model && r["so_type"].ToString() == soType)
                        .Sum(r => Convert.ToInt32(r["sales_qty"]));
                    data.Add(qty);
                }
                datasets.Add(new
                {
                    label = soType,
                    data = data,
                    backgroundColor = soType == "SPO" ? "#e74c3c" : "#3498db",
                    borderColor = soType == "SPO" ? "#c0392b" : "#2980b9",
                    borderWidth = 1
                });
            }
            var chartData = new
            {
                labels = labels,
                datasets = datasets
            };
            hfSoTypeData.Value = new JavaScriptSerializer().Serialize(chartData);
        }

        private void LoadCustomerHeatmap(string startDate, string endDate)
        {
            _selectedModels = GetSelectedModels();
            List<string> modelsToAnalyze;
            if (_selectedModels != null && _selectedModels.Any())
            {
                modelsToAnalyze = _selectedModels;
            }
            else
            {
                DataTable top5Data = _salesModel.GetTopProductSales(startDate, endDate, 5);
                modelsToAnalyze = top5Data.AsEnumerable().Select(r => r["product_code"].ToString()).ToList();
            }
            DataTable heatmapData = _salesModel.GetCustomerHeatmap(startDate, endDate, modelsToAnalyze, 10);
            var customers = heatmapData.AsEnumerable()
                .Select(r => r["end_customer"].ToString())
                .Distinct()
                .OrderByDescending(c => heatmapData.AsEnumerable()
                    .Where(r => r["end_customer"].ToString() == c)
                    .Sum(r => Convert.ToInt32(r["sales_qty"])))
                .Take(10)
                .ToList();
            var models = heatmapData.AsEnumerable()
                .Select(r => r["model_name"].ToString())
                .Distinct()
                .ToList();
            var dataMatrix = new List<List<int>>();
            foreach (var model in models)
            {
                var modelData = new List<int>();
                foreach (var customer in customers)
                {
                    var qty = heatmapData.AsEnumerable()
                        .Where(r => r["model_name"].ToString() == model && r["end_customer"].ToString() == customer)
                        .Sum(r => Convert.ToInt32(r["sales_qty"]));
                    modelData.Add(qty);
                }
                dataMatrix.Add(modelData);
            }
            var chartData = new
            {
                customers = customers,
                models = models,
                data = dataMatrix
            };
            hfHeatmapData.Value = new JavaScriptSerializer().Serialize(chartData);
        }

        private void LoadSalesDetail(string startDate, string endDate)
        {
            _selectedModels = GetSelectedModels();
            DataTable detailData;
            if (_selectedModels != null && _selectedModels.Any())
            {
                var productCode = _selectedModels.First();
                detailData = _salesModel.GetOrderSalesDetail(startDate, endDate, productCode);
            }
            else
            {
                detailData = _salesModel.GetSalesData(startDate, endDate);
            }
            if (detailData.Rows.Count > 50)
            {
                DataTable limitedData = detailData.Clone();
                for (int i = 0; i < 50; i++)
                {
                    limitedData.ImportRow(detailData.Rows[i]);
                }
                detailData = limitedData;
            }
            gvSalesDetail.DataSource = detailData;
            gvSalesDetail.DataBind();
        }

        private List<string> GetSelectedModels()
        {
            if (!string.IsNullOrEmpty(hfSelectedModels.Value))
            {
                return new List<string> { hfSelectedModels.Value };
            }
            return null;
        }

        private void UpdateFilterDisplay(string startDate, string endDate)
        {
            filterSection.Visible = true;
            if (_selectedModels != null && _selectedModels.Any())
            {
                selectedModels.InnerText = string.Join(", ", _selectedModels);
            }
            else
            {
                selectedModels.InnerText = "全部模型";
            }
            DataTable top5Data = _salesModel.GetTopProductSales(startDate, endDate, 5);
            var top5Models = top5Data.AsEnumerable().Select(r => r["product_code"].ToString()).ToList();
            DataTable heatmapData = _salesModel.GetCustomerHeatmap(startDate, endDate, top5Models, 10);
            customerCount.InnerText = heatmapData.AsEnumerable().Select(r => r["end_customer"].ToString()).Distinct().Count().ToString();
            dateRange.InnerText = $"{startDate} ~ {endDate}";
        }

        private void ExportToCSV()
        {
            string startDate = txtStartDate.Text;
            string endDate = txtEndDate.Text;
            DataTable exportData;
            if (_selectedModels != null && _selectedModels.Any())
            {
                var productCode = _selectedModels.First();
                exportData = _salesModel.GetOrderSalesDetail(startDate, endDate, productCode);
            }
            else
            {
                exportData = _salesModel.GetSalesData(startDate, endDate);
            }
            StringBuilder csv = new StringBuilder();
            csv.AppendLine("產品代碼,銷售量");
            foreach (DataRow row in exportData.Rows)
            {
                csv.AppendLine($"{row["model_pn"]},{row["sales_qty"]}");
            }
            Response.Clear();
            Response.Buffer = true;
            Response.AddHeader("content-disposition", $"attachment;filename=OPD_SalesAnalysis_{startDate}_{endDate}.csv");
            Response.Charset = "UTF-8";
            Response.ContentType = "application/text";
            Response.ContentEncoding = Encoding.UTF8;
            Response.BinaryWrite(new byte[] { 0xEF, 0xBB, 0xBF });
            Response.Output.Write(csv.ToString());
            Response.Flush();
            Response.End();
        }
    }
} 