using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Web.UI.WebControls;

namespace IT_WorkPlant.Pages
{
    public partial class IT_BorrowStatus : System.Web.UI.Page
    {
        string connStr = WebConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                txtDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                ddlViewType.SelectedValue = "weekly";

                string itemIdStr = Request.QueryString["item"];
                if (!string.IsNullOrEmpty(itemIdStr) && int.TryParse(itemIdStr, out int itemId))
                {
                    ViewState["ItemID"] = itemId;
                    LoadSerials(itemId);
                    LoadItemName(itemId);
                }
            }
        }

        private void LoadItemName(int itemId)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT ItemName FROM IT_Items WHERE ItemID = @ItemID", con);
                cmd.Parameters.AddWithValue("@ItemID", itemId);
                ltItemName.Text = cmd.ExecuteScalar()?.ToString() ?? "-";
            }
        }

        private void LoadSerials(int itemId)
        {
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand("SELECT SerialItemID, SerialNumber FROM IT_ItemSerials WHERE ItemID = @ItemID", con);
                cmd.Parameters.AddWithValue("@ItemID", itemId);
                con.Open();

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlSerial.Items.Clear(); // เคลียร์ก่อนทุกครั้ง

                if (dt.Rows.Count == 1)
                {
                    // กรณีมีแค่ 1 ซีเรียล — ซ่อน dropdown ไปเลย
                    ddlSerial.Items.Add(new ListItem(dt.Rows[0]["SerialNumber"].ToString(), dt.Rows[0]["SerialItemID"].ToString()));
                    pnlSerial.Visible = false;
                    ViewState["SerialItemID"] = dt.Rows[0]["SerialItemID"].ToString();

                    DateTime selectedDate = DateTime.Parse(txtDate.Text);
                    int serialId = int.Parse(ViewState["SerialItemID"].ToString());
                    string viewType = ddlViewType.SelectedValue.ToLower();

                    switch (viewType)
                    {
                        case "weekly":
                            BuildWeeklyMatrixTable(serialId, selectedDate);
                            break;
                        case "monthly":
                            BuildMonthlyMatrixTable(serialId, selectedDate);
                            break;
                        default:
                            BuildDailyMatrixTable(serialId, selectedDate);
                            break;
                    }
                }
                else if (dt.Rows.Count > 1)
                {
                    // กรณีมีหลายซีเรียล — แสดง dropdown ให้เลือก
                    pnlSerial.Visible = true;
                    ddlSerial.Items.Add(new ListItem("-- Select Serial --", "")); // default option

                    foreach (DataRow row in dt.Rows)
                    {
                        ddlSerial.Items.Add(new ListItem(row["SerialNumber"].ToString(), row["SerialItemID"].ToString()));
                    }
                }
                else
                {
                    // กรณีไม่มีซีเรียลเลย
                    pnlSerial.Visible = false;
                    ltMatrixTable.Text = "<div class='text-danger'>No serials found for this item.</div>";
                }
            }
        }
        protected void ddlSerial_SelectedIndexChanged(object sender, EventArgs e)
        {
            ViewState["SerialItemID"] = ddlSerial.SelectedValue;
        }

        protected void btnLoad_Click(object sender, EventArgs e)
        {
            if (!DateTime.TryParse(txtDate.Text, out DateTime selectedDate))
            {
                ltMatrixTable.Text = "<div class='text-danger'>Invalid date.</div>";
                return;
            }

            int serialId;
            if (ddlSerial.Items.Count == 0 && ViewState["ItemID"] != null)
            {
                LoadSerials((int)ViewState["ItemID"]);
            }

            if (pnlSerial.Visible)
            {
                if (!int.TryParse(ddlSerial.SelectedValue, out serialId))
                {
                    ltMatrixTable.Text = "<div class='text-warning'>Please select a serial number.</div>";
                    return;
                }
            }
            else
            {
                if (ViewState["SerialItemID"] == null)
                {
                    ltMatrixTable.Text = "<div class='text-danger'>No serial found.</div>";
                    return;
                }
                serialId = int.Parse(ViewState["SerialItemID"].ToString());
            }

            string viewType = ddlViewType.SelectedValue;
            switch (viewType)
            {
                case "weekly":
                    BuildWeeklyMatrixTable(serialId, selectedDate);
                    break;
                case "monthly":
                    BuildMonthlyMatrixTable(serialId, selectedDate); 
                    break;
                default:
                    BuildDailyMatrixTable(serialId, selectedDate);
                    break;
            }
        }
        private void BuildWeeklyMatrixTable(int serialItemId, DateTime startDate)
        {
            List<string> timeSlots = GetTimeSlots();
            StringBuilder html = new StringBuilder();

            html.Append("<table class='table table-bordered text-center table-sm'>");
            html.Append("<thead><tr>");
            html.Append($"<th>{GetLabel("time")}</th>");

            for (int i = 0; i < 7; i++)
            {
                DateTime day = startDate.Date.AddDays(i);
                string dayKey = day.ToString("ddd").ToLower(); // ex. sun, mon
                html.Append($"<th>{GetLabel(dayKey)}<br/>{day:dd/MM}</th>");
            }

            html.Append("</tr></thead><tbody>");

            for (int t = 0; t < timeSlots.Count; t++)
            {
                string slot = timeSlots[t];
                TimeSpan slotStart = TimeSpan.Parse(slot);
                TimeSpan slotEnd = slotStart.Add(new TimeSpan(0, 30, 0));

                html.Append("<tr>");
                html.Append($"<td>{slot} - {slotEnd:hh\\:mm}</td>");

                for (int i = 0; i < 7; i++)
                {
                    DateTime date = startDate.Date.AddDays(i);
                    DataTable borrowData = GetBorrowData(serialItemId, date);

                    bool isFull = borrowData.AsEnumerable().Any(row =>
                    {
                        TimeSpan borrowStart = (TimeSpan)row["BorrowStartTime"];
                        TimeSpan borrowEnd = (TimeSpan)row["BorrowEndTime"];
                        return slotStart < borrowEnd && slotEnd > borrowStart;
                    });

                    string status = GetLabel(isFull ? "full" : "free");
                    string css = isFull ? "table-danger" : "table-success";

                    html.Append($"<td class='{css}'>{status}</td>");
                }

                html.Append("</tr>");
            }

            html.Append("</tbody></table>");
            ltMatrixTable.Text = html.ToString();
        }

        private void BuildMonthlyMatrixTable(int serialItemId, DateTime selectedDate)
        {
            int year = selectedDate.Year;
            int month = selectedDate.Month;

            DateTime firstDayOfMonth = new DateTime(year, month, 1);
            int dayOffset = (int)firstDayOfMonth.DayOfWeek; // Sunday = 0
            DateTime startDate = firstDayOfMonth.AddDays(-dayOffset);

            Dictionary<DateTime, List<string>> borrowByDept = new Dictionary<DateTime, List<string>>();

            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(@"
            SELECT DISTINCT BorrowDate, DeptName
            FROM IT_BorrowTransactions
            WHERE SerialItemID = @SerialItemID
              AND MONTH(BorrowDate) = @Month
              AND YEAR(BorrowDate) = @Year
              AND (Status = 'PASS' OR Status IS NULL)", con);

                cmd.Parameters.AddWithValue("@SerialItemID", serialItemId);
                cmd.Parameters.AddWithValue("@Month", month);
                cmd.Parameters.AddWithValue("@Year", year);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    DateTime d = Convert.ToDateTime(reader["BorrowDate"]).Date;
                    string dept = reader["DeptName"].ToString();

                    if (!borrowByDept.ContainsKey(d))
                        borrowByDept[d] = new List<string>();

                    if (!borrowByDept[d].Contains(dept))
                        borrowByDept[d].Add(dept);
                }
            }

            StringBuilder html = new StringBuilder();
            html.Append("<table class='table table-bordered text-center table-sm'>");
            html.Append("<thead><tr>");
            html.Append($"<th>{GetLabel("sun")}</th><th>{GetLabel("mon")}</th><th>{GetLabel("tue")}</th><th>{GetLabel("wed")}</th><th>{GetLabel("thu")}</th><th>{GetLabel("fri")}</th><th>{GetLabel("sat")}</th>");
            html.Append("</tr></thead><tbody>");

            DateTime day = startDate;
            for (int week = 0; week < 6; week++)
            {
                html.Append("<tr>");
                for (int i = 0; i < 7; i++)
                {
                    bool inMonth = day.Month == month;
                    string css = inMonth ? "bg-white" : "bg-light text-muted";

                    html.Append($"<td class='{css}'>");
                    html.Append($"<div><strong>{day.Day}</strong></div>");

                    if (borrowByDept.ContainsKey(day))
                    {
                        foreach (var dept in borrowByDept[day])
                        {
                            html.Append($"<div class='text-danger fw-bold'>{GetLabel("borrowedby")} {dept}</div>");
                        }
                    }
                    else
                    {
                        html.Append($"<div class='text-success'>{GetLabel("free")}</div>");
                    }

                    html.Append("</td>");
                    day = day.AddDays(1);
                }
                html.Append("</tr>");
            }

            html.Append("</tbody></table>");
            ltMatrixTable.Text = html.ToString();
        }
        private DataTable GetBorrowData(int serialItemId, DateTime date)
        {
            DataTable borrowData = new DataTable();
            using (SqlConnection con = new SqlConnection(connStr))
            {
                SqlCommand cmd = new SqlCommand(@"
            SELECT BorrowStartTime, BorrowEndTime
            FROM IT_BorrowTransactions
            WHERE SerialItemID = @SerialItemID
              AND BorrowDate = @SelectedDate
              AND (Status = 'PASS' OR Status IS NULL)", con);

                cmd.Parameters.AddWithValue("@SerialItemID", serialItemId);
                cmd.Parameters.AddWithValue("@SelectedDate", date);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(borrowData);
            }
            return borrowData;
        }

        private void BuildDailyMatrixTable(int serialItemId, DateTime selectedDate)
        {
            List<string> timeSlots = GetTimeSlots();
            DataTable borrowData = new DataTable();

            using (SqlConnection con = new SqlConnection(connStr))
            {
                con.Open();

                SqlCommand cmd = new SqlCommand(@"
            SELECT BorrowStartTime, BorrowEndTime
            FROM IT_BorrowTransactions
            WHERE SerialItemID = @SerialItemID
              AND BorrowDate = @SelectedDate
              AND (Status = 'PASS' OR Status IS NULL)", con);

                cmd.Parameters.AddWithValue("@SerialItemID", serialItemId);
                cmd.Parameters.AddWithValue("@SelectedDate", selectedDate);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(borrowData);
            }

            StringBuilder html = new StringBuilder();
            html.Append("<table class='table table-bordered table-sm text-center'>");
            html.Append($"<thead><tr><th>{GetLabel("time")}</th><th>{GetLabel("viewstatus")}</th></tr></thead>");
            html.Append("<tbody>");

            foreach (string slot in timeSlots)
            {
                TimeSpan slotStart = TimeSpan.Parse(slot);
                TimeSpan slotEnd = slotStart.Add(new TimeSpan(0, 30, 0));

                bool isFull = borrowData.AsEnumerable().Any(row =>
                {
                    TimeSpan borrowStart = (TimeSpan)row["BorrowStartTime"];
                    TimeSpan borrowEnd = (TimeSpan)row["BorrowEndTime"];
                    return slotStart < borrowEnd && slotEnd > borrowStart;
                });

                string css = isFull ? "table-danger" : "table-success";
                string statusText = GetLabel(isFull ? "full" : "free");

                html.Append("<tr>");
                html.Append($"<td>{slot} - {slotEnd:hh\\:mm}</td>");
                html.Append($"<td class='{css}'>{statusText}</td>");
                html.Append("</tr>");
            }

            html.Append("</tbody></table>");
            ltMatrixTable.Text = html.ToString();
        }
        private List<string> GetTimeSlots()
        {
            List<string> slots = new List<string>();
            DateTime time = DateTime.Parse("08:00");
            DateTime end = DateTime.Parse("17:00");

            while (time < end)
            {
                if (time.Hour == 12 && time.Minute >= 30)
                {
                    time = DateTime.Parse("13:00");
                    continue;
                }

                slots.Add(time.ToString("HH:mm"));
                time = time.AddMinutes(30);
            }

            return slots;
        }
        protected string GetLabel(string key)
        {
            string lang = Session["lang"]?.ToString() ?? "th";

            var th = new Dictionary<string, string> {
        { "selectdate", "เลือกวันที่" },
        { "viewtype", "รูปแบบมุมมอง" },
        { "selectserial", "เลือกหมายเลขซีเรียล" },
        { "viewstatus", "ดูสถานะ" },
        { "time", "ช่วงเวลา" },
        { "full", "เต็ม" },
        { "free", "ว่าง" },
        { "sun", "อาทิตย์" },
        { "mon", "จันทร์" },
        { "tue", "อังคาร" },
        { "wed", "พุธ" },
        { "thu", "พฤหัสบดี" },
        { "fri", "ศุกร์" },
        { "sat", "เสาร์" },
        { "invaliddate", "วันที่ไม่ถูกต้อง" },
        { "pleaseselectserial", "กรุณาเลือกหมายเลขซีเรียลก่อน" },
        { "noserialfound", "ไม่พบซีเรียลของอุปกรณ์นี้" },
        { "borrowedby", "ยืมโดย" },        

    };

            var en = new Dictionary<string, string> {
        { "selectdate", "Select Date" },
        { "viewtype", "View Type" },
        { "selectserial", "Select Serial" },
        { "viewstatus", "View Status" },
        { "time", "Time" },
        { "full", "FULL" },
        { "free", "FREE" },
        { "sun", "Sun" },
        { "mon", "Mon" },
        { "tue", "Tue" },
        { "wed", "Wed" },
        { "thu", "Thu" },
        { "fri", "Fri" },
        { "sat", "Sat" },
        { "invaliddate", "Invalid date" },
        { "pleaseselectserial", "Please select a serial number" },
        { "noserialfound", "No serial found for this item" },
        { "borrowedby", "Borrowed by" },

    };

            var zh = new Dictionary<string, string> {
        { "selectdate", "选择日期" },
        { "viewtype", "查看方式" },
        { "selectserial", "选择序列号" },
        { "viewstatus", "查看状态" },
        { "time", "时间段" },
        { "full", "满了" },
        { "free", "空闲" },
        { "sun", "周日" },
        { "mon", "周一" },
        { "tue", "周二" },
        { "wed", "周三" },
        { "thu", "周四" },
        { "fri", "周五" },
        { "sat", "周六" },
        { "invaliddate", "无效日期" },
        { "pleaseselectserial", "请选择序列号" },
        { "noserialfound", "未找到该设备的序列号" },
        { "borrowedby", "借用者" },

    };
            Dictionary<string, string> dict;
            if (lang == "zh") dict = zh;
            else if (lang == "en") dict = en;
            else dict = th;
           return dict.ContainsKey(key.ToLower()) ? dict[key.ToLower()] : key;
        }

    }
}