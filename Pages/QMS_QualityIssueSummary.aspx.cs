using System;
using System.Data;
using System.Linq;
using System.Text;

namespace IT_WorkPlant.Pages
{
    public partial class QMS_QualityIssueSummary : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindFilters();
                ApplyAndBind();
            }
        }

        // ---------- Events ----------
        protected void btnSearch_Click(object sender, EventArgs e) => ApplyAndBind();

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ddlWeek.SelectedIndex = 0;
            ddlUnit.SelectedIndex = 0;
            ddlDefectType.SelectedIndex = 0;
            ddlProduct.SelectedIndex = 0;
            ddlDept.SelectedIndex = 0;
            ddlStatus.SelectedIndex = 0;
            txtSearchTop.Text = string.Empty;
            ApplyAndBind();
        }

        protected void btnExport_Click(object sender, EventArgs e)
        {
            var filtered = FilterData(GetMock());
            var csv = ToCsv(filtered);
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment;filename=QMS_Issue_Summary.csv");
            Response.BinaryWrite(bytes);
            Response.End();
        }

        // ---------- Data ----------
        private DataTable GetMock()
        {
            var dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("ProductionWeek");
            dt.Columns.Add("Unit");
            dt.Columns.Add("Line");
            dt.Columns.Add("Product");
            dt.Columns.Add("DefectType");
            dt.Columns.Add("Description");
            dt.Columns.Add("Qty", typeof(int));
            dt.Columns.Add("Department");
            dt.Columns.Add("Action");
            dt.Columns.Add("FeedbackDate", typeof(DateTime));
            dt.Columns.Add("Status");
            dt.Columns.Add("Remarks");

            dt.Rows.Add("QI-2024-001", "2024-W35", "Unit A", "Line 1", "Product X-100", "Surface Defect",
                "Scratches on surface", 5, "Quality Control", "Polishing adjustment", DateTime.Parse("2024-08-29"), "Completed", "Resolved");

            dt.Rows.Add("QI-2024-002", "2024-W35", "Unit B", "Line 2", "Product Y-200", "Dimensional",
                "Out of tolerance", 12, "Production", "Equipment calibration", DateTime.Parse("2024-08-30"), "In Progress", "Monitoring");

            dt.Rows.Add("QI-2024-003", "2024-W36", "Unit A", "Line 3", "Product Z-300", "Assembly",
                "Component misalignment", 8, "Assembly", "Work instruction update", DBNull.Value, "Open", "Training required");

            dt.Rows.Add("QI-2024-004", "2024-W36", "Unit C", "Line 1", "Product X-100", "Material",
                "Raw material quality", 20, "Procurement", "Supplier audit", DBNull.Value, "Open", "Urgent");

            dt.Rows.Add("QI-2024-005", "2024-W36", "Unit B", "Line 2", "Product Y-200", "Process",
                "Temperature control", 3, "Maintenance", "Sensor replacement", DateTime.Parse("2024-09-04"), "Completed", "Fixed");

            return dt;
        }

        private void BindFilters()
        {
            var dt = GetMock();

            void BindDistinct(System.Web.UI.WebControls.DropDownList ddl, string col)
            {
                ddl.Items.Clear();
                ddl.Items.Add(new System.Web.UI.WebControls.ListItem("All", ""));
                foreach (var v in dt.AsEnumerable()
                        .Select(r => r[col]?.ToString())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct().OrderBy(s => s))
                    ddl.Items.Add(v);
            }

            BindDistinct(ddlWeek, "ProductionWeek");
            BindDistinct(ddlUnit, "Unit");
            BindDistinct(ddlDefectType, "DefectType");
            BindDistinct(ddlProduct, "Product");
            BindDistinct(ddlDept, "Department");

            ddlStatus.Items.Clear();
            ddlStatus.Items.Add(new System.Web.UI.WebControls.ListItem("All", ""));
            ddlStatus.Items.Add("Open");
            ddlStatus.Items.Add("In Progress");
            ddlStatus.Items.Add("Completed");
        }

        // ---------- Apply filters & bind ----------
        private void ApplyAndBind()
        {
            var dt = FilterData(GetMock());
            gvSummary.DataSource = dt;
            gvSummary.DataBind();

            litResultCount.Text = dt.Rows.Count.ToString();
            UpdateStats(dt);
        }

        private DataTable FilterData(DataTable source)
        {
            var q = source.AsEnumerable();

            string w = ddlWeek.SelectedValue;
            string u = ddlUnit.SelectedValue;
            string d = ddlDefectType.SelectedValue;
            string p = ddlProduct.SelectedValue;
            string dep = ddlDept.SelectedValue;
            string st = ddlStatus.SelectedValue;
            string key = (txtSearchTop.Text ?? "").Trim();

            if (!string.IsNullOrEmpty(w)) q = q.Where(r => EqualsCI(r["ProductionWeek"], w));
            if (!string.IsNullOrEmpty(u)) q = q.Where(r => EqualsCI(r["Unit"], u));
            if (!string.IsNullOrEmpty(d)) q = q.Where(r => EqualsCI(r["DefectType"], d));
            if (!string.IsNullOrEmpty(p)) q = q.Where(r => EqualsCI(r["Product"], p));
            if (!string.IsNullOrEmpty(dep)) q = q.Where(r => EqualsCI(r["Department"], dep));
            if (!string.IsNullOrEmpty(st)) q = q.Where(r => EqualsCI(r["Status"], st));

            if (!string.IsNullOrEmpty(key))
            {
                q = q.Where(r =>
                    ContainsCI(r["Id"], key) ||
                    ContainsCI(r["Product"], key) ||
                    ContainsCI(r["DefectType"], key) ||
                    ContainsCI(r["Description"], key) ||
                    ContainsCI(r["Department"], key) ||
                    ContainsCI(r["Action"], key) ||
                    ContainsCI(r["Remarks"], key)
                );
            }

            var rows = q.ToArray();
            return rows.Length > 0 ? rows.CopyToDataTable() : source.Clone();
        }

        // ---------- Stats ----------
        private void UpdateStats(DataTable dt)
        {
            int total = dt.Rows.Count;
            int completed = dt.AsEnumerable().Count(r => EqualsCI(r["Status"], "Completed"));
            int open = dt.AsEnumerable().Count(r => !EqualsCI(r["Status"], "Completed")); // = Open + In Progress
            int qty = dt.AsEnumerable().Sum(r => r["Qty"] == DBNull.Value ? 0 : Convert.ToInt32(r["Qty"]));

            litTotalIssues.Text = total.ToString();
            litCompleted.Text = completed.ToString();
            litOpenIssues.Text = open.ToString();
            litTotalQty.Text = qty.ToString();
        }

        // ---------- Helpers for filtering ----------
        private static bool EqualsCI(object a, string b) =>
            string.Equals(Convert.ToString(a), b, StringComparison.OrdinalIgnoreCase);

        private static bool ContainsCI(object a, string key) =>
            Convert.ToString(a)?.IndexOf(key, StringComparison.OrdinalIgnoreCase) >= 0;

        // ---------- HTML helpers for table ----------
        protected string QtyChip(object qtyObj)
        {
            int q = 0; if (qtyObj != null && qtyObj != DBNull.Value) q = Convert.ToInt32(qtyObj);
            return $"<span class='pill qty-chip'>{q}</span>";
        }

        protected string FeedbackDateHtml(object dateObj)
        {
            if (dateObj == null || dateObj == DBNull.Value)
                return "<span class='muted'>Pending</span>";
            var d = (DateTime)dateObj;
            return $"<i class='bi bi-calendar2 me-1'></i>{d:yyyy-MM-dd}";
        }

        protected string RemarksHtml(object remarksObj)
        {
            var s = Convert.ToString(remarksObj) ?? "";
            if (string.IsNullOrWhiteSpace(s)) return "";
            var tags = s.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0)
                .Select(t => string.Equals(t, "Urgent", StringComparison.OrdinalIgnoreCase)
                    ? $"<span class='pill tag-danger'>Urgent</span>"
                    : $"<span class='pill tag-default'>{Server.HtmlEncode(t)}</span>");
            return string.Join(" ", tags);
        }

        // Badge HTML (เข้ากับ QMS.theme.css)
        protected string GetStatusBadge(string status)
        {
            if (string.Equals(status, "Open", StringComparison.OrdinalIgnoreCase))
                return "<span class='pill badge-open'>Open</span>";
            if (string.Equals(status, "In Progress", StringComparison.OrdinalIgnoreCase))
                return "<span class='pill badge-progress'>In Progress</span>";
            if (string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase))
                return "<span class='pill badge-done'>Completed</span>";
            return $"<span class='pill bg-secondary'>{Server.HtmlEncode(status)}</span>";
        }

        // ---------- CSV ----------
        private string ToCsv(DataTable dt)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", dt.Columns.Cast<DataColumn>().Select(c => CsvEscape(c.ColumnName))));
            foreach (DataRow r in dt.Rows)
            {
                var cells = dt.Columns.Cast<DataColumn>().Select(c => CsvEscape(r[c]));
                sb.AppendLine(string.Join(",", cells));
            }
            return sb.ToString();
        }

        private static string CsvEscape(object value)
        {
            var s = value == null || value == DBNull.Value ? "" : Convert.ToString(value);
            s = s.Replace("\"", "\"\"");
            return $"\"{s}\"";
        }
    }
}
