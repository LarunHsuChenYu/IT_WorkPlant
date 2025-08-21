using System;
using System.Data;
using System.Web.Script.Serialization;
using System.Web.UI;
using IT_WorkPlant.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;

namespace IT_WorkPlant.Pages
{
    public partial class OPD_WorkOrderAnalysis : Page
    {
        private OPD_vwModelWorkOrder _workOrderModel;

        protected void Page_Load(object sender, EventArgs e)
        {
            _workOrderModel = new OPD_vwModelWorkOrder();
            if (!IsPostBack)
            {
                DateTime today = DateTime.Today;
                txtStartDate.Text = today.AddDays(-15).ToString("yyyy-MM-dd");
                txtEndDate.Text = today.ToString("yyyy-MM-dd");
                BindPOCompletion();
                BindAllPOData();
            }
        }

        protected void btnSearchPO_Click(object sender, EventArgs e)
        {
            BindPOCompletion();
            BindAllPOData();
        }

        protected void btnSearchWO_Click(object sender, EventArgs e)
        {
            BindWODetail();
        }

        protected void btnSearchPOWO_Click(object sender, EventArgs e)
        {
            BindPOWOList();
        }

        protected void gvAllPO_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAllPO.PageIndex = e.NewPageIndex;
            BindAllPOData();
        }

        private void BindPOCompletion()
        {
            DateTime startDate = DateTime.Parse(txtStartDate.Text);
            DateTime endDate = DateTime.Parse(txtEndDate.Text);
            int topN = 20;
            decimal maxRate = 100;
            string keyword = txtPOKeyword.Text.Trim();
            int.TryParse(txtTopN.Text, out topN);
            decimal.TryParse(txtMaxRate.Text, out maxRate);

            // Get all PO data
            var dt = _workOrderModel.GetPOCompletionList(startDate, endDate, null, null);
            var allRows = dt.AsEnumerable();
            if (!string.IsNullOrEmpty(keyword))
                allRows = allRows.Where(r => r.Field<string>("PO_NO").Contains(keyword));
            DataTable allList = allRows.Any() ? allRows.CopyToDataTable() : dt.Clone();
            hfAllPOData.Value = DataTableToJson(allList);

            // Summary cards
            double avgRate = allRows.Any() ? allRows.Average(r => Convert.ToDouble(r.Field<decimal>("COMPLETION_RATE"))) : 0;
            int unfinishedWO = allRows.Sum(r => r.Field<decimal>("COMPLETION_RATE") < 100 ? Convert.ToInt32(r.Field<decimal>("TOTAL_WO")) : 0);
            var topRisk = allRows.OrderBy(r => r.Field<decimal>("COMPLETION_RATE")).ThenByDescending(r => r.Field<decimal>("PLAN_QTY")).FirstOrDefault();
            var summary = new {
                avgRate = Math.Round(avgRate, 2),
                unfinishedWO = unfinishedWO,
                topRiskPO = topRisk != null ? topRisk["PO_NO"] : "--",
                topRiskDetail = topRisk != null ? $"Qty: {topRisk["PLAN_QTY"]}, Rate: {topRisk["COMPLETION_RATE"]}%" : "--"
            };
            hfSummaryData.Value = new JavaScriptSerializer().Serialize(summary);

            // Distribution chart buckets
            var bucketGroups = allRows.GroupBy(r => r.Field<string>("BUCKET"))
                .Select(g => new { bucket = g.Key, count = g.Count() }).ToList();
            hfBucketDistData.Value = new JavaScriptSerializer().Serialize(bucketGroups);

            // TopN chart
            string topNSort = ddlTopNSort.SelectedValue;
            IEnumerable<DataRow> topNRows;
            if (topNSort == "unfinished")
            {
                topNRows = allRows.OrderByDescending(r => r.Field<decimal>("PLAN_QTY") - r.Field<decimal>("FINISH_QTY")).Take(topN);
            }
            else // lowrate
            {
                topNRows = allRows.OrderBy(r => r.Field<decimal>("COMPLETION_RATE")).ThenByDescending(r => r.Field<decimal>("PLAN_QTY")).Take(topN);
            }
            DataTable topNList = topNRows.Any() ? topNRows.CopyToDataTable() : dt.Clone();
            hfTopNData.Value = DataTableToJson(topNList);
        }

        private void BindAllPOData()
        {
            DateTime startDate = DateTime.Parse(txtStartDate.Text);
            DateTime endDate = DateTime.Parse(txtEndDate.Text);
            string keyword = txtPOKeyword.Text.Trim();
            decimal maxRate = 100;
            decimal.TryParse(txtMaxRate.Text, out maxRate);
            var allPOData = _workOrderModel.GetAllPOData(startDate, endDate, keyword, maxRate);
            gvAllPO.DataSource = allPOData;
            gvAllPO.DataBind();
        }

        private void BindWODetail()
        {
            string woNo = txtWONo.Text.Trim();
            DataTable dt = _workOrderModel.GetWorkOrderDetail(woNo);
            hfWODetailData.Value = DataTableToJson(dt);
        }

        private void BindPOWOList()
        {
            // Get PO number from either the drill-down hidden field or the manual input
            string poNo = !string.IsNullOrEmpty(hfDrillPONo.Value.Trim()) ? 
                         hfDrillPONo.Value.Trim() : 
                         txtPONo.Text.Trim();
            
            if (string.IsNullOrEmpty(poNo))
            {
                // If no PO number provided, clear the chart
                hfPOWOListData.Value = "[]";
                return;
            }

            DateTime startDate = DateTime.Parse(txtStartDate.Text);
            DateTime endDate = DateTime.Parse(txtEndDate.Text);
            DataTable dt = _workOrderModel.GetPOWorkOrderList(poNo, startDate, endDate);
            hfPOWOListData.Value = DataTableToJson(dt);
            
            // Clear the drill-down hidden field after processing
            hfDrillPONo.Value = "";
        }

        private string DataTableToJson(DataTable dt)
        {
            var rows = new List<Dictionary<string, object>>();
            foreach (DataRow dr in dt.Rows)
            {
                var row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row[col.ColumnName] = dr[col];
                }
                rows.Add(row);
            }
            return new JavaScriptSerializer().Serialize(rows);
        }
    }
} 