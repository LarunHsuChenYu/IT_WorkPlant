using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI.WebControls;
using IT_WorkPlant.Models;
using Org.BouncyCastle.Asn1.Cmp;

namespace IT_WorkPlant.Pages
{
    public partial class IT_UserManagement : System.Web.UI.Page
    {
        private readonly MssqlDatabaseHelper _dbHelper = new MssqlDatabaseHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGridView();
                BindDepartmentDropDowns();
                BindPositionDropDown();
            }
        }

        private void BindDepartmentDropDowns()
        {
            // 定義查詢和參數（無參數的情況下可以使用空字典）
            string query = "SELECT DISTINCT DeptNameID, DeptName_en FROM Departments";
            var parameters = new Dictionary<string, object>();

            // 執行查詢
            DataTable dt = _dbHelper.ExecuteDynamicQuery(query, parameters);

            // 綁定搜尋區域 DropDownList
            ddlSearchDept.DataSource = dt;
            ddlSearchDept.DataTextField = "DeptName_en";  // 顯示英文部門名稱
            ddlSearchDept.DataValueField = "DeptNameID";  // 使用 DeptNameID 作為值
            ddlSearchDept.DataBind();
            ddlSearchDept.Items.Insert(0, new ListItem("All Departments", ""));

            // 綁定新增區域 DropDownList
            ddlNewDept.DataSource = dt;
            ddlNewDept.DataTextField = "DeptName_en";
            ddlNewDept.DataValueField = "DeptNameID";
            ddlNewDept.DataBind();
            ddlNewDept.Items.Insert(0, new ListItem("Select Department", ""));
        }

        private DataTable GetPositions()
        {
            string query = "SELECT PositionID, PositionName_EN FROM Positions WHERE IsActive = 1";
            return _dbHelper.ExecuteQuery(query, null);
        }

        private void BindPositionDropDown()
        {
            var dt = GetPositions();
            ddlNewPosition.DataSource = dt;
            ddlNewPosition.DataTextField = "PositionName_EN";
            ddlNewPosition.DataValueField = "PositionID";
            ddlNewPosition.DataBind();
            ddlNewPosition.Items.Insert(0, new ListItem("Select Position", ""));
        }

        // 綁定 GridView 數據
        private void BindGridView()
        {
            string query = @"SELECT u.*, p.PositionName_EN 
                             FROM Users u
                             LEFT JOIN Positions p ON u.PositionID = p.PositionID
                             WHERE 1=1";
            var parameters = new List<SqlParameter>();

            // 搜尋條件
            if (!string.IsNullOrEmpty(tbSearchEmpID.Text.Trim()))
            {
                query += " AND u.UserEmpID LIKE @EmpID";
                parameters.Add(new SqlParameter("@EmpID", $"%{tbSearchEmpID.Text.Trim()}%"));
            }
            if (!string.IsNullOrEmpty(tbSearchName.Text.Trim()))
            {
                query += " AND u.UserName LIKE @Name";
                parameters.Add(new SqlParameter("@Name", $"%{tbSearchName.Text.Trim()}%"));
            }
            if (!string.IsNullOrEmpty(ddlSearchDept.SelectedValue))
            {
                query += " AND u.DeptName = @Dept";
                parameters.Add(new SqlParameter("@Dept", ddlSearchDept.SelectedValue));
            }

            query += " ORDER BY u.UserIndex";
            DataTable dt = _dbHelper.ExecuteQuery(query, parameters.ToArray());
            gvUsers.DataSource = dt;
            gvUsers.DataBind();
        }

        protected void gvUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow && (e.Row.RowState & DataControlRowState.Edit) > 0)
            {
                DropDownList ddlPosition = (DropDownList)e.Row.FindControl("ddlPosition");
                if (ddlPosition != null)
                {
                    ddlPosition.DataSource = GetPositions();
                    ddlPosition.DataTextField = "PositionName_EN";
                    ddlPosition.DataValueField = "PositionID";
                    ddlPosition.DataBind();

                    var drv = (DataRowView)e.Row.DataItem;
                    if (drv != null)
                    {
                        string posId = drv["PositionID"].ToString();
                        if (ddlPosition.Items.FindByValue(posId) != null)
                            ddlPosition.SelectedValue = posId;
                    }
                }
            }
        }

        // 搜尋按鈕事件
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            BindGridView();
        }

        // 新增用戶
        protected void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                string empID = tbNewEmpID.Text.Trim();
                string name = tbNewName.Text.Trim();
                string email = tbNewEmail.Text.Trim();
                string dept = ddlNewDept.SelectedValue;
                string password = tbNewPassword.Text.Trim();
                string positionId = ddlNewPosition.SelectedValue;

                string query = @"
                    INSERT INTO Users (UserEmpID, UserName, UserEmpMail, DeptName, UserEmpPW, PositionID)
                    VALUES (@UserEmpID, @UserName, @UserEmpMail, @DeptName, @UserEmpPW, @PositionID)";

                SqlParameter[] parameters = {
                    new SqlParameter("@UserEmpID", empID),
                    new SqlParameter("@UserName", name),
                    new SqlParameter("@UserEmpMail", email),
                    new SqlParameter("@DeptName", dept),
                    new SqlParameter("@UserEmpPW", password),
                    new SqlParameter("@PositionID", positionId)
                };

                _dbHelper.ExecuteNonQuery(query, parameters);
                lblStatus.Text = "User added successfully.";
                ShowAlert(lblStatus.Text);
                BindGridView();
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error adding user: {ex.Message}";
                ShowAlert(lblStatus.Text);
            }
        }

        // 進入編輯模式
        protected void gvUsers_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvUsers.EditIndex = e.NewEditIndex;
            BindGridView();
        }

        // 取消編輯模式
        protected void gvUsers_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvUsers.EditIndex = -1;
            BindGridView();
        }

        // 更新用戶資料
        protected void gvUsers_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                // 獲取當前行
                GridViewRow row = gvUsers.Rows[e.RowIndex];

                string empID = ((Label)row.FindControl("lblUserEmpID"))?.Text;
                string name = ((TextBox)row.FindControl("tbUserName"))?.Text;
                string email = ((TextBox)row.FindControl("tbUserEmpMail"))?.Text;
                string dept = ((TextBox)row.FindControl("tbDeptName"))?.Text;
                string password = ((TextBox)row.FindControl("tbUserEmpPW"))?.Text;
                DropDownList ddlPosition = (DropDownList)row.FindControl("ddlPosition");
                string positionId = ddlPosition?.SelectedValue;

                string query = @"
                    UPDATE Users 
                    SET UserName = @UserName, 
                        UserEmpMail = @UserEmpMail, 
                        DeptName = @DeptName, 
                        UserEmpPW = @UserEmpPW,
                        PositionID = @PositionID
                    WHERE UserEmpID = @UserEmpID";

                SqlParameter[] parameters = {
                    new SqlParameter("@UserEmpID", empID),
                    new SqlParameter("@UserName", name),
                    new SqlParameter("@UserEmpMail", email),
                    new SqlParameter("@DeptName", dept),
                    new SqlParameter("@UserEmpPW", password),
                    new SqlParameter("@PositionID", positionId)
                };

                _dbHelper.ExecuteNonQuery(query, parameters);

                lblStatus.Text = "User updated successfully.";
                ShowAlert(lblStatus.Text);
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error updating user: {ex.Message}";
                ShowAlert(lblStatus.Text);
            }
            finally
            {
                gvUsers.EditIndex = -1;
                BindGridView();
            }
        }

        // 分頁處理
        protected void gvUsers_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvUsers.PageIndex = e.NewPageIndex;
            BindGridView();
        }

        private void ShowAlert(string message)
        {
            // 確保消息正確轉義
            string safeMessage = HttpUtility.JavaScriptStringEncode(message);
            string script = $"alert('{safeMessage}');";

            // 注入腳本到頁面
            if (!ClientScript.IsStartupScriptRegistered("alert"))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", script, true);
            }
        }
    }
}
