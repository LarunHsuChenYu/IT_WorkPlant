using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization; // serialize JSON
using System.Web.UI;

namespace IT_WorkPlant.Pages
{
    public partial class QMS_Dashboard : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtFrom.Text = DateTime.Today.AddDays(-28).ToString("yyyy-MM-dd");
                txtTo.Text = DateTime.Today.ToString("yyyy-MM-dd");
                BindFilters();
                LoadDashboard();
            }
        }

        protected void btnApply_Click(object sender, EventArgs e) => LoadDashboard();
        protected void btnRefresh_Click(object sender, EventArgs e) => LoadDashboard();
        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtFrom.Text = DateTime.Today.AddDays(-28).ToString("yyyy-MM-dd");
            txtTo.Text = DateTime.Today.ToString("yyyy-MM-dd");
            ddlUnit.SelectedIndex = 0;
            ddlDefect.SelectedIndex = 0;
            LoadDashboard();
        }

        private void BindFilters()
        {
            ddlUnit.Items.Clear();
            ddlUnit.Items.Add(new System.Web.UI.WebControls.ListItem("All", ""));
            ddlUnit.Items.Add(new System.Web.UI.WebControls.ListItem("Unit A", "Unit A"));
            ddlUnit.Items.Add(new System.Web.UI.WebControls.ListItem("Unit B", "Unit B"));
            ddlUnit.Items.Add(new System.Web.UI.WebControls.ListItem("Unit C", "Unit C"));

            ddlDefect.Items.Clear();
            ddlDefect.Items.Add(new System.Web.UI.WebControls.ListItem("All", ""));
            ddlDefect.Items.Add(new System.Web.UI.WebControls.ListItem("Surface Defect", "Surface Defect"));
            ddlDefect.Items.Add(new System.Web.UI.WebControls.ListItem("Dimensional", "Dimensional"));
            ddlDefect.Items.Add(new System.Web.UI.WebControls.ListItem("Assembly", "Assembly"));
            ddlDefect.Items.Add(new System.Web.UI.WebControls.ListItem("Material", "Material"));
            ddlDefect.Items.Add(new System.Web.UI.WebControls.ListItem("Process", "Process"));
        }

        private void LoadDashboard()
        {
            // ตรงนี้ต่อฐานจริงได้ภายหลัง — ตอนนี้เป็น mock ให้ภาพรวมทำงานครบ
            var data = BuildMockData();

            // KPI
            litTotalIssues.Text = data.TotalIssues.ToString();
            litOpenIssues.Text = data.OpenIssues.ToString();
            litCompleted.Text = data.CompletedIssues.ToString();
            litAvgDowntime.Text = data.AvgDowntimeMin.ToString("0");

            // ตาราง Summary
            gvDeptPerf.DataSource = data.Performance;
            gvDeptPerf.DataBind();

            // JSON payload สำหรับ Chart.js
            var payload = new
            {
                weekly = data.Weekly.Select(x => new { x.Week, x.Issues, x.Target, x.Resolved }),
                defects = data.Defects.Select(x => new { x.Name, x.Value }),
                deptTrend = data.DeptTrend.Select(x => new {
                    x.Week,
                    x.QualityControl,
                    x.Production,
                    x.Assembly,
                    x.Maintenance
                }),
                downtime = data.Downtime.Select(x => new { x.Department, x.Downtime, x.Issues })
            };
            var json = new JavaScriptSerializer().Serialize(payload);
            litDashboardJson.Text = json;  // ใส่ลง <script type="application/json"> ใน .aspx
        }

        #region Mock data
        private DashboardData BuildMockData()
        {
            var weekly = new List<WeeklyRow> {
                new WeeklyRow("W30",12,15,10), new WeeklyRow("W31",18,15,15),
                new WeeklyRow("W32", 8,15, 8), new WeeklyRow("W33",22,15,18),
                new WeeklyRow("W34",15,15,14), new WeeklyRow("W35",11,15,11),
                new WeeklyRow("W36",16,15,12)
            };

            var defects = new List<DefectSlice> {
                new DefectSlice("Surface Defect",35),
                new DefectSlice("Dimensional",25),
                new DefectSlice("Assembly",20),
                new DefectSlice("Material",12),
                new DefectSlice("Process",8)
            };

            var deptTrend = new List<DeptTrendRow> {
                new DeptTrendRow("W30",3,5,2,2),
                new DeptTrendRow("W31",4,8,3,3),
                new DeptTrendRow("W32",2,3,2,1),
                new DeptTrendRow("W33",5,10,4,3),
                new DeptTrendRow("W34",3,7,3,2),
                new DeptTrendRow("W35",2,5,2,2),
                new DeptTrendRow("W36",4,7,3,2)
            };

            var downtime = new List<DowntimeRow> {
                new DowntimeRow("Production",180,8),
                new DowntimeRow("Assembly",120,5),
                new DowntimeRow("Quality Control",90,6),
                new DowntimeRow("Maintenance",150,4),
                new DowntimeRow("Procurement",60,2)
            };

            var perf = new List<DeptPerfRow> {
                new DeptPerfRow("Production",8,1,180,23,"High"),
                new DeptPerfRow("Assembly",5,1,120,24,"Medium"),
                new DeptPerfRow("Quality Control",6,1,90,15,"Low"),
                new DeptPerfRow("Maintenance",4,1,150,38,"Medium"),
                new DeptPerfRow("Procurement",2,0,60,30,"Low")
            };

            int totalIssues = weekly.Sum(x => x.Issues);
            int totalResolved = weekly.Sum(x => x.Resolved);
            int completed = totalResolved;                 // resolve = completed
            int open = totalIssues - totalResolved;
            var avgDownMin = downtime.Average(x => x.Downtime / Math.Max(1, x.Issues));

            return new DashboardData
            {
                Weekly = weekly,
                Defects = defects,
                DeptTrend = deptTrend,
                Downtime = downtime,
                Performance = perf,
                TotalIssues = totalIssues,
                CompletedIssues = completed,
                OpenIssues = open,
                AvgDowntimeMin = avgDownMin
            };
        }

        private class DashboardData
        {
            public List<WeeklyRow> Weekly { get; set; }
            public List<DefectSlice> Defects { get; set; }
            public List<DeptTrendRow> DeptTrend { get; set; }
            public List<DowntimeRow> Downtime { get; set; }
            public List<DeptPerfRow> Performance { get; set; }
            public int TotalIssues { get; set; }
            public int CompletedIssues { get; set; }
            public int OpenIssues { get; set; }
            public double AvgDowntimeMin { get; set; }
        }

        private class WeeklyRow
        {
            public WeeklyRow(string week, int issues, int target, int resolved)
            { Week = week; Issues = issues; Target = target; Resolved = resolved; }

            public string Week { get; set; }
            public int Issues { get; set; }
            public int Target { get; set; }
            public int Resolved { get; set; }
        }

        private class DefectSlice
        {
            public DefectSlice(string n, int v) { Name = n; Value = v; }
            public string Name { get; set; }
            public int Value { get; set; }
        }

        private class DeptTrendRow
        {
            public DeptTrendRow(string w, int qc, int prod, int asm, int mt)
            { Week = w; QualityControl = qc; Production = prod; Assembly = asm; Maintenance = mt; }

            public string Week { get; set; }
            public int QualityControl { get; set; }
            public int Production { get; set; }
            public int Assembly { get; set; }
            public int Maintenance { get; set; }
        }

        private class DowntimeRow
        {
            public DowntimeRow(string d, int m, int i) { Department = d; Downtime = m; Issues = i; }
            public string Department { get; set; }
            public int Downtime { get; set; }
            public int Issues { get; set; }
        }

        private class DeptPerfRow
        {
            public DeptPerfRow(string dep, int total, int avg, double totMin, double avgMin, string status)
            { Department = dep; TotalIssues = total; AvgWeek = avg; TotalDowntimeMin = totMin; AvgDowntimePerIssueMin = avgMin; Status = status; }

            public string Department { get; set; }
            public int TotalIssues { get; set; }
            public int AvgWeek { get; set; }
            public double TotalDowntimeMin { get; set; }
            public double AvgDowntimePerIssueMin { get; set; }
            public string Status { get; set; }
        }
        #endregion

        // สำหรับแสดง badge สถานะในตาราง
        public string GetStatusChip(object status)
        {
            var s = Convert.ToString(status) ?? "";
            if (s.Equals("High", StringComparison.OrdinalIgnoreCase))
                return "<span class='badge-chip chip-high'>High</span>";
            if (s.Equals("Medium", StringComparison.OrdinalIgnoreCase))
                return "<span class='badge-chip chip-med'>Medium</span>";
            return "<span class='badge-chip chip-low'>Low</span>";
        }
    }
}
