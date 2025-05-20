using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using IT_WorkPlant.Models;

namespace IT_WorkPlant.Pages
{
    public partial class IT_RequestsDashboard : Page
    {
        private IT_RequestModel _model;

        // ✅ เพิ่ม Dictionary สำหรับตัวย่อของแผนก
        private static readonly Dictionary<string, string> DepartmentShortMap = new Dictionary<string, string>
        {
            { "Administration Dept.", "AD" },
            { "Chairman Office", "CO" },
            { "Document Control Center", "IA" },
            { "EHS Dept.", "EH" },
            { "Finance & Accounting Dept.", "FI" },
            { "GMs Office", "GO" },
            { "Information Technology Dept.", "IT" },
            { "Manufacturing Division", "MF" },
            { "Management Representative", "MR" },
            { "Production Control Dept.", "PC" },
            { "Purchase Dept.", "PU" },
            { "Quality Control Dept.", "QC" },
            { "Trial Production Section", "TP" }
        };

        protected void Page_Load(object sender, EventArgs e)
        {
            // ✅ บังคับให้ล็อกอินก่อนเข้าหน้านี้
            if (Session["UserEmpID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (_model == null)
                _model = new IT_RequestModel();

            if (!IsPostBack)
            {
                ddlTimeFilter.SelectedValue = "all";
                LoadDashboardData();
            }
        }


        protected void ddlTimeFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            string filter = ddlTimeFilter.SelectedValue;
            DataTable allRequests = _model.GetAllRequests();

            // ✅ กรองช่วงเวลาแบบปลอดภัย
            if (filter == "month")
            {
                var filtered = allRequests.AsEnumerable()
                    .Where(r => Convert.ToDateTime(r["IssueDate"]).Month == DateTime.Today.Month &&
                                Convert.ToDateTime(r["IssueDate"]).Year == DateTime.Today.Year);
                allRequests = filtered.Any() ? filtered.CopyToDataTable() : allRequests.Clone();
            }
            else if (filter == "today")
            {
                var filtered = allRequests.AsEnumerable()
                    .Where(r => Convert.ToDateTime(r["IssueDate"]).Date == DateTime.Today.Date);
                allRequests = filtered.Any() ? filtered.CopyToDataTable() : allRequests.Clone();
            }

            // ✅ Summary บนการ์ด
            lblTotal.Text = allRequests.Rows.Count.ToString();
            lblWIP.Text = allRequests.AsEnumerable().Count(r => r["Status"].ToString() == "WIP").ToString();
            lblDone.Text = allRequests.AsEnumerable().Count(r => r["Status"].ToString() == "Done").ToString();
            lblDoneToday.Text = allRequests.AsEnumerable()
                .Count(r => r["Status"].ToString() == "Done"
                         && r["FinishedDate"] != DBNull.Value
                         && Convert.ToDateTime(r["FinishedDate"]).Date == DateTime.Today.Date)
                .ToString();

            // ✅ Chart JSON ส่งให้ JS
            var chartData = new
            {
                type = GetChartData(allRequests, "IssueType"),
                dept = GetChartData(allRequests, "Department"),
                trend = GetChartData(allRequests, "IssueDate", "yyyy-MM"),
                dri = GetChartData(allRequests, "DRI_UserName", replaceEmpty: "Unassigned")
            };

            hfChartData.Value = new JavaScriptSerializer().Serialize(chartData);

            // ✅ ตารางใต้กราฟ
            var typeData = chartData.type;
            var deptData = chartData.dept;
            var trendData = chartData.trend;
            var driData = chartData.dri;

            RenderSummaryTable(ToTupleList(typeData), ltTableType);
            RenderSummaryTable(ToTupleList(deptData, mapShortDept: true), ltTableDept); // ← ใช้ตัวย่อ
            RenderSummaryTable(ToTupleList(trendData), ltTableTrend);
            RenderSummaryTable(ToTupleList(driData), ltTableDRI);
        }

        private object GetChartData(DataTable table, string columnName, string formatDate = null, string replaceEmpty = null)
        {
            var result = table.AsEnumerable()
                .Select(r =>
                {
                    object raw = r[columnName];
                    if (raw == DBNull.Value || string.IsNullOrWhiteSpace(raw.ToString()))
                        return replaceEmpty ?? "";
                    else if (!string.IsNullOrEmpty(formatDate) && DateTime.TryParse(raw.ToString(), out DateTime dt))
                        return dt.ToString(formatDate);
                    else
                        return raw.ToString();
                })
                .GroupBy(val => val)
                .Select(g => new { Label = g.Key, Count = g.Count() })
                .OrderBy(g => g.Label)
                .ToList();

            return new
            {
                labels = result.Select(r => r.Label).ToList(),
                data = result.Select(r => r.Count).ToList()
            };
        }

        private List<(string Label, int Count)> ToTupleList(dynamic chartObj, bool mapShortDept = false)
        {
            var result = new List<(string Label, int Count)>();
            var labels = (List<string>)chartObj.labels;
            var counts = (List<int>)chartObj.data;

            for (int i = 0; i < labels.Count; i++)
            {
                string label = labels[i];
                if (mapShortDept && DepartmentShortMap.ContainsKey(label))
                    label = DepartmentShortMap[label];

                result.Add((label, counts[i]));
            }

            return result;
        }

        private void RenderSummaryTable(List<(string Label, int Count)> data, Literal target)
        {
            string html = "<table class='table table-sm table-bordered summary-table text-center'><tr>";
            foreach (var item in data) html += $"<th>{item.Label}</th>";
            html += "</tr><tr>";
            foreach (var item in data) html += $"<td>{item.Count}</td>";
            html += "</tr></table>";
            target.Text = html;
        }
    }
}
