using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IT_WorkPlant.Pages
{
    public partial class IT_ComputerListView : System.Web.UI.Page
    {
        private readonly string _connStr = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;

        private DataTable FullData
        {
            get => ViewState["FullData"] as DataTable;
            set => ViewState["FullData"] = value;
        }

        private string CurrentDept
        {
            get => (ViewState["CurrentDept"] as string) ?? "ALL";
            set => ViewState["CurrentDept"] = value;
        }

        // ตัวกรอง Type (ALL/PC/NB)
        private string CurrentType
        {
            get => (ViewState["CurrentType"] as string) ?? "ALL";
            set => ViewState["CurrentType"] = value;
        }

        private string CurrentSearch
        {
            get => (ViewState["CurrentSearch"] as string) ?? "";
            set => ViewState["CurrentSearch"] = value;
        }

        private string CurrentSort
        {
            get => (ViewState["CurrentSort"] as string) ?? "ComputerName ASC";
            set => ViewState["CurrentSort"] = value;
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (FullData == null) LoadData();
            BuildDeptButtons();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack) BindGrid();
        }

        private void LoadData()
        {
            string sql = @"
SELECT
    ID            AS ComputerID,
    NamePC        AS ComputerName,
    UserName,
    CAST(UserID AS NVARCHAR(20)) AS EmpId,
    DeptName      AS Dept,
    Brand,
    Model,
    SerialNumber  AS Serial,
    Tyb           AS Type,
    Warranty,
    Status
FROM IT_ComputerList WITH (NOLOCK)
ORDER BY NamePC;";
            using (var con = new SqlConnection(_connStr))
            using (var da = new SqlDataAdapter(sql, con))
            {
                var dt = new DataTable();
                da.Fill(dt);
                FullData = dt;
            }

            UpdateSummaryCards(); // อัปเดตการ์ดหลังโหลดข้อมูล
        }

        // ===== การ์ดสรุป (Total/PC/NB/Warranty) =====
        private void UpdateSummaryCards()
        {
            int total = 0, pc = 0, nb = 0, warrantyCount = 0;

            if (FullData != null && FullData.Rows.Count > 0)
            {
                total = FullData.Rows.Count;

                pc = FullData.AsEnumerable()
                    .Count(r => string.Equals((r["Type"] ?? "").ToString(), "PC", StringComparison.OrdinalIgnoreCase));

                nb = FullData.AsEnumerable()
                    .Count(r => string.Equals((r["Type"] ?? "").ToString(), "NB", StringComparison.OrdinalIgnoreCase));

                // นับเครื่องที่มีประกัน (ไม่ว่าง และไม่เท่ากับ "No")
                warrantyCount = FullData.AsEnumerable()
                    .Count(r => !string.IsNullOrWhiteSpace(r["Warranty"]?.ToString())
                             && !string.Equals(r["Warranty"].ToString(), "No", StringComparison.OrdinalIgnoreCase));
            }

            if (lblTotalComputers != null) lblTotalComputers.Text = total.ToString();
            if (lblTotalPC != null) lblTotalPC.Text = pc.ToString();
            if (lblTotalNB != null) lblTotalNB.Text = nb.ToString();
            if (lblWarrantyCount != null) lblWarrantyCount.Text = warrantyCount.ToString();
        }

        private void BuildDeptButtons()
        {
            phDeptButtons.Controls.Clear();
            phDeptButtons.Controls.Add(MakeDeptButton("ALL", "All Dept"));
            if (FullData == null || FullData.Rows.Count == 0) return;

            var depts = FullData.DefaultView.ToTable(true, "Dept")
                .AsEnumerable()
                .Select(r => (r["Dept"] ?? "").ToString())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .OrderBy(s => s);

            foreach (var d in depts)
                phDeptButtons.Controls.Add(MakeDeptButton(d, d));

            UpdateActiveFilterLabel();
        }

        private LinkButton MakeDeptButton(string deptKey, string text)
        {
            var btn = new LinkButton
            {
                Text = text,
                CommandName = "Dept",
                CommandArgument = deptKey,
                CssClass = "btn btn-sm btn-outline-primary pill dept-btn" + (CurrentDept == deptKey ? " active" : "")
            };
            btn.Click += DeptButton_Click;
            return btn;
        }

        private void DeptButton_Click(object sender, EventArgs e)
        {
            CurrentDept = ((LinkButton)sender).CommandArgument;
            gvDevices.PageIndex = 0;
            BuildDeptButtons();
            BindGrid();
        }

        protected void txtSearch_TextChanged(object sender, EventArgs e)
        {
            CurrentSearch = txtSearch.Text?.Trim() ?? "";
            gvDevices.PageIndex = 0;
            BindGrid();
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            CurrentSearch = "";
            CurrentDept = "ALL";
            CurrentType = "ALL";   // รีเซ็ต Type ด้วย
            gvDevices.PageIndex = 0;
            BuildDeptButtons();
            BindGrid();
        }

        private void BindGrid()
        {
            if (FullData == null)
            {
                gvDevices.DataSource = null;
                gvDevices.DataBind();
                lblCount.Text = "0";
                return;
            }

            var dv = new DataView(FullData);
            var sb = new StringBuilder();

            // กรอง Dept
            if (!string.Equals(CurrentDept, "ALL", StringComparison.OrdinalIgnoreCase))
                sb.AppendFormat("Dept = '{0}'", EscapeForRowFilter(CurrentDept));

            // กรอง Type
            if (!string.Equals(CurrentType, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                if (sb.Length > 0) sb.Append(" AND ");
                sb.AppendFormat("Type = '{0}'", EscapeForRowFilter(CurrentType));
            }

            // ค้นหา (ใช้ * แทน %)
            if (!string.IsNullOrWhiteSpace(CurrentSearch))
            {
                var term = EscapeForRowFilter(CurrentSearch);
                var orFilter =
                    $"ComputerName LIKE '*{term}*' OR UserName LIKE '*{term}*' OR EmpId LIKE '*{term}*' " +
                    $"OR Dept LIKE '*{term}*' OR Brand LIKE '*{term}*' OR Model LIKE '*{term}*' " +
                    $"OR Serial LIKE '*{term}*' OR Type LIKE '*{term}*' OR Warranty LIKE '*{term}*' OR Status LIKE '*{term}*'";
                if (sb.Length > 0) sb.Append(" AND ");
                sb.Append("(" + orFilter + ")");
            }

            dv.RowFilter = sb.ToString();
            dv.Sort = CurrentSort;

            gvDevices.DataSource = dv;
            gvDevices.DataBind();

            lblCount.Text = dv.Count.ToString();
            UpdateActiveFilterLabel();
        }


        private void UpdateActiveFilterLabel()
        {
            var deptText = CurrentDept == "ALL" ? "All Dept" : $"Dept: {CurrentDept}";
            var typeText = CurrentType == "ALL" ? "" : $" | Type: {CurrentType}";
            var searchText = string.IsNullOrWhiteSpace(CurrentSearch) ? "" : $" | Search: \"{CurrentSearch}\"";
            lblActiveFilter.Text = deptText + typeText + searchText;
        }

        private string EscapeForRowFilter(string input)
        {
            return (input ?? "")
                .Replace("'", "''")
                .Replace("[", "[[]")
                .Replace("%", "[%]")   // เผื่อผู้ใช้พิมพ์ %
                .Replace("*", "[*]");  // กัน * ที่ผู้ใช้พิมพ์มากลายเป็น wildcard
        }
        protected void gvDevices_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvDevices.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        protected void gvDevices_Sorting(object sender, GridViewSortEventArgs e)
        {
            var col = e.SortExpression;
            if (string.IsNullOrWhiteSpace(col)) return;

            var parts = CurrentSort.Split(' ');
            var curCol = parts[0];
            var curDir = (parts.Length > 1 ? parts[1] : "ASC");

            if (string.Equals(curCol, col, StringComparison.OrdinalIgnoreCase))
                CurrentSort = $"{col} {(curDir.Equals("ASC", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC")}";
            else
                CurrentSort = $"{col} ASC";

            gvDevices.PageIndex = 0;
            BindGrid();
        }

        protected void gvDevices_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int rowIndex = (gvDevices.PageIndex * gvDevices.PageSize) + e.Row.RowIndex + 1;
                var lbl = (Label)e.Row.FindControl("lblNo");
                if (lbl != null) lbl.Text = rowIndex.ToString();

                if ((e.Row.RowState & DataControlRowState.Edit) == DataControlRowState.Edit)
                {
                    var ddlStatus = e.Row.FindControl("ddlStatus") as DropDownList;
                    if (ddlStatus != null)
                    {
                        var drv = (DataRowView)e.Row.DataItem;
                        var current = drv?["Status"]?.ToString();
                        if (!string.IsNullOrEmpty(current) && ddlStatus.Items.FindByValue(current) != null)
                            ddlStatus.SelectedValue = current;
                    }
                }
            }
        }

        // ===== Inline Edit =====
        protected void gvDevices_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvDevices.EditIndex = e.NewEditIndex;
            BindGrid();
        }

        protected void gvDevices_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvDevices.EditIndex = -1;
            BindGrid();
        }

        protected void gvDevices_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int id = Convert.ToInt32(gvDevices.DataKeys[e.RowIndex].Value);
            GridViewRow row = gvDevices.Rows[e.RowIndex];

            string computerName = ((TextBox)row.FindControl("txtComputerName"))?.Text?.Trim();
            string empIdText = ((TextBox)row.FindControl("txtUserID"))?.Text?.Trim();
            string type = ((DropDownList)row.FindControl("ddlType"))?.SelectedValue?.Trim();
            string brand = ((TextBox)row.FindControl("txtBrand"))?.Text?.Trim();
            string model = ((TextBox)row.FindControl("txtModel"))?.Text?.Trim();
            string serial = ((TextBox)row.FindControl("txtSerial"))?.Text?.Trim();
            string warranty = ((TextBox)row.FindControl("txtWarranty"))?.Text?.Trim();
            string status = ((DropDownList)row.FindControl("ddlStatus"))?.SelectedValue?.Trim();

            if (string.IsNullOrWhiteSpace(empIdText))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "empErr",
                    "Swal.fire('Invalid Employee Code','กรุณาใส่รหัสพนักงาน','warning');", true);
                return;
            }

            string resolvedUserName = null;
            string resolvedDeptName = null;

            try
            {
                using (var con = new SqlConnection(_connStr))
                using (var cmd = con.CreateCommand())
                {
                    cmd.CommandText = @"
SELECT TOP 1 UserName, DeptName
FROM Users WITH (NOLOCK)
WHERE RTRIM(UserEmpID) = RTRIM(@EmpID);";
                    cmd.Parameters.Add("@EmpID", SqlDbType.NVarChar, 50).Value = empIdText?.Trim();

                    con.Open();
                    using (var rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            resolvedUserName = rd["UserName"] as string;
                            resolvedDeptName = rd["DeptName"] as string;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message.Replace("'", "''");
                ScriptManager.RegisterStartupScript(this, GetType(), "lookupErr",
                    $"Swal.fire('Lookup Users Error','{msg}','error');", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(resolvedUserName))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "notfound",
                    $"Swal.fire('ไม่พบพนักงาน','ไม่มีรหัส {empIdText} ในตาราง Users','error');", true);
                return;
            }

            using (var con = new SqlConnection(_connStr))
            using (var cmd = con.CreateCommand())
            {
                cmd.CommandText = @"
UPDATE IT_ComputerList
SET NamePC       = @NamePC,
    UserID       = @UserID,
    UserName     = @UserName,
    DeptName     = @DeptName,
    Tyb          = @Tyb,
    Brand        = @Brand,
    Model        = @Model,
    SerialNumber = @SerialNumber,
    Warranty     = @Warranty,
    Status       = @Status
WHERE ID = @ID;";

                cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                cmd.Parameters.Add("@NamePC", SqlDbType.NVarChar, 100).Value = (object)computerName ?? DBNull.Value;
                cmd.Parameters.Add("@UserID", SqlDbType.NVarChar, 50).Value = empIdText ?? (object)DBNull.Value;
                cmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 100).Value = (object)resolvedUserName ?? DBNull.Value;
                cmd.Parameters.Add("@DeptName", SqlDbType.NVarChar, 100).Value = (object)resolvedDeptName ?? DBNull.Value;
                cmd.Parameters.Add("@Tyb", SqlDbType.NVarChar, 10).Value = (object)type ?? DBNull.Value;
                cmd.Parameters.Add("@Brand", SqlDbType.NVarChar, 100).Value = (object)brand ?? DBNull.Value;
                cmd.Parameters.Add("@Model", SqlDbType.NVarChar, 100).Value = (object)model ?? DBNull.Value;
                cmd.Parameters.Add("@SerialNumber", SqlDbType.NVarChar, 100).Value = (object)serial ?? DBNull.Value;
                cmd.Parameters.Add("@Warranty", SqlDbType.NVarChar, 50).Value = (object)warranty ?? DBNull.Value;
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = (object)status ?? DBNull.Value;

                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    gvDevices.EditIndex = -1;
                    LoadData();   // reload + update cards
                    BindGrid();
                }
                catch (Exception ex)
                {
                    var msg = ex.Message.Replace("'", "''");
                    ScriptManager.RegisterStartupScript(this, GetType(), "updErr",
                        $"Swal.fire('อัปเดตไม่สำเร็จ',''{msg}''','error');", true);
                }
            }
        }

        // ===== ลบแถว =====
        protected void gvDevices_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "DeleteRow") return;
            if (!int.TryParse(e.CommandArgument?.ToString(), out int id)) return;

            using (var con = new SqlConnection(_connStr))
            using (var cmd = new SqlCommand("DELETE FROM IT_ComputerList WHERE ID=@ID", con))
            {
                cmd.Parameters.Add("@ID", SqlDbType.Int).Value = id;
                try
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                    LoadData();   // reload + update cards
                    BindGrid();
                }
                catch (Exception ex)
                {
                    var msg = ex.Message.Replace("'", "''");
                    ScriptManager.RegisterStartupScript(this, GetType(), "delErr",
                        $"Swal.fire('ลบไม่สำเร็จ','{msg}','error');", true);
                }
            }
        }

        // ===== คลิกการ์ด =====
        protected void btnCardTotal_Click(object sender, EventArgs e)
        {
            CurrentType = "ALL";
            CurrentDept = "ALL";
            CurrentSearch = "";
            gvDevices.PageIndex = 0;
            BuildDeptButtons();
            BindGrid();
        }

        protected void btnCardPC_Click(object sender, EventArgs e)
        {
            CurrentType = "PC";
            CurrentDept = "ALL";
            gvDevices.PageIndex = 0;
            BuildDeptButtons();
            BindGrid();
        }

        protected void btnCardNB_Click(object sender, EventArgs e)
        {
            CurrentType = "NB";
            CurrentDept = "ALL";
            gvDevices.PageIndex = 0;
            BuildDeptButtons();
            BindGrid();
        }

        // การ์ด Warranty: กรองเฉพาะที่มีประกัน
        protected void btnCardWarranty_Click(object sender, EventArgs e)
        {
            CurrentDept = "ALL";
            CurrentType = "ALL";
            CurrentSearch = "";
            gvDevices.PageIndex = 0;

            var dv = new DataView(FullData)
            {
                RowFilter = "Warranty IS NOT NULL AND Warranty <> '' AND Warranty <> 'No'",
                Sort = CurrentSort
            };

            gvDevices.DataSource = dv;
            gvDevices.DataBind();

            lblCount.Text = dv.Count.ToString();
            lblActiveFilter.Text = "Warranty: Yes";
        }

        // (ใช้ถ้าคุณยังมีปุ่มการ์ด Dept อื่น ๆ )
        protected void btnCardDept_Click(object sender, EventArgs e)
        {
            CurrentDept = "ALL";
            gvDevices.PageIndex = 0;
            BuildDeptButtons();
            BindGrid();
        }
    }
}
