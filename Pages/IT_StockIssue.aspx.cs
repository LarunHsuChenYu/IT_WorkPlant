using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections.Generic;



namespace IT_WorkPlant.Pages
{
    public partial class IT_StockIssue : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["username"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }
            Page.DataBind();
            if (!IsPostBack)
            {
                LoadProductCodes();
                LoadIssuedUsers();
                LoadStatus();
                txtIssueDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                txtApprovedBy.Text = Session["username"].ToString();
                LoadIssueHistory();
            }
        }

        private void LoadProductCodes()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT ItemID, ProductName FROM IT_StockItems ORDER BY ProductName";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                ddlProductCode.Items.Clear();
                ddlProductCode.Items.Add(new System.Web.UI.WebControls.ListItem("เลือก", ""));
                while (reader.Read())
                {
                    string itemId = reader["ItemID"].ToString();
                    string name = reader["ProductName"].ToString();
                    ddlProductCode.Items.Add(new System.Web.UI.WebControls.ListItem(itemId + " - " + name, itemId));
                }

                reader.Close();
            }
        }

        protected void ddlProductCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedCode = ddlProductCode.SelectedValue;
            if (string.IsNullOrEmpty(selectedCode)) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT ProductName, Model, Unit FROM IT_StockItems WHERE ItemID = @ItemID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ItemID", selectedCode);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtProductName.Text = reader["ProductName"].ToString();
                    txtModel.Text = reader["Model"].ToString();
                    txtUnit.Text = reader["Unit"].ToString();
                }
                reader.Close();
            }
        }
        private void LoadIssuedUsers()
        {
            string connStr = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT UserName FROM Users ORDER BY UserName";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                ddlIssuedBy.Items.Clear();
                ddlIssuedBy.Items.Add(new ListItem(GetLabel("selectname"), ""));
                while (reader.Read())
                {
                    string username = reader["UserName"].ToString();
                    ddlIssuedBy.Items.Add(new ListItem(username, username));
                }
                reader.Close();
            }
        }
        protected void ddlIssuedBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            string username = ddlIssuedBy.SelectedValue;
            if (string.IsNullOrEmpty(username)) return;

            string connStr = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT DeptName FROM Users WHERE UserName = @UserName";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@UserName", username);
                conn.Open();
                object deptShort = cmd.ExecuteScalar();

                if (deptShort != null)
                {
                    // ดึงชื่อแผนกเต็มจากตาราง Departments
                    string deptName = deptShort.ToString();
                    string fullQuery = "SELECT DeptName_en FROM Departments WHERE DeptNameID = @DeptName";
                    SqlCommand fullCmd = new SqlCommand(fullQuery, conn);
                    fullCmd.Parameters.AddWithValue("@DeptName", deptName);
                    object fullName = fullCmd.ExecuteScalar();

                    txtDepartment.Text = fullName?.ToString() ?? deptName;
                }
            }
        }
        private void LoadIssueHistory()
{
    string filter = Session["IssueFilter"]?.ToString() ?? "all";
    string query = @"
        SELECT IssueID, IssueDate, IssuedBy, Department, ProductName, Model, Quantity, Purpose, 
               Status, IssueType, ApprovedBy, IsReturned, ReturnDate
        FROM IT_StockIssue
        WHERE 1=1";

    if (filter == "used")
        query += " AND IssueType = 'Used'";
    else if (filter == "borrow")
        query += " AND IssueType = 'Borrowed'";

    query += " ORDER BY IssueDate DESC";

    using (SqlConnection conn = new SqlConnection(connectionString))
    {
        SqlDataAdapter da = new SqlDataAdapter(query, conn);
        DataTable dt = new DataTable();
        da.Fill(dt);

        // Set Panel and GridView visibility
        pnlUsed.Visible = false;
        pnlBorrowed.Visible = false;
        gvUsed.Visible = false;
        gvBorrowed.Visible = false;

        if (filter == "used")
        {
            pnlUsed.Visible = true;
            gvUsed.Visible = true;
            gvUsed.DataSource = dt;
            gvUsed.DataBind();
        }
        else if (filter == "borrow")
        {
            pnlBorrowed.Visible = true;
            gvBorrowed.Visible = true;
            gvBorrowed.DataSource = dt;
            gvBorrowed.DataBind();
        }
        else
        {
            // All = แสดงทุกประเภท (รวม)
            pnlUsed.Visible = true;
            gvUsed.Visible = true;
            gvUsed.DataSource = dt;
            gvUsed.DataBind();
        }
    }
}

        protected void btnShowAll_Click(object sender, EventArgs e)
        {
            Session["IssueFilter"] = "all";
            LoadIssueHistory();
        }

        protected void btnShowUsed_Click(object sender, EventArgs e)
        {
            Session["IssueFilter"] = "used";
            LoadIssueHistory();
        }

        protected void btnShowBorrowed_Click(object sender, EventArgs e)
        {
            Session["IssueFilter"] = "borrow";
            LoadIssueHistory();
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    string productCode = ddlProductCode.SelectedValue;

                    // 1) Insert ลง IT_StockIssue
                    string insertQuery = @"INSERT INTO IT_StockIssue 
(IssueDate, ProductCode, ProductName, Model, Quantity, Unit, Purpose, IssueType, IssuedBy, Department, ApprovedBy, Status, Remarks)
VALUES
(@IssueDate, @ProductCode, @ProductName, @Model, @Quantity, @Unit, @Purpose, @IssueType, @IssuedBy, @Department, @ApprovedBy, @Status, @Remarks)";

                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn, tran);
                    insertCmd.Parameters.AddWithValue("@IssueDate", Convert.ToDateTime(txtIssueDate.Text));
                    insertCmd.Parameters.AddWithValue("@ProductCode", productCode);
                    insertCmd.Parameters.AddWithValue("@ProductName", txtProductName.Text);
                    insertCmd.Parameters.AddWithValue("@Model", txtModel.Text);
                    insertCmd.Parameters.AddWithValue("@Quantity", Convert.ToInt32(txtQuantity.Text));
                    insertCmd.Parameters.AddWithValue("@Unit", txtUnit.Text);
                    insertCmd.Parameters.AddWithValue("@Purpose", txtPurpose.Text);
                    insertCmd.Parameters.AddWithValue("@IssueType", ddlIssueType.SelectedValue);
                    insertCmd.Parameters.AddWithValue("@IssuedBy", ddlIssuedBy.SelectedValue);
                    insertCmd.Parameters.AddWithValue("@Department", txtDepartment.Text);
                    insertCmd.Parameters.AddWithValue("@ApprovedBy", txtApprovedBy.Text);
                    insertCmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                    insertCmd.Parameters.AddWithValue("@Remarks", "");
                    insertCmd.ExecuteNonQuery();

                    // 2) หักจำนวนจากสต็อก (เฉพาะกรณี "เบิกใช้")
                    if (ddlIssueType.SelectedValue == "Used")
                    {
                        string updateStock = @"UPDATE IT_StockItems 
SET InventoryQty = InventoryQty - @Qty 
WHERE ItemID = @ItemID";

                        SqlCommand updateCmd = new SqlCommand(updateStock, conn, tran);
                        updateCmd.Parameters.AddWithValue("@Qty", Convert.ToInt32(txtQuantity.Text));
                        updateCmd.Parameters.AddWithValue("@ItemID", productCode);
                        updateCmd.ExecuteNonQuery();
                    }

                    tran.Commit();

                    // โหลดประวัติใหม่
                    LoadIssueHistory();

                    // เคลียร์ฟอร์ม
                    ddlProductCode.SelectedIndex = 0;
                    txtProductName.Text = "";
                    txtModel.Text = "";
                    txtUnit.Text = "";
                    txtQuantity.Text = "";
                    txtPurpose.Text = "";
                    ddlIssueType.SelectedIndex = 0;
                    ddlIssuedBy.SelectedIndex = 0;
                    txtDepartment.Text = "";
                    ddlStatus.SelectedIndex = 0;

                    // แสดงข้อความสำเร็จ (ไม่ redirect)
                    ScriptManager.RegisterStartupScript(this, GetType(), "Success",
                        "alert('เบิกของออกเรียบร้อยแล้ว!');", true);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ScriptManager.RegisterStartupScript(this, GetType(), "Error",
                        $"alert('เกิดข้อผิดพลาด: {ex.Message}');", true);
                }
            }
        }
        protected void btnReturn_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string issueId = btn.CommandArgument;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    // 1) อัปเดตสถานะคืนของ
                    string updateQuery = @"
                UPDATE IT_StockIssue
                SET IsReturned = 1,
                    ReturnDate = GETDATE()
                WHERE IssueID = @IssueID";

                    SqlCommand cmd = new SqlCommand(updateQuery, conn, tran);
                    cmd.Parameters.AddWithValue("@IssueID", issueId);
                    cmd.ExecuteNonQuery();

                    // 2) คืนของเข้า stock
                    string returnQtyQuery = @"
                UPDATE IT_StockItems
                SET InventoryQty = InventoryQty + (
                    SELECT Quantity FROM IT_StockIssue WHERE IssueID = @IssueID
                )
                WHERE ItemID = (
                    SELECT ProductCode FROM IT_StockIssue WHERE IssueID = @IssueID
                )";

                    SqlCommand cmdReturnQty = new SqlCommand(returnQtyQuery, conn, tran);
                    cmdReturnQty.Parameters.AddWithValue("@IssueID", issueId);
                    cmdReturnQty.ExecuteNonQuery();

                    tran.Commit();

                    LoadIssueHistory();

                    ScriptManager.RegisterStartupScript(this, GetType(), "ReturnSuccess",
                        "alert('บันทึกการคืนของเรียบร้อยแล้ว!');", true);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ScriptManager.RegisterStartupScript(this, GetType(), "ReturnError",
                        $"alert('เกิดข้อผิดพลาดขณะคืนของ: {ex.Message}');", true);
                }
            }
        }
        protected string GetLabel(string key)
        {
            string lang = (Session["lang"]?.ToString()?.ToLower()) ?? "th";
            key = key.ToLower(); // ✅ ป้องกัน headerText เป็นตัวใหญ่

            var th = new Dictionary<string, string> {
        { "title", "เบิกของออกจากคลัง" },
        { "productcode", "รหัสสินค้า" },
        { "productname", "ชื่อสินค้า" },
        { "model", "รุ่น" },
        { "unit", "หน่วย" },
        { "quantity", "จำนวน" },
        { "issuedby", "ผู้เบิก" },
        { "department", "แผนก" },
        { "approvedby", "ผู้อนุมัติ" },
        { "issuetype", "ประเภทการเบิก" },
        { "status", "สถานะ" },
        { "purpose", "นำไปใช้ทำอะไร" },
        { "submit", "บันทึกการเบิก" },
        { "history", "ประวัติการเบิกล่าสุด" },
        { "return", "คืนของ" },
        { "returndate", "วันที่คืนของ" },
        { "isreturned", "คืนของแล้ว?" },
        { "filter_all", "ทั้งหมด" },
        { "filter_used", "เบิกใช้" },
        { "filter_borrowed", "ยืมของ" },
        { "selectname", "เลือกชื่อ" },
        { "pending", "รออนุมัติ" },
        { "approved", "อนุมัติ" },
        { "rejected", "ไม่อนุมัติ" }
    };

            var en = new Dictionary<string, string> {
        { "title", "Stock Issue" },
        { "productcode", "Product Code" },
        { "productname", "Product Name" },
        { "model", "Model" },
        { "unit", "Unit" },
        { "quantity", "Quantity" },
        { "issuedby", "Issued By" },
        { "department", "Department" },
        { "approvedby", "Approved By" },
        { "issuetype", "Issue Type" },
        { "status", "Status" },
        { "purpose", "Purpose" },
        { "submit", "Submit" },
        { "history", "Issue History" },
        { "return", "Return" },
        { "returndate", "Return Date" },
        { "isreturned", "Returned?" },
        { "filter_all", "All" },
        { "filter_used", "Used" },
        { "filter_borrowed", "Borrowed" },
        { "selectname", "Select Name" },
        { "pending", "Pending" },
        { "approved", "Approved" },
        { "rejected", "Rejected" }
    };

            var zh = new Dictionary<string, string> {
        { "title", "出庫作業" },
        { "productcode", "產品代碼" },
        { "productname", "產品名稱" },
        { "model", "型號" },
        { "unit", "單位" },
        { "quantity", "數量" },
        { "issuedby", "領用人" },
        { "department", "部門" },
        { "approvedby", "審核人" },
        { "issuetype", "出庫類型" },
        { "status", "狀態" },
        { "purpose", "用途" },
        { "submit", "提交" },
        { "history", "出庫記錄" },
        { "return", "歸還" },
        { "returndate", "歸還日期" },
        { "isreturned", "已歸還？" },
        { "filter_all", "全部" },
        { "filter_used", "領用" },
        { "filter_borrowed", "借用" },
        { "selectname", "選擇姓名" },
        { "pending", "待批准" },
        { "approved", "已批准" },
        { "rejected", "未批准" }
    };

            Dictionary<string, string> dict = lang == "en" ? en : lang == "zh" ? zh : th;
            return dict.ContainsKey(key) ? dict[key] : key;
        }
        protected void gvUsed_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                for (int i = 0; i < gvUsed.Columns.Count; i++)
                {
                    if (gvUsed.Columns[i] is DataControlField field)
                    {
                        string key = field.HeaderText.ToLower().Trim();
                        field.HeaderText = GetLabel(key);
                    }
                }
            }
        }

        protected void gvBorrowed_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                for (int i = 0; i < gvBorrowed.Columns.Count; i++)
                {
                    if (gvBorrowed.Columns[i] is DataControlField field)
                    {
                        string key = field.HeaderText.ToLower().Trim();
                        field.HeaderText = GetLabel(key);
                    }
                }
            }
        }
        private void LoadStatus()
        {
            ddlStatus.Items.Clear();
            ddlStatus.Items.Add(new ListItem(GetLabel("pending"), "รออนุมัติ"));
            ddlStatus.Items.Add(new ListItem(GetLabel("approved"), "อนุมัติ"));
            ddlStatus.Items.Add(new ListItem(GetLabel("rejected"), "ไม่อนุมัติ"));
        }

    }
}

