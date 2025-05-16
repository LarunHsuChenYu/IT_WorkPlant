using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using OfficeOpenXml;
using System.IO;
using System.Windows;
using System.Configuration;
using System.Data.SqlClient;


namespace IT_WorkPlant.Pages
{
    enum ErrorCode
    {
        CancelFailed,
        UserNotFound,
        InvalidTimeFormat,
        RoomNotSelected,
        EndBeforeStart,
        BookingNotFound
    }

    public partial class MeetingRoomBooking : Page
    {
        private string excelFilePath;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["username"] == null)
            {
                Response.Redirect("../Login.aspx");
                return;
            }
            string lang = Session["lang"] != null ? Session["lang"].ToString() : "en";
            Session["lang"] = lang; // ถ้ายังไม่มี ให้เซ็ตเลย

            GridView1.RowCommand += GridView1_RowCommand;

            if (!IsPostBack)
            {
                lblTitle.Text = GetLabel("title");
                lblDateLabel.Text = GetLabel("date");      
                lblRoomLabel.Text = GetLabel("room");
                submitButton.Text = GetLabel("bookroom");
                btnEditMode.Text = GetLabel("edit");
                btnExitEditMode.Text = GetLabel("exit");
                lblSummary.Text = GetLabel("summary");
                btnExport.Text = GetLabel("export");
                lblStartLabel.Text = GetLabel("start");
                lblEndLabel.Text = GetLabel("end");
                lblActionsLabel.Text = GetLabel("actions");
                lblSummaryDate.Text = GetLabel("date");
                lblSummaryDept.Text = GetLabel("department");
                lblSummaryRoom.Text = GetLabel("room");
                lblSummaryStart.Text = GetLabel("start");
                lblSummaryEnd.Text = GetLabel("end");


                UpdateStartTimeList();
                UpdateEndTimeList();
                UpdateDepartmentList();

                Calendar1.SelectedDate = DateTime.Today;
                Calendar1.VisibleDate = DateTime.Today;

                string username = Session["username"].ToString();
                string dept = GetDepartmentByUsername(username);
                txtBookingDate.Text = DateTime.Today.ToString("dd/MM/yyyy");
                ViewState["userDept"] = dept;

                lblUser.Text = username;
                lblDept.Text = dept;

                BindRoomSchedule(Calendar1.SelectedDate);
            }

            if (Session["SuccessMessage"] != null)
            {
                string message = Session["SuccessMessage"].ToString();
                string script = $@"
Swal.fire({{
  icon: 'success',
  title: '{message}',
  confirmButtonText: 'OK'
}});";
                ScriptManager.RegisterStartupScript(this, this.GetType(), Guid.NewGuid().ToString(), script, true);
                Session["SuccessMessage"] = null;
            }
        }
        protected void Calendar1_SelectionChanged(object sender, EventArgs e)
        {
            DateTime selectedDate = Calendar1.SelectedDate;
            txtBookingDate.Text = selectedDate.ToString("dd/MM/yyyy");

            BindRoomSchedule(selectedDate);
        }

        private void BindRoomSchedule(DateTime dtCheckDate)
        {
            DataTable excelData = GetBookingDataFromExcel(dtCheckDate);

            DataTable dt = new DataTable();
            dt.Columns.Add("TimeSlot");
            string[] roomCodes = { "101", "102", "103", "201", "202", "203" };

            foreach (var room in roomCodes)
            {
                dt.Columns.Add(room);
                dt.Columns.Add($"{room}_ReservedBy");
                dt.Columns.Add($"{room}_Department");
            }

            List<string> timeSlots = GetTimeSlotsFromDatabase();
            foreach (string timeSlot in timeSlots)
            {
                DataRow dr = dt.NewRow();
                dr["TimeSlot"] = timeSlot;
                foreach (var room in roomCodes)
                {
                    dr[room] = "Free";
                    dr[$"{room}_ReservedBy"] = "";
                    dr[$"{room}_Department"] = "";
                }
                dt.Rows.Add(dr);
            }

            if (excelData != null && excelData.Rows.Count > 0)
            {
                foreach (DataRow excelRow in excelData.Rows)
                {
                    string timeSlot = excelRow["TimeSlot"].ToString();
                    string room = excelRow["Room"].ToString();
                    string department = excelRow["Department"].ToString();
                    string reservedBy = excelRow["ReservedBy"].ToString();

                    DataRow[] foundRows = dt.Select($"TimeSlot = '{timeSlot}'");
                    if (foundRows.Length > 0)
                    {
                        DataRow dr = foundRows[0];

                        dr[room] = "Booked";
                        dr[$"{room}_ReservedBy"] = reservedBy;
                        dr[$"{room}_Department"] = department;
                    }
                }
            }

            GridView1.DataSource = dt;
            GridView1.DataBind();
        }
        private DataTable GetBookingDataFromExcel(DateTime date)
        {
            // Pull data from SQL instead of Excel
            DataTable dt = new DataTable();
            string connStr = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT * FROM AD_MeetingRoom_Bk WHERE BookingDate = @Date";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Date", date);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }
        private List<string> GetTimeSlotsFromDatabase()
        {
            // 模擬從資料庫中查詢的結果，實際應用中替換成查詢邏輯
            return new List<string> {
        "08:00-08:30", "08:30-09:00", "09:00-09:30", "09:30-10:00",
        "10:00-10:30", "10:30-11:00", "11:00-11:30", "11:30-12:00",
        "13:00-13:30", "13:30-14:00", "14:00-14:30", "14:30-15:00",
        "15:00-15:30", "15:30-16:00", "16:00-16:30", "16:30-17:00"
    };
        }

        // 從資料庫獲取部門列表（範例，實際需要實作資料庫查詢邏輯）
        private List<Tuple<string, string>> GetDepartmentsFromDatabase()
        {
            // 假設從資料庫中取得以下部門數據
            return new List<Tuple<string, string>>
            {
                new Tuple<string, string>("CMO", "Chairman Office(董事長室)"),
                new Tuple<string, string>("GMO", "GM Office(總經理室)"),
                new Tuple<string, string>("ADM", "ADM(行政)"),
                new Tuple<string, string>("FIN", "FIN&ACC(財務與會計)"),
                new Tuple<string, string>("MFG", "MFG (製造)"),
                new Tuple<string, string>("QC", "QC(品管)"),
                new Tuple<string, string>("NPI", "Trail Run(試作部)"),
                new Tuple<string, string>("PMC", "PMC(生管)"),
                new Tuple<string, string>("IT", "IT(信息)"),
                new Tuple<string, string>("PUR", "PUR(採購)"),
                new Tuple<string, string>("EHS", "EHS(安全處)")
            };
        }

        // 填充開始時間選單 (從早上 8:00 AM 開始，每 30 分鐘)
        private void UpdateStartTimeList()
        {
            startTimeList.Items.Clear();

            DateTime startTime = DateTime.Today.AddHours(8); // 08:00
            DateTime endTime = DateTime.Today.AddHours(16).AddMinutes(30); // 16:30

            while (startTime <= endTime)
            {
                string timeDisplay = startTime.ToString("HH:mm");

                // ⛔  12:00 และ 12:30
                if (timeDisplay == "12:00" || timeDisplay == "12:30")
                {
                    ListItem lunchBreakItem = new ListItem($"⛔ {timeDisplay} (พักกลางวัน)", timeDisplay);
                    lunchBreakItem.Attributes.Add("style", "color: red;");
                    lunchBreakItem.Enabled = false;
                    startTimeList.Items.Add(lunchBreakItem);
                }
                else
                {
                    startTimeList.Items.Add(new ListItem(timeDisplay, timeDisplay));
                }

                startTime = startTime.AddMinutes(30);
            }

            startTimeList.SelectedIndex = 0;
        }


        // 填充會議時數選單 (0.5 到 4 小時)
        private void UpdateEndTimeList()
        {
            endTimeList.Items.Clear();

            DateTime startTime = DateTime.Today.AddHours(8); // 08:00
            DateTime endTime = DateTime.Today.AddHours(17);  // 17:00

            while (startTime <= endTime)
            {
                string timeDisplay = startTime.ToString("HH:mm");

                if (timeDisplay == "12:30")
                {
                    ListItem lunchBreakItem = new ListItem($"⛔ {timeDisplay} (พักกลางวัน)", timeDisplay);
                    lunchBreakItem.Attributes.Add("style", "color: red;");
                    lunchBreakItem.Enabled = false;
                    endTimeList.Items.Add(lunchBreakItem);
                }
                else
                {
                    endTimeList.Items.Add(new ListItem(timeDisplay, timeDisplay));
                }

                startTime = startTime.AddMinutes(30);
            }

            endTimeList.SelectedIndex = 0;
        }



        private void UpdateDepartmentList()
        {
            departmentList.Items.Clear();
            departmentList.Items.Add(new ListItem("---Select---", ""));

            // 假設 GetDepartments() 從資料庫中取得部門列表
            List<Tuple<string, string>> departments = GetDepartmentsFromDatabase();

            foreach (var dept in departments)
            {
                departmentList.Items.Add(new ListItem(dept.Item2, dept.Item1));
                // Item2 = 顯示文字, Item1 = 部門代碼
            }
        }


        // New Button Click Event: Handle form submission
        protected void submitButton_Click(object sender, EventArgs e)
        {
            DateTime selectedDate = Calendar1.SelectedDate;
            string selectedRoom = roomList.SelectedValue;
            string selectedStartTime = startTimeList.SelectedValue;
            string selectedEndTime = endTimeList.SelectedValue;
            string reservedBy = lblUser.Text;


            string selectedDepartment = ViewState["userDept"]?.ToString();
            if (string.IsNullOrEmpty(selectedDepartment))
            {
                selectedDepartment = GetDepartmentByUsername(reservedBy);
            }

            if (string.IsNullOrEmpty(selectedRoom))
            {
                ShowError(ErrorCode.RoomNotSelected);
                return;
            }


            if (string.IsNullOrEmpty(selectedStartTime) || selectedStartTime.Length != 5 ||
                string.IsNullOrEmpty(selectedEndTime) || selectedEndTime.Length != 5)
            {
                ShowError(ErrorCode.InvalidTimeFormat);

                return;
            }

            DateTime dtStart = DateTime.ParseExact(selectedStartTime, "HH:mm", null);
            DateTime dtEnd = DateTime.ParseExact(selectedEndTime, "HH:mm", null);

            if (dtEnd <= dtStart)
            {
                ShowError(ErrorCode.EndBeforeStart);
                return;
            }

            double durationMinutes = (dtEnd - dtStart).TotalMinutes;
            double durationHours = durationMinutes / 60.0;
            string timeSlot = $"{selectedStartTime}-{selectedEndTime}";

            InsertBooking(selectedDate, selectedRoom, selectedStartTime, selectedEndTime,
            selectedDepartment, reservedBy);
        }
        private bool IsRoomBooked(DateTime date, string room, string timeSlot)
        {
            // ใช้ฐานข้อมูล SQL ในการดึงข้อมูลจอง
            DataTable dt = GetBookingDataFromDatabase(date);

            // ใช้ LINQ เพื่อกรองข้อมูลการจองในห้องและช่วงเวลาที่กำหนด
            var isBooked = dt.AsEnumerable()
                .Any(row => row.Field<string>("Room") == room && row.Field<string>("TimeSlot") == timeSlot);

            return isBooked;
        }

        // Function to retrieve data from an SQL database
        private DataTable GetBookingDataFromDatabase(DateTime date)
        {
            DataTable dt = new DataTable();
            string connStr = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT * FROM AD_MeetingRoom_Bk WHERE BookingDate = @Date";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Date", date);
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }


        // New Method: Insert booking data into the Excel file
        protected void InsertBooking(DateTime date, string room, string startTime, string endTime, string department, string reservedByFromUI)
        {
            string reservedBy = Session["username"]?.ToString();
            if (string.IsNullOrEmpty(reservedBy))
            {
                ShowError(ErrorCode.UserNotFound);
                return;
            }

            DateTime dtStart = DateTime.ParseExact(startTime, "HH:mm", null);
            DateTime dtEnd = DateTime.ParseExact(endTime, "HH:mm", null);

            if (dtEnd <= dtStart)
            {
                ShowError(ErrorCode.EndBeforeStart);
                return;
            }

            List<string> splitSlots = new List<string>();
            DateTime currentStartTime = dtStart;

            while (currentStartTime.AddMinutes(30) <= dtEnd)
            {
                string slot = $"{currentStartTime:HH:mm}-{currentStartTime.AddMinutes(30):HH:mm}";
                splitSlots.Add(slot);
                currentStartTime = currentStartTime.AddMinutes(30);
            }

            string connStr = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();

                // Double-check all fields first
                List<string> conflictedSlots = new List<string>();

                foreach (var slot in splitSlots)
                {
                    string checkQuery = @"SELECT COUNT(*) FROM AD_MeetingRoom_Bk
                WHERE BookingDate = @Date AND Room = @Room AND TimeSlot = @TimeSlot";

                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Date", date);
                        checkCmd.Parameters.AddWithValue("@Room", room);
                        checkCmd.Parameters.AddWithValue("@TimeSlot", slot);

                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            conflictedSlots.Add(slot);
                        }
                    }
                }

                // If there is a duplicate, do not insert.
                if (conflictedSlots.Any())
                {
                    string slotsText = string.Join(", ", conflictedSlots);
                    ShowErrorMessage($"❌ Cannot book. Conflicting time slots: {slotsText} in Room {room}.");
                    return;
                }

                // Insert only after passing the validation.
                foreach (var slot in splitSlots)
                {
                    var times = slot.Split('-');
                    string slotStart = times[0];
                    string slotEnd = times[1];
                    int slotDuration = 30;

                    string insertQuery = @"
            INSERT INTO AD_MeetingRoom_Bk
            (Room, BookingDate, TimeSlot, Department, ReservedBy, StartTime, EndTime, DurationMinutes)
            VALUES (@Room, @BookingDate, @TimeSlot, @Department, @ReservedBy, @StartTime, @EndTime, @DurationMinutes)";

                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, conn))
                    {
                        insertCmd.Parameters.AddWithValue("@Room", room);
                        insertCmd.Parameters.AddWithValue("@BookingDate", date);
                        insertCmd.Parameters.AddWithValue("@TimeSlot", slot);
                        insertCmd.Parameters.AddWithValue("@Department", department);
                        insertCmd.Parameters.AddWithValue("@ReservedBy", reservedBy);
                        insertCmd.Parameters.AddWithValue("@StartTime", slotStart);
                        insertCmd.Parameters.AddWithValue("@EndTime", slotEnd);
                        insertCmd.Parameters.AddWithValue("@DurationMinutes", slotDuration);

                        insertCmd.ExecuteNonQuery();
                    }
                }

                ShowBookingSuccess();
                BindRoomSchedule(Calendar1.SelectedDate);

                // ✅ แสดงข้อมูลการจองล่าสุดด้านล่าง GridView
                lblDate.Text = date.ToString("dd MMM yyyy");
                lblDeptBooked.Text = department;
                lblRoom.Text = room;
                lblStart.Text = startTime;
                lblEnd.Text = endTime;
                pnlBookingResult.Visible = true;

                if (UpdatePanel1 != null)
                {
                    UpdatePanel1.Update();
                }
            }
        }


        private void ShowErrorMessage(string message)
        {
            string script = $@"
Swal.fire({{
  icon: 'error',
  title: '{message}',
  confirmButtonText: 'OK'
}});";
            ScriptManager.RegisterStartupScript(this, this.GetType(),
                Guid.NewGuid().ToString(), script, true);
        }


        private string GetDepartmentByUsername(string username)
        {
            string department = "";
            string connStr = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
            SELECT TOP 1 DeptName
            FROM Users
            WHERE UserName = @UserName AND DeptName IS NOT NULL";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserName", username);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        department = result.ToString();
                    }
                }
            }
            return department;
        }
        protected void Page_PreRender(object sender, EventArgs e)
        {
            GridView1.HeaderRow.TableSection = TableRowSection.TableHeader;
        }
        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                return;
            }

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Attributes["style"] = "background-color: #e0ffe0 !important;";

                string[] rooms = { "101", "102", "103", "201", "202", "203" };
                string currentUser = Session["username"]?.ToString();
                string timeSlot = DataBinder.Eval(e.Row.DataItem, "TimeSlot")?.ToString(); 
                string lang = Session["lang"]?.ToString() ?? "en";
                string freeText = lang == "th" ? "ว่าง" : lang == "zh" ? "閒置" : "Free";
                string bookedFormat = lang == "th" ? "จองโดย {0}" : lang == "zh" ? "已被 {0} 預約" : "Booked by {0}";
                string cancelText = lang == "th" ? "ยกเลิก" : lang == "zh" ? "取消" : "cancel";

                string swalTitle = lang == "th" ? "คุณแน่ใจหรือไม่?" : lang == "zh" ? "您確定要取消嗎？" : "Are you sure?";
                string swalText = lang == "th" ? "การกระทำนี้จะยกเลิกการจอง" : lang == "zh" ? "此操作將取消預約。" : "This will cancel your booking.";
                string swalConfirm = lang == "th" ? "ใช่, ยกเลิกเลย!" : lang == "zh" ? "是的，取消！" : "Yes, cancel it!";
                string swalCancel = lang == "th" ? "ไม่, เก็บไว้" : lang == "zh" ? "不，保留" : "No, keep it";

                for (int i = 1; i <= rooms.Length; i++)
                {
                    TableCell cell = e.Row.Cells[i];
                    string room = rooms[i - 1];
                    string reservedBy = DataBinder.Eval(e.Row.DataItem, room + "_ReservedBy")?.ToString();
                    string department = DataBinder.Eval(e.Row.DataItem, room + "_Department")?.ToString();
                    string status = DataBinder.Eval(e.Row.DataItem, room)?.ToString();

                    if (status == "Booked" && !string.IsNullOrEmpty(reservedBy))
                    {
                        cell.Attributes["style"] = "background-color: #ffcccc !important;";
                        cell.Controls.Clear();

                        Literal lit = new Literal();
                        lit.Text = !string.IsNullOrEmpty(department)
                            ? $"<span style='white-space: nowrap'>{string.Format(bookedFormat, department)}</span>"
                            : freeText;

                        cell.Controls.Add(lit);

                        string currentUserDept = ViewState["userDept"]?.ToString();
                        if (currentUserDept == department && IsEditMode)
                        {
                            LinkButton btn = new LinkButton
                            {
                                Text = cancelText,
                                CommandName = "CancelBooking",
                                CommandArgument = timeSlot + "|" + room,
                                CssClass = "btn btn-sm btn-danger mt-1"
                            };

                            btn.OnClientClick = $@"Swal.fire({{title: '{swalTitle}',text: '{swalText}',icon: 'warning',showCancelButton: 
true,confirmButtonText: '{swalConfirm}',cancelButtonText: '{swalCancel}'}}).then((result) => {{if (result.isConfirmed) {{__doPostBack('{GridView1.UniqueID}','CancelBooking${timeSlot}|{room}');}}
}});
return false;
";
                            cell.Controls.Add(btn);
                        }
                    }
                    else
                    {
                        cell.Controls.Clear();
                        Literal lit = new Literal { Text = freeText };
                        cell.Controls.Add(lit);
                    }
                }
            }
        }


        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "CancelBooking")
            {
                string[] parts = e.CommandArgument.ToString().Split('|');
                if (parts.Length == 2)
                {
                    string slot = parts[0];
                    string room = parts[1];

                    string connStr = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
                    using (SqlConnection conn = new SqlConnection(connStr))
                    {
                        conn.Open();

                        // ✅ ดึง ReservedBy และ Department มาเช็ก
                        string checkSql = @"SELECT ReservedBy, Department FROM AD_MeetingRoom_Bk
                    WHERE BookingDate = @Date
                    AND TimeSlot = @TimeSlot
                    AND Room = @Room";

                        using (SqlCommand checkCmd = new SqlCommand(checkSql, conn))
                        {
                            checkCmd.Parameters.Add("@Date", SqlDbType.Date).Value = Calendar1.SelectedDate.Date;
                            checkCmd.Parameters.Add("@TimeSlot", SqlDbType.VarChar, 20).Value = slot.Trim();
                            checkCmd.Parameters.Add("@Room", SqlDbType.VarChar, 10).Value = room.Trim();

                            using (SqlDataReader reader = checkCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    string reservedByFromDb = reader["ReservedBy"].ToString();
                                    string departmentFromDb = reader["Department"].ToString();
                                    string currentUserDept = ViewState["userDept"]?.ToString();


                                    if (currentUserDept != departmentFromDb)
                                    {
                                        ShowError(ErrorCode.CancelFailed);
                                        return;
                                    }
                                }
                                else
                                {
                                    ShowError(ErrorCode.BookingNotFound);
                                    return;
                                }
                            }
                        }


                        string deleteSql = @"DELETE FROM AD_MeetingRoom_Bk
                     WHERE BookingDate = @Date
                     AND TimeSlot = @TimeSlot
                     AND Room = @Room";

                        using (SqlCommand deleteCmd = new SqlCommand(deleteSql, conn))
                        {
                            deleteCmd.Parameters.Add("@Date", SqlDbType.Date).Value = Calendar1.SelectedDate.Date;
                            deleteCmd.Parameters.Add("@TimeSlot", SqlDbType.VarChar, 20).Value = slot.Trim();
                            deleteCmd.Parameters.Add("@Room", SqlDbType.VarChar, 10).Value = room.Trim();

                            int rows = deleteCmd.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                ShowCancelSuccess();
                                BindRoomSchedule(Calendar1.SelectedDate);

                                if (UpdatePanel1 != null)
                                {
                                    UpdatePanel1.Update();
                                }
                            }
                            else
                            {
                                ShowError(ErrorCode.CancelFailed);
                            }
                        }
                    }
                }
            }
        }
        private void ShowError(ErrorCode code)
        {
            string lang = Session["lang"] as string ?? "en";
            string message;
            switch (code)
            {
                case ErrorCode.CancelFailed:
                    message = lang == "th"
                        ? "❌ ยกเลิกไม่สำเร็จ คุณไม่ใช่เจ้าของการจอง"
                        : lang == "zh"
                            ? "❌ 取消失败。您不是此预订的所有者。"
                            : "❌ Cancel failed. You are not the owner of this booking.";
                    break;

                case ErrorCode.UserNotFound:
                    message = lang == "th"
                        ? "❌ ไม่พบผู้ใช้ในระบบ"
                        : lang == "zh"
                            ? "❌ 系统中未找到用户。"
                            : "❌ User not found in the system.";
                    break;

                case ErrorCode.InvalidTimeFormat:
                    message = lang == "th"
                        ? "❌ รูปแบบเวลาไม่ถูกต้อง"
                        : lang == "zh"
                            ? "❌ 时间格式无效。"
                            : "❌ Invalid start or end time format.";
                    break;

                case ErrorCode.RoomNotSelected:
                    message = lang == "th"
                        ? "❌ โปรดเลือกห้องประชุม"
                        : lang == "zh"
                            ? "❌ 请选择会议室。"
                            : "❌ Please select a room.";
                    break;

                case ErrorCode.EndBeforeStart:
                    message = lang == "th"
                        ? "❌ เวลาสิ้นสุดต้องหลังเวลาเริ่มต้น"
                        : lang == "zh"
                            ? "❌ 结束时间必须在开始时间之后。"
                            : "❌ End time must be after start time.";
                    break;

                default:
                    message = "❌ Unknown error";
                    break;
                case ErrorCode.BookingNotFound:
                    message = lang == "th"
                        ? "❌ ไม่พบข้อมูลการจองนี้ หรืออาจถูกยกเลิกไปแล้ว"
                        : lang == "zh"
                            ? "❌ 找不到此预订或已被取消。"
                            : "❌ This booking does not exist or has already been cancelled.";
                    break;

            }

            string script = $@"
Swal.fire({{
  icon: 'error',
  title: '{message}',
  confirmButtonText: 'OK'
}});";
            ScriptManager.RegisterStartupScript(this, this.GetType(),
                Guid.NewGuid().ToString(), script, true);
        }
        protected bool IsEditMode
        {
            get
            {
                return ViewState["IsEditMode"] != null && (bool)ViewState["IsEditMode"];
            }
            set
            {
                ViewState["IsEditMode"] = value;
            }
        }

        private void ShowBookingSuccess()
        {
            string script = @"
Swal.fire({
  icon: 'success',
  title: '✅ Booking successfully added!',
  confirmButtonText: 'OK'
});";
            ScriptManager.RegisterStartupScript(this, this.GetType(),
                Guid.NewGuid().ToString(), script, true);
        }

        private void ShowCancelSuccess()
        {
            string script = @"
Swal.fire({
  icon: 'success',
  title: '✅ Cancel successfully!',
  confirmButtonText: 'OK'
});";
            ScriptManager.RegisterStartupScript(this, this.GetType(),
                Guid.NewGuid().ToString(), script, true);
        }

        protected void btnEditMode_Click(object sender, EventArgs e)
        {
            IsEditMode = true;
            btnEditMode.CssClass = "d-none";
            btnExitEditMode.CssClass = "btn btn-outline-primary px-4 py-2 fw-bold equal-width-btn";

            BindRoomSchedule(Calendar1.SelectedDate);
        }

        protected void btnExitEditMode_Click(object sender, EventArgs e)
        {
            IsEditMode = false;
            btnEditMode.CssClass = "btn btn-outline-primary px-4 py-2 fw-bold equal-width-btn";
            btnExitEditMode.CssClass = "d-none";
            BindRoomSchedule(Calendar1.SelectedDate);
        }

        protected void Calendar1_DayRender(object sender, DayRenderEventArgs e)
        {
            if (e.Day.Date == DateTime.Today)
            {
                e.Cell.BackColor = System.Drawing.ColorTranslator.FromHtml("#0d6efd");
                e.Cell.ForeColor = System.Drawing.Color.White;
                e.Cell.Font.Bold = true;
            }
        }
        private string GetLabel(string key)
        {
            string lang = Session["lang"]?.ToString() ?? "en";

            Dictionary<string, string> en = new Dictionary<string, string>
            {
                ["title"] = "Meeting Room Booking",
                ["bookroom"] = "Book Room",
                ["edit"] = "Edit",
                ["exit"] = "Exit Edit Mode",
                ["export"] = "📤 Export as Image",
                ["summary"] = "✨ Booking Summary ✨",
                ["department"] = "Department",
                ["room"] = "Room",
                ["date"] = "Date",
                ["start"] = "Start Time",
                ["end"] = "End Time",
                ["actions"] = "Actions"
            };

            Dictionary<string, string> th = new Dictionary<string, string>
            {
                ["title"] = "การจองห้องประชุม",
                ["bookroom"] = "จองห้อง",
                ["edit"] = "แก้ไข",
                ["exit"] = "ออกจากโหมดแก้ไข",
                ["export"] = "📤 บันทึกเป็นภาพ",
                ["summary"] = "✨ สรุปการจอง ✨",
                ["department"] = "แผนก",
                ["room"] = "ห้อง",
                ["date"] = "วันที่",
                ["start"] = "เวลาเริ่ม",
                ["end"] = "เวลาสิ้นสุด",
                ["actions"] = "การดำเนินการ"
            };
            Dictionary<string, string> zh = new Dictionary<string, string>
            {
                ["title"] = "會議室預約",
                ["bookroom"] = "預約會議室",
                ["edit"] = "編輯",
                ["exit"] = "離開編輯模式",
                ["export"] = "匯出圖片",
                ["summary"] = "✨ 預約摘要 ✨",
                ["department"] = "部門",
                ["room"] = "會議室",
                ["date"] = "日期",
                ["start"] = "開始時間",
                ["end"] = "結束時間",
                ["actions"] = "操作"
            };

            Dictionary<string, string> dict;
            if (lang == "zh")
            {
                dict = zh;
            }
            else if (lang == "th")
            {
                dict = th;
            }
            else
            {
                dict = en;
            }

            return dict.ContainsKey(key) ? dict[key] : key;
        }
    }

}


