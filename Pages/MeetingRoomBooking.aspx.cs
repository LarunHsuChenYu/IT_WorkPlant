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


namespace IT_WorkPlant.Pages
{
    public partial class MeetingRoomBooking : Page
    {
        private string excelFilePath;
        

        protected void Page_Load(object sender, EventArgs e)
        {

            //if (Session["UserEmpID"] == null)
            //{
            //    Response.Redirect("../Login.aspx");
            //}

            excelFilePath = Server.MapPath("~/App_Data/MeetingRoomBookings.xlsx");
            if (!IsPostBack)
            {
                UpdateStartTimeList();
                UpdateDurationList();
                UpdateDepartmentList();

                Calendar1.SelectedDate = DateTime.Today;
                Calendar1.VisibleDate = DateTime.Today;
                
                BindRoomSchedule(Calendar1.SelectedDate);
            }
        }

        protected void Calendar1_SelectionChanged(object sender, EventArgs e)
        {
            DateTime selectedDate = Calendar1.SelectedDate;
            string room = roomList.SelectedValue;
            
            BindRoomSchedule(selectedDate);
        }

        private string GetExcelConnectionString()
        {
            return $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={excelFilePath};Extended Properties=""Excel 12.0 Xml;HDR=YES;""";
        }

        private void BindRoomSchedule(DateTime dtCheckDate)
        {
            // 取得今天所有會議室的預訂資料
            DataTable roomSchedule = GetRoomSchedule(dtCheckDate);

            // 繫結資料到 GridView
            GridView1.DataSource = roomSchedule;
            GridView1.DataBind();
            
        }

        private DataTable GetRoomSchedule(DateTime date)
        {
            // 取得 Excel 中的預訂資料
            DataTable excelData = GetBookingDataFromExcel(date);

            // 建立顯示的資料表，包含所有時段與房間
            DataTable dt = new DataTable();
            dt.Columns.Add("TimeSlot");
            dt.Columns.Add("101");
            dt.Columns.Add("102");
            dt.Columns.Add("103");
            dt.Columns.Add("201");
            dt.Columns.Add("202");
            dt.Columns.Add("203");

            // 定義預設時段
            List<string> timeSlots = GetTimeSlotsFromDatabase();
            // 先填充所有時段和可預訂狀態
            foreach (string timeSlot in timeSlots)
            {
                DataRow dr = dt.NewRow();
                dr["TimeSlot"] = timeSlot;
                dr["101"] = "Free";
                dr["102"] = "Free";
                dr["103"] = "Free";
                dr["201"] = "Free";
                dr["202"] = "Free";
                dr["203"] = "Free";
                dt.Rows.Add(dr);
            }

            // 如果 Excel 資料不為空，則覆蓋對應時段與房間的預訂狀態
            if (excelData != null && excelData.Rows.Count > 0)
            {
                foreach (DataRow excelRow in excelData.Rows)
                {
                    String timeSlot = excelRow["TimeSlot"].ToString();
                    String sRoom = excelRow["Room"].ToString();
                    // 找到對應的時段行
                    DataRow[] rows = dt.Select($"TimeSlot = '{timeSlot}'");
                    
                    if (rows.Length > 0)
                    {
                        DataRow dr = rows[0];

                        // 根據 Excel 資料覆蓋預設的 "Free" 狀態
                        dr[sRoom] = $"Booked{Environment.NewLine}by {excelRow["Department"]}";
                    }
                }
            }

            return dt;
        }

        private DataTable GetBookingDataFromExcel(DateTime date)
        {
            DataTable dt = new DataTable();

            if (File.Exists(excelFilePath))
            {
                using (var package = new ExcelPackage(new FileInfo(excelFilePath)))
                {
                    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                    var worksheet = package.Workbook.Worksheets[0];
                    bool hasHeader = true;
                    int dateColumnIndex = -1;

                    // 建立 DataTable 的欄位
                    foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                    {
                        dt.Columns.Add(hasHeader ? firstRowCell.Text : $"Column {firstRowCell.Start.Column}");

                        // 假設日期欄位的名稱為 "Date"
                        if (firstRowCell.Text == "Date")
                        {
                            dateColumnIndex = firstRowCell.Start.Column;
                        }
                    }

                    if (dateColumnIndex == -1)
                    {
                        throw new Exception("找不到名為 'Date' 的欄位");
                    }

                    // 從第2列開始讀取資料（假設第1列是標題）
                    var startRow = hasHeader ? 2 : 1;
                    for (var rowNum = startRow; rowNum <= worksheet.Dimension.End.Row; rowNum++)
                    {
                        var wsRow = worksheet.Cells[rowNum, 1, rowNum, worksheet.Dimension.End.Column];

                        // 取得當前列的日期值
                        var dateCell = worksheet.Cells[rowNum, dateColumnIndex];
                        if (DateTime.TryParse(dateCell.Text, out DateTime rowDate))
                        {
                            // 假設傳入的篩選條件為 "date"
                            if (rowDate.Date == date.Date)
                            {
                                DataRow row = dt.NewRow();
                                foreach (var cell in wsRow)
                                {
                                    row[cell.Start.Column - 1] = cell.Text;
                                }
                                dt.Rows.Add(row);
                            }
                        }
                    }
                }
            }

            return dt;
        }

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                for(int i = 1; i < e.Row.Cells.Count; i++) // 從第二欄開始檢查（第一欄為時間）
                {
                    TableCell cell = e.Row.Cells[i];
                    if (cell.Text.Contains("Booked"))
                    {
                        cell.BackColor = Color.LightPink; // 或其他您想要的顏色
                    }
                }
            }
        }

        private List<string> GetTimeSlotsFromDatabase()
        {
            // 模擬從資料庫中查詢的結果，實際應用中替換成查詢邏輯
            return new List<string> { 
                "08:00-08:30","08:30-09:00","09:00-09:30","09:30-10:00","10:00-10:30",
                "10:30-11:00","11:00-11:30","11:30-12:00","13:00-13:30","13:30-14:00",
                "14:00-14:30","14:30-15:00","15:00-15:30","15:30-16:00","16:00-16:30",
                "16:30-17:00"};
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
            // 清空現有選項
            startTimeList.Items.Clear();

            // 設置開始時間
            DateTime startTime = DateTime.Today.AddHours(8); // 8:00 AM
            DateTime endTime = DateTime.Today.AddHours(16).AddMinutes(30);  // 5:00 PM

            // 動態添加選項到 DropDownList，每 30 分鐘一個時間段
            while (startTime <= endTime)
            {
                string timeDisplay = startTime.ToString("HH:mm"); // 例如 "8:00 AM"
                startTimeList.Items.Add(new ListItem(timeDisplay, timeDisplay));
                startTime = startTime.AddMinutes(30); // 增加 30 分鐘
            }

            // 設定第一個選項為預設選擇
            startTimeList.SelectedIndex = 0;
        }

        // 填充會議時數選單 (0.5 到 4 小時)
        private void UpdateDurationList()
        {
            // 清空現有選項
            durationList.Items.Clear();

            // 定義會議時長選項 (0.5 小時, 1 小時, 2 小時, 3 小時, 4 小時)
            List<string> durations = new List<string>
            {
                "0.5", "1", "1.5", "2", "3", "3.5", "4"
            };

            // 動態添加選項到 DropDownList
            foreach (string duration in durations)
            {
                durationList.Items.Add(new ListItem(duration + " Hours", duration));
            }

            // 設定第一個選項為預設選擇
            durationList.SelectedIndex = 0;
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
            // Gather input data
            DateTime selectedDate = Calendar1.SelectedDate;
            string selectedRoom = roomList.SelectedValue;
            string selectedStartTime = startTimeList.SelectedValue;
            Double selectedDurationTime;
            string selectedDepartment = departmentList.SelectedValue;
            string reservedBy = tbUser.Text;  // This could be dynamic, based on logged-in user.

            
            if (string.IsNullOrEmpty(selectedRoom) )
            {
                // Handle error: room not selected
                ShowError("Please select a room.");
                return;
            }

            if (string.IsNullOrEmpty(selectedStartTime))
            {
                // Handle error: start time not selected
                ShowError("Please select a start time.");
                return;
            }

            if (!double.TryParse(durationList.SelectedValue, out selectedDurationTime) || selectedDurationTime <= 0)
            {
                // Handle error: duration not valid
                ShowError("Please enter a valid duration.");
                return;
            }

            if (string.IsNullOrEmpty(selectedDepartment))
            {
                // Handle error: department not selected
                ShowError("Please select a department.");
                return;
            }

            if (string.IsNullOrEmpty(reservedBy))
            {
                // Handle error: user not entered
                ShowError("Please enter your name.");
                return;
            }

            double.TryParse(durationList.SelectedValue, out selectedDurationTime);
            // Insert new booking into Excel
            InsertBooking(selectedDate, selectedRoom, selectedStartTime, selectedDurationTime, selectedDepartment, reservedBy);
        }

        // Method to show error message
        private void ShowError(string message)
        {
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", $"alert('{message}');", true);
        }

        private bool IsRoomBooked(DateTime date, string room, string timeSlot)
        {
            // 取得 Excel 中對應日期的資料
            DataTable dt = GetBookingDataFromExcel(date);

            // 使用 LINQ 過濾特定房間和時間段的資料
            var isBooked = dt.AsEnumerable()
                .Any(row => row.Field<string>("Room") == room && row.Field<string>("TimeSlot") == timeSlot);

            return isBooked;
        }

        // New Method: Insert booking data into the Excel file

        protected void InsertBooking(DateTime date, string room, string startTime, 
            double duration, string department, string reservedBy)
        {
            // 生成對應的時間段列表
            List<string> timeSlots = GetTimeSlotsForBooking(startTime, duration);

            // 檢查是否所有時間段都可以預訂
            foreach (var timeSlot in timeSlots)
            {
                if (IsRoomBooked(date, room, timeSlot))
                {
                    ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", $"alert('Room {room} is already booked for time slot {timeSlot}.');", true);
                    return;
                }
            }

            // 插入每個時間段的預訂
            foreach (var timeSlot in timeSlots)
            {
                InsertTimeSlotIntoExcel(date, room, timeSlot, department, reservedBy);
            }

            // 成功訊息
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('Booking successfully added!');window.location.href = '/Pages/MeetingRoomBooking';", true);
        }

        // 生成會議的所有時間段
        private List<string> GetTimeSlotsForBooking(string startTime, double duration)
        {
            List<string> allTimeSlots = GetTimeSlotsFromDatabase();
            List<string> bookedSlots = new List<string>();

            DateTime start = DateTime.Parse(startTime);
            for (double i = 0; i < duration; i += 0.5)
            {
                string slot = $"{start.ToString("HH:mm")}-{start.AddMinutes(30).ToString("HH:mm")}";
                if (allTimeSlots.Contains(slot))
                {
                    bookedSlots.Add(slot);
                }
                start = start.AddMinutes(30);
            }

            return bookedSlots;
        }

        // 插入時間段到 Excel
        private void InsertTimeSlotIntoExcel(DateTime date, string room, string timeSlot, string department, string reservedBy)
        {
            string filePath = Server.MapPath("~/App_Data/MeetingRoomBookings.xlsx");
            FileInfo fileInfo = new FileInfo(filePath);

            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets["MeetingRoomBooking"];
                int newRow = worksheet.Dimension.End.Row + 1;

                worksheet.Cells[newRow, 1].Value = room;                // Room
                worksheet.Cells[newRow, 2].Value = date.ToString("yyyy/MM/dd");  // Date
                worksheet.Cells[newRow, 3].Value = timeSlot;            // TimeSlot
                worksheet.Cells[newRow, 4].Value = department;          // Department
                worksheet.Cells[newRow, 5].Value = reservedBy;          // ReservedBy

                package.Save();
            }
        }

    }
}