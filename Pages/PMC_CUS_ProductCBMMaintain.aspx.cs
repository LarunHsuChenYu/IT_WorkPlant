using Org.BouncyCastle.Asn1.Cmp;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Web.UI.WebControls;
using IT_WorkPlant.Models;
using System.Text.RegularExpressions;
using System.Web;

namespace IT_WorkPlant.Pages
{
    public partial class PMC_CUS_ProductCBMMaintain : System.Web.UI.Page
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
            string query = "SELECT * FROM PMC_ProdPackInfo WHERE 1=1";

            // 添加搜尋條件
            if (!string.IsNullOrEmpty(tbSearchProdCode.Text.Trim()))
            {
                query += " AND Vic_ProdCode LIKE @ProdCode";
            }

            var parameters = new List<SqlParameter>
            {
                new SqlParameter("@ProdCode", $"%{tbSearchProdCode.Text.Trim()}%")
            };

            DataTable dt = _dbHelper.ExecuteQuery(query, parameters.ToArray());
            gvProductCBM.DataSource = dt;
            gvProductCBM.DataBind();
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            try
            {
                // 查詢 Vic_ProdCode
                string prodCode = tbSearchProdCode.Text.Trim();

                string query = "SELECT * FROM PMC_ProdPackInfo WHERE Vic_ProdCode LIKE @ProdCode";
                
                SqlParameter[] parameters = {
                    new SqlParameter("@ProdCode", $"%{prodCode}%")
                };

                DataTable dt = _dbHelper.ExecuteQuery(query, parameters);
                gvProductCBM.DataSource = dt;
                gvProductCBM.DataBind();

                lblStatus.Text = dt.Rows.Count > 0
                    ? $"Found {dt.Rows.Count} record(s) for '{prodCode}'."
                    : "No records found.";
                ShowAlert(lblStatus.Text);
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error while searching: {ex.Message}";
                ShowAlert(lblStatus.Text);
            }
        }

        protected void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                string prodCode = tbNewProdCode.Text.Trim();
                decimal grossWeight = decimal.Parse(tbNewGrossWeight.Text.Trim());
                decimal netWeight = decimal.Parse(tbNewNetWeight.Text.Trim());
                decimal volCBM = decimal.Parse(tbNewVolCBM.Text.Trim());
                decimal length = decimal.Parse(tbNewLength.Text.Trim());
                decimal width = decimal.Parse(tbNewWidth.Text.Trim());
                decimal height = decimal.Parse(tbNewHeight.Text.Trim());
                int units = int.Parse(tbNewUnits.Text.Trim());

                string insertQuery = @"
                    INSERT INTO PMC_ProdPackInfo (Vic_ProdCode, GrossWeight, NetWeight, Vol_CBM, Length, Width, Height, Units)
                    VALUES (@ProdCode, @GrossWeight, @NetWeight, @VolCBM, @Length, @Width, @Height, @Units)";
                SqlParameter[] insertParams = {
                    new SqlParameter("@ProdCode", prodCode),
                    new SqlParameter("@GrossWeight", grossWeight),
                    new SqlParameter("@NetWeight", netWeight),
                    new SqlParameter("@VolCBM", volCBM),
                    new SqlParameter("@Length", length),
                    new SqlParameter("@Width", width),
                    new SqlParameter("@Height", height),
                    new SqlParameter("@Units", units)
                };

                _dbHelper.ExecuteNonQuery(insertQuery, insertParams);
                lblStatus.Text = "Product CBM added successfully.";
                ShowAlert(lblStatus.Text);
                BindGridView();
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error adding product: {ex.Message}";
                ShowAlert(lblStatus.Text);
            }
        }

        protected void gvProductCBM_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvProductCBM.EditIndex = e.NewEditIndex;
            BindGridView();
        }

        protected void gvProductCBM_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            // 退出編輯模式
            gvProductCBM.EditIndex = -1;
            // 重新綁定數據
            BindGridView();
        }

        protected void gvProductCBM_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            // 設置新的頁索引
            gvProductCBM.PageIndex = e.NewPageIndex;
            // 重新綁定數據
            BindGridView();
        }

        protected void gvProductCBM_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                // 獲取當前編輯行
                GridViewRow row = gvProductCBM.Rows[e.RowIndex];

                // 獲取行中的控件數據
                string prodCode = ((Label)row.FindControl("lblVic_ProdCode"))?.Text;
                decimal grossWeight = decimal.Parse(((TextBox)row.FindControl("tbGrossWeight"))?.Text ?? "0");
                decimal netWeight = decimal.Parse(((TextBox)row.FindControl("tbNetWeight"))?.Text ?? "0");
                decimal volCBM = decimal.Parse(((TextBox)row.FindControl("tbVolCBM"))?.Text ?? "0");
                decimal length = decimal.Parse(((TextBox)row.FindControl("tbLength"))?.Text ?? "0");
                decimal width = decimal.Parse(((TextBox)row.FindControl("tbWidth"))?.Text ?? "0");
                decimal height = decimal.Parse(((TextBox)row.FindControl("tbHeight"))?.Text ?? "0");
                int units = int.Parse(((TextBox)row.FindControl("tbUnits"))?.Text ?? "0");

                // 構建 SQL 更新語句
                string updateQuery = @"
                    UPDATE PMC_ProdPackInfo
                    SET 
                        GrossWeight = @GrossWeight,
                        NetWeight = @NetWeight,
                        Vol_CBM = @VolCBM,
                        Length = @Length,
                        Width = @Width,
                        Height = @Height,
                        Units = @Units
                    WHERE Vic_ProdCode = @ProdCode";

                SqlParameter[] updateParams = {
                    new SqlParameter("@ProdCode", prodCode),
                    new SqlParameter("@GrossWeight", grossWeight),
                    new SqlParameter("@NetWeight", netWeight),
                    new SqlParameter("@VolCBM", volCBM),
                    new SqlParameter("@Length", length),
                    new SqlParameter("@Width", width),
                    new SqlParameter("@Height", height),
                    new SqlParameter("@Units", units)
                };

                // 執行更新操作
                int rowsAffected = _dbHelper.ExecuteNonQuery(updateQuery, updateParams);

                if (rowsAffected > 0)
                {
                    ShowAlert("Record updated successfully.");
                }
                else
                {
                    ShowAlert("No record was updated.");
                }
            }
            catch (FormatException ex)
            {
                lblStatus.Text = $"Invalid input format. Please check the entered values. {ex.Message}";
                ShowAlert(lblStatus.Text);
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error updating record: {ex.Message}";
                ShowAlert(lblStatus.Text);
            }
            finally
            {
                // 重設編輯模式並重新綁定數據
                gvProductCBM.EditIndex = -1;
                BindGridView();
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

                    // 定義 dtChanges 用於顯示更新的結果
                    DataTable dtChanges = new DataTable();
                    dtChanges.Columns.Add("Vic_ProdCode", typeof(string));
                    dtChanges.Columns.Add("GrossWeight", typeof(decimal));
                    dtChanges.Columns.Add("NetWeight", typeof(decimal));
                    dtChanges.Columns.Add("Vol_CBM", typeof(decimal));
                    dtChanges.Columns.Add("Length", typeof(decimal));
                    dtChanges.Columns.Add("Width", typeof(decimal));
                    dtChanges.Columns.Add("Height", typeof(decimal));
                    dtChanges.Columns.Add("Units", typeof(int));
                   
                    foreach (DataRow row in dtExcel.Rows)
                    {
                        string prodCode = row[0]?.ToString();
                        if (decimal.TryParse(row[1]?.ToString(), out decimal grossWeight) &&
                            decimal.TryParse(row[2]?.ToString(), out decimal netWeight) &&
                            decimal.TryParse(row[3]?.ToString(), out decimal volCBM))
                        {
                            // 解析 包装尺寸Dimension(mm)
                            string dimension = row[4]?.ToString();
                            decimal length = 0, width = 0, height = 0;
                            int units = 0;

                            if (!string.IsNullOrWhiteSpace(dimension))
                            {
                                string[] parts = dimension.Split('*');
                                if (parts.Length == 4)
                                {
                                    if (!decimal.TryParse(parts[0], out length))
                                    {
                                        lblStatus.Text = $"Error parsing Length from dimension: {dimension}";
                                        continue; 
                                    }

                                    if (!decimal.TryParse(parts[1], out width))
                                    {
                                        lblStatus.Text = $"Error parsing Width from dimension: {dimension}";
                                        continue; 
                                    }

                                    if (!decimal.TryParse(parts[2], out height))
                                    {
                                        lblStatus.Text = $"Error parsing Height from dimension: {dimension}";
                                        continue; 
                                    }

                                    string unitString = parts[3].Trim();
                                    unitString = Regex.Replace(unitString, @"\bPC(S)?\b", "", RegexOptions.IgnoreCase).Trim();

                                    if (string.IsNullOrEmpty(unitString))
                                    {
                                        units = 1; 
                                    }
                                    else if (!int.TryParse(unitString, out units))
                                    {
                                        lblStatus.Text = $"Error parsing Units from dimension: {dimension}. Defaulting Units to 1.";
                                        units = 1; 
                                    }

                                    lblStatus.Text = $"Dimension parsed successfully: {length} x {width} x {height}, {units} unit(s)";
                                }
                                else
                                {
                                    lblStatus.Text = $"Error: Dimension format is incorrect: {dimension}";
                                    continue; 
                                }
                            }
                            else
                            {
                                lblStatus.Text = "Error: Dimension field is empty or null";
                                continue; 
                            }

                            string checkQuery = "SELECT COUNT(1) FROM PMC_ProdPackInfo WHERE Vic_ProdCode = @ProdCode";
                            SqlParameter[] checkParams = { new SqlParameter("@ProdCode", prodCode) };
                            int exists = Convert.ToInt32(_dbHelper.ExecuteScalar(checkQuery, checkParams));

                            if (exists > 0)
                            {
                                string updateQuery = 
                                    @"UPDATE PMC_ProdPackInfo "
                                    + "SET "
                                    + "GrossWeight = @GrossWeight, "
                                    + "NetWeight = @NetWeight, "
                                    + "Vol_CBM = @VolCBM, "
                                    + "Length = @Length, "
                                    + "Width = @Width,"
                                    + "Height = @Height, "
                                    + "Units = @Units "
                                    + "WHERE Vic_ProdCode = @ProdCode";
                                SqlParameter[] updateParams =
                                {
                                    new SqlParameter("@ProdCode", prodCode),
                                    new SqlParameter("@GrossWeight", grossWeight),
                                    new SqlParameter("@NetWeight", netWeight),
                                    new SqlParameter("@VolCBM", volCBM),
                                    new SqlParameter("@Length", length),
                                    new SqlParameter("@Width", width),
                                    new SqlParameter("@Height", height),
                                    new SqlParameter("@Units", units)
                                };
                                _dbHelper.ExecuteNonQuery(updateQuery, updateParams);

                                // 記錄更新操作
                                DataRow changeRow = dtChanges.NewRow();
                                changeRow["Vic_ProdCode"] = prodCode;
                                changeRow["GrossWeight"] = grossWeight;
                                changeRow["NetWeight"] = netWeight;
                                changeRow["Vol_CBM"] = volCBM;
                                changeRow["Length"] = length;
                                changeRow["Width"] = width;
                                changeRow["Height"] = height;
                                changeRow["Units"] = units;
                                
                                dtChanges.Rows.Add(changeRow);
                            }
                            else
                            {
                                // 插入新記錄
                                string insertQuery = @"INSERT INTO PMC_ProdPackInfo "
                                    +"(Vic_ProdCode, GrossWeight, NetWeight, Vol_CBM, Length, Width, Height, Units) "
                                    +"VALUES "
                                    +"(@ProdCode, @GrossWeight, @NetWeight, @VolCBM, @Length, @Width, @Height, @Units)";
                                SqlParameter[] insertParams =
                                {
                                    new SqlParameter("@ProdCode", prodCode),
                                    new SqlParameter("@GrossWeight", grossWeight),
                                    new SqlParameter("@NetWeight", netWeight),
                                    new SqlParameter("@VolCBM", volCBM),
                                    new SqlParameter("@Length", length),
                                    new SqlParameter("@Width", width),
                                    new SqlParameter("@Height", height),
                                    new SqlParameter("@Units", units)
                                };
                                _dbHelper.ExecuteNonQuery(insertQuery, insertParams);

                                // 記錄插入操作
                                DataRow changeRow = dtChanges.NewRow();
                                changeRow["Vic_ProdCode"] = prodCode;
                                changeRow["GrossWeight"] = grossWeight;
                                changeRow["NetWeight"] = netWeight;
                                changeRow["Vol_CBM"] = volCBM;
                                changeRow["Length"] = length;
                                changeRow["Width"] = width;
                                changeRow["Height"] = height;
                                changeRow["Units"] = units;
                                
                                dtChanges.Rows.Add(changeRow);
                            }
                        }
                    }

                    // 將變更的數據顯示在 GridView
                    gvProductCBM.DataSource = dtChanges;
                    gvProductCBM.DataBind();

                    lblStatus.Text = "Batch update completed successfully.\r\nTotal update " 
                        + dtChanges.Rows.Count.ToString() 
                        + "\r\n " + DateTime.Now.ToString();
                    ShowAlert(lblStatus.Text);
                }
                catch (Exception ex)
                {
                    lblStatus.Text = $"Error: {ex.Message}";
                    ShowAlert(lblStatus.Text);
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