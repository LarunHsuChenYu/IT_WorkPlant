using System;
using System.Data;
using System.Data.SqlClient;

namespace IT_WorkPlant.Models
{
    public class UserInfo
    {
        public string UserName { get; set; }
        public string UserEmpMail { get; set; }
        public string DeptName { get; set; }

        private readonly MssqlDatabaseHelper _dbHelper = new MssqlDatabaseHelper();
        public int? GetRequestUserID(string userName, string userEmpID)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userEmpID))
            {
                return null; // 如果任何一個輸入為空，返回 null
            }

            string query = "SELECT UserIndex FROM Users WHERE UserName = @UserName AND UserEmpID = @UserEmpID";
            SqlParameter[] parameters =
            {
                new SqlParameter("@UserName", userName),
                new SqlParameter("@UserEmpID", userEmpID)
            };

            object result = _dbHelper.ExecuteScalar(query, parameters);

            if (result == null)
            {
                return null; // 如果找不到對應的結果，返回 null
            }

            return Convert.ToInt32(result);
        }
        public UserInfo AuthenticateUser(string userId, string password)
        {
            string query = "SELECT UserName, UserEmpMail, DeptName FROM Users WHERE UserEmpID = @UserID AND UserEmpPW = @Password";

            SqlParameter[] parameters =
            {
                new SqlParameter("@UserID", SqlDbType.NVarChar) { Value = userId },
                new SqlParameter("@Password", SqlDbType.NVarChar) { Value = password }
            };

            DataTable resultTable = _dbHelper.ExecuteQuery(query, parameters);

            if (resultTable.Rows.Count > 0)
            {
                DataRow row = resultTable.Rows[0];
                return new UserInfo
                {
                    UserName = row["UserName"].ToString(),
                    UserEmpMail = row["UserEmpMail"].ToString(),
                    DeptName = row["DeptName"].ToString()
                };
            }

            return null;
        }
    }

}