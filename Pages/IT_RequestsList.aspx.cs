using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using IT_WorkPlant.Models;

namespace IT_WorkPlant.Pages
{
    public partial class IT_RequestsList : System.Web.UI.Page
    {
        private IT_RequestModel _model;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (_model == null)
                _model = new IT_RequestModel();

            // App_Temp
            string uploadFolder = Server.MapPath("~/App_Temp");
            if (!System.IO.Directory.Exists(uploadFolder))
            {
                // ถ้าไม่มีก็สร้างขึ้นมา
                System.IO.Directory.CreateDirectory(uploadFolder);
            }

            if (!IsPostBack)
            {
                if (Session["UserEmpID"] == null)
                    Response.Redirect("../Login.aspx");

                string userDept = Session["DeptName"]?.ToString();
                gvRequests.Columns[gvRequests.Columns.Count - 1].Visible = userDept == "IT";

                BindDepartmentsDropdown(userDept);
                BindRequestData();
                BindFilters();

                ddlStatus.SelectedValue = "WIP";
                ViewState["Status"] = "WIP";

                BindRequestData();
            }
        }


        private void BindDepartmentsDropdown(string userDept)
        {
            DataTable dtDepartments = _model.GetDepartments();
            ddlDeptName.DataSource = dtDepartments;
            ddlDeptName.DataTextField = "DeptName_en";
            ddlDeptName.DataValueField = "DeptNameID";
            ddlDeptName.DataBind();

            if (userDept == "IT") //✅ If the user is an IT department, you can select it// ถ้าคนเข้าใช้งานคือฝ่ายไอที ให้เลือกได้
            {
                ddlDeptName.Items.Insert(0, new ListItem("-- Select Department --", ""));
                ddlDeptName.Enabled = true;
            }
            else //✅ If you are not an IT person, lock the dropdown // ถ้าไม่ใช่ฝ่ายไอทีให้ล็อก Dropdown
            {
                DataRow deptRow = dtDepartments.AsEnumerable()
                    .FirstOrDefault(r => r["DeptName_en"].ToString() == userDept);

                if (deptRow != null)
                    ddlDeptName.SelectedValue = deptRow["DeptNameID"].ToString();

                ddlDeptName.Enabled = false;
            }

#if DEBUG
            //✅ Debug check data ** ตรวจสอบข้อมูล **
            System.Diagnostics.Debug.WriteLine("===== START DEBUG: Dropdown Departments =====");
            foreach (ListItem item in ddlDeptName.Items)
                System.Diagnostics.Debug.WriteLine($"{item.Value} - {item.Text}");
            System.Diagnostics.Debug.WriteLine("===== END DEBUG: Dropdown Departments =====");
#endif
        }

        private void BindRequestData()
        {
            string issueMonth = ViewState["IssueMonth"]?.ToString();
            string issueDate = ViewState["IssueDate"]?.ToString();
            string deptName = ViewState["Department"]?.ToString();
            string requestUser = ViewState["RequestUser"]?.ToString();
            string issueType = ViewState["IssueType"]?.ToString();
            string status = ViewState["Status"]?.ToString();

            // ✅ Retrieve data from the database (already sorted) // ดึงข้อมูลจากฐานข้อมูล (ที่เรียงถูกแล้ว)
            DataTable dt = _model.GetFilteredRequests(deptName, requestUser, issueType, status, issueMonth, issueDate);
            DataView dv = dt.DefaultView;
            dv.Sort = "ReportID ASC";

#if DEBUG
            // ✅ Debug to check the retrieved values  // เพื่อตรวจสอบค่าที่ดึงมา
            foreach (DataRow row in dt.Rows)
                System.Diagnostics.Debug.WriteLine("ReportID: " + row["ReportID"]);
#endif

            gvRequests.DataSource = dv;
            gvRequests.DataBind();
        }

        private void BindFilters()
        {
            BindIssueMonthFilter(); // Call the month filter

            //✅ dded this section to ensure the department dropdown only shows "All Department"
            ddlDeptName.Items.Clear();
            ddlDeptName.Items.Insert(0, new ListItem("All Department", ""));

            BindFilterDropdown(ddlRequestUser, "RequestUser", "All Request Users");
            BindFilterDropdown(ddlIssueType, "IssueType", "All Issue Types");
            BindFilterDropdown(ddlStatus, "Status", "All Statuses");
        }

        private void BindFilterDropdown(DropDownList dropdown, string columnName, string defaultText)
        {
            DataTable dt = _model.GetFilterOptions(columnName);
            dropdown.DataSource = dt;
            dropdown.DataTextField = "Value";
            dropdown.DataValueField = "Value";
            dropdown.DataBind();
            dropdown.Items.Insert(0, new ListItem(defaultText, ""));
        }

        private void BindIssueMonthFilter()
        {
            DataTable dt = _model.GetFilterOptions("FORMAT(IssueDate, 'yyyy-MM')");
            ddlIssueMonth.DataSource = dt;
            ddlIssueMonth.DataTextField = "Value";
            ddlIssueMonth.DataValueField = "Value";
            ddlIssueMonth.DataBind();
            ddlIssueMonth.Items.Insert(0, new ListItem("All Months", ""));
        }

        protected void FilterChanged(object sender, EventArgs e)
        {
            ViewState["IssueMonth"] = ddlIssueMonth.SelectedValue;
            ViewState["Department"] = ddlDeptName.SelectedValue;
            ViewState["RequestUser"] = ddlRequestUser.SelectedValue;
            ViewState["IssueType"] = ddlIssueType.SelectedValue;
            ViewState["Status"] = ddlStatus.SelectedValue;

            BindRequestData();
        }

        protected void gvRequests_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvRequests.PageIndex = e.NewPageIndex;
            BindRequestData();
        }

        protected void gvRequests_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvRequests.EditIndex = e.NewEditIndex;
            BindRequestData();

            DropDownList ddlDRIUser = gvRequests.Rows[e.NewEditIndex].FindControl("ddlDRIUser") as DropDownList;
            if (ddlDRIUser != null)
            {
                DataTable dtUsers = _model.GetUsersByDepartment("IT");
                ddlDRIUser.DataSource = dtUsers;
                ddlDRIUser.DataTextField = "UserName";
                ddlDRIUser.DataValueField = "UserIndex";
                ddlDRIUser.DataBind();
            }

            DropDownList ddlDepartment = gvRequests.Rows[e.NewEditIndex].FindControl("ddlDepartment") as DropDownList;
            if (ddlDepartment != null)
            {
                DataTable dtDepartments = _model.GetDepartments();
                ddlDepartment.DataSource = dtDepartments;
                ddlDepartment.DataTextField = "DeptName_en";
                ddlDepartment.DataValueField = "DeptNameID";
                ddlDepartment.DataBind();

                object deptObj = gvRequests.DataKeys[e.NewEditIndex].Values["Department"];
                string currentDept = deptObj?.ToString().Trim();

#if DEBUG
                System.Diagnostics.Debug.WriteLine($"===== Current Department: {currentDept} =====");
                foreach (ListItem item in ddlDepartment.Items)
                    System.Diagnostics.Debug.WriteLine($"{item.Value} - {item.Text}");
#endif
                ListItem selectedItem = ddlDepartment.Items.FindByText(currentDept);
                if (selectedItem != null)
                    ddlDepartment.SelectedValue = selectedItem.Value;
                else
                    System.Diagnostics.Debug.WriteLine("⚠️ ไม่พบค่าใน DropDownList: " + currentDept);
            }

            DropDownList ddlIssueType = gvRequests.Rows[e.NewEditIndex].FindControl("ddlIssueType") as DropDownList;
            if (ddlIssueType != null)
            {
                DataTable dtIssueTypes = _model.GetAllIssueTypes();
                ddlIssueType.DataSource = dtIssueTypes;
                ddlIssueType.DataTextField = "IssueTypeCode";
                ddlIssueType.DataValueField = "IssueTypeID";
                ddlIssueType.DataBind();

                object issueTypeObj = gvRequests.DataKeys[e.NewEditIndex].Values["IssueTypeID"];
                ddlIssueType.SelectedValue = issueTypeObj?.ToString();
            }
        }

        protected void gvRequests_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvRequests.EditIndex = -1;
            BindRequestData();
        }

        protected void gvRequests_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int reportID = Convert.ToInt32(gvRequests.DataKeys[e.RowIndex].Value);

            DropDownList ddlIssueType = gvRequests.Rows[e.RowIndex].FindControl("ddlIssueType") as DropDownList;
            int selectedIssueTypeID = ddlIssueType != null ? Convert.ToInt32(ddlIssueType.SelectedValue) : 0;

            DropDownList ddlDRIUser = gvRequests.Rows[e.RowIndex].FindControl("ddlDRIUser") as DropDownList;
            int? driUserIndex = ddlDRIUser != null ? (int?)Convert.ToInt32(ddlDRIUser.SelectedValue) : null;

            DropDownList ddlStatus = gvRequests.Rows[e.RowIndex].FindControl("DropDownList1") as DropDownList;
            bool isDone = ddlStatus != null && ddlStatus.SelectedValue == "1"; // 1 = Done

            string solution = ((TextBox)gvRequests.Rows[e.RowIndex].Cells[7].Controls[0]).Text;
            string remark = ((TextBox)gvRequests.Rows[e.RowIndex].Cells[11].Controls[0]).Text;
            object finishedDate = isDone ? (object)DateTime.Now : DBNull.Value;

            DropDownList ddlDepartment = gvRequests.Rows[e.RowIndex].FindControl("ddlDepartment") as DropDownList;
            string selectedDeptID = ddlDepartment?.SelectedValue;

#if DEBUG
            System.Diagnostics.Debug.WriteLine("======= DEBUG LOG =======");
            System.Diagnostics.Debug.WriteLine("ReportID: " + reportID);
            System.Diagnostics.Debug.WriteLine("isDone: " + isDone);
            System.Diagnostics.Debug.WriteLine("FinishedDate ก่อนอัปเดต: " + finishedDate);
            System.Diagnostics.Debug.WriteLine("SelectedDeptID ก่อนอัปเดต: " + selectedDeptID);
            System.Diagnostics.Debug.WriteLine("=========================");
#endif

            string query = @"
                UPDATE IT_RequestList
                SET DRI_UserID = @DRIUserID,
                    DeptNameID = @DeptNameID,
                    IssueTypeID = @IssueTypeID,
                    Solution = @Solution, 
                    Status = @Status, 
                    FinishedDate = @FinishedDate, 
                    Remark = @Remark, 
                    LastUpdateDate = GETDATE()
                WHERE ReportID = @ReportID";

            SqlParameter[] parameters =
            {
                new SqlParameter("@DRIUserID", driUserIndex ?? (object)DBNull.Value),
                new SqlParameter("@DeptNameID", selectedDeptID ?? (object)DBNull.Value),
                new SqlParameter("@IssueTypeID", selectedIssueTypeID),
                new SqlParameter("@Solution", solution),
                new SqlParameter("@Status", isDone ? 1 : 0),
                new SqlParameter("@FinishedDate", finishedDate),
                new SqlParameter("@Remark", remark),
                new SqlParameter("@ReportID", reportID)
            };

            _model.UpdateRequest(query, parameters);
            gvRequests.EditIndex = -1;
            BindRequestData();
        }

        protected void gvRequests_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                Label lblFinishedDate = (Label)e.Row.FindControl("lblFinishedDate");
                HiddenField hfStatus = (HiddenField)e.Row.FindControl("hfStatus");

                if (lblFinishedDate != null && hfStatus != null)
                {
                    string status = hfStatus.Value;

                    if (status == "WIP")
                    {
                        lblFinishedDate.Text = " ";
                    }
                    else if (status == "Done")
                    {
                        DataRowView drv = (DataRowView)e.Row.DataItem;
                        lblFinishedDate.Text = drv["FinishedDate"] != DBNull.Value
                            ? Convert.ToDateTime(drv["FinishedDate"]).ToString("yyyy-MM-dd")
                            : "-";
                        lblFinishedDate.Attributes["style"] = "color: black;";
                    }
                }
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            // ✅ Verify that only the IT department has permission to delete// เช็กสิทธิ์ว่าเฉพาะแผนก IT เท่านั้นที่ลบได้
            if (Session["DeptName"]?.ToString() != "IT")
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('คุณไม่มีสิทธิ์ลบรายการนี้');", true);
                return;
            }

            LinkButton btn = (LinkButton)sender;
            int reportID = Convert.ToInt32(btn.CommandArgument);

            string deleteQuery = "DELETE FROM IT_RequestList WHERE ReportID = @ReportID";
            SqlParameter[] parameters = { new SqlParameter("@ReportID", reportID) };

            _model.UpdateRequest(deleteQuery, parameters);
            gvRequests.EditIndex = -1;
            BindRequestData();
        }

        protected void gvRequests_SelectedIndexChanged(object sender, EventArgs e) { }
    }
}
