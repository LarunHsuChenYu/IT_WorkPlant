using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Configuration;

namespace IT_WorkPlant.Models
{
    public class MssqlDatabaseHelper
    {
        private readonly string _connectionString;

        public MssqlDatabaseHelper()
        {
            _connectionString = WebConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
        }

        public DataTable ExecuteQuery(string query, SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    conn.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable resultTable = new DataTable();
                        adapter.Fill(resultTable);
                        return resultTable;
                    }
                }
            }
        }

        public int ExecuteNonQuery(string query, SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    conn.Open();
                    return cmd.ExecuteNonQuery(); 
                }
            }
        }

        public object ExecuteScalar(string query, SqlParameter[] parameters)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    conn.Open();
                    return cmd.ExecuteScalar(); // 返回第一行第一列的值
                }
            }
        }

        public DataTable ExecuteDynamicQuery(string baseQuery, Dictionary<string, object> parameters)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(baseQuery, conn))
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }

                    conn.Open();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable resultTable = new DataTable();
                        adapter.Fill(resultTable);
                        return resultTable;
                    }
                }
            }
        }

        public int InsertData(string tableName, Dictionary<string, object> columnValues)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 動態構建 INSERT 語句
                string columns = string.Join(", ", columnValues.Keys);
                string parameters = string.Join(", ", columnValues.Keys.Select(key => "@" + key));
                string query = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    foreach (var columnValue in columnValues)
                    {
                        cmd.Parameters.AddWithValue("@" + columnValue.Key, columnValue.Value ?? DBNull.Value);
                    }

                    conn.Open();
                    return cmd.ExecuteNonQuery(); // 返回受影響的行數
                }
            }
        }
    }
}
