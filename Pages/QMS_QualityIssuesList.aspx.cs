using System;
using System.Data;

namespace IT_WorkPlant.Pages
{
    public partial class QMS_QualityIssuesList : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindFilters();
                BindTable();
            }
        }

        private void BindFilters()
        {
            ddlFWeek.Items.Clear(); ddlFWeek.Items.Add(""); ddlFWeek.Items.Add("2024-W36"); ddlFWeek.Items.Add("2024-W35"); ddlFWeek.Items.Add("2024-W34");
            ddlFUnit.Items.Clear(); ddlFUnit.Items.Add(""); ddlFUnit.Items.Add("Unit A"); ddlFUnit.Items.Add("Unit B"); ddlFUnit.Items.Add("Unit C");
            ddlFDefect.Items.Clear(); ddlFDefect.Items.Add(""); ddlFDefect.Items.Add("Surface Defect"); ddlFDefect.Items.Add("Dimensional"); ddlFDefect.Items.Add("Assembly"); ddlFDefect.Items.Add("Material"); ddlFDefect.Items.Add("Process");
            ddlFProduct.Items.Clear(); ddlFProduct.Items.Add(""); ddlFProduct.Items.Add("Product X-100"); ddlFProduct.Items.Add("Product Y-200"); ddlFProduct.Items.Add("Product Z-300");
            ddlFRespUnit.Items.Clear(); ddlFRespUnit.Items.Add(""); ddlFRespUnit.Items.Add("Quality Control"); ddlFRespUnit.Items.Add("Production"); ddlFRespUnit.Items.Add("Assembly"); ddlFRespUnit.Items.Add("Maintenance"); ddlFRespUnit.Items.Add("Procurement");
            ddlFStatus.Items.Clear(); ddlFStatus.Items.Add(""); ddlFStatus.Items.Add("Open"); ddlFStatus.Items.Add("In Progress"); ddlFStatus.Items.Add("Completed");
        }

        private DataTable GetMock()
        {
            var dt = new DataTable();
            dt.Columns.Add("Id"); dt.Columns.Add("ProductionWeek"); dt.Columns.Add("Date", typeof(DateTime));
            dt.Columns.Add("ProductionUnit"); dt.Columns.Add("Line"); dt.Columns.Add("DefectType");
            dt.Columns.Add("Product"); dt.Columns.Add("Description"); dt.Columns.Add("Downtime", typeof(int));
            dt.Columns.Add("ResponsibleUnit"); dt.Columns.Add("Status");

            dt.Rows.Add("QI-2024-001", "2024-W35", DateTime.Parse("2024-08-28"), "Unit A", "Line 1", "Surface Defect", "Product X-100", "Scratches found on product surface during final inspection", 45, "Quality Control", "Completed");
            dt.Rows.Add("QI-2024-002", "2024-W35", DateTime.Parse("2024-08-29"), "Unit B", "Line 2", "Dimensional", "Product Y-200", "Parts out of tolerance specification", 120, "Production", "In Progress");
            dt.Rows.Add("QI-2024-003", "2024-W36", DateTime.Parse("2024-09-02"), "Unit A", "Line 3", "Assembly", "Product Z-300", "Incorrect component assembly detected", 90, "Assembly", "Open");
            dt.Rows.Add("QI-2024-004", "2024-W36", DateTime.Parse("2024-09-03"), "Unit C", "Line 1", "Material", "Product X-100", "Raw material quality issue identified", 180, "Procurement", "Open");
            dt.Rows.Add("QI-2024-005", "2024-W36", DateTime.Parse("2024-09-04"), "Unit B", "Line 2", "Process", "Product Y-200", "Temperature control malfunction", 60, "Maintenance", "Completed");
            return dt;
        }

        private void BindTable()
        {
            // demo filter แบบง่าย
            var dt = GetMock();
            // (ถ้าต้องการ filter จริง ให้ Select() ตามค่าจากฟิลเตอร์ที่เลือก แล้ว CopyToDataTable())
            gvIssues.DataSource = dt;
            gvIssues.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e) => BindTable();
        protected void btnClear_Click(object sender, EventArgs e)
        {
            foreach (var d in new[] { ddlFWeek, ddlFUnit, ddlFDefect, ddlFProduct, ddlFRespUnit, ddlFStatus })
                d.SelectedIndex = 0;
            txtFDateFrom.Text = ""; txtFDateTo.Text = "";
            BindTable();
        }

        protected string GetStatusBadge(string status)
        {
            if (string.Equals(status, "Open", StringComparison.OrdinalIgnoreCase))
                return "<span class='badge-open'>Open</span>";
            if (string.Equals(status, "In Progress", StringComparison.OrdinalIgnoreCase))
                return "<span class='badge-progress'>In Progress</span>";
            if (string.Equals(status, "Completed", StringComparison.OrdinalIgnoreCase))
                return "<span class='badge-done'>Completed</span>";
            return $"<span class='badge bg-secondary'>{status}</span>";
        }
    }
}
