using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
            {
                _model = new IT_RequestModel();
            }

            if (!IsPostBack)
            {
                if (Session["UserEmpID"] == null)
                {
                    Response.Redirect("../Login.aspx");
                }

                string userDept = Session["DeptName"]?.ToString();
                gvRequests.Columns[gvRequests.Columns.Count - 1].Visible = userDept == "IT";

                BindDepartmentsDropdown(userDept); //✅ เพิ่มเรียกฟังก์ชันที่นี่

                BindRequestData();
                BindFilters();
            }
        }

private void BindDepartmentsDropdown(string userDept)
        {
            DataTable dtDepartments = _model.GetDepartments();
            ddlDeptName.DataSource = dtDepartments;
            ddlDeptName.DataTextField = "DeptName_en";
            ddlDeptName.DataValueField = "DeptNameID";
            ddlDeptName.DataBind();

            if (userDept == "IT") //✅ ถ้าคนเข้าใช้งานคือฝ่ายไอที ให้เลือกได้
            {
                ddlDeptName.Items.Insert(0, new ListItem("-- Select Department --", ""));
                ddlDeptName.Enabled = true;
            }
            else //✅ ถ้าไม่ใช่ฝ่ายไอทีให้ล็อก Dropdown
            {
                DataRow deptRow = dtDepartments.AsEnumerable()
                    .FirstOrDefault(r => r["DeptName_en"].ToString() == userDept);

                if (deptRow != null)
                {
                    ddlDeptName.SelectedValue = deptRow["DeptNameID"].ToString();
                }
                ddlDeptName.Enabled = false;
            }
            
            System.Diagnostics.Debug.WriteLine("===== Dropdown Departments =====");
            foreach (ListItem item in ddlDeptName.Items)
            {
                System.Diagnostics.Debug.WriteLine($"{item.Value} - {item.Text}");
            }
            System.Diagnostics.Debug.WriteLine("==================================");
            
        }

        private void BindRequestData()
        {
            string issueMonth = ViewState["IssueMonth"]?.ToString();
            string issueDate = ViewState["IssueDate"]?.ToString();  // ✅ เพิ่มตัวแปรนี้
            string deptName = ViewState["Department"]?.ToString();
            string requestUser = ViewState["RequestUser"]?.ToString();
            string issueType = ViewState["IssueType"]?.ToString();
            string status = ViewState["Status"]?.ToString();



            // ✅ ส่ง issueDate เข้าไปในฟังก์ชัน GetFilteredRequests()
            DataTable dt = _model.GetFilteredRequests(deptName, requestUser, issueType, status, issueMonth, issueDate);

            // ✅ Debug เพื่อตรวจสอบค่าของ IssueDate
            foreach (DataRow row in dt.Rows)
            {
                System.Diagnostics.Debug.WriteLine("ReportID: " + row["ReportID"] + " | IssueDate: " + row["IssueDate"]);
            }

            gvRequests.DataSource = dt;
            gvRequests.DataBind();
        }



      private void BindFilters()
{
    BindIssueMonthFilter(); // เรียกใช้ตัวกรองเดือน

    //✅ เพิ่มส่วนนี้เข้ามาเพื่อให้ Dropdown แผนกมีแค่ All Department
    ddlDeptName.Items.Clear();
    ddlDeptName.Items.Insert(0, new ListItem("All Department", ""));

    //✅ ส่วนของเดิมที่คุณมีอยู่แล้ว
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

        protected void FilterChanged(object sender, EventArgs e)
        {
            ViewState["IssueMonth"] = ddlIssueMonth.SelectedValue;
            ViewState["Department"] = ddlDeptName.SelectedValue;
            ViewState["RequestUser"] = ddlRequestUser.SelectedValue;
            ViewState["IssueType"] = ddlIssueType.SelectedValue;
            ViewState["Status"] = ddlStatus.SelectedValue;

            BindRequestData();
        }

        protected void gvRequests_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvRequests.EditIndex = e.NewEditIndex;
            BindRequestData();

            DropDownList ddlStatus = gvRequests.Rows[e.NewEditIndex].FindControl("ddlStatus") as DropDownList;

            DropDownList ddlDRIUser = gvRequests.Rows[e.NewEditIndex].FindControl("ddlDRIUser") as DropDownList;
            if (ddlDRIUser != null)
            {
                DataTable dtUsers = _model.GetUsersByDepartment("IT");
                ddlDRIUser.DataSource = dtUsers;
                ddlDRIUser.DataTextField = "UserName";
                ddlDRIUser.DataValueField = "UserIndex";
                ddlDRIUser.DataBind();
            }

            // ✅ ดึงข้อมูลแผนกทั้งหมดและ Bind ลง DropDownList
            DropDownList ddlDepartment = gvRequests.Rows[e.NewEditIndex].FindControl("ddlDepartment") as DropDownList;
            if (ddlDepartment != null)
            {
                // ✅ สร้างอ็อบเจ็กต์ของ Model
                IT_RequestModel model = new IT_RequestModel();
                DataTable dtDepartments = model.GetDepartments();

                // ✅ ผูกข้อมูลจากฐานข้อมูลเข้ากับ DropDownList
                ddlDepartment.DataSource = dtDepartments;
                ddlDepartment.DataTextField = "DeptName_en"; // ใช้ชื่อเต็มของแผนก
                ddlDepartment.DataValueField = "DeptNameID"; // ใช้ ID ของแผนก
                ddlDepartment.DataBind();

                // ✅ ดึงค่าของแผนกปัจจุบันจาก DataKeys และแก้ปัญหา NULL
                object deptObj = gvRequests.DataKeys[e.NewEditIndex].Values["Department"];
                string currentDept = deptObj != null ? deptObj.ToString().Trim() : "";

                // ✅ Debug Log เพื่อตรวจสอบค่าแผนกปัจจุบันจาก DataKeys
                System.Diagnostics.Debug.WriteLine($"===== Current Department: {currentDept} =====");

                // ✅ Debug Log เพื่อตรวจสอบข้อมูลที่ถูกเพิ่มใน DropDownList
                System.Diagnostics.Debug.WriteLine("===== Department List in DropDownList =====");
                foreach (ListItem item in ddlDepartment.Items)
                {
                    System.Diagnostics.Debug.WriteLine($"{item.Value} - {item.Text}");
                }
                System.Diagnostics.Debug.WriteLine("==========================================");

                // ✅ ตั้งค่าให้ DropDownList แสดงค่าของแผนกปัจจุบัน
                ListItem selectedItem = ddlDepartment.Items.FindByText(currentDept);
                if (selectedItem != null)
                {
                    ddlDepartment.SelectedValue = selectedItem.Value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ ไม่พบค่าใน DropDownList: " + currentDept);
                }
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

            DropDownList ddlDRIUser = gvRequests.Rows[e.RowIndex].FindControl("ddlDRIUser") as DropDownList;
            int? driUserIndex = ddlDRIUser != null ? (int?)Convert.ToInt32(ddlDRIUser.SelectedValue) : null;

            DropDownList ddlStatus = gvRequests.Rows[e.RowIndex].FindControl("DropDownList1") as DropDownList;
            bool isDone = ddlStatus != null && ddlStatus.SelectedValue == "1"; // 1 = Done

            string solution = ((TextBox)gvRequests.Rows[e.RowIndex].Cells[7].Controls[0]).Text;
            string remark = ((TextBox)gvRequests.Rows[e.RowIndex].Cells[11].Controls[0]).Text;

            // ✅ ใช้ DateTime โดยตรง และ Debug Log เพื่อตรวจสอบค่า
            object finishedDate = isDone ? (object)DateTime.Now : DBNull.Value;

            // ✅ เพิ่มตรงนี้: ดึงค่าแผนกที่เลือกจาก Dropdown ใน GridView
            DropDownList ddlDepartment = gvRequests.Rows[e.RowIndex].FindControl("ddlDepartment") as DropDownList;
            string selectedDeptID = ddlDepartment != null ? ddlDepartment.SelectedValue : null;

            System.Diagnostics.Debug.WriteLine("======= DEBUG LOG =======");
            System.Diagnostics.Debug.WriteLine("ReportID: " + reportID);
            System.Diagnostics.Debug.WriteLine("isDone: " + isDone);
            System.Diagnostics.Debug.WriteLine("FinishedDate ก่อนอัปเดต: " + finishedDate);
            System.Diagnostics.Debug.WriteLine("SelectedDeptID ก่อนอัปเดต: " + selectedDeptID);
            System.Diagnostics.Debug.WriteLine("=========================");

            // ✅ เพิ่ม DeptNameID เข้าไปในคำสั่ง SQL เพื่ออัปเดตแผนกด้วย
            string query = @"
    UPDATE IT_RequestList
    SET DRI_UserID = @DRIUserID,
        DeptNameID = @DeptNameID, -- ✅ แก้เฉพาะตรงนี้
        Solution = @Solution, 
        Status = @Status, 
        FinishedDate = @FinishedDate, 
        Remark = @Remark, 
        LastUpdateDate = GETDATE()
    WHERE ReportID = @ReportID";

            // ✅ เพิ่ม Parameter ใหม่ "DeptNameID" อย่างเดียว
            SqlParameter[] parameters =
            {
        new SqlParameter("@DRIUserID", driUserIndex ?? (object)DBNull.Value),
        new SqlParameter("@DeptNameID", selectedDeptID ?? (object)DBNull.Value), //✅ใหม่
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

        private void BindIssueMonthFilter()
        {
            DataTable dt = _model.GetFilterOptions("FORMAT(IssueDate, 'yyyy-MM')"); // ดึงค่าปี-เดือน
            ddlIssueMonth.DataSource = dt;
            ddlIssueMonth.DataTextField = "Value";
            ddlIssueMonth.DataValueField = "Value";
            ddlIssueMonth.DataBind();
            ddlIssueMonth.Items.Insert(0, new ListItem("All Months", ""));
        }



        protected void gvRequests_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvRequests.PageIndex = e.NewPageIndex;
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
                        lblFinishedDate.Text = "⏳ Done = Date!";
                        lblFinishedDate.Attributes["style"] = "color: red; font-weight: bold;";
                    }
                    else if (status == "Done")
                    {
                        DataRowView drv = (DataRowView)e.Row.DataItem;
                        lblFinishedDate.Text = drv["FinishedDate"] != DBNull.Value
                            ? Convert.ToDateTime(drv["FinishedDate"]).ToString("yyyy-MM-dd HH:mm:ss")
                            : "-";
                        lblFinishedDate.Attributes["style"] = "color: black;";
                    }
                }
            }
        }

    }
}

