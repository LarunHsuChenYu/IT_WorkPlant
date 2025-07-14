using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IT_WorkPlant.Pages
{
    public partial class IT_BorrowApproval : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserEmpID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                ApplyLanguage();
                LoadBorrowRequests();  

            }
        }
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            gvBorrowRequests.RowCreated += gvBorrowRequests_RowCreated;
        }

        protected void gvBorrowRequests_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                e.Row.Cells[0].Text = GetLabel("RequestID");
                e.Row.Cells[1].Text = GetLabel("Item");
                e.Row.Cells[2].Text = GetLabel("User");
                e.Row.Cells[3].Text = GetLabel("Department");
                e.Row.Cells[4].Text = GetLabel("Date");
                e.Row.Cells[5].Text = GetLabel("StartTime");
                e.Row.Cells[6].Text = GetLabel("EndTime");
                e.Row.Cells[7].Text = GetLabel("Qty");
                e.Row.Cells[8].Text = GetLabel("Purpose");
                e.Row.Cells[9].Text = GetLabel("Status");
                e.Row.Cells[10].Text = GetLabel("Returned");
                e.Row.Cells[11].Text = GetLabel("Action");
            }
        }

        private void ApplyLanguage()
        {
            string lang = Session["lang"]?.ToString() ?? "en";

            switch (lang)
            {
                case "th":
                    lblTitle.Text = "📋 รายการคำขอยืมที่รอการอนุมัติ";
                    break;
                case "zh":
                    lblTitle.Text = "📋 待批准的借用请求列表";
                    break;
                default:
                    lblTitle.Text = "📋 Pending Borrow Requests";
                    break;
            }
        }


        private void LoadBorrowRequests()
        {
            string connStr = WebConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = @"
SELECT 
    b.BorrowID,
    i.ItemName,
    b.UserName,
    b.DeptName,
    b.BorrowDate,
    b.BorrowStartTime,
    b.BorrowEndTime,
    b.Quantity,
    b.Purpose,
    b.Status,
    b.Returned,
    b.SerialItemID
FROM IT_BorrowTransactions b
INNER JOIN IT_Items i ON b.ItemID = i.ItemID
ORDER BY b.BorrowID DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gvBorrowRequests.DataSource = dt;
                gvBorrowRequests.DataBind();
            }
        }
        protected void gvBorrowRequests_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int borrowId = Convert.ToInt32(e.CommandArgument);

            if (e.CommandName == "Approve")
            {
                UpdateStatus(borrowId, "PASS");
            }
            else if (e.CommandName == "Reject")
            {
                UpdateStatus(borrowId, "REJECT");
                ReturnStock(borrowId); // คืนของเข้าสต็อกเมื่อไม่อนุมัติ
            }
            else if (e.CommandName == "Return")
            {
                ProcessReturn(borrowId);
            }

            LoadBorrowRequests(); // refresh grid
        }

        private void UpdateStatus(int borrowId, string newStatus)
        {
            string connStr = WebConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();

                if (newStatus == "PASS")
                {
                    SqlCommand getOriginal = new SqlCommand(@"
                SELECT ItemID, UserName, DeptName, BorrowDate, BorrowStartTime, BorrowEndTime,
                       Quantity, Purpose
                FROM IT_BorrowTransactions
                WHERE BorrowID = @BorrowID", con);
                    getOriginal.Parameters.AddWithValue("@BorrowID", borrowId);
                    SqlDataReader r = getOriginal.ExecuteReader();
                    if (!r.Read()) return;

                    int itemId = (int)r["ItemID"];
                    string username = r["UserName"].ToString();
                    string dept = r["DeptName"].ToString();
                    DateTime date = (DateTime)r["BorrowDate"];
                    TimeSpan start = (TimeSpan)r["BorrowStartTime"];
                    TimeSpan end = (TimeSpan)r["BorrowEndTime"];
                    int qty = (int)r["Quantity"];
                    string purpose = r["Purpose"].ToString();
                    r.Close();

                    // ✅ ดึง Serial ที่ถูกเลือกไว้จากคำขอเดิม
                    SqlCommand getBorrowedSerials = new SqlCommand(@"
                SELECT SerialItemID 
                FROM IT_BorrowTransactions 
                WHERE BorrowID = @BorrowID", con);
                    getBorrowedSerials.Parameters.AddWithValue("@BorrowID", borrowId);
                    SqlDataReader rSerials = getBorrowedSerials.ExecuteReader();
                    List<int> serialIds = new List<int>();
                    while (rSerials.Read()) serialIds.Add((int)rSerials["SerialItemID"]);
                    rSerials.Close();

                    if (serialIds.Count < qty)
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "alert",
                            "Swal.fire('ไม่สามารถอนุมัติได้', 'Serial Number ไม่เพียงพอ', 'error');", true);
                        return;
                    }

                    foreach (int sid in serialIds)
                    {
                        SqlCommand update = new SqlCommand(@"
                    UPDATE IT_BorrowTransactions
                    SET Status = 'PASS', Returned = 0
                    WHERE BorrowID = @BorrowID AND SerialItemID = @SerialID", con);
                        update.Parameters.AddWithValue("@BorrowID", borrowId);
                        update.Parameters.AddWithValue("@SerialID", sid);
                        update.ExecuteNonQuery();

                        SqlCommand markUsed = new SqlCommand(
                            "UPDATE IT_ItemSerials SET IsAvailable = 0 WHERE SerialItemID = @SerialID", con);
                        markUsed.Parameters.AddWithValue("@SerialID", sid);
                        markUsed.ExecuteNonQuery();
                    }

                    // ✅ หักสต็อกในตาราง IT_Items เฉพาะตอนอนุมัติ
                    SqlCommand reduceQty = new SqlCommand(
                        "UPDATE IT_Items SET AvailableQty = AvailableQty - @Qty WHERE ItemID = @ItemID", con);
                    reduceQty.Parameters.AddWithValue("@Qty", qty);
                    reduceQty.Parameters.AddWithValue("@ItemID", itemId);
                    reduceQty.ExecuteNonQuery();
                }
                else
                {
                    // ❌ REJECT → แค่เปลี่ยน Status
                    SqlCommand cmd = new SqlCommand(
                        "UPDATE IT_BorrowTransactions SET Status = @Status WHERE BorrowID = @BorrowID", con);
                    cmd.Parameters.AddWithValue("@Status", newStatus);
                    cmd.Parameters.AddWithValue("@BorrowID", borrowId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
        private void ProcessReturn(int borrowId)
        {
            string connStr = WebConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();

                SqlCommand getCmd = new SqlCommand(@"
            SELECT ItemID, Quantity, Status, Returned, SerialItemID
            FROM IT_BorrowTransactions
            WHERE BorrowID = @BorrowID", con);
                getCmd.Parameters.AddWithValue("@BorrowID", borrowId);
                SqlDataReader reader = getCmd.ExecuteReader();

                if (reader.Read())
                {
                    int itemId = Convert.ToInt32(reader["ItemID"]);
                    int qty = Convert.ToInt32(reader["Quantity"]);
                    string status = reader["Status"].ToString();
                    bool returned = Convert.ToBoolean(reader["Returned"]);
                    int serialId = Convert.ToInt32(reader["SerialItemID"]);
                    reader.Close();

                    // ✅ เงื่อนไข: คืนได้เฉพาะที่ผ่านอนุมัติแล้ว และยังไม่ถูกคืน
                    if (status == "PASS" && !returned)
                    {
                        // 1. อัปเดตสถานะคืน
                        SqlCommand updateStatus = new SqlCommand(@"
                    UPDATE IT_BorrowTransactions
                    SET Returned = 1,
                        BorrowActualReturnDate = @Now
                    WHERE BorrowID = @BorrowID", con);
                        updateStatus.Parameters.AddWithValue("@Now", DateTime.Now);
                        updateStatus.Parameters.AddWithValue("@BorrowID", borrowId);
                        updateStatus.ExecuteNonQuery();

                        // 2. อัปเดต Serial → ให้กลับมา Available
                        SqlCommand updateSerial = new SqlCommand(@"
                    UPDATE IT_ItemSerials 
                    SET IsAvailable = 1 
                    WHERE SerialItemID = @SerialID", con);
                        updateSerial.Parameters.AddWithValue("@SerialID", serialId);
                        updateSerial.ExecuteNonQuery();

                        // 3. อัปเดต Stock ใน IT_Items +1
                        SqlCommand updateQty = new SqlCommand(
                            "UPDATE IT_Items SET AvailableQty = AvailableQty + @Qty WHERE ItemID = @ItemID", con);
                        updateQty.Parameters.AddWithValue("@Qty", qty);
                        updateQty.Parameters.AddWithValue("@ItemID", itemId);
                        updateQty.ExecuteNonQuery();
                    }
                }
            }
        }
        private void ReturnStock(int borrowId)
        {
            string connStr = WebConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();

                SqlCommand getCmd = new SqlCommand(
                    "SELECT ItemID, Quantity, Returned FROM IT_BorrowTransactions WHERE BorrowID = @BorrowID", con);
                getCmd.Parameters.AddWithValue("@BorrowID", borrowId);
                SqlDataReader reader = getCmd.ExecuteReader();

                if (reader.Read())
                {
                    int itemId = Convert.ToInt32(reader["ItemID"]);
                    int qty = Convert.ToInt32(reader["Quantity"]);
                    bool returned = Convert.ToBoolean(reader["Returned"]);
                    reader.Close();

                    if (!returned && qty > 0)
                    {
                        SqlCommand updateQty = new SqlCommand(
                            "UPDATE IT_Items SET AvailableQty = AvailableQty + @Qty WHERE ItemID = @ItemID", con);
                        updateQty.Parameters.AddWithValue("@Qty", qty);
                        updateQty.Parameters.AddWithValue("@ItemID", itemId);
                        updateQty.ExecuteNonQuery();
                    }
                }
            }
        }

        // ✅ ใส่ RowDataBound เพื่อแสดงเวลาอย่างถูกต้อง
        protected void gvBorrowRequests_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DataRowView drv = (DataRowView)e.Row.DataItem;

                // แปลงเวลาให้สวย
                Label lblStart = (Label)e.Row.FindControl("lblStartTime");
                Label lblEnd = (Label)e.Row.FindControl("lblEndTime");

                if (drv["BorrowStartTime"] != DBNull.Value && lblStart != null)
                {
                    TimeSpan start = (TimeSpan)drv["BorrowStartTime"];
                    lblStart.Text = start.ToString(@"hh\:mm");
                }

                if (drv["BorrowEndTime"] != DBNull.Value && lblEnd != null)
                {
                    TimeSpan end = (TimeSpan)drv["BorrowEndTime"];
                    lblEnd.Text = end.ToString(@"hh\:mm");
                }

                // 🔄 ควบคุมปุ่มตามสถานะ
                string status = drv["Status"].ToString();
                bool returned = drv["Returned"] != DBNull.Value && Convert.ToBoolean(drv["Returned"]);

                Button btnApprove = (Button)e.Row.FindControl("btnApprove");
                Button btnReject = (Button)e.Row.FindControl("btnReject");
                Button btnReturn = (Button)e.Row.FindControl("btnReturn");

                if (btnApprove != null) btnApprove.Visible = (status == "PENDING");
                if (btnReject != null) btnReject.Visible = (status == "PENDING");
                if (btnReturn != null) btnReturn.Visible = (status == "PASS" && !returned);
            }
        }

        protected string GetStatusBadge(string status)
        {
            string lang = Session["lang"]?.ToString() ?? "en";
            string label = "";
            string css = "";

            switch (status)
            {
                case "PENDING":
                    css = "bg-warning text-dark";
                    label = lang == "th" ? "รออนุมัติ" : lang == "zh" ? "待批准" : "Pending";
                    break;
                case "PASS":
                    css = "bg-success";
                    label = lang == "th" ? "อนุมัติแล้ว" : lang == "zh" ? "已批准" : "Approved";
                    break;
                case "REJECT":
                    css = "bg-danger";
                    label = lang == "th" ? "ไม่อนุมัติ" : lang == "zh" ? "已拒绝" : "Rejected";
                    break;
                default:
                    css = "bg-secondary";
                    label = "-";
                    break;
            }

            return $"<span class='badge {css}'>{label}</span>";
        }
        protected string GetLabel(string key)
        {
            string lang = Session["lang"]?.ToString() ?? "en";

            var dict = new Dictionary<string, Dictionary<string, string>>
    {
        { "RequestID",     new Dictionary<string, string> { { "th", "รหัสคำขอ" },     { "zh", "请求编号" },     { "en", "Request ID" } } },
        { "Item",          new Dictionary<string, string> { { "th", "อุปกรณ์" },      { "zh", "设备" },         { "en", "Item" } } },
        { "User",          new Dictionary<string, string> { { "th", "ผู้ขอ" },        { "zh", "使用者" },       { "en", "User" } } },
        { "Department",    new Dictionary<string, string> { { "th", "แผนก" },       { "zh", "部门" },         { "en", "Department" } } },
        { "Date",          new Dictionary<string, string> { { "th", "วันที่" },        { "zh", "日期" },         { "en", "Date" } } },
        { "StartTime",     new Dictionary<string, string> { { "th", "เวลาเริ่ม" },      { "zh", "开始时间" },     { "en", "Start Time" } } },
        { "EndTime",       new Dictionary<string, string> { { "th", "เวลาสิ้นสุด" },    { "zh", "结束时间" },      { "en", "End Time" } } },
        { "Qty",           new Dictionary<string, string> { { "th", "จำนวน" },       { "zh", "数量" },         { "en", "Qty" } } },
        { "Purpose",       new Dictionary<string, string> { { "th", "วัตถุประสงค์" },   { "zh", "目的" },         { "en", "Purpose" } } },
        { "Status",        new Dictionary<string, string> { { "th", "สถานะ" },       { "zh", "状态" },         { "en", "Status" } } },
        { "Returned",      new Dictionary<string, string> { { "th", "คืนแล้ว?" },      { "zh", "是否归还" },     { "en", "Returned?" } } },
        { "NotReturned",   new Dictionary<string, string> { { "th", "ยังไม่คืน" },      { "zh", "未归还" },       { "en", "Not Returned" } } },
        { "Action",        new Dictionary<string, string> { { "th", "การทำงาน" },     { "zh", "操作" },         { "en", "Action" } } },
        { "Approve",       new Dictionary<string, string> { { "th", "อนุมัติ" },        { "zh", "批准" },         { "en", "Approve" } } },
        { "Reject",        new Dictionary<string, string> { { "th", "ไม่อนุมัติ" },      { "zh", "拒绝" },         { "en", "Reject" } } },
        { "Return",        new Dictionary<string, string> { { "th", "คืนของ" },       { "zh", "归还" },         { "en", "Return" } } },
        { "NoData",        new Dictionary<string, string> { { "th", "ไม่มีข้อมูลคำขอยืม" },{ "zh", "没有请求记录" },  { "en", "No borrow requests found." } } },
        { "Done",          new Dictionary<string, string> { { "en", "Done" },        { "th", "เสร็จแล้ว" }, { "zh", "已完成" } } },
        { "WIP",           new Dictionary<string, string> { { "en", "WIP" },         { "th", "กำลังใช้งาน ยังไม่คืน" }, { "zh", "處理中" } } },

    };
            return dict.ContainsKey(key) && dict[key].ContainsKey(lang)
                ? dict[key][lang]
                : key;
        }
        protected string GetSerialNumber(object serialIdObj)
        {
            if (serialIdObj == DBNull.Value) return "";
            int serialId = Convert.ToInt32(serialIdObj);

            string connStr = WebConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT SerialNumber FROM IT_ItemSerials WHERE SerialItemID = @id", con);
                cmd.Parameters.AddWithValue("@id", serialId);
                object result = cmd.ExecuteScalar();
                return result != null ? result.ToString() : "";
            }
        }

    }
}

