using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Web;
using IT_WorkPlant.Models;
using OfficeOpenXml;

namespace IT_WorkPlant.Pages
{
    public partial class PMC_CUS_InvoiceCreate : System.Web.UI.Page
    {
        private DataTable _invoiceData
        {
            get { return ViewState["InvoiceData"] as DataTable; }
            set { ViewState["InvoiceData"] = value; }
        }

        private DataTable _packingListData
        {
            get { return ViewState["PackingListData"] as DataTable; }
            set { ViewState["PackingListData"] = value; }
        }

        private String _sDestinationPort
        {
            get { return ViewState["DestinationPort"] as String; }
            set { ViewState["DestinationPort"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                Calendar1.Style["display"] = "none";
            }
        }

        protected void btnProcessPlan_Click(object sender, EventArgs e)
        {
            if (fileUploadPlan.HasFile)
            {
                string fileExtension = Path.GetExtension(fileUploadPlan.FileName).ToLower();

                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    try
                    {
                        // 儲存文件
                        string filePath = new ExcelHelper().SaveUploadedFile(fileUploadPlan.PostedFile, Server);

                        // 清理 Excel 文件內容
                        CleanExcelFile(filePath);

                        // 使用 ExcelHelper 解析文件
                        DataTable dtExcel = new ExcelHelper().ReadExcelData(filePath);
                        
                        if (dtExcel.Rows.Count > 0 && dtExcel.Columns.Contains("目的港"))
                        {
                            _sDestinationPort = dtExcel.Rows[0]["目的港"]?.ToString();
                        }
                        else
                        {
                            Console.WriteLine("DataTable 中沒有資料或不包含 '目的港' 欄位！");
                        }


                        //_sDestinationPort = dtExcel.;
                        DataTable augmentedData = AugmentDataWithDatabase(dtExcel);

                        // 初始化全域變數並保存到 ViewState
                        _invoiceData = GetInvoiceData(augmentedData);
                        _packingListData = GetPackingListData(augmentedData);

                        // 綁定到 GridView
                        gvINV.DataSource = _invoiceData;
                        gvINV.DataBind();

                        gvPLS.DataSource = _packingListData;
                        gvPLS.DataBind();

                        // 更新按鈕可見性
                        ToggleControlsVisibility();

                        lblStatus.Text = "File uploaded and processed successfully.";
                        ShowAlert(lblStatus.Text);
                    }
                    catch (Exception ex)
                    {
                        lblStatus.Text = "Error: " + ex.Message;
                        ShowAlert(lblStatus.Text);
                    }
                }
                else
                {
                    lblStatus.Text = "Only Excel files (.xls, .xlsx) are allowed.";
                    ShowAlert(lblStatus.Text);
                }
            }
            else
            {
                lblStatus.Text = "Please select a file to upload.";
                ShowAlert(lblStatus.Text);
            }
        }

        private void CleanExcelFile(string filePath)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Enrich_Testing");
            using (var package = new ExcelPackage(new System.IO.FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                if (worksheet == null) return;

                // 刪除前 5 行
                worksheet.DeleteRow(1, 5);

                // 刪除多餘的空行（檢查關鍵欄位是否都有值）
                int totalRows = worksheet.Dimension.End.Row;
                string[] criticalColumns = { "PO", "SKU" }; // 根據實際需求調整欄位名稱
                List<int> rowsToDelete = new List<int>();

                for (int row = totalRows; row >= 2; row--)
                {
                    bool hasValues = true;
                    foreach (string columnName in criticalColumns)
                    {
                        int colIndex = FindColumnIndex(worksheet, columnName);
                        if (colIndex > 0)
                        {
                            string cellValue = worksheet.Cells[row, colIndex]?.Text;
                            if (string.IsNullOrWhiteSpace(cellValue))
                            {
                                hasValues = false;
                                break; // 若有任一欄位為空，則不保留該行
                            }
                        }
                    }

                    if (!hasValues)
                    {
                        rowsToDelete.Add(row);
                    }
                }

                // 批量刪除行
                foreach (int row in rowsToDelete)
                {
                    worksheet.DeleteRow(row);
                }

                // 保存變更
                package.Save();
            }
        }

        private int FindColumnIndex(ExcelWorksheet worksheet, string columnName)
        {
            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                if (worksheet.Cells[1, col].Text.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return col;
                }
            }
            return -1;
        }

        private DataTable AugmentDataWithDatabase(DataTable dataTable)
        {
            // 增加擴充用的欄位
            if (!dataTable.Columns.Contains("GW")) dataTable.Columns.Add("GW", typeof(decimal));
            if (!dataTable.Columns.Contains("NW")) dataTable.Columns.Add("NW", typeof(decimal));
            if (!dataTable.Columns.Contains("CBM")) dataTable.Columns.Add("CBM", typeof(decimal));
            if (!dataTable.Columns.Contains("UnitPrice")) dataTable.Columns.Add("UnitPrice", typeof(decimal));
            if (!dataTable.Columns.Contains("TotalPrice")) dataTable.Columns.Add("TotalPrice", typeof(decimal));

            foreach (DataRow row in dataTable.Rows)
            {
                string prodCode = row["产品名称"]?.ToString();
                string sku = row["SKU"]?.ToString();

                if (!string.IsNullOrWhiteSpace(prodCode) && !string.IsNullOrWhiteSpace(sku))
                {
                    DataRow augmentedData = new CUS_DataAugmentation().GetAugmentedData(prodCode, sku);
                    if (augmentedData != null)
                    {
                        row["UnitPrice"] = augmentedData["UnitPrice"];
                        
                        decimal quantity = Convert.ToDecimal(row["数量"]);
                        decimal unitPrice = Convert.ToDecimal(augmentedData["UnitPrice"]);
                        decimal Product_cbm = Convert.ToDecimal(augmentedData["CBM"]);
                        decimal Product_GW = Convert.ToDecimal(augmentedData["GW"]);
                        decimal Product_NW = Convert.ToDecimal(augmentedData["NW"]);
                        row["TotalPrice"] = quantity * unitPrice;
                        row["CBM"] = quantity * Product_cbm;
                        row["GW"] = quantity * Product_GW;
                        row["NW"] = quantity * Product_NW;
                    }
                }
            }

            return dataTable;
        }

        private DataTable GetInvoiceData(DataTable augmentedData)
        {
            // 準備目標 DataTable
            DataTable invoiceData = new DataTable();
            invoiceData.Columns.Add("No");
            invoiceData.Columns.Add("PO_NO");
            invoiceData.Columns.Add("DESCRIPTION_OF_GOODS");
            invoiceData.Columns.Add("Prod_Code");
            invoiceData.Columns.Add("Sofa_Type");
            invoiceData.Columns.Add("QUANTITY");
            invoiceData.Columns.Add("UnitPrice");
            invoiceData.Columns.Add("Amount");

            int rowNo = 1;
            foreach (DataRow row in augmentedData.Rows)
            {
                DataRow newRow = invoiceData.NewRow();
                newRow["No"] = rowNo++;
                newRow["PO_NO"] = row["PO"]?.ToString();

                // DESCRIPTION_OF_GOODS 根據【面料】和【产品名称】
                string material = row["面料"]?.ToString();
                string productName = row["产品名称"]?.ToString();
                string description = $"{(material?.Contains("P") == true ? "CUSTOMIZED LEATHER SOFA" : "FABRIC SOFA")}";

                newRow["DESCRIPTION_OF_GOODS"] = description;
                newRow["Prod_code"] = productName;
                newRow["Sofa_Type"] =
                    (!string.IsNullOrEmpty(productName) && productName.ToUpper().Contains("WE")) ?
                    "FSC MIX 100%" : " ";
                // 數量
                newRow["QUANTITY"] = row["数量"]?.ToString();

                // 單價和總價
                newRow["UnitPrice"] = row["UnitPrice"]?.ToString();
                newRow["Amount"] = row["TotalPrice"]?.ToString();

                invoiceData.Rows.Add(newRow);
            }

            return invoiceData;
        }

        private DataTable GetPackingListData(DataTable augmentedData)
        {
            // 準備目標 DataTable
            DataTable packingListData = new DataTable();
            packingListData.Columns.Add("No");
            packingListData.Columns.Add("PO_NO");
            packingListData.Columns.Add("DESCRIPTION_OF_GOODS");
            packingListData.Columns.Add("Prod_Code");
            packingListData.Columns.Add("Sofa_Type");
            packingListData.Columns.Add("QUANTITY");
            packingListData.Columns.Add("GROSS_WT");
            packingListData.Columns.Add("NET_WT");
            packingListData.Columns.Add("CBM");

            int rowNo = 1;
            foreach (DataRow row in augmentedData.Rows)
            {
                DataRow newRow = packingListData.NewRow();
                newRow["No"] = rowNo++;
                newRow["PO_NO"] = row["PO"]?.ToString();

                // DESCRIPTION_OF_GOODS 根據【面料】和【产品名称】
                string material = row["面料"]?.ToString();
                string productName = row["产品名称"]?.ToString();
                string description = $"{(material?.Contains("P") == true ? "CUSTOMIZED LEATHER SOFA" : "FABRIC SOFA")} ";

                newRow["DESCRIPTION_OF_GOODS"] = description;
                newRow["Prod_code"] = productName;
                newRow["Sofa_Type"] =
                    (!string.IsNullOrEmpty(productName) && productName.ToUpper().Contains("WE")) ?
                    "FSC MIX 100%" : " ";
                // 數量
                newRow["QUANTITY"] = row["数量"]?.ToString();

                // 重量和體積
                newRow["GROSS_WT"] = row["GW"]?.ToString();
                newRow["NET_WT"] = row["NW"]?.ToString();
                newRow["CBM"] = row["CBM"]?.ToString();

                packingListData.Rows.Add(newRow);
            }

            return packingListData;
        }

        private void GenerateExcel(DataTable data, string templateSheetName, string outputFileName, string linkControlId)
        {
            string templatePath = Server.MapPath("~/App_Data/INV-PLS-Template.xlsx");

            if (!File.Exists(templatePath))
            {
                lblStatus.Text = $"{templateSheetName} template not found!";
                ShowAlert(lblStatus.Text);
                return;
            }

            string tempExcelPath = Server.MapPath($"~/App_Temp/{outputFileName}_{DateTime.Now.ToString("yyyyMMddhhmm")}.xlsx");
            
            try
            {
                if (data != null)
                {
                    string[] sHeadInfo = new string[4]
                    {
                        tbInvNo.Text, tbDate.Text, tbVessel.Text, _sDestinationPort  
                    };

                    new ExcelHelper().FillExcelTemplate(templatePath, sHeadInfo, data, 18, tempExcelPath, templateSheetName);

                    DownloadExcelFile(tempExcelPath);

                    //File.Delete(tempExcelPath);

                    lblStatus.Text = $"{templateSheetName} Excel generated successfully.";
                    ShowAlert(lblStatus.Text);
                }
                else
                {
                    lblStatus.Text = $"No {templateSheetName} data available for Excel generation.";
                    ShowAlert(lblStatus.Text);
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Error generating {templateSheetName} Excel: {ex.Message}";
                ShowAlert(lblStatus.Text);
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

        protected void btnGenerate_INV_PDF_Click(object sender, EventArgs e)
        {
            GenerateExcel(_invoiceData, "INV", "Invoice", "lnkInvoice");
        }

        protected void btnGenerate_PLS_PDF_Click(object sender, EventArgs e)
        {
            GenerateExcel(_packingListData, "PLS", "PackingList", "lnkPackingList");
        }

        private void ToggleControlsVisibility()
        {
            btnGenerate_INV_PDF.Visible = gvINV.Rows.Count > 0;
            btnGenerate_PLS_PDF.Visible = gvPLS.Rows.Count > 0;
        }

        private void DownloadExcelFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found for download.", filePath);
            }

            string fileName = Path.GetFileName(filePath);

            // 設置 HTTP 響應頭，讓瀏覽器觸發下載
            Response.Clear();
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"; // MIME 類型
            Response.AddHeader("Content-Disposition", $"attachment; filename={fileName}");
            Response.WriteFile(filePath);
            Response.End(); // 結束響應
        }

        protected void Calendar1_SelectionChanged(object sender, EventArgs e)
        {
            CultureInfo cultureInfo = new CultureInfo("en-US");
            tbDate.Text = Calendar1.SelectedDate.ToString("dd MMM yyyy", cultureInfo);

            Calendar1.Style["display"] = "none";
        }

    }
}
