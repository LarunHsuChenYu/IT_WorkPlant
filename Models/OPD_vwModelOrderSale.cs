using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;

namespace IT_WorkPlant.Models
{
    public class OPD_vwModelOrderSale
    {
        private readonly OracleDatabaseHelper _dbHelper;
        private const string BASE_QUERY = @"
SELECT 
    oay.oaysign AS so_type,
    occ.occ02 AS end_customer,
    oeb.oeb04 AS model_pn,
    ima.ima02 AS model_name,
    SUM(NVL(oeb.oeb12, 0)) AS sales_qty
FROM DS5.oea_file oea
LEFT JOIN DS5.occ_file occ ON oea.oea04 = occ.occ01   
LEFT JOIN DS5.oeb_file oeb ON oea.oea01 = oeb.oeb01
LEFT JOIN DS5.oay_file oay ON SUBSTR(oea.oea01,1,3) = oay.oayslip
LEFT JOIN DS5.ima_file ima ON oeb.oeb04 = ima.ima01      
WHERE oea.oeaconf = 'Y'  
  AND oea.oea02 BETWEEN TO_DATE(:startDate, 'YYYY-MM-DD') AND TO_DATE(:endDate, 'YYYY-MM-DD')
  AND ima.ima06 = '10'
GROUP BY oay.oaysign, occ.occ02, oeb.oeb04, ima.ima02
";

        public OPD_vwModelOrderSale()
        {
            _dbHelper = new OracleDatabaseHelper(ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString);
        }

        /// <summary>
        /// 取得指定區間Top N產品（可用於本月、上月、任意區間）
        /// </summary>
        public DataTable GetTopProductSales(string startDate, string endDate, int topCount = 5)
        {
            var excludeCodes = new List<string> {
                "8WE461", "8WE455(1)", "8WE459-1", "8WE457(1)", "8WE459", "8WE457(1)-1", "8WE460-1", "8WE456(1)", "8WE458", "8WE460"
            };
            string notInList = string.Join(",", excludeCodes.Select(c => $"'{c.Replace("'", "''")}'"));
            string sql = $@"
WITH base AS (
    {BASE_QUERY}
)
SELECT * FROM (
    SELECT
        REGEXP_SUBSTR(model_name, '^[^;]+') AS product_code,
        SUM(sales_qty) AS sales_qty
    FROM base
    WHERE REGEXP_SUBSTR(model_name, '^[^;]+') NOT IN ({notInList})
    GROUP BY REGEXP_SUBSTR(model_name, '^[^;]+')
    ORDER BY sales_qty DESC
) WHERE ROWNUM <= :topCount";
            var parameters = new List<OracleParameter> {
                new OracleParameter("startDate", startDate),
                new OracleParameter("endDate", endDate),
                new OracleParameter("topCount", topCount)
            };
            return _dbHelper.ExecuteQuery(sql, parameters.ToArray());
        }

        /// <summary>
        /// 根據指定的 SO_TYPE（STO/SPO）取得 Top N 產品銷售數據
        /// </summary>
        public DataTable GetTopProductSalesByType(string startDate, string endDate, string soType, int topCount = 5)
        {
            var excludeCodes = new List<string> {
                "8WE461", "8WE455(1)", "8WE459-1", "8WE457(1)", "8WE459", "8WE457(1)-1", "8WE460-1", "8WE456(1)", "8WE458", "8WE460"
            };
            string notInList = string.Join(",", excludeCodes.Select(c => $"'{c.Replace("'", "''")}'"));
            string sql = $@"
WITH base AS (
    SELECT
        oay.oaysign AS so_type,
        occ.occ02 AS end_customer,
        oeb.oeb04 AS model_pn,
        ima.ima02 AS model_name,
        NVL(oeb.oeb12, 0) AS sales_qty_single_record 
    FROM
        DS5.oea_file oea
    LEFT JOIN DS5.occ_file occ ON oea.oea04 = occ.occ01
    LEFT JOIN DS5.oeb_file oeb ON oea.oea01 = oeb.oeb01
    LEFT JOIN DS5.oay_file oay ON SUBSTR(oea.oea01, 1, 3) = oay.oayslip
    LEFT JOIN DS5.ima_file ima ON oeb.oeb04 = ima.ima01
    WHERE
        oea.oeaconf = 'Y'
        AND oea.oea02 BETWEEN TO_DATE(:startDate, 'YYYY-MM-DD') AND TO_DATE(:endDate, 'YYYY-MM-DD')
        AND ima.ima06 = '10'
),
pre_aggregated AS (
    SELECT
        REGEXP_SUBSTR(model_name, '^[^;]+') AS product_code,
        so_type,
        model_name, 
        SUM(sales_qty_single_record) AS total_sales_qty_for_model
    FROM
        base
    WHERE
        so_type = :soType
        AND REGEXP_SUBSTR(model_name, '^[^;]+') NOT IN ({notInList})
    GROUP BY
        REGEXP_SUBSTR(model_name, '^[^;]+'),
        so_type,
        model_name 
),
final_aggregated AS (
    SELECT
        product_code,
        so_type,
        SUM(total_sales_qty_for_model) AS sales_qty,
        RTRIM(XMLAGG(XMLElement(""e"", model_name || '; ') ORDER BY model_name).EXTRACT('//text()').GETSTRINGVAL(), '; ') AS full_model_names
    FROM
        pre_aggregated
    GROUP BY
        product_code,
        so_type
    ORDER BY
        SUM(total_sales_qty_for_model) DESC 
)
SELECT *
FROM final_aggregated
WHERE ROWNUM <= :topCount
";
            var parameters = new List<OracleParameter>
            {
                new OracleParameter("startDate", startDate),
                new OracleParameter("endDate", endDate),
                new OracleParameter("soType", soType),
                new OracleParameter("topCount", topCount)
            };
            return _dbHelper.ExecuteQuery(sql, parameters.ToArray());
        }

        /// <summary>
        /// 取得完整的銷售資料（包含所有欄位）
        /// </summary>
        public DataTable GetSalesData(string startDate, string endDate)
        {
            var parameters = new[]
            {
                new OracleParameter("startDate", startDate),
                new OracleParameter("endDate", endDate)
            };
            return _dbHelper.ExecuteQuery(BASE_QUERY, parameters);
        }

        #region ② SO_TYPE 分佈
        /// <summary>
        /// 取得 SO_TYPE 分佈（依指定模型）
        /// </summary>
        public DataTable GetSoTypeDistribution(string startDate, string endDate, List<string> modelPns)
        {
            if (modelPns == null || modelPns.Count == 0)
            {
                return new DataTable();
            }
            string inClause = string.Join(",", modelPns.Select((pn, i) => $":pn{i}"));
            string sql = $@"
WITH base AS (
    {BASE_QUERY}
)
SELECT REGEXP_SUBSTR(model_name, '^[^;]+') AS model_name, so_type, SUM(sales_qty) AS sales_qty
FROM base
WHERE REGEXP_SUBSTR(model_name, '^[^;]+') IN ({inClause})
GROUP BY REGEXP_SUBSTR(model_name, '^[^;]+'), so_type
ORDER BY model_name, so_type";
            var parameters = new List<OracleParameter> {
                new OracleParameter("startDate", startDate),
                new OracleParameter("endDate", endDate)
            };
            parameters.AddRange(modelPns.Select((pn, i) => new OracleParameter($"pn{i}", pn)));
            return _dbHelper.ExecuteQuery(sql, parameters.ToArray());
        }
        #endregion

        #region ③ 客戶熱點 (Heatmap)
        /// <summary>
        /// 取得客戶熱點（依指定模型，取前 N 客戶）
        /// </summary>
        public DataTable GetCustomerHeatmap(string startDate, string endDate, List<string> modelPns, int topN = 10)
        {
            if (modelPns == null || modelPns.Count == 0)
            {
                return new DataTable();
            }
            string inClause = string.Join(",", modelPns.Select((pn, i) => $":pn{i}"));
            string sql = $@"
WITH base AS (
    {BASE_QUERY}
),
agg AS (
    SELECT REGEXP_SUBSTR(model_name, '^[^;]+') AS model_name, end_customer, SUM(sales_qty) AS sales_qty
    FROM base
    WHERE REGEXP_SUBSTR(model_name, '^[^;]+') IN ({inClause})
    GROUP BY REGEXP_SUBSTR(model_name, '^[^;]+'), end_customer
),
top_customers AS (
    SELECT end_customer, SUM(sales_qty) AS total_qty
    FROM agg
    GROUP BY end_customer
    ORDER BY total_qty DESC
    FETCH FIRST :topN ROWS ONLY
)
SELECT a.model_name, a.end_customer, a.sales_qty
FROM agg a
JOIN top_customers t ON a.end_customer = t.end_customer
ORDER BY t.total_qty DESC, a.model_name";
            var parameters = new List<OracleParameter> {
                new OracleParameter("startDate", startDate),
                new OracleParameter("endDate", endDate),
                new OracleParameter("topN", topN)
            };
            parameters.AddRange(modelPns.Select((pn, i) => new OracleParameter($"pn{i}", pn)));
            return _dbHelper.ExecuteQuery(sql, parameters.ToArray());
        }
        #endregion

        #region ④ 明細表
        /// <summary>
        /// 取得訂單銷售明細（可依模型、SO_TYPE、客戶篩選）
        /// </summary>
        public DataTable GetOrderSalesDetail(string startDate, string endDate, string modelPn = null, string soType = null, string customer = null)
        {
            string whereExtra = "";
            var parameters = new List<OracleParameter> {
                new OracleParameter("startDate", startDate),
                new OracleParameter("endDate", endDate)
            };
            if (!string.IsNullOrEmpty(modelPn))
            {
                whereExtra += " AND REGEXP_SUBSTR(model_name, '^[^;]+') = :modelPn";
                parameters.Add(new OracleParameter("modelPn", modelPn));
            }
            if (!string.IsNullOrEmpty(soType))
            {
                whereExtra += " AND so_type = :soType";
                parameters.Add(new OracleParameter("soType", soType));
            }
            if (!string.IsNullOrEmpty(customer))
            {
                whereExtra += " AND end_customer = :customer";
                parameters.Add(new OracleParameter("customer", customer));
            }
            string sql = $@"
WITH base AS (
    {BASE_QUERY}
)
SELECT * FROM base WHERE 1=1 {whereExtra} ORDER BY sales_qty DESC";
            return _dbHelper.ExecuteQuery(sql, parameters.ToArray());
        }
        #endregion
    }
} 