using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using IT_WorkPlant.Models;

namespace IT_WorkPlant.Pages
{
    public partial class IT_RequestsList : System.Web.UI.Page
    {
        private IT_RequestModel _model;

        protected void Page_Load(object sender, EventArgs e)
        {
            // 初始化模型
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

                BindRequestData();
                BindFilters();
            }
        }

        private void BindRequestData()
        {
            string issueMonth = ViewState["IssueMonth"]?.ToString(); // ค่าที่เลือกใน DropDown เดือน
            string deptName = ViewState["Department"]?.ToString();
            string requestUser = ViewState["RequestUser"]?.ToString();
            string issueType = ViewState["IssueType"]?.ToString();
            string status = ViewState["Status"]?.ToString();

            DataTable dt = _model.GetFilteredRequests(deptName, requestUser, issueType, status, issueMonth);

            if (dt.Columns.Contains("ReportID"))
            {
                gvRequests.DataKeyNames = new[] { "ReportID" };
            }

            gvRequests.DataSource = dt;
            gvRequests.DataBind();
        }


        private void BindFilters()
        {
            BindIssueMonthFilter(); // เรียกใช้ตัวกรองเดือน
            BindFilterDropdown(ddlIssueDate, "IssueDate", "All Issue Dates");
            BindFilterDropdown(ddlDeptName, "Department", "All Departments");
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


        }

        protected void gvRequests_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvRequests.EditIndex = -1;
            BindRequestData();
        }

        protected void gvRequests_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int reportID = Convert.ToInt32(gvRequests.DataKeys[e.RowIndex].Value);

            // 提取下拉選單的選中值
            DropDownList ddlDRIUser = gvRequests.Rows[e.RowIndex].FindControl("ddlDRIUser") as DropDownList;
            int? driUserIndex = ddlDRIUser != null ? (int?)Convert.ToInt32(ddlDRIUser.SelectedValue) : null;

            DropDownList ddlStatus = gvRequests.Rows[e.RowIndex].FindControl("ddlStatus") as DropDownList;
            bool status = ddlStatus != null && ddlStatus.SelectedValue == "1"; // 1 為 Done，0 為 WIP

            string solution = ((TextBox)gvRequests.Rows[e.RowIndex].Cells[7].Controls[0]).Text;
            string finishedDate = ((TextBox)gvRequests.Rows[e.RowIndex].Cells[10].Controls[0]).Text;
            string remark = ((TextBox)gvRequests.Rows[e.RowIndex].Cells[11].Controls[0]).Text;

            string query = @"
                UPDATE IT_RequestList
                SET DRI_UserID = @DRIUserID,
                    Solution = @Solution, 
                    Status = @Status, 
                    FinishedDate = @FinishedDate, 
                    Remark = @Remark, 
                    LastUpdateDate = GETDATE()
                WHERE ReportID = @ReportID";

            SqlParameter[] parameters =
            {
                new SqlParameter("@DRIUserID", driUserIndex ?? (object)DBNull.Value),
                new SqlParameter("@Solution", solution),
                new SqlParameter("@Status", status),
                new SqlParameter("@FinishedDate", string.IsNullOrEmpty(finishedDate) ? (object)DBNull.Value : finishedDate),
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
    }
}

