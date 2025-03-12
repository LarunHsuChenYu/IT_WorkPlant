using IT_WorkPlant.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Web;

namespace IT_WorkPlant.Pages
{
    public partial class PMC_CUS_ShippingPriceMaintain : System.Web.UI.Page
    {
        private readonly MssqlDatabaseHelper _dbHelper = new MssqlDatabaseHelper();
        private readonly ExcelHelper _excelHelper = new ExcelHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGridView();
            }
        }

        private void BindGridView()
        {
            string query = @"
        SELECT 
            Prod_ID,
            Inventory_Code,
            Prod_SKU,
            Item_Name,
            Update_Price,
            Update_Date
        FROM PMC_ShippingPrice
        WHERE 1 = 1
        ";

            var parameters = new List<SqlParameter>();

            // 過濾條件
            if (!string.IsNullOrEmpty(tbSearchInvCode.Text))
            {
                query += " AND Inventory_Code LIKE @InvCode";
                parameters.Add(new SqlParameter("@InvCode", $"%{tbSearchInvCode.Text.Trim()}%"));
            }

            if (!string.IsNullOrEmpty(tbSearchSKU.Text))
            {
                query += " AND Prod_SKU LIKE @SKU";
                parameters.Add(new SqlParameter("@SKU", $"%{tbSearchSKU.Text.Trim()}%"));
            }

            if (!string.IsNullOrEmpty(tbSearchItemName.Text))
            {
                query += " AND Item_Name LIKE @ItemName";
                parameters.Add(new SqlParameter("@ItemName", $"%{tbSearchItemName.Text.Trim()}%"));
            }

            query += " ORDER BY Prod_ID";

            DataTable dt = _dbHelper.ExecuteQuery(query, parameters.ToArray());
            gvShippingPrices.DataSource = dt;
            gvShippingPrices.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            BindGridView();
        }

        protected void gvShippingPrices_RowEditing(object sender, GridViewEditEventArgs e)
        {
            // 將 GridView 切換到編輯模式
            gvShippingPrices.EditIndex = e.NewEditIndex;
            BindGridView();
        }

        protected void gvShippingPrices_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            // 設定新頁索引
            gvShippingPrices.PageIndex = e.NewPageIndex;

            // 重新綁定數據到 GridView
            BindGridView();
        }


        protected void gvShippingPrices_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                // 獲取當前行
                GridViewRow row = gvShippingPrices.Rows[e.RowIndex];

                // 獲取 SKU（不可編輯，只用於更新條件）
                string sku = ((Label)row.FindControl("lblEditSKU")).Text;

                // 獲取價格（可編輯）
                string priceText = ((TextBox)row.FindControl("tbPrice")).Text;

                if (decimal.TryParse(priceText, out decimal newPrice))
                {
                    // 更新數據庫
                    string query = "UPDATE PMC_ShippingPrice SET Update_Price = @Price, Update_Date = GETDATE() WHERE Prod_SKU = @SKU";
                    SqlParameter[] parameters = {
                new SqlParameter("@Price", newPrice),
                new SqlParameter("@SKU", sku)
            };

                    _dbHelper.ExecuteNonQuery(query, parameters);

                    lblStatus.Text = "Price updated successfully.";
                }
                else
                {
                    lblStatus.Text = "Invalid price format.";
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error updating record: {ex.Message}";
            }
            finally
            {
                // 退出編輯模式並重新綁定數據
                gvShippingPrices.EditIndex = -1;
                BindGridView();
            }
        }


        protected void gvShippingPrices_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            // 取消編輯模式
            gvShippingPrices.EditIndex = -1;
            BindGridView();
        }

        protected void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                // 獲取輸入值
                string inventoryCode = tbNewInvCode.Text.Trim();
                string sku = tbNewSKU.Text.Trim();
                string itemName = tbNewItemName.Text.Trim();
                string priceText = tbNewPrice.Text.Trim();

                // 驗證輸入值
                if (string.IsNullOrEmpty(inventoryCode) || string.IsNullOrEmpty(sku) || string.IsNullOrEmpty(itemName))
                {
                    lblStatus.Text = "Please fill in all required fields.";
                    return;
                }

                if (!decimal.TryParse(priceText, out decimal newPrice))
                {
                    lblStatus.Text = "Invalid price format.";
                    return;
                }

                // 插入新記錄
                string query = @"
                    INSERT INTO PMC_ShippingPrice (Inventory_Code, Prod_SKU, Item_Name, Update_Price, Update_Date)
                    VALUES (@Inventory_Code, @SKU, @Item_Name, @Price, GETDATE())";

                SqlParameter[] parameters = {
                    new SqlParameter("@Inventory_Code", inventoryCode),
                    new SqlParameter("@SKU", sku),
                    new SqlParameter("@Item_Name", itemName),
                    new SqlParameter("@Price", newPrice)
                };

                _dbHelper.ExecuteNonQuery(query, parameters);

                lblStatus.Text = "New record added successfully.";

                tbNewInvCode.Text = "";
                tbNewSKU.Text = "";
                tbNewItemName.Text = "";
                tbNewPrice.Text = "";

                // 重新綁定 GridView 顯示最新數據
                BindGridView();
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error adding new record: {ex.Message}";
            }
        }

        protected void btnUpload_Click(object sender, EventArgs e)
        {
            if (fileUpload.HasFile)
            {
                try
                {
                    // 儲存上傳的 Excel 文件
                    string filePath = _excelHelper.SaveUploadedFile(fileUpload.PostedFile, Server);
                    DataTable dtExcel = _excelHelper.ReadExcelData(filePath);

                    // 定義 dtChanges 與資料庫結構一致
                    DataTable dtChanges = new DataTable();
                    dtChanges.Columns.Add("Inventory_Code", typeof(string));
                    dtChanges.Columns.Add("Prod_SKU", typeof(string));
                    dtChanges.Columns.Add("Item_Name", typeof(string));
                    dtChanges.Columns.Add("Update_Price", typeof(decimal));
                    dtChanges.Columns.Add("Update_Date", typeof(DateTime));

                    foreach (DataRow row in dtExcel.Rows)
                    {
                        string inventoryCode = row[0]?.ToString();
                        string sku = row[1]?.ToString();
                        string itemName = row[2]?.ToString();
                        string priceText = row[3]?.ToString();

                        if (decimal.TryParse(priceText, out decimal newPrice))
                        {
                            // 查詢是否存在該 SKU
                            string checkQuery = "SELECT COUNT(1) FROM PMC_ShippingPrice WHERE Prod_SKU = @SKU";
                            SqlParameter[] checkParams = { new SqlParameter("@SKU", sku) };
                            int exists = Convert.ToInt32(_dbHelper.ExecuteScalar(checkQuery, checkParams));

                            if (exists > 0)
                            {
                                // 僅當價格不同時進行更新
                                string updateQuery = @"
                                    UPDATE PMC_ShippingPrice 
                                    SET Update_Price = @Price, Update_Date = GETDATE()
                                    WHERE Prod_SKU = @SKU AND Update_Price != @Price";
                                SqlParameter[] updateParams =
                                {
                                    new SqlParameter("@SKU", sku),
                                    new SqlParameter("@Price", newPrice)
                                };

                                int rowsAffected = _dbHelper.ExecuteNonQuery(updateQuery, updateParams);

                                // 僅當資料被更新時加入到變更的記錄
                                if (rowsAffected > 0)
                                {
                                    DataRow changeRow = dtChanges.NewRow();
                                    changeRow["Inventory_Code"] = inventoryCode;
                                    changeRow["Prod_SKU"] = sku;
                                    changeRow["Item_Name"] = itemName;
                                    changeRow["Update_Price"] = newPrice;
                                    changeRow["Update_Date"] = DateTime.Now;
                                    dtChanges.Rows.Add(changeRow);
                                }
                            }
                            else
                            {
                                // 插入新的記錄
                                string insertQuery = @"
                                    INSERT INTO PMC_ShippingPrice (Inventory_Code, Prod_SKU, Item_Name, Update_Price, Update_Date)
                                    VALUES (@Inventory_Code, @SKU, @Item_Name, @Price, GETDATE())";
                                SqlParameter[] insertParams =
                                {
                                    new SqlParameter("@Inventory_Code", inventoryCode),
                                    new SqlParameter("@SKU", sku),
                                    new SqlParameter("@Item_Name", itemName),
                                    new SqlParameter("@Price", newPrice)
                                };
                                _dbHelper.ExecuteNonQuery(insertQuery, insertParams);

                                // 加入到變更的記錄
                                DataRow changeRow = dtChanges.NewRow();
                                changeRow["Inventory_Code"] = inventoryCode;
                                changeRow["Prod_SKU"] = sku;
                                changeRow["Item_Name"] = itemName;
                                changeRow["Update_Price"] = newPrice;
                                changeRow["Update_Date"] = DateTime.Now;
                                dtChanges.Rows.Add(changeRow);
                            }
                        }
                    }

                    // 將變更的數據顯示在 GridView
                    gvShippingPrices.DataSource = dtChanges;
                    gvShippingPrices.DataBind();

                    lblStatus.Text = "Batch update completed successfully.\rTotal update " + dtChanges.Rows.Count.ToString();
                    ShowAlert(lblStatus.Text);
                }
                catch (Exception ex)
                {
                    lblStatus.Text = $"Error: {ex.Message}";
                }
            }
            else
            {
                lblStatus.Text = "Please select an Excel file to upload.";
            }
        }
        private void ShowAlert(string message)
        {
            // 確保消息正確轉義
            string safeMessage = HttpUtility.JavaScriptStringEncode(message);
            string script = $"alert('{safeMessage}');";

            // 注入腳本到頁面
            if (!ClientScript.IsStartupScriptRegistered("alert"))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", script, true);
            }
        }

    }
}
