using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace IT_WorkPlant.Pages
{
    public partial class QMS_QualityIssueDetail : System.Web.UI.Page
    {
        private const string VS_EDIT = "QMS_EDIT_MODE";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropdowns();
                LoadDataFromQuery();

                // ถ้าถูกเรียกด้วย &edit=1 ให้เข้าโหมดแก้ไขทันที
                if (Request.QueryString["edit"] == "1") SetEditMode(true);
                else SetEditMode(false);
            }
        }

        // --------- View/Edit toggle ----------
        private void SetEditMode(bool editing)
        {
            ViewState[VS_EDIT] = editing;

            pnlView.Visible = !editing;
            pnlEdit.Visible = editing;

            btnEdit.Visible = !editing;
            btnSave.Visible = editing;
            btnCancel.Visible = editing;
        }

        private bool IsEditMode() => (bool?)ViewState[VS_EDIT] == true;

        // --------- Bind dropdowns (mock) ----------
        private void BindDropdowns()
        {
            ddlUnit.Items.Clear(); ddlUnit.Items.Add("Unit A"); ddlUnit.Items.Add("Unit B"); ddlUnit.Items.Add("Unit C");
            ddlLine.Items.Clear(); ddlLine.Items.Add("Line 1"); ddlLine.Items.Add("Line 2"); ddlLine.Items.Add("Line 3");
            ddlProduct.Items.Clear(); ddlProduct.Items.Add("Product X-100"); ddlProduct.Items.Add("Product Y-200"); ddlProduct.Items.Add("Product Z-300");
            ddlDefectType.Items.Clear(); ddlDefectType.Items.Add("Surface Defect"); ddlDefectType.Items.Add("Dimensional"); ddlDefectType.Items.Add("Assembly"); ddlDefectType.Items.Add("Material"); ddlDefectType.Items.Add("Process");
            ddlDept.Items.Clear(); ddlDept.Items.Add("Quality Control"); ddlDept.Items.Add("Production"); ddlDept.Items.Add("Assembly"); ddlDept.Items.Add("Maintenance"); ddlDept.Items.Add("Procurement");
            ddlStatus.Items.Clear(); ddlStatus.Items.Add("Open"); ddlStatus.Items.Add("In Progress"); ddlStatus.Items.Add("Completed");
        }

        // --------- Data (Mock) ----------
        private DataTable GetMock()
        {
            var dt = new DataTable();
            dt.Columns.Add("Id"); dt.Columns.Add("ProductionWeek");
            dt.Columns.Add("Date", typeof(DateTime));
            dt.Columns.Add("Unit"); dt.Columns.Add("Line");
            dt.Columns.Add("DefectType"); dt.Columns.Add("Product");
            dt.Columns.Add("Description"); dt.Columns.Add("Short"); dt.Columns.Add("Corrective"); dt.Columns.Add("Remarks");
            dt.Columns.Add("Downtime", typeof(int));
            dt.Columns.Add("Dept"); dt.Columns.Add("Person");
            dt.Columns.Add("Status"); dt.Columns.Add("CompletedDate", typeof(DateTime));
            dt.Columns.Add("CreatedBy"); dt.Columns.Add("CreatedDate", typeof(DateTime));
            dt.Columns.Add("ModifiedBy"); dt.Columns.Add("ModifiedDate", typeof(DateTime));

            dt.Rows.Add("QI-2024-001", "2024-W35", DateTime.Parse("2024-08-28"), "Unit A", "Line 1", "Surface Defect", "Product X-100",
                "Scratches found on product surface during final inspection",
                "Immediate inspection of all units in batch, rework of affected products",
                "Adjusted polishing parameters and replaced worn brushes",
                "Issue resolved, monitoring ongoing. Preventive maintenance scheduled.",
                45, "Quality Control", "John Smith", "Completed", DateTime.Parse("2024-08-29"),
                "Jane Doe", DateTime.Parse("2024-08-28"), "John Smith", DateTime.Parse("2024-08-29"));

            dt.Rows.Add("QI-2024-002", "2024-W35", DateTime.Parse("2024-08-29"), "Unit B", "Line 2", "Dimensional", "Product Y-200",
                "Parts out of tolerance specification", "—", "—", "—",
                120, "Production", "Alice", "In Progress", DBNull.Value,
                "Jane Doe", DateTime.Parse("2024-08-29"), "Alice", DateTime.Parse("2024-08-30"));

            dt.Rows.Add("QI-2024-003", "2024-W36", DateTime.Parse("2024-09-02"), "Unit A", "Line 3", "Assembly", "Product Z-300",
                "Incorrect component assembly detected", "—", "—", "—",
                90, "Assembly", "Bob", "Open", DBNull.Value,
                "Jane Doe", DateTime.Parse("2024-09-02"), "Bob", DateTime.Parse("2024-09-02"));

            return dt;
        }

        // --------- Load ----------
        private void LoadDataFromQuery()
        {
            pnlAlert.Visible = pnlSuccess.Visible = false;

            string id = Request.QueryString["id"];
            if (string.IsNullOrWhiteSpace(id))
            {
                ShowError("Missing issue id.");
                return;
            }

            id = id.Trim();
            var dt = GetMock();
            var rows = dt.Select($"Id = '{id.Replace("'", "''")}'");
            if (rows.Length == 0)
            {
                ShowError($"Issue '{Server.HtmlEncode(id)}' not found (mock).");
                return;
            }

            var r = rows[0];

            // Header + Status
            litIssueId.Text = r["Id"].ToString();
            litStatusBadgeTop.Text = GetStatusBadge(r["Status"].ToString());

            // ---------- Fill EDIT controls ----------
            txtWeek.Text = r["ProductionWeek"].ToString();
            txtDate.Text = ((DateTime)r["Date"]).ToString("yyyy-MM-dd");

            SelectOrAdd(ddlUnit, r["Unit"].ToString());
            SelectOrAdd(ddlLine, r["Line"].ToString());
            SelectOrAdd(ddlProduct, r["Product"].ToString());
            SelectOrAdd(ddlDefectType, r["DefectType"].ToString());

            txtDowntime.Text = Convert.ToInt32(r["Downtime"]).ToString();
            txtDescription.Text = r["Description"].ToString();
            txtShort.Text = r["Short"].ToString();
            txtCorrective.Text = r["Corrective"].ToString();
            txtRemarks.Text = r["Remarks"].ToString();

            SelectOrAdd(ddlDept, r["Dept"].ToString());
            txtPerson.Text = r["Person"].ToString();
            SelectOrAdd(ddlStatus, r["Status"].ToString());
            txtCompletedDate.Text = r["CompletedDate"] == DBNull.Value ? "" : ((DateTime)r["CompletedDate"]).ToString("yyyy-MM-dd");

            // ---------- Fill VIEW literals ----------
            vWeek.Text = Server.HtmlEncode(r["ProductionWeek"].ToString());
            vDate.Text = ((DateTime)r["Date"]).ToString("yyyy/MM/dd");
            vUnit.Text = Server.HtmlEncode(r["Unit"].ToString());
            vLine.Text = Server.HtmlEncode(r["Line"].ToString());
            vProduct.Text = Server.HtmlEncode(r["Product"].ToString());
            vDefectType.Text = Server.HtmlEncode(r["DefectType"].ToString());
            vDowntime.Text = Convert.ToInt32(r["Downtime"]).ToString();

            vDescription.Text = Server.HtmlEncode(r["Description"].ToString());
            vShort.Text = Server.HtmlEncode(r["Short"].ToString());
            vCorrective.Text = Server.HtmlEncode(r["Corrective"].ToString());
            vRemarks.Text = Server.HtmlEncode(r["Remarks"].ToString());

            vDept.Text = Server.HtmlEncode(r["Dept"].ToString());
            vPerson.Text = Server.HtmlEncode(r["Person"].ToString());
            vStatus.Text = GetStatusBadge(r["Status"].ToString());
            vCompletedDate.Text = r["CompletedDate"] == DBNull.Value ? "—" : ((DateTime)r["CompletedDate"]).ToString("yyyy/MM/dd");

            // Audit
            litCreatedBy.Text = $"{Server.HtmlEncode(r["CreatedBy"].ToString())}<br/>{((DateTime)r["CreatedDate"]).ToString("yyyy-MM-dd")}";
            litModifiedBy.Text = $"{Server.HtmlEncode(r["ModifiedBy"].ToString())}<br/>{((DateTime)r["ModifiedDate"]).ToString("yyyy-MM-dd")}";
        }

        private void SelectOrAdd(System.Web.UI.WebControls.DropDownList ddl, string value)
        {
            var item = ddl.Items.FindByText(value) ?? ddl.Items.FindByValue(value);
            if (item == null) ddl.Items.Add(value);
            ddl.SelectedValue = value;
        }

        private void ShowError(string msg)
        {
            litError.Text = msg;
            pnlAlert.Visible = true;
            SetEditMode(false);
        }

        // --------- Buttons ----------
        protected void btnEdit_Click(object sender, EventArgs e)
        {
            pnlAlert.Visible = pnlSuccess.Visible = false;
            SetEditMode(true);
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            pnlAlert.Visible = pnlSuccess.Visible = false;
            LoadDataFromQuery();  // revert
            SetEditMode(false);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            pnlAlert.Visible = pnlSuccess.Visible = false;

            // Required
            if (string.IsNullOrWhiteSpace(txtWeek.Text) ||
                string.IsNullOrWhiteSpace(txtDate.Text) ||
                string.IsNullOrWhiteSpace(txtDescription.Text) ||
                string.IsNullOrWhiteSpace(txtPerson.Text))
            {
                ShowError("Please fill required fields: Week, Date, Description, Responsible Person.");
                return;
            }

            // Validate numeric
            if (!int.TryParse(txtDowntime.Text, out var minutes) || minutes < 0)
            {
                ShowError("Downtime must be a non-negative number.");
                return;
            }

            // mock "save": sync back to VIEW literals
            vWeek.Text = Server.HtmlEncode(txtWeek.Text.Trim());
            vDate.Text = txtDate.Text;
            vUnit.Text = Server.HtmlEncode(ddlUnit.SelectedValue);
            vLine.Text = Server.HtmlEncode(ddlLine.SelectedValue);
            vProduct.Text = Server.HtmlEncode(ddlProduct.SelectedValue);
            vDefectType.Text = Server.HtmlEncode(ddlDefectType.SelectedValue);
            vDowntime.Text = minutes.ToString();

            vDescription.Text = Server.HtmlEncode(txtDescription.Text.Trim());
            vShort.Text = Server.HtmlEncode(txtShort.Text.Trim());
            vCorrective.Text = Server.HtmlEncode(txtCorrective.Text.Trim());
            vRemarks.Text = Server.HtmlEncode(txtRemarks.Text.Trim());

            vDept.Text = Server.HtmlEncode(ddlDept.SelectedValue);
            vPerson.Text = Server.HtmlEncode(txtPerson.Text.Trim());
            vStatus.Text = GetStatusBadge(ddlStatus.SelectedValue);
            vCompletedDate.Text = string.IsNullOrWhiteSpace(txtCompletedDate.Text) ? "—" : txtCompletedDate.Text;

            litStatusBadgeTop.Text = GetStatusBadge(ddlStatus.SelectedValue);

            pnlSuccess.Visible = true;
            SetEditMode(false);
        }

        // --------- Badge ----------
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
    }
}
