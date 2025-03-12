using System;
using System.Data;
using System.Data.SqlClient;

namespace IT_WorkPlant.Models
{
    public class CUS_DataAugmentation
    {
        private readonly MssqlDatabaseHelper _dbHelper;

        public CUS_DataAugmentation()
        {
            _dbHelper = new MssqlDatabaseHelper();
        }

        private DataRow GetProdPackInfo(string prodCode)
        {
            string query = @"
        SELECT 
            GrossWeight AS GW,
            NetWeight AS NW,
            Vol_CBM AS CBM
        FROM 
            PMC_ProdPackInfo
        WHERE 
            Vic_ProdCode = @ProdCode;";

            SqlParameter[] parameters = { new SqlParameter("@ProdCode", prodCode) };

            DataTable result = ExecuteSingleQuery(query, parameters);

            // 如果結果為空，創建空行並返回
            if (result.Rows.Count == 0)
            {
                DataRow emptyRow = result.NewRow();
                foreach (DataColumn column in result.Columns)
                {
                    if (column.DataType == typeof(decimal) || column.DataType == typeof(double))
                    {
                        emptyRow[column.ColumnName] = 0; // 預設為 0
                    }
                    else
                    {
                        emptyRow[column.ColumnName] = DBNull.Value; // 使用空值
                    }
                }
                return emptyRow;
            }

            // 有結果時返回第一行
            return result.Rows[0];
        }

        private DataRow GetShippingPrice(string sku)
        {
            string query = @"
        SELECT 
            Update_Price AS UnitPrice
        FROM 
            PMC_ShippingPrice
        WHERE 
            Prod_SKU = @SKU;";

            SqlParameter[] parameters = { new SqlParameter("@SKU", sku)};
            DataTable result = ExecuteSingleQuery(query, parameters);

            // 如果結果為空，創建空行並返回
            if (result.Rows.Count == 0)
            {
                DataRow emptyRow = result.NewRow();
                foreach (DataColumn column in result.Columns)
                {
                    if (column.DataType == typeof(decimal) || column.DataType == typeof(double))
                    {
                        emptyRow[column.ColumnName] = 0; // 預設為 0
                    }
                    else
                    {
                        emptyRow[column.ColumnName] = DBNull.Value; // 使用空值
                    }
                }
                return emptyRow;
            }

            // 有結果時返回第一行
            return result.Rows[0];
        }

        public DataRow GetAugmentedData(string prodCode, string sku)
        {
            DataRow prodPackInfoRow = GetProdPackInfo(prodCode);
            DataRow shippingPriceRow = GetShippingPrice(sku);

            DataTable mergedResult = new DataTable();
            mergedResult.Columns.Add("GW", typeof(decimal));
            mergedResult.Columns.Add("NW", typeof(decimal));
            mergedResult.Columns.Add("CBM", typeof(decimal));
            mergedResult.Columns.Add("UnitPrice", typeof(decimal));

            DataRow mergedRow = mergedResult.NewRow();

            // 合併數據，處理空值
            mergedRow["GW"] = prodPackInfoRow["GW"] != DBNull.Value ? prodPackInfoRow["GW"] : 0;
            mergedRow["NW"] = prodPackInfoRow["NW"] != DBNull.Value ? prodPackInfoRow["NW"] : 0;
            mergedRow["CBM"] = prodPackInfoRow["CBM"] != DBNull.Value ? prodPackInfoRow["CBM"] : 0;
            mergedRow["UnitPrice"] = shippingPriceRow["UnitPrice"] != DBNull.Value ? shippingPriceRow["UnitPrice"] : 0;

            return mergedRow;
        }

        private DataTable ExecuteSingleQuery(string query, SqlParameter[] parameters)
        {
            return _dbHelper.ExecuteQuery(query, parameters);
        }

    }
}
