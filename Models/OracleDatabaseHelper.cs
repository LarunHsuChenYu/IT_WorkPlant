using System;
using System.Data;
using Oracle.ManagedDataAccess.Client;

namespace IT_WorkPlant.Models
{
    public class OracleDatabaseHelper
    {
        private readonly string _connectionString;

        public OracleDatabaseHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DataTable ExecuteQuery(string query, OracleParameter[] parameters = null)
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                using (var cmd = new OracleCommand(query, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    conn.Open();
                    using (var adapter = new OracleDataAdapter(cmd))
                    {
                        DataTable resultTable = new DataTable();
                        try
                        {
                            adapter.Fill(resultTable);
                        }
                        catch (NotSupportedException ex) when (ex.Message.Contains("SafeMapping"))
                        {
                            // 如果 SafeMapping 出現問題，使用替代方法
                            using (var reader = cmd.ExecuteReader())
                            {
                                resultTable.Load(reader);
                            }
                        }
                        return resultTable;
                    }
                }
            }
        }

        public int ExecuteNonQuery(string query, OracleParameter[] parameters)
        {
            using (var conn = new OracleConnection(_connectionString))
            {
                using (var cmd = new OracleCommand(query, conn))
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
    }
}
