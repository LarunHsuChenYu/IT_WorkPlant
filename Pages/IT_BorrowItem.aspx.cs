using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IT_WorkPlant.Pages
{
    public partial class IT_BorrowItem : System.Web.UI.Page
    {
        private string connStr = WebConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserEmpID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                txtUser.Text = Session["UserName"]?.ToString();
                txtDept.Text = Session["DeptName"]?.ToString();
                txtToday.Text = DateTime.Now.ToString("yyyy-MM-dd");

                btnSubmitMemo.Text = GetLabel("Submit");

                DataTable dt = new DataTable();
                dt.Columns.Add("RowIndex", typeof(int));
                dt.Rows.Add(0); // 👈 เริ่มแค่ 1 แถว
                ViewState["BorrowItems"] = dt;

                gvBorrowItems.DataSource = dt;
                gvBorrowItems.DataBind();

                if (gvBorrowItems.HeaderRow != null && gvBorrowItems.HeaderRow.Cells.Count >= 6)
                {
                    gvBorrowItems.HeaderRow.Cells[1].Text = GetLabel("Item");
                    gvBorrowItems.HeaderRow.Cells[2].Text = GetLabel("SerialNo");
                    gvBorrowItems.HeaderRow.Cells[3].Text = GetLabel("UseTime");
                    gvBorrowItems.HeaderRow.Cells[4].Text = GetLabel("ReturnTime");
                    gvBorrowItems.HeaderRow.Cells[5].Text = GetLabel("Purpose");
                }

                gvBorrowHistory.Columns[1].HeaderText = GetLabel("Item");
                gvBorrowHistory.Columns[2].HeaderText = GetLabel("Qty");
                gvBorrowHistory.Columns[3].HeaderText = GetLabel("Date");
                gvBorrowHistory.Columns[4].HeaderText = GetLabel("Time");
                gvBorrowHistory.Columns[5].HeaderText = GetLabel("Status");
                gvBorrowHistory.Columns[6].HeaderText = GetLabel("Returned");
                gvBorrowHistory.Columns[7].HeaderText = GetLabel("ReturnBy");

                LoadDeviceStatus(txtToday.Text);
                LoadUserBorrowHistory();

                DataTable borrowersTable = GetAllBorrowersForToday(txtToday.Text);
                Session["BorrowersToday"] = borrowersTable;
            }
        }
        protected void btnAddRow_Click(object sender, EventArgs e)
        {
            DataTable dt = ViewState["BorrowItems"] as DataTable;
            if (dt == null)
            {
                dt = new DataTable();
                dt.Columns.Add("RowIndex", typeof(int));
            }

            dt.Rows.Add(dt.Rows.Count);
            ViewState["BorrowItems"] = dt;

            gvBorrowItems.DataSource = dt;
            gvBorrowItems.DataBind();
        }
        protected void txtToday_TextChanged(object sender, EventArgs e)
        {
            LoadDeviceStatus(txtToday.Text);

            // ✅ Reload BorrowersToday again
            DataTable borrowersTable = GetAllBorrowersForToday(txtToday.Text);
            Session["BorrowersToday"] = borrowersTable;

            // ✅ Load the borrow history of the user too
            LoadUserBorrowHistory();
        }

        private void LoadDeviceStatus(string selectedDate)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                string query = @"
SELECT i.ItemID, i.ItemName,  i.AvailableQty,
       b.UserName, b.DeptName, b.BorrowEndTime
FROM IT_Items i
LEFT JOIN IT_BorrowTransactions b ON i.ItemID = b.ItemID
    AND b.Status = 'PASS' AND b.Returned = 0
    AND CONVERT(date, b.BorrowDate) = @SelectedDate
ORDER BY i.ItemID, b.BorrowEndTime";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@SelectedDate", selectedDate);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                // ✨ Prepare final DataTable
                DataTable final = new DataTable();
                final.Columns.Add("ItemID", typeof(int));
                final.Columns.Add("ItemName", typeof(string));
                final.Columns.Add("SerialNumber", typeof(string));
                final.Columns.Add("AvailableQty", typeof(int));
                final.Columns.Add("Borrowers", typeof(DataTable));

                // 🔁 Group data and assign each set of borrowers
                var grouped = dt.AsEnumerable()
                    .GroupBy(r => new
                    {
                        ItemID = r.Field<int>("ItemID"),
                        ItemName = r.Field<string>("ItemName"),  
                        AvailableQty = r.Field<int>("AvailableQty")
                    });

                foreach (var group in grouped)
                {
                    DataRow newRow = final.NewRow();
                    newRow["ItemID"] = group.Key.ItemID;
                    newRow["ItemName"] = group.Key.ItemName;
                    newRow["AvailableQty"] = group.Key.AvailableQty;

                    // 📌 Borrowers specific to this item
                    DataTable dtBorrowers = group.CopyToDataTable();
                    newRow["Borrowers"] = dtBorrowers;

                    final.Rows.Add(newRow);
                }

                rptDeviceStatus.DataSource = final;
                rptDeviceStatus.DataBind();
            }
        }
        protected void rptDeviceStatus_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DataRowView drv = (DataRowView)e.Item.DataItem;

                Repeater rptBorrowers = (Repeater)e.Item.FindControl("rptBorrowers");

                if (drv.Row.Table.Columns.Contains("Borrowers") && rptBorrowers != null)
                {
                    DataTable dtBorrowers = drv["Borrowers"] as DataTable;
                    rptBorrowers.DataSource = dtBorrowers;
                    rptBorrowers.DataBind();
                }

                Label lblAvailableAt = (Label)e.Item.FindControl("lblAvailableAt");
                if (lblAvailableAt != null && drv["AvailableQty"] != DBNull.Value &&
                    Convert.ToInt32(drv["AvailableQty"]) == 0 &&
                    drv["BorrowEndTime"] != DBNull.Value &&
                    TimeSpan.TryParse(drv["BorrowEndTime"].ToString(), out TimeSpan endTime))
                {
                    lblAvailableAt.Text = endTime.ToString(@"hh\:mm");
                }
            }
        }
        protected void gvBorrowItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList ddlItem = (DropDownList)e.Row.FindControl("ddlItem");
                DropDownList ddlUseTime = (DropDownList)e.Row.FindControl("ddlUseTime");
                DropDownList ddlReturnTime = (DropDownList)e.Row.FindControl("ddlReturnTime");

                // ดึงรายการอุปกรณ์
                if (ddlItem != null)
                {
                    using (SqlConnection con = new SqlConnection(connStr))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand("SELECT ItemID, ItemName, AvailableQty FROM IT_Items", con);
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        ddlItem.Items.Clear();
                        ddlItem.Items.Insert(0, new ListItem("-- Select Item --", ""));

                        foreach (DataRow dr in dt.Rows)
                        {
                            string text = dr["ItemName"].ToString();
                            string value = dr["ItemID"].ToString();
                            int available = Convert.ToInt32(dr["AvailableQty"]);

                            ListItem item = new ListItem(text + (available == 0 ? " (Out of Stock)" : ""), value);
                            if (available == 0)
                                item.Attributes["disabled"] = "disabled";

                            ddlItem.Items.Add(item);
                        }
                    }
                }

                // โหลด Use Time (เฉพาะเวลาเริ่ม เช่น 08:00, 08:30 ...)
                if (ddlUseTime != null)
                {
                    ddlUseTime.Items.Clear();
                    ddlUseTime.Items.Insert(0, new ListItem("-- Select --", ""));
                    foreach (string time in GetTimeSlots())
                    {
                        ddlUseTime.Items.Add(new ListItem(time, time)); // ใช้ HH:mm ทั้ง Text และ Value
                    }
                }

                if (ddlReturnTime != null)
                {
                    ddlReturnTime.Items.Clear();
                    ddlReturnTime.Items.Insert(0, new ListItem("-- Select --", ""));
                }
            }
        }
        protected void ddlItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddl = (DropDownList)sender;
            GridViewRow row = (GridViewRow)ddl.NamingContainer;

            CheckBoxList chkSerials = (CheckBoxList)row.FindControl("chkSerials");
            DropDownList ddlUseTime = (DropDownList)row.FindControl("ddlUseTime");
            DropDownList ddlReturnTime = (DropDownList)row.FindControl("ddlReturnTime");

            string itemId = ddl.SelectedValue;
            if (string.IsNullOrEmpty(itemId)) return;

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();

                SqlCommand getSerials = new SqlCommand(@"
            SELECT SerialItemID, SerialNumber 
            FROM IT_ItemSerials 
            WHERE ItemID = @ItemID AND IsAvailable = 1
            ORDER BY SerialItemID", con);
                getSerials.Parameters.AddWithValue("@ItemID", itemId);

                SqlDataReader rdr = getSerials.ExecuteReader();
                chkSerials.Items.Clear();

                while (rdr.Read())
                {
                    string serialText = rdr["SerialNumber"].ToString();
                    string serialValue = rdr["SerialItemID"].ToString();
                    chkSerials.Items.Add(new ListItem(serialText, serialValue));
                }

                rdr.Close();
            }

            if (ddlUseTime != null)
            {
                ddlUseTime.SelectedIndex = 0;
            }

            if (ddlReturnTime != null)
            {
                ddlReturnTime.Items.Clear();
                ddlReturnTime.Items.Insert(0, new ListItem("-- Select --", ""));
            }
        }
        protected void ddlUseTime_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlUseTime = (DropDownList)sender;
            GridViewRow row = (GridViewRow)ddlUseTime.NamingContainer;
            DropDownList ddlReturnTime = (DropDownList)row.FindControl("ddlReturnTime");

            if (ddlReturnTime != null)
            {
                ddlReturnTime.Items.Clear();
                ddlReturnTime.Items.Insert(0, new ListItem("-- Select --", ""));

                if (!string.IsNullOrEmpty(ddlUseTime.SelectedValue))
                {
                    TimeSpan start = TimeSpan.Parse(ddlUseTime.SelectedValue);
                    TimeSpan end = new TimeSpan(17, 0, 0);

                    start = start.Add(new TimeSpan(0, 30, 0));

                    while (start <= end)
                    {
                        // ✅ ข้ามเฉพาะ 12:01 - 12:59 (แต่ให้ 12:00 แสดง)
                        if (start.Hours == 12 && start.Minutes > 0)
                        {
                            start = new TimeSpan(13, 0, 0);
                            continue;
                        }

                        ddlReturnTime.Items.Add(new ListItem(start.ToString(@"hh\:mm"), start.ToString(@"hh\:mm")));
                        start = start.Add(new TimeSpan(0, 30, 0));
                    }

                    if (ddlReturnTime.Items.Count == 1)
                    {
                        ddlReturnTime.Items.Add(new ListItem("No available return time", ""));
                    }
                }
            }
        }
        protected void btnSubmitMemo_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();

                foreach (GridViewRow row in gvBorrowItems.Rows)
                {
                    DropDownList ddlItem = (DropDownList)row.FindControl("ddlItem");
                    DropDownList ddlStart = (DropDownList)row.FindControl("ddlUseTime");
                    DropDownList ddlEnd = (DropDownList)row.FindControl("ddlReturnTime");
                    TextBox txtRemark = (TextBox)row.FindControl("txtRemark");
                    CheckBoxList chkSerials = (CheckBoxList)row.FindControl("chkSerials");

                    if (ddlItem == null || string.IsNullOrEmpty(ddlItem.SelectedValue)) continue;
                    if (ddlStart == null || ddlEnd == null || string.IsNullOrEmpty(ddlStart.SelectedValue) || string.IsNullOrEmpty(ddlEnd.SelectedValue)) continue;

                    List<int> serialList = new List<int>();
                    foreach (ListItem item in chkSerials.Items)
                    {
                        if (item.Selected)
                            serialList.Add(int.Parse(item.Value));
                    }

                    if (serialList.Count == 0) continue;

                    string itemId = ddlItem.SelectedValue;
                    DateTime borrowDate = DateTime.ParseExact(txtToday.Text, "yyyy-MM-dd", null);
                    TimeSpan borrowStartTime = TimeSpan.Parse(ddlStart.SelectedValue);
                    TimeSpan borrowEndTime = TimeSpan.Parse(ddlEnd.SelectedValue);

                    // 🔍 เช็กซ้ำแบบแยก Serial
                    bool conflict = false;
                    foreach (int serialId in serialList)
                    {
                        SqlCommand checkSerialOverlap = new SqlCommand(@"
                    SELECT COUNT(*) FROM IT_BorrowTransactions
                    WHERE SerialItemID = @SerialID
                    AND Returned = 0 AND Status IN ('PENDING', 'PASS')
                    AND BorrowDate = @BorrowDate
                    AND (@Start < BorrowEndTime AND @End > BorrowStartTime)", con);
                        checkSerialOverlap.Parameters.AddWithValue("@SerialID", serialId);
                        checkSerialOverlap.Parameters.AddWithValue("@BorrowDate", borrowDate);
                        checkSerialOverlap.Parameters.AddWithValue("@Start", borrowStartTime);
                        checkSerialOverlap.Parameters.AddWithValue("@End", borrowEndTime);

                        int serialConflict = Convert.ToInt32(checkSerialOverlap.ExecuteScalar());
                        if (serialConflict > 0)
                        {
                            conflict = true;
                            break;
                        }
                    }

                    if (conflict)
                    {
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "notEnoughStock",
                            "Swal.fire('Serial already in use!', 'One of the selected serials is already booked for this time.', 'error');", true);
                        return;
                    }

                    // ✅ Insert ได้เลย
                    foreach (int serialId in serialList)
                    {
                        SqlCommand markUsed = new SqlCommand("UPDATE IT_ItemSerials SET IsAvailable = 0 WHERE SerialItemID = @ID", con);
                        markUsed.Parameters.AddWithValue("@ID", serialId);
                        markUsed.ExecuteNonQuery();

                        SqlCommand insert = new SqlCommand(@"
                    INSERT INTO IT_BorrowTransactions 
                    (ItemID, UserName, DeptName, BorrowDate, BorrowStartTime, BorrowEndTime, Quantity, Purpose, Status, Returned, SerialItemID)
                    VALUES 
                    (@ItemID, @User, @Dept, @Date, @Start, @End, 1, @Purpose, 'PENDING', 0, @SerialID)", con);
                        insert.Parameters.AddWithValue("@ItemID", itemId);
                        insert.Parameters.AddWithValue("@User", txtUser.Text);
                        insert.Parameters.AddWithValue("@Dept", txtDept.Text);
                        insert.Parameters.AddWithValue("@Date", borrowDate);
                        insert.Parameters.AddWithValue("@Start", borrowStartTime);
                        insert.Parameters.AddWithValue("@End", borrowEndTime);
                        insert.Parameters.AddWithValue("@Purpose", txtRemark.Text);
                        insert.Parameters.AddWithValue("@SerialID", serialId);
                        insert.ExecuteNonQuery();
                    }

                    SqlCommand updateQty = new SqlCommand("UPDATE IT_Items SET AvailableQty = AvailableQty - @Qty WHERE ItemID = @ItemID", con);
                    updateQty.Parameters.AddWithValue("@Qty", serialList.Count);
                    updateQty.Parameters.AddWithValue("@ItemID", itemId);
                    //updateQty.ExecuteNonQuery();
                }

                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert",
                    "Swal.fire({ icon: 'success', title: 'Submitted!', text: 'Your borrow request has been successfully submitted for approval.' });", true);
            }
        }

        private List<string> GetTimeSlots()
        {
            List<string> slots = new List<string>();
            DateTime start = DateTime.Parse("08:00");
            DateTime end = DateTime.Parse("17:00");

            while (start < end)
            {
                // ✅ ข้ามเฉพาะหลัง 12:00 ไปจนถึงก่อน 13:00 (แต่ให้ 12:00 ผ่านได้)
                if (start.TimeOfDay > new TimeSpan(12, 0, 0) && start.TimeOfDay < new TimeSpan(13, 0, 0))
                {
                    start = DateTime.Parse("13:00");
                    continue;
                }

                slots.Add(start.ToString("HH:mm"));
                start = start.AddMinutes(30);
            }

            return slots;
        }
        private void LoadUserBorrowHistory()
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(@"
    SELECT 
        b.BorrowID,
        i.ItemName,
        b.Quantity,
        b.BorrowDate,
        b.BorrowStartTime,
        b.BorrowEndTime,
        b.Status,
        b.Returned
    FROM IT_BorrowTransactions b
    INNER JOIN IT_Items i ON b.ItemID = i.ItemID
    WHERE b.UserName = @UserName
    ORDER BY b.BorrowDate DESC, b.BorrowStartTime ASC", con);  

                cmd.Parameters.AddWithValue("@UserName", Session["UserName"].ToString());

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvBorrowHistory.DataSource = dt;
                gvBorrowHistory.DataBind();
            }
        }
        private DataTable GetAllBorrowersForToday(string selectedDate)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                string sql = @"SELECT i.ItemID,i.ItemName,
                b.UserName,b.DeptName,b.BorrowEndTime
                FROM IT_BorrowTransactions b
                INNER JOIN IT_Items i ON b.ItemID = i.ItemID
                WHERE b.Returned = 0 
                AND b.Status = 'PASS'
                AND CONVERT(date, b.BorrowDate) = @SelectedDate
                ORDER BY i.ItemID, b.BorrowEndTime";


                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@SelectedDate", selectedDate);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
        public DataTable GetBorrowersForItem(object itemIdObj)
        {
            int itemId = Convert.ToInt32(itemIdObj);
            DataTable allBorrowers = Session["BorrowersToday"] as DataTable;

            if (allBorrowers == null) return new DataTable();

            DataView view = new DataView(allBorrowers);
            view.RowFilter = $"ItemID = {itemId}";
            return view.ToTable();
        }
        protected string GetLabel(string key)
        {
            string lang = Session["lang"]?.ToString() ?? "en";

            var th = new Dictionary<string, string> {
        { "BorrowRequestForm", "แบบฟอร์มขอยืมอุปกรณ์" },
        { "Today", "วันที่วันนี้" },
        { "Date", "วันที่" },
        { "User", "ผู้ขอ" },
        { "Department", "แผนก" },
        { "DeviceStatus", "สถานะอุปกรณ์" },
        { "DeviceStatusOverview", "ภาพรวมสถานะอุปกรณ์" },
        { "Available", "ว่าง" },
        { "Unavailable", "ไม่ว่าง" },
        { "AvailableAt", "จะว่างเวลา" },
        { "Item", "อุปกรณ์" },
        { "SerialNo", "ซีเรียล" },
        { "Qty", "จำนวน" },
        { "QtyLeft", "จำนวนที่เหลือ:" },
        { "UseTime", "เวลาที่ใช้" },
        { "ReturnTime", "เวลาคืน" },
        { "Purpose", "เหตุผลการยืม" },
        { "Submit", "ส่งคำขอ" },
        { "BorrowHistory", "ประวัติการยืม" },
        { "YourBorrowHistory", "ประวัติการยืมของคุณ" },
        { "StartTime", "เวลาเริ่ม" },
        { "EndTime", "เวลาสิ้นสุด" },
        { "Status", "สถานะ" },
        { "Returned", "คืนแล้ว?" },
        { "SelectItem", "-- เลือกอุปกรณ์ --" },
        { "SelectTime", "-- เลือกเวลา --" },
        { "NoReturnTime", "ไม่มีเวลาคืนที่ว่าง" },
        { "ViewBorrowers", "ดูผู้ยืม" },
        { "ViewHistory", "ดูประวัติของคุณ" },
        { "DeviceBorrowStatus", "สถานะการยืมอุปกรณ์" },
        { "SelectDate", "เลือกวันที่" },
        { "ViewType", "ประเภทมุมมอง" },
        { "Time", "เวลา" }
    };

            var en = new Dictionary<string, string> {
        { "BorrowRequestForm", "Borrow Request Form" },
        { "Today", "Today" },
        { "Date", "Date" },
        { "User", "User" },
        { "Department", "Department" },
        { "DeviceStatus", "Device Status" },
        { "DeviceStatusOverview", "Device Status Overview" },
        { "Available", "Available" },
        { "Unavailable", "Unavailable" },
        { "AvailableAt", "Available at" },
        { "Item", "Item" },
        { "SerialNo", "Serial No." },
        { "Qty", "Qty" },
        { "QtyLeft", "Qty Left:" },
        { "UseTime", "Use Time" },
        { "ReturnTime", "Return Time" },
        { "Purpose", "Purpose" },
        { "Submit", "Submit" },
        { "BorrowHistory", "Borrow History" },
        { "YourBorrowHistory", "Your Borrow History" },
        { "StartTime", "Start Time" },
        { "EndTime", "End Time" },
        { "Status", "Status" },
        { "Returned", "Returned?" },
        { "SelectItem", "-- Select Item --" },
        { "SelectTime", "-- Select --" },
        { "NoReturnTime", "No available return time" },
        { "ViewBorrowers", "View Borrowers" },
        { "ViewHistory", "View History" },
        { "DeviceBorrowStatus", "Device Borrow Status" },
        { "SelectDate", "Select Date" },
        { "ViewType", "View Type" },
        { "Time", "Time" }
    };

            var zh = new Dictionary<string, string> {
        { "BorrowRequestForm", "借用需求表" },
        { "Today", "今天的日期" },
        { "Date", "日期" },
        { "User", "使用者" },
        { "Department", "部門" },
        { "DeviceStatus", "設備狀態" },
        { "DeviceStatusOverview", "設備狀態一覽" },
        { "Available", "可用" },
        { "Unavailable", "不可用" },
        { "AvailableAt", "可用時間" },
        { "Item", "設備" },
        { "SerialNo", "序號" },
        { "Qty", "數量" },
        { "QtyLeft", "設備殘留數量:" },
        { "UseTime", "使用時間" },
        { "ReturnTime", "歸還時間" },
        { "Purpose", "目的" },
        { "Submit", "提交" },
        { "BorrowHistory", "出借歷史紀錄" },
        { "YourBorrowHistory", "出借歷史紀錄" },
        { "StartTime", "開始時間" },
        { "EndTime", "結束時間" },
        { "Status", "狀態" },
        { "Returned", "是否歸還" },
        { "SelectItem", "-- 選擇設備 --" },
        { "SelectTime", "-- 選擇時間 --" },
        { "NoReturnTime", "没有可用的歸還時間" },
        { "ViewBorrowers", "出借總覽" },
        { "ViewHistory", "出借歷史紀錄" },
        { "DeviceBorrowStatus", "設備出借狀態" },
        { "SelectDate", "選擇日期" },
        { "ViewType", "瀏覽時間別" },
        { "Time", "時間" }
    };

            Dictionary<string, string> dict;
            if (lang == "zh") dict = zh;
            else if (lang == "th") dict = th;
            else dict = en;

            return dict.ContainsKey(key) ? dict[key] : key;
        }    
        protected string GetItemImageUrl(string itemName)
        {
            itemName = itemName.ToLower();

            string basePath = "/Image/";

            if (itemName.Contains("projector")) return basePath + "icon_1.png";
            if (itemName.Contains("speaker")) return basePath + "icon_2.png";
            if (itemName.Contains("monitor")) return basePath + "icon_3.png";
            if (itemName.Contains("set m/k")) return basePath + "icon_4.png";
            if (itemName.Contains("dell") && itemName.Contains("mouse")) return basePath + "icon_5.png";
            if (itemName.Contains("logitech") && itemName.Contains("mouse")) return basePath + "icon_6.png";
            if (itemName.Contains("logitech") && itemName.Contains("keyboard")) return basePath + "icon_7.png";
            if (itemName.Contains("adapter n/b")) return basePath + "icon_8.png";
            if (itemName.Contains("adapter 4port")) return basePath + "icon_9.png";
            if (itemName.Contains("notebook")) return basePath + "icon_10.png";

            return basePath + "icon_1.png"; // fallback
        }

    }
}
