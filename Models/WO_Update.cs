using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace IT_WorkPlant.Models
{
    public class WO_UpdateService
    {
        private readonly OracleDatabaseHelper _dbHelper;

        public WO_UpdateService(string connectionString)
        {
            _dbHelper = new OracleDatabaseHelper(connectionString);
        }

        public void UpdateDatabase(DataRow row)
        {
            // 從 DataRow 中提取值
            string sfa01 = row[0]?.ToString();
            string sfa03A = row[2]?.ToString();
            string sfa03B = row[1]?.ToString();

            // 更新的 SQL 查詢
            string updateSql = "UPDATE DS5.sfa_file SET sfa03=:sfa03A WHERE sfa01=:sfa01 AND sfa03=:sfa03B";

            // 定義參數
            var parameters = new[]
            {
                new OracleParameter("sfa03A", sfa03A),
                new OracleParameter("sfa01", sfa01),
                new OracleParameter("sfa03B", sfa03B)
            };

            try
            {
                // 執行更新操作
                int rowsUpdated = _dbHelper.ExecuteNonQuery(updateSql, parameters);

                // 更新該列的狀態
                row["Status"] = rowsUpdated > 0 ? "Success" : "No Match Found";
            }
            catch (Exception ex)
            {
                // 如果發生例外，記錄失敗狀態和錯誤訊息
                row["Status"] = "Failed";
                row["Error"] = ex.Message; // 可選，記錄詳細錯誤訊息
            }
        }

    }
}
