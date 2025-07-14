using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using System.Configuration;

namespace IT_WorkPlant.Models
{
    public class OPD_vwModelShipmentSale
    {
        private readonly OracleDatabaseHelper _dbHelper;
        private const string BASE_QUERY = @"
SELECT 
    oay.oaysign AS so_type,
    occ.occ02 AS end_customer,
    ogb.ogb04 AS model_pn,
    ima.ima02 AS model_name,
    SUM(NVL(ogb.ogb12, 0)) AS shipping_qty
FROM DS5.oga_file oga
LEFT JOIN DS5.occ_file occ ON oga.oga04 = occ.occ01   
LEFT JOIN DS5.ogb_file ogb ON oga.oga01 = ogb.ogb01
LEFT JOIN DS5.oay_file oay ON SUBSTR(ogb.ogb31,1,3) = oay.oayslip
LEFT JOIN DS5.ima_file ima ON ogb.ogb04 = ima.ima01      
WHERE oga.ogapost = 'Y'  
  AND oga.oga02 BETWEEN TO_DATE(:startDate, 'YYYY-MM-DD') AND TO_DATE(:endDate, 'YYYY-MM-DD')
  AND ima.ima06 = '10'
GROUP BY oay.oaysign, occ.occ02, ogb.ogb04, ima.ima02
";

        public OPD_vwModelShipmentSale()
        {
            _dbHelper = new OracleDatabaseHelper(ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString);
        }

        /// <summary>
        /// 取得完整的出貨資料（包含所有欄位）
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

        /// <summary>
        /// 取得指定區間Top N出貨產品
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
        SUM(shipping_qty) AS shipping_qty
    FROM base
    WHERE REGEXP_SUBSTR(model_name, '^[^;]+') NOT IN ({notInList})
    GROUP BY REGEXP_SUBSTR(model_name, '^[^;]+')
    ORDER BY shipping_qty DESC
) WHERE ROWNUM <= :topCount";
            var parameters = new List<OracleParameter> {
                new OracleParameter("startDate", startDate),
                new OracleParameter("endDate", endDate),
                new OracleParameter("topCount", topCount)
            };
            return _dbHelper.ExecuteQuery(sql, parameters.ToArray());
        }

        /// <summary>
        /// 根據指定的 SO_TYPE（STO/SPO）取得 Top N 出貨產品
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
        ogb.ogb04 AS model_pn,
        ima.ima02 AS model_name,
        NVL(ogb.ogb12, 0) AS shipping_qty_single_record 
    FROM
        DS5.oga_file oga
    LEFT JOIN DS5.occ_file occ ON oga.oga04 = occ.occ01
    LEFT JOIN DS5.ogb_file ogb ON oga.oga01 = ogb.ogb01
    LEFT JOIN DS5.oay_file oay ON SUBSTR(ogb.ogb31, 1, 3) = oay.oayslip
    LEFT JOIN DS5.ima_file ima ON ogb.ogb04 = ima.ima01
    WHERE
        oga.ogapost = 'Y'
        AND oga.oga02 BETWEEN TO_DATE(:startDate, 'YYYY-MM-DD') AND TO_DATE(:endDate, 'YYYY-MM-DD')
        AND ima.ima06 = '10'
),
pre_aggregated AS (
    SELECT
        REGEXP_SUBSTR(model_name, '^[^;]+') AS product_code,
        so_type,
        model_name, 
        SUM(shipping_qty_single_record) AS total_shipping_qty_for_model
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
        SUM(total_shipping_qty_for_model) AS shipping_qty,
        RTRIM(XMLAGG(XMLElement(""e"", model_name || '; ') ORDER BY model_name).EXTRACT('//text()').GETSTRINGVAL(), '; ') AS full_model_names
    FROM
        pre_aggregated
    GROUP BY
        product_code,
        so_type
    ORDER BY
        SUM(total_shipping_qty_for_model) DESC 
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
    }
} 