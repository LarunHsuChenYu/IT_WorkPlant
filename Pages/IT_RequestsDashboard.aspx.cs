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
            else if (filter == "week")
            {
                var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
                var endOfWeek = startOfWeek.AddDays(5); // ✅ จันทร์-ศุกร์

                var filtered = allRequests.AsEnumerable()
                    .Where(r =>
                    {
                        DateTime issueDate;
                        return DateTime.TryParse(r["IssueDate"].ToString(), out issueDate)
                            && issueDate >= startOfWeek && issueDate < endOfWeek;
                    });
                allRequests = filtered.Any() ? filtered.CopyToDataTable() : allRequests.Clone();
            }
            lblTotal.Text = allRequests.Rows.Count.ToString();
            lblWIP.Text = allRequests.AsEnumerable().Count(r => r["Status"].ToString() == "WIP").ToString();
            lblDone.Text = allRequests.AsEnumerable().Count(r => r["Status"].ToString() == "Done").ToString();
            lblDoneToday.Text = allRequests.AsEnumerable()
                .Count(r => r["Status"].ToString() == "Done"
                         && r["FinishedDate"] != DBNull.Value
                         && Convert.ToDateTime(r["FinishedDate"]).Date == DateTime.Today.Date)
                .ToString();
            string trendFormat = "yyyy-MM";
            if (filter == "today" || filter == "week")
                trendFormat = "yyyy-MM-dd";
            else if (filter == "month")
                trendFormat = "yyyy-ww";
            var chartData = new
            {
                type = GetChartData(allRequests, "IssueType"),
                dept = GetChartData(allRequests, "Department"),
                trend = GetChartData(allRequests, "IssueDate", trendFormat),
                dri = GetChartData(allRequests, "DRI_UserName", replaceEmpty: "Unassigned")
            };

            hfChartData.Value = new JavaScriptSerializer().Serialize(chartData);

            var typeData = chartData.type;
            var deptData = chartData.dept;
            var trendData = chartData.trend;
            var driData = chartData.dri;

            RenderSummaryTable(ToTupleList(typeData), ltTableType);
            RenderSummaryTable(ToTupleList(deptData, mapShortDept: true), ltTableDept);
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

                    if (formatDate == "yyyy-ww" && DateTime.TryParse(raw.ToString(), out DateTime weekDate))
                        return GetWeekLabel(weekDate); // ✅ ใช้ label แบบ 06–10 May
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

        private string GetWeekLabel(DateTime date)
        {
            // หาวันจันทร์ของสัปดาห์
            DateTime startOfWeek = date.AddDays(-(int)date.DayOfWeek + (int)DayOfWeek.Monday);
            DateTime endOfWeek = startOfWeek.AddDays(4); // จันทร์ถึงศุกร์

            // รูปแบบ: 06–10 May
            string label = $"{startOfWeek:dd}-{endOfWeek:dd} {endOfWeek:MMM}";
            return label;
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
