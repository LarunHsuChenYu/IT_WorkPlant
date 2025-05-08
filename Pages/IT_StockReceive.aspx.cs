using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;

namespace IT_WorkPlant.Pages
{
    public partial class IT_StockReceive : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["username"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                LoadProductCodes();
                txtReceiveDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                LoadReceiveHistory();
            }
        }

        private void LoadProductCodes()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT ItemID, ProductName FROM IT_StockItems ORDER BY ProductName";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                ddlProductCode.Items.Clear();
                ddlProductCode.Items.Add(new System.Web.UI.WebControls.ListItem("เลือก", ""));
                while (reader.Read())
                {
                    string itemId = reader["ItemID"].ToString();
                    string name = reader["ProductName"].ToString();
                    ddlProductCode.Items.Add(new System.Web.UI.WebControls.ListItem(name, itemId));
                }

                reader.Close();
            }
        }

        protected void ddlMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            string mode = ddlMode.SelectedValue;

            pnlSelectExisting.Visible = (mode == "existing");
            pnlNewProduct.Visible = (mode == "new");

            txtProductName.Text = "";
            txtModel.Text = "";
            txtUnit.Text = "";
            txtNewProductName.Text = "";
            txtNewModel.Text = "";
            txtNewUnit.Text = "";
        }

        protected void ddlProductCode_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedCode = ddlProductCode.SelectedValue;
            if (string.IsNullOrEmpty(selectedCode)) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT ProductName, Model, Unit, UnitCost FROM IT_StockItems WHERE ItemID = @ItemID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@ItemID", selectedCode);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    txtProductName.Text = reader["ProductName"].ToString();
                    txtModel.Text = reader["Model"].ToString();
                    txtUnit.Text = reader["Unit"].ToString();
                    txtUnitCost.Text = reader["UnitCost"].ToString();
                }
                reader.Close();
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    string mode = ddlMode.SelectedValue;
                    string productCode = "";
                    string productName = "";
                    string model = "";
                    string unit = "";
                    string source = "";
                    string receiveType = ddlReceiveType.SelectedValue;
                    string referenceNo = txtReference.Text.Trim();
                    string remarks = "";
                    string createdBy = Session["username"]?.ToString() ?? "unknown";

                    int quantity = 0;
                    int minimumQty = 0;
                    decimal unitCost = 0;

                    if (mode == "new")
                    {
                        productName = txtNewProductName.Text.Trim();
                        model = txtNewModel.Text.Trim();
                        unit = txtNewUnit.Text.Trim();
                        source = txtNewSource.Text.Trim();
                        remarks = txtNewRemarks.Text.Trim();

                        string qtyText = txtNewQuantity.Text.Trim().Replace(",", "").Replace(" ", "");
                        string costText = txtNewUnitCost.Text.Trim().Replace(",", "").Replace(" ", "");
                        string minText = txtMinimumQty.Text.Trim().Replace(",", "").Replace(" ", "");

                        if (!int.TryParse(qtyText, out quantity) || quantity <= 0)
                        {
                            throw new Exception("กรุณากรอกจำนวนให้ถูกต้องและมากกว่า 0");
                        }
                        if (!decimal.TryParse(costText, out unitCost) || unitCost < 0)
                        {
                            throw new Exception("กรุณากรอกราคาต่อชิ้นให้ถูกต้อง");
                        }
                        int.TryParse(minText, out minimumQty);

                        string insertItem = @"
                    INSERT INTO IT_StockItems 
                    (ProductName, Model, Unit, MinimumQty, UnitCost, InventoryQty, ReplenishQty, CreateDate)
                    VALUES 
                    (@ProductName, @Model, @Unit, @MinimumQty, @UnitCost, @InventoryQty, 0, GETDATE());
                    SELECT SCOPE_IDENTITY();";

                        SqlCommand cmdNew = new SqlCommand(insertItem, conn, tran);
                        cmdNew.Parameters.AddWithValue("@ProductName", productName);
                        cmdNew.Parameters.AddWithValue("@Model", model);
                        cmdNew.Parameters.AddWithValue("@Unit", unit);
                        cmdNew.Parameters.AddWithValue("@MinimumQty", minimumQty);
                        cmdNew.Parameters.AddWithValue("@UnitCost", unitCost);
                        cmdNew.Parameters.AddWithValue("@InventoryQty", quantity);

                        object newId = cmdNew.ExecuteScalar();
                        int newItemId = Convert.ToInt32(newId);
                        productCode = newItemId.ToString();
                    }
                    else if (mode == "existing")
                    {
                        productCode = ddlProductCode.SelectedValue;
                        productName = txtProductName.Text;
                        model = txtModel.Text;
                        unit = txtUnit.Text;
                        source = txtSource.Text.Trim();
                        remarks = txtRemarks.Text.Trim();

                        string qtyText = txtQuantity.Text.Trim().Replace(",", "").Replace(" ", "");
                        string costText = txtUnitCost.Text.Trim().Replace(",", "").Replace(" ", "");

                        if (!int.TryParse(qtyText, out quantity) || quantity <= 0)
                        {
                            throw new Exception("กรุณากรอกจำนวนให้ถูกต้องและมากกว่า 0");
                        }
                        if (!decimal.TryParse(costText, out unitCost) || unitCost < 0)
                        {
                            throw new Exception("กรุณากรอกราคาต่อชิ้นให้ถูกต้อง");
                        }

                        string updateQty = "UPDATE IT_StockItems SET InventoryQty = InventoryQty + @Qty WHERE ItemID = @ItemID";
                        SqlCommand cmdUpdate = new SqlCommand(updateQty, conn, tran);
                        cmdUpdate.Parameters.AddWithValue("@Qty", quantity);
                        cmdUpdate.Parameters.AddWithValue("@ItemID", productCode);
                        cmdUpdate.ExecuteNonQuery();

                        minimumQty = 0;
                        referenceNo = "";
                    }

                    // ✅ Insert ลง IT_StockReceive
                    string insertReceive = @"
                INSERT INTO IT_StockReceive
                (ReceiveDate, ProductCode, ProductName, Model, Quantity, Unit, UnitCost, Source, ReceiveType, ReferenceNo, Remarks, CreatedBy)
                VALUES
                (@ReceiveDate, @ProductCode, @ProductName, @Model, @Quantity, @Unit, @UnitCost, @Source, @ReceiveType, @ReferenceNo, @Remarks, @CreatedBy)";

                    SqlCommand cmdReceive = new SqlCommand(insertReceive, conn, tran);
                    cmdReceive.Parameters.AddWithValue("@ReceiveDate", Convert.ToDateTime(txtReceiveDate.Text));
                    cmdReceive.Parameters.AddWithValue("@ProductCode", productCode);
                    cmdReceive.Parameters.AddWithValue("@ProductName", productName);
                    cmdReceive.Parameters.AddWithValue("@Model", model);
                    cmdReceive.Parameters.AddWithValue("@Quantity", quantity);
                    cmdReceive.Parameters.AddWithValue("@Unit", unit);
                    cmdReceive.Parameters.AddWithValue("@UnitCost", unitCost);
                    cmdReceive.Parameters.AddWithValue("@Source", source);
                    cmdReceive.Parameters.AddWithValue("@ReceiveType", receiveType);
                    cmdReceive.Parameters.AddWithValue("@ReferenceNo", referenceNo);
                    cmdReceive.Parameters.AddWithValue("@Remarks", remarks);
                    cmdReceive.Parameters.AddWithValue("@CreatedBy", createdBy);
                    cmdReceive.ExecuteNonQuery();

                    tran.Commit();
                    ScriptManager.RegisterStartupScript(this, GetType(), "Success", "alert('รับของเข้าสำเร็จแล้ว!'); window.location='IT_StockItems.aspx';", true);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    ScriptManager.RegisterStartupScript(this, GetType(), "Error", $"alert('เกิดข้อผิดพลาด: {ex.Message}');", true);
                }
            }
        }
        private void LoadReceiveHistory()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT TOP 10 
                ReceiveDate,
                ProductName,
                Model,
                Quantity,
                Unit,
                UnitCost,
                (Quantity * UnitCost) AS TotalPrice,
                Source, -- ✅ เพิ่มบรรทัดนี้
                CreatedBy
            FROM IT_StockReceive
            ORDER BY ReceiveDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvHistory.DataSource = dt;
                gvHistory.DataBind();
            }
        }
    }
}
