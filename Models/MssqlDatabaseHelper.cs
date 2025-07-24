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
        private SqlTransaction _currentTransaction;
        private SqlConnection _currentConnection;

        public MssqlDatabaseHelper()
        {
            _connectionString = WebConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
        }

        public SqlTransaction BeginTransaction()
        {
            if (_currentTransaction != null)
                throw new InvalidOperationException("Transaction already exists");

            _currentConnection = new SqlConnection(_connectionString);
            _currentConnection.Open();
            _currentTransaction = _currentConnection.BeginTransaction();
            return _currentTransaction;
        }

        public void CommitTransaction()
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("No active transaction");

            try
            {
                _currentTransaction.Commit();
            }
            finally
            {
                CleanupTransaction();
            }
        }

        public void RollbackTransaction()
        {
            if (_currentTransaction == null)
                throw new InvalidOperationException("No active transaction");

            try
            {
                if (_currentTransaction.Connection != null)
                {
                    _currentTransaction.Rollback();
                }
            }
            finally
            {
                CleanupTransaction();
            }
        }
        private void CleanupTransaction()
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }

            if (_currentConnection != null)
            {
                _currentConnection.Dispose();
                _currentConnection = null;
            }
        }

        public DataTable ExecuteQuery(string sql, SqlParameter[] parameters, SqlTransaction tran = null)
        {
            SqlConnection conn;
            bool selfOpened = false;

            if (tran != null && tran.Connection != null)
            {
                conn = tran.Connection;
            }
            else
            {
                conn = new SqlConnection(_connectionString);
                conn.Open();
                selfOpened = true;
            }

            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);

                    using (SqlDataAdapter adp = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adp.Fill(dt);
                        return dt;
                    }
                }
            }
            finally
            {
                if (selfOpened && conn != null)
                    conn.Dispose();
            }
        }


        public int ExecuteNonQuery(string query, SqlParameter[] parameters, SqlTransaction transaction = null)
        {
            using (SqlConnection conn = transaction?.Connection ?? new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (transaction != null)
                        cmd.Transaction = transaction;

                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    if (transaction == null)
                        conn.Open();

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public object ExecuteScalar(string query, SqlParameter[] parameters, SqlTransaction transaction = null)
        {
            using (SqlConnection conn = transaction?.Connection ?? new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (transaction != null)
                        cmd.Transaction = transaction;

                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    if (transaction == null)
                        conn.Open();

                    return cmd.ExecuteScalar();
                }
            }
        }

        public DataTable ExecuteDynamicQuery(string baseQuery, Dictionary<string, object> parameters, SqlTransaction transaction = null)
        {
            using (SqlConnection conn = transaction?.Connection ?? new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(baseQuery, conn))
                {
                    if (transaction != null)
                        cmd.Transaction = transaction;

                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                    }

                    if (transaction == null)
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

        public int InsertDataReturnId(string tableName, Dictionary<string, object> columnValues, SqlTransaction transaction = null)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction), "Transaction is required for InsertDataReturnId");
            }

            string columns = string.Join(", ", columnValues.Keys);
            string parameters = string.Join(", ", columnValues.Keys.Select(key => "@" + key));
            
            // Determine the primary key column name based on the table
            string pkColumn;
            switch (tableName)
            {
                case "IT_RequestList":
                    pkColumn = "ReportID";
                    break;
                case "WF_Forms":
                    pkColumn = "FormID";
                    break;
                case "WF_FormStatusLog":
                case "WF_RequestApproveLog":
                    pkColumn = "LogID";
                    break;
                default:
                    throw new ArgumentException($"Unknown table name: {tableName}");
            }

            string query = $@"
                INSERT INTO {tableName} ({columns}) 
                OUTPUT INSERTED.{pkColumn} 
                VALUES ({parameters})";

            using (SqlCommand cmd = new SqlCommand(query, transaction.Connection, transaction))
            {
                foreach (var columnValue in columnValues)
                {
                    cmd.Parameters.AddWithValue("@" + columnValue.Key, columnValue.Value ?? DBNull.Value);
                }

                return (int)cmd.ExecuteScalar();
            }
        }

        public int InsertData(string tableName, Dictionary<string, object> columnValues, SqlTransaction transaction = null)
        {
            using (SqlConnection conn = transaction?.Connection ?? new SqlConnection(_connectionString))
            {
                string columns = string.Join(", ", columnValues.Keys);
                string parameters = string.Join(", ", columnValues.Keys.Select(key => "@" + key));
                string query = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (transaction != null)
                        cmd.Transaction = transaction;

                    foreach (var columnValue in columnValues)
                    {
                        cmd.Parameters.AddWithValue("@" + columnValue.Key, columnValue.Value ?? DBNull.Value);
                    }

                    if (transaction == null)
                        conn.Open();

                    return cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
