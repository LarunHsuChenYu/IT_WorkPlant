using System;
using System.Data;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;
using System.Linq; // Added for .Where() and .Any()

namespace IT_WorkPlant.Models
{
    public class OPD_vwModelWorkOrder
    {
        private readonly OracleDatabaseHelper _dbHelper;

        public OPD_vwModelWorkOrder()
        {
            _dbHelper = new OracleDatabaseHelper(ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString);
        }

        // 1. 所有PO對應WO的完成度(含摘要分層、分群分布、TopN、權重排序)
        public DataTable GetPOCompletionList(DateTime startDate, DateTime endDate, int? topN = null, decimal? maxRate = null)
        {
            var sql = new System.Text.StringBuilder(@"
WITH po AS (
  SELECT sfb22 AS PO_NO,
         COUNT(sfb01) AS TOTAL_WO,
         SUM(TO_NUMBER(sfb08)) AS PLAN_QTY,
         SUM(TO_NUMBER(sfb09)) AS FINISH_QTY,
         ROUND(SUM(TO_NUMBER(sfb09))/NULLIF(SUM(TO_NUMBER(sfb08)),0)*100,2) AS COMPLETION_RATE,
         CASE
           WHEN ROUND(SUM(TO_NUMBER(sfb09))/NULLIF(SUM(TO_NUMBER(sfb08)),0)*100,2) < 50  THEN '0-50%'
           WHEN ROUND(SUM(TO_NUMBER(sfb09))/NULLIF(SUM(TO_NUMBER(sfb08)),0)*100,2) < 80  THEN '50-80%'
           WHEN ROUND(SUM(TO_NUMBER(sfb09))/NULLIF(SUM(TO_NUMBER(sfb08)),0)*100,2) < 95  THEN '80-95%'
           ELSE '95-100%'
         END AS BUCKET
  FROM   DS5.sfb_file
  WHERE  sfb22 IS NOT NULL
    AND  sfb81 BETWEEN TO_DATE(:start_date, 'YYYY-MM-DD') AND TO_DATE(:end_date, 'YYYY-MM-DD')
  GROUP BY sfb22
)
SELECT * FROM po WHERE 1=1
");
            var p = new List<Oracle.ManagedDataAccess.Client.OracleParameter>
            {
                new Oracle.ManagedDataAccess.Client.OracleParameter("start_date", startDate.ToString("yyyy-MM-dd")),
                new Oracle.ManagedDataAccess.Client.OracleParameter("end_date", endDate.ToString("yyyy-MM-dd"))
            };
            if (maxRate.HasValue)
            {
                sql.Append(" AND COMPLETION_RATE < :maxRate");
                p.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("maxRate", maxRate));
            }
            sql.Append(" ORDER BY PLAN_QTY DESC ");
            if (topN.HasValue)
            {
                sql.Append(" FETCH FIRST :topN ROWS ONLY ");
                p.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("topN", topN));
            }
            return _dbHelper.ExecuteQuery(sql.ToString(), p.ToArray());
        }

        // 2. 單一工單查詢
        public DataTable GetWorkOrderDetail(string woNo)
        {
            string sql = @"SELECT 
    sfb01 AS WO_NO,
    sfb22 AS PO_NO,
    sfb05 AS PRODUCT_NO,
    ima02 AS PRODUCT_NAME,
    TO_NUMBER(sfb08) AS PLAN_QTY,
    TO_NUMBER(sfb09) AS FINISH_QTY,
    ROUND((TO_NUMBER(sfb09)/NULLIF(TO_NUMBER(sfb08),0))*100,2) AS COMPLETION_RATE,
    TO_CHAR(sfb15, 'YYYY-MM-DD') AS PLAN_FINISH_DATE,
    TO_CHAR(sfb81, 'YYYY-MM-DD') AS ACTUAL_FINISH_DATE
FROM DS5.sfb_file
JOIN DS5.ima_file ON sfb05 = ima01
WHERE sfb01 = :wo_no";
            var parameters = new[]
            {
                new OracleParameter("wo_no", woNo)
            };
            return _dbHelper.ExecuteQuery(sql, parameters);
        }

        // 3. PO完成度查詢
        public DataTable GetPOWorkOrderList(string poNo, DateTime startDate, DateTime endDate)
        {
            string sql = @"SELECT 
    sfb22 AS PO_NO,
    sfb01 AS WO_NO,
    sfb05 AS PRODUCT_NO,
    ima02 AS PRODUCT_NAME,
    TO_NUMBER(sfb08) AS PLAN_QTY,
    TO_NUMBER(sfb09) AS FINISH_QTY,
    ROUND((TO_NUMBER(sfb09)/NULLIF(TO_NUMBER(sfb08),0))*100,2) AS COMPLETION_RATE,
    TO_CHAR(sfb15, 'YYYY-MM-DD') AS PLAN_FINISH_DATE,
    TO_CHAR(sfb81, 'YYYY-MM-DD') AS ACTUAL_FINISH_DATE
FROM DS5.sfb_file
JOIN DS5.ima_file ON sfb05 = ima01
WHERE sfb22 = :po_no";
            var parameters = new[]
            {
                new OracleParameter("po_no", poNo)
            };
            return _dbHelper.ExecuteQuery(sql, parameters);
        }

        // 4. All PO Data 查詢 (for MVC)
        public DataTable GetAllPOData(DateTime startDate, DateTime endDate, string keyword = null, decimal? maxRate = null)
        {
            var dt = GetPOCompletionList(startDate, endDate, null, maxRate);
            var allRows = dt.AsEnumerable();
            if (!string.IsNullOrEmpty(keyword))
                allRows = allRows.Where(r => r.Field<string>("PO_NO").Contains(keyword));
            return allRows.Any() ? allRows.CopyToDataTable() : dt.Clone();
        }
    }
}
