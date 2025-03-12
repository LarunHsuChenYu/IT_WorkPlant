using System.Collections.Generic;
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
        public DataTable GetFilteredRequests(string deptName, string requestUser, string issueType, string status, string issueMonth)
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
        LEFT JOIN IssueType it ON r.IssueTypeID = it.IssueTypeID
        WHERE 1 = 1";

            var parameters = new List<SqlParameter>();

            // เพิ่มเงื่อนไขกรองเดือน
            if (!string.IsNullOrEmpty(issueMonth))
            {
                query += " AND FORMAT(r.IssueDate, 'yyyy-MM') = @IssueMonth";
                parameters.Add(new SqlParameter("@IssueMonth", issueMonth));
            }

            if (!string.IsNullOrEmpty(deptName))
            {
                query += " AND d.DeptName_en = @DeptName";
                parameters.Add(new SqlParameter("@DeptName", deptName));
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
                query += " AND r.Status = @Status";
                parameters.Add(new SqlParameter("@Status", status == "Done" ? 1 : 0));
            }
            // ✅ เพิ่มคำสั่ง ORDER BY ให้เรียง ReportID จากน้อยไปมาก
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

    }
}

