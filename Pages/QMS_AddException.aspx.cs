using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IT_WorkPlant.Pages
{
    public partial class QMS_AddException : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // set page title in master
            var lit = Master.FindControl("litPageTitle") as Literal;
            if (lit != null) lit.Text = "Add Exception";

            if (!IsPostBack)
            {
                BindDropdowns();
                pnSuccess.Visible = false;
            }
        }

        private void BindDropdowns()
        {
            // Years
            ddlYear.Items.Clear();
            ddlYear.Items.Add(new ListItem("-- Select year --", ""));
            foreach (var y in new[] { "2024", "2023", "2022" })
                ddlYear.Items.Add(new ListItem(y, y));

            // Weeks
            ddlWeek.Items.Clear();
            ddlWeek.Items.Add(new ListItem("-- Select week --", ""));
            foreach (var w in new[] { "W36", "W35", "W34", "W33" })
                ddlWeek.Items.Add(new ListItem(w, w));

            // Units/Lines/Defect/Product (sample options; replace with DB binding as needed)
            BindList(ddlUnit, new[] { "Unit A", "Unit B", "Unit C" }, "-- Select unit --");
            BindList(ddlLine, new[] { "Line 1", "Line 2", "Line 3" }, "-- Select line --");
            BindList(ddlDefect, new[] { "Surface Defect", "Dimensional", "Assembly", "Material", "Process" }, "-- Select defect --");
            BindList(ddlProduct, new[] { "Product X-100", "Product Y-200", "Product Z-300" }, "-- Select product --");
            BindList(ddlRespDept, new[] { "Quality Control", "Production", "Assembly", "Procurement", "Maintenance", "Engineering" }, "-- Select department --");
        }

        private void BindList(DropDownList ddl, string[] items, string first)
        {
            ddl.Items.Clear();
            ddl.Items.Add(new ListItem(first, ""));
            foreach (var it in items) ddl.Items.Add(new ListItem(it, it));
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            // clear inputs
            ddlYear.SelectedIndex = 0; ddlWeek.SelectedIndex = 0;
            txtDate.Text = string.Empty;

            ddlUnit.SelectedIndex = 0; ddlLine.SelectedIndex = 0; ddlDefect.SelectedIndex = 0; ddlProduct.SelectedIndex = 0;
            txtDesc.Text = string.Empty; txtQty.Text = string.Empty;

            ddlRespDept.SelectedIndex = 0; txtRespAction.Text = string.Empty; txtRemarks.Text = string.Empty;

            pnSuccess.Visible = false;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            // server-side validate
            Page.Validate();
            if (!Page.IsValid) return;

            // basic sanitization
            string year = HttpUtility.HtmlEncode(ddlYear.SelectedValue);
            string week = HttpUtility.HtmlEncode(ddlWeek.SelectedValue);
            string date = HttpUtility.HtmlEncode(txtDate.Text);
            string unit = HttpUtility.HtmlEncode(ddlUnit.SelectedValue);
            string line = HttpUtility.HtmlEncode(ddlLine.SelectedValue);
            string defect = HttpUtility.HtmlEncode(ddlDefect.SelectedValue);
            string product = HttpUtility.HtmlEncode(ddlProduct.SelectedValue);
            string desc = HttpUtility.HtmlEncode(txtDesc.Text);
            int qty = 0; int.TryParse(txtQty.Text, out qty);
            string dept = HttpUtility.HtmlEncode(ddlRespDept.SelectedValue);
            string action = HttpUtility.HtmlEncode(txtRespAction.Text);
            string remarks = HttpUtility.HtmlEncode(txtRemarks.Text);

            // TODO: persist to DB using parameterized SQL (example)
            // using (var conn = new SqlConnection(...))
            // using (var cmd = new SqlCommand("INSERT INTO QMS_Exceptions (...) VALUES (...)", conn)) {
            //   cmd.Parameters.AddWithValue("@Year", year);
            //   ...
            //   conn.Open(); cmd.ExecuteNonQuery();
            // }

            // handle uploaded images (save to a safe folder if needed)
            // var files = Request.Files; // iterate and save images

            pnSuccess.Visible = true;
        }
    }
}
