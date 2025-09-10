using System;
using System.Data;
using System.Linq;

namespace IT_WorkPlant.Pages
{
    public partial class QMS_AddQualityIssue : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindDropdowns();
            }
        }

        private void BindDropdowns()
        {
            // ปี
            ddlYear.Items.Clear();
            ddlYear.Items.Add("");
            ddlYear.Items.Add("2024");
            ddlYear.Items.Add("2023");
            ddlYear.Items.Add("2022");

            // สัปดาห์
            ddlWeek.Items.Clear();
            ddlWeek.Items.Add("");
            ddlWeek.Items.Add("W36");
            ddlWeek.Items.Add("W35");
            ddlWeek.Items.Add("W34");
            ddlWeek.Items.Add("W33");

            // Unit
            ddlUnit.Items.Clear();
            ddlUnit.Items.Add("");
            ddlUnit.Items.Add("Unit A");
            ddlUnit.Items.Add("Unit B");
            ddlUnit.Items.Add("Unit C");
            ddlUnit.Items.Add("Unit D");

            // Line
            ddlLine.Items.Clear();
            ddlLine.Items.Add("");
            ddlLine.Items.Add("Line 1");
            ddlLine.Items.Add("Line 2");
            ddlLine.Items.Add("Line 3");
            ddlLine.Items.Add("Line 4");

            // Defect Category / SubType
            ddlDefectCategory.Items.Clear();
            ddlDefectCategory.Items.Add("");
            ddlDefectCategory.Items.Add("Surface Defect");
            ddlDefectCategory.Items.Add("Dimensional");
            ddlDefectCategory.Items.Add("Assembly");
            ddlDefectCategory.Items.Add("Material");
            ddlDefectCategory.Items.Add("Process");
            ddlDefectCategory.Items.Add("Electrical");

            ddlDefectSubType.Items.Clear();
            ddlDefectSubType.Items.Add("");
            ddlDefectSubType.Items.Add("Scratch");
            ddlDefectSubType.Items.Add("Dent");
            ddlDefectSubType.Items.Add("Crack");
            ddlDefectSubType.Items.Add("Discoloration");
            ddlDefectSubType.Items.Add("Out of Tolerance");
            ddlDefectSubType.Items.Add("Missing Component");
            ddlDefectSubType.Items.Add("Wrong Component");

            // Product Series
            ddlProductSeries.Items.Clear();
            ddlProductSeries.Items.Add("");
            ddlProductSeries.Items.Add("Product X-100");
            ddlProductSeries.Items.Add("Product Y-200");
            ddlProductSeries.Items.Add("Product Z-300");
            ddlProductSeries.Items.Add("Product A-400");
            ddlProductSeries.Items.Add("Product B-500");

            // Dept (Responsible)
            ddlDept.Items.Clear();
            ddlDept.Items.Add("");
            ddlDept.Items.Add("Quality Control");
            ddlDept.Items.Add("Production");
            ddlDept.Items.Add("Assembly");
            ddlDept.Items.Add("Maintenance");
            ddlDept.Items.Add("Procurement");
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            foreach (var c in new[] { txtDate, txtDescription, txtDownH, txtDownM, txtPerson, txtShort, txtRemarks })
                c.Text = "";
            foreach (var d in new[] { ddlYear, ddlWeek, ddlUnit, ddlLine, ddlDefectCategory, ddlDefectSubType, ddlProductSeries, ddlDept })
                d.SelectedIndex = 0;

            pnlAlert.Visible = false;
            pnlSuccess.Visible = false;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            pnlAlert.Visible = false;
            pnlSuccess.Visible = false;

            // Validate required:
            string[] req =
            {
                ddlYear.SelectedValue,
                ddlWeek.SelectedValue,
                txtDate.Text,
                ddlUnit.SelectedValue,
                ddlLine.SelectedValue,
                ddlDefectCategory.SelectedValue,
                ddlDefectSubType.SelectedValue,
                ddlProductSeries.SelectedValue,
                txtDescription.Text,
                ddlDept.SelectedValue,
                txtPerson.Text
            };

            if (req.Any(string.IsNullOrWhiteSpace))
            {
                litError.Text = "Please fill in all required fields (marked with *).";
                pnlAlert.Visible = true;
                return;
            }

            // TODO: บันทึกลงฐานข้อมูลจริง (Stored Procedure/EF/Dapper)
            // ตัวอย่าง demo only
            pnlSuccess.Visible = true;
            btnReset_Click(sender, e);
        }
    }
}
