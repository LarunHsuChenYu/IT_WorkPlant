using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using IT_WorkPlant.Models;

namespace IT_WorkPlant.Pages
{
    public partial class IT_StockItems : Page
    {
        private readonly MssqlDatabaseHelper db = new MssqlDatabaseHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["username"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            lblUsername.Text = Session["username"].ToString();

            txtSearch.Attributes["placeholder"] = GetLabel("searchplaceholder");
            btnSearch.Text = "🔍 " + GetLabel("search");

            if (Session["ForceRebindHeader"] != null)
            {
                gvStockItems.DataSource = new DataTable();
                gvStockItems.DataBind();
                Session["ForceRebindHeader"] = null;
            }

            if (!IsPostBack)
            {
                Session["FilterMode"] = "all";
                lblTotalItems.Text = CountAllItems().ToString();
                lblNeedsCount.Text = CountNeeds().ToString();
                LoadStockItems();
            }
        }
        protected void ddlLang_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            Session["ForceRebindHeader"] = true;
            Response.Redirect(Request.RawUrl);
        }

        protected void gvStockItems_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvStockItems.PageIndex = e.NewPageIndex;
            LoadStockItems();
        }

        private void LoadStockItems()
        {
            string connStr = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string keyword = txtSearch.Text.Trim();
                string mode = Session["FilterMode"]?.ToString() ?? "all";

                string query = @"
SELECT 
    s.ItemID, 
    s.ProductName, 
    s.Model, 
    s.Unit, 
    s.MinimumQty,
    s.InventoryQty, 
    s.ReplenishQty, 
    s.UnitCost,
    (s.InventoryQty * s.UnitCost) AS InventoryValue,
    s.CreateDate,
    r.LatestReceiveDate,
    r.ReceivedBy
FROM IT_StockItems s
LEFT JOIN (
    SELECT ProductCode, 
           MAX(CreatedDate) AS LatestReceiveDate,
           MAX(CreatedBy) AS ReceivedBy
    FROM IT_StockReceive
    GROUP BY ProductCode
) r ON CAST(s.ItemID AS NVARCHAR) = r.ProductCode
WHERE (@keyword = '' OR s.ProductName LIKE '%' + @keyword + '%')
";


                if (mode == "needs")
                {
                    query += " AND InventoryQty <= 0";
                }

                query += " ORDER BY ItemID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@keyword", keyword);

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                // หลังจาก da.Fill(dt);
                foreach (DataRow row in dt.Rows)
                {
                    int minimumQty = Convert.ToInt32(row["MinimumQty"]);
                    int inventoryQty = Convert.ToInt32(row["InventoryQty"]);

                    int replenishQty = (inventoryQty < minimumQty) ? (minimumQty - inventoryQty) : 0;
                    row["ReplenishQty"] = replenishQty;
                }

                gvStockItems.DataSource = dt;
                gvStockItems.DataBind();
            }
        }

        private int CountAllItems()
        {
            string connStr = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT COUNT(*) FROM IT_StockItems";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        private int CountNeeds()
        {
            string connStr = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT COUNT(*) FROM IT_StockItems WHERE InventoryQty <= 0";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                return (int)cmd.ExecuteScalar();
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            gvStockItems.PageIndex = 0;
            LoadStockItems();
        }

        protected void btnShowNeeds_Click(object sender, EventArgs e)
        {
            Session["FilterMode"] = "needs";
            gvStockItems.PageIndex = 0;
            LoadStockItems();
            lblTotalItems.Text = CountAllItems().ToString();
            lblNeedsCount.Text = CountNeeds().ToString();
        }

        protected void btnShowAll_Click(object sender, EventArgs e)
        {
            Session["FilterMode"] = "all";
            gvStockItems.PageIndex = 0;
            LoadStockItems();
            lblTotalItems.Text = CountAllItems().ToString();
            lblNeedsCount.Text = CountNeeds().ToString();
        }

        protected void gvStockItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // ✅ ลำดับแถว
                int rowIndex = e.Row.RowIndex + 1 + (gvStockItems.PageIndex * gvStockItems.PageSize);
                Label lblRowNumber = (Label)e.Row.FindControl("lblRowNumber");
                if (lblRowNumber != null)
                {
                    lblRowNumber.Text = rowIndex.ToString();
                }

                // ✅ เช็คค่า ReplenishQty
                int replenishQty = Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "ReplenishQty"));

                // ✅ เซลล์คอลัมน์ "ต้องเติมเพิ่ม" อยู่ลำดับที่ 6 (เริ่มจาก 0)
                // 👉 ตรวจสอบชื่อหัวตารางเพื่อความชัวร์ว่า index ตรงไหม
                int replenishColumnIndex = -1;
                for (int i = 0; i < gvStockItems.Columns.Count; i++)
                {
                    if (gvStockItems.Columns[i].HeaderText == "ต้องเติมเพิ่ม")
                    {
                        replenishColumnIndex = i;
                        break;
                    }
                }

                if (replenishColumnIndex >= 0 && replenishQty > 0)
                {
                    e.Row.Cells[replenishColumnIndex].BackColor = System.Drawing.Color.FromArgb(255, 230, 230); // แดงอ่อน
                    e.Row.Cells[replenishColumnIndex].ForeColor = System.Drawing.Color.Red; // ตัวอักษรแดง
                    e.Row.Cells[replenishColumnIndex].Font.Bold = true;
                }
            }
        }
        protected void gvStockItems_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvStockItems.EditIndex = e.NewEditIndex;
            LoadStockItems();
        }
        protected void gvStockItems_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvStockItems.EditIndex = -1;
            LoadStockItems();
        }
        protected void gvStockItems_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridViewRow row = gvStockItems.Rows[e.RowIndex];
            string itemId = gvStockItems.DataKeys[e.RowIndex].Value.ToString();

            int minimumQty = Convert.ToInt32(((TextBox)row.FindControl("txtMinimumQty")).Text);
            int inventoryQty = Convert.ToInt32(((TextBox)row.FindControl("txtInventoryQty")).Text);
            decimal unitCost = Convert.ToDecimal(((TextBox)row.FindControl("txtUnitCost")).Text);

            // ✅ คำนวณ ReplenishQty อัตโนมัติ
            int replenishQty = (minimumQty > inventoryQty) ? (minimumQty - inventoryQty) : 0;

            string connStr = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = @"
        UPDATE IT_StockItems
        SET MinimumQty = @MinimumQty,
            InventoryQty = @InventoryQty,
            ReplenishQty = @ReplenishQty,
            UnitCost = @UnitCost
        WHERE ItemID = @ItemID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MinimumQty", minimumQty);
                cmd.Parameters.AddWithValue("@InventoryQty", inventoryQty);
                cmd.Parameters.AddWithValue("@ReplenishQty", replenishQty);
                cmd.Parameters.AddWithValue("@UnitCost", unitCost);
                cmd.Parameters.AddWithValue("@ItemID", itemId);

                conn.Open();
                cmd.ExecuteNonQuery();
            }

            gvStockItems.EditIndex = -1;
            LoadStockItems();
        }
        protected string GetLabel(string key)
        {
            string lang = Session["lang"]?.ToString() ?? "th";

            var th = new Dictionary<string, string> {
        { "title", "รายการสินค้าในคลัง" },
        { "search", "ค้นหา" },
        { "showall", "แสดงทั้งหมด" },
        { "showneeds", "เฉพาะสินค้าที่ต้องเติม" },
        { "totalitems", "จำนวนรายการทั้งหมด" },
        { "needsreplenish", "จำนวนสินค้าที่ต้องเติม" },
        { "no", "ลำดับ" },
        { "productname", "ชื่อสินค้า" },
        { "model", "รุ่น" },
        { "unit", "หน่วย" },
        { "minimumqty", "ขั้นต่ำ" },
        { "inventoryqty", "คงเหลือในคลัง" },
        { "replenishqty", "ต้องเติมเพิ่ม" },
        { "unitcost", "ราคาต่อหน่วย" },
        { "inventoryvalue", "มูลค่าคงคลัง" },
        { "createdate", "วันที่เพิ่ม" },
        { "latestreceivedate", "วันที่รับเข้าล่าสุด" },
        { "receivedby", "ผู้รับล่าสุด" },
        { "edit", "แก้ไข" },
        { "save", "บันทึก" },
        { "cancel", "ยกเลิก" },
        { "receive", "รับของเข้า" },
        { "receivenote", "เพิ่มสินค้าลงคลัง" },
        { "issue", "เบิกของออก" },
        { "issuenote", "จ่ายออกจากคลัง" }
    };

            var en = new Dictionary<string, string> {
        { "title", "Stock Item List" },
        { "search", "Search" },
        { "showall", "Show All" },
        { "showneeds", "Show Only Items to Replenish" },
        { "totalitems", "Total Items" },
        { "needsreplenish", "Items Needing Replenishment" },
        { "no", "No." },
        { "productname", "Product Name" },
        { "model", "Model" },
        { "unit", "Unit" },
        { "minimumqty", "Minimum Qty" },
        { "inventoryqty", "Inventory Qty" },
        { "replenishqty", "To Replenish" },
        { "unitcost", "Unit Cost" },
        { "inventoryvalue", "Inventory Value" },
        { "createdate", "Created Date" },
        { "latestreceivedate", "Latest Receive Date" },
        { "receivedby", "Received By" },
        { "edit", "Edit" },
        { "save", "Save" },
        { "cancel", "Cancel" },
        { "receive", "Receive" },
        { "receivenote", "Add items to stock" },
        { "issue", "Issue" },
        { "issuenote", "Deduct from stock" }

    };

            var zh = new Dictionary<string, string> {
        { "title", "庫存項目列表" },
        { "search", "搜尋" },
        { "showall", "顯示全部" },
        { "showneeds", "僅顯示需補貨項目" },
        { "totalitems", "總項目數" },
        { "needsreplenish", "需補貨項目" },
        { "no", "編號" },
        { "productname", "產品名稱" },
        { "model", "型號" },
        { "unit", "單位" },
        { "minimumqty", "最低庫存" },
        { "inventoryqty", "目前庫存" },
        { "replenishqty", "需補數量" },
        { "unitcost", "單價" },
        { "inventoryvalue", "庫存價值" },
        { "createdate", "建立日期" },
        { "latestreceivedate", "最近入庫日期" },
        { "receivedby", "最近入庫者" },
        { "edit", "編輯" },
        { "save", "儲存" },
        { "cancel", "取消" },
        { "receive", "入庫" },
        { "receivenote", "新增庫存項目" },
        { "issue", "出庫" },
        { "issuenote", "從庫存扣除" }

    };
            Dictionary<string, string> dict;
            if (lang == "en")
            {
                dict = en;
            }
            else if (lang == "zh")
            {
                dict = zh;
            }
            else
            {
                dict = th;
            }

            return dict.ContainsKey(key) ? dict[key] : key;
        }
        protected void gvStockItems_RowCreated(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                for (int i = 0; i < gvStockItems.Columns.Count; i++)
                {
                    if (gvStockItems.Columns[i] is TemplateField tf)
                    {
                        string originalKey = tf.HeaderText?.Trim().ToLower();
                        tf.HeaderText = GetLabel(originalKey);
                    }
                }
            }
        }


    }
}
