using System;
using System.Collections.Generic;
using System.Configuration; // ✅ Connection String form Web.config
using System.Data;
using System.Data.SqlClient;

namespace IT_WorkPlant.Models
{
    public class IT_RequestModel
    {
        private readonly MssqlDatabaseHelper _dbHelper;

        public IT_RequestModel()
        {
            _dbHelper = new MssqlDatabaseHelper();
        }

        // ✅ Add a function to retrieve all department data from the database // เพิ่มฟังก์ชันดึงข้อมูลแผนกทั้งหมดจากฐานข้อมูล
        public DataTable GetDepartments()
        {
            // ✅ Get the connection string from Web.config // ดึง Connection String จาก Web.config
            string connectionString = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;

            // ✅ SQL query to retrieve all department data // SQL Query ดึงข้อมูลแผนกทั้งหมด 
            string query = "SELECT DISTINCT DeptNameID, DeptName_en FROM Departments ORDER BY DeptName_en";

            // ✅ Create a DataTable to store the results // สร้าง DataTable สำหรับเก็บผลลัพธ์ 
            DataTable dt = new DataTable();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                // ✅ Debug log to check which departments are available // Debug Log เพื่อตรวจสอบว่ามีแผนกอะไรบ้าง =
                System.Diagnostics.Debug.WriteLine("===== Department List from DB =====");
                if (dt.Rows.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠ No department data found in the database!");
                }
                else
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        System.Diagnostics.Debug.WriteLine($"{row["DeptNameID"]} - {row["DeptName_en"]}");
                    }
                }
                System.Diagnostics.Debug.WriteLine("==================================");
            }
            catch (Exception ex)
            {
                // ✅ Log an error if there is a problem // Log Error ถ้ามีปัญหา
                System.Diagnostics.Debug.WriteLine($"❌ Error ในการดึงข้อมูลแผนก: {ex.Message}");
            }

            return dt;
        }

        // 獲取所有請求資料
        public DataTable GetAllRequests()
        {
            string query = @"
                SELECT 
                    r.ReportID, 
                    r.IssueDate, 
                    d.DeptName_en AS Department, 
                    u.UserName AS RequestUser, 
                    r.IssueDetails, 
                    it.IssueTypeCode AS IssueType, 
                    r.DRI_UserID, 
                    r.IssueTypeID,
                    (SELECT UserName FROM Users WHERE UserIndex = r.DRI_UserID) AS DRI_UserName,
                    r.Solution, 
                    r.Status AS StatusValue,  
                    CASE 
                        WHEN r.Status = 1 THEN 'Done'
                        ELSE 'WIP'
                    END AS Status, 
                    r.LastUpdateDate, 
                    r.FinishedDate, 
                    r.Remark
                FROM IT_RequestList r
                LEFT JOIN Departments d ON r.DeptNameID = d.DeptNameID
                LEFT JOIN Users u ON r.RequestUserID = u.UserIndex
                LEFT JOIN IssueType it ON r.IssueTypeID = it.IssueTypeID";

            return _dbHelper.ExecuteQuery(query, null);
        }

        // 根據條件篩選請求資料
        public DataTable GetFilteredRequests(string deptName, string requestUser, string issueType, string status, string issueMonth, string issueDate)
        {
            string query = @"
        SELECT 
            r.ReportID, 
            r.IssueDate, 
            r.DeptNameID,                              -- <<== ✅ เพิ่มบรรทัดนี้เข้าไป
            d.DeptName_en AS Department, 
            u.UserName AS RequestUser, 
            r.IssueDetails, 
            it.IssueTypeCode AS IssueType, 
            r.IssueTypeID,
            r.DRI_UserID, 
            (SELECT UserName FROM Users WHERE UserIndex = r.DRI_UserID) AS DRI_UserName,
            r.Solution, 
            r.Status AS StatusValue,  
            CASE 
                WHEN r.Status = 1 THEN 'Done'
                ELSE 'WIP'
            END AS Status, 
            r.LastUpdateDate, 
            r.FinishedDate, 
            r.Remark,
            r.ImagePath
        FROM IT_RequestList r
        LEFT JOIN Departments d ON r.DeptNameID = d.DeptNameID
        LEFT JOIN Users u ON r.RequestUserID = u.UserIndex
        LEFT JOIN IssueType it ON r.IssueTypeID = it.IssueTypeID
        WHERE 1 = 1";


            var parameters = new List<SqlParameter>();

            // ✅ Apply a filter condition `IssueMonth`
            if (!string.IsNullOrEmpty(issueMonth))
            {
                query += " AND FORMAT(r.IssueDate, 'yyyy-MM') = @IssueMonth";
                parameters.Add(new SqlParameter("@IssueMonth", issueMonth));
            }

            // ✅ Apply a filter condition `IssueDate`
            if (!string.IsNullOrEmpty(issueDate))
            {
                query += " AND CONVERT(DATE, r.IssueDate) = @IssueDate";
                parameters.Add(new SqlParameter("@IssueDate", issueDate));
            }

            if (!string.IsNullOrEmpty(deptName))
            {
                query += " AND r.DeptNameID = @DeptNameID";
                parameters.Add(new SqlParameter("@DeptNameID", deptName));
            }

            if (!string.IsNullOrEmpty(requestUser))
            {
                query += " AND u.UserName = @RequestUser";
                parameters.Add(new SqlParameter("@RequestUser", requestUser));
            }

            if (!string.IsNullOrEmpty(issueType))
            {
                query += " AND it.IssueTypeCode = @IssueType";
                parameters.Add(new SqlParameter("@IssueType", issueType));
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Today")
                {
                    query += " AND CONVERT(DATE, r.FinishedDate) = @Today";
                    parameters.Add(new SqlParameter("@Today", DateTime.Today));
                }
                else
                {
                    query += " AND r.Status = @Status";
                    parameters.Add(new SqlParameter("@Status", status == "Done" ? 1 : 0));
                }
            }

            // ✅ Add an ORDER BY clause to sort ReportID in ascending order // เพิ่มคำสั่ง ORDER BY ให้เรียง ReportID จากน้อยไปมาก 
            query += " ORDER BY r.ReportID ASC";

            return _dbHelper.ExecuteQuery(query, parameters.ToArray());
        }

        // 獲取篩選條件選項
        public DataTable GetFilterOptions(string columnName)
        {
            string query;
            switch (columnName)
            {
                case "Department":
                    query = "SELECT DISTINCT DeptName_en AS Value FROM Departments";
                    break;
                case "RequestUser":
                    query = "SELECT DISTINCT UserName AS Value FROM Users";
                    break;
                case "IssueType":
                    query = "SELECT DISTINCT IssueTypeCode AS Value FROM IssueType";
                    break;
                case "Status":
                    query = "SELECT DISTINCT CASE WHEN Status = 1 THEN 'Done' ELSE 'WIP' END AS Value FROM IT_RequestList";
                    break;
                default:
                    query = $"SELECT DISTINCT {columnName} AS Value FROM IT_RequestList";
                    break;
            }

            return _dbHelper.ExecuteQuery(query, null);
        }

        public void UpdateRequest(string query, SqlParameter[] parameters)
        {
            _dbHelper.ExecuteNonQuery(query, parameters);
        }

        public DataTable GetUsersByDepartment(string deptName)
        {
            string query = @"
                SELECT UserIndex, UserName
                FROM Users
                WHERE DeptName = @DeptName";

            SqlParameter[] parameters = { new SqlParameter("@DeptName", deptName) };
            return _dbHelper.ExecuteQuery(query, parameters);
        }

        public DataTable GetAllIssueTypes()
        {
            string query = "SELECT IssueTypeID, IssueTypeCode FROM IssueType ORDER BY IssueTypeID";
            return _dbHelper.ExecuteQuery(query, null);
        }

    }
}
