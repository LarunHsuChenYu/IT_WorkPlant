using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IT_WorkPlant.Models
{
	public partial class IT_PurchaseItemsList : System.Web.UI.Page
	{
        private readonly MssqlDatabaseHelper _db = new MssqlDatabaseHelper();
        protected void Page_Load(object sender, EventArgs e)
		{
            if (!IsPostBack)
            {
                LoadGrid();
                SetPageLabels();
            }
        }

        private void LoadGrid()
        {
            string query = "SELECT * FROM IT_PurchaseItems ORDER BY ItemName";
            gvItems.DataSource = _db.ExecuteQuery(query, null);
            gvItems.DataBind();
        }


        protected void btnAddNew_Click(object sender, EventArgs e)
        {
            pnlAddItem.Visible = true;
        }

        protected void btnDeleteSelected_Click(object sender, EventArgs e)
        {

        }

        protected void btnSaveItem_Click(object sender, EventArgs e)
        {
            var values = new Dictionary<string, object>
            {
                {"ItemName", txtItemName.Text.Trim() },
                {"Category", txtCategory.Text.Trim() },
                {"Unit", txtUnit.Text.Trim() },
                {"UnitPrice", txtUnitPrice.Text.Trim() },
                {"Description", txtDescription.Text.Trim() },
                {"Status", chkStatus.Checked }
            };

            _db.InsertData("IT_PurchaseItems", values);
            LoadGrid();
            ClearForm();
            pnlAddItem.Visible = false;
        }

        protected void btnCancelAdd_Click(object sender, EventArgs e)
        {
            ClearForm();
            pnlAddItem.Visible = false;
        }

        protected void gvItems_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvItems.EditIndex = e.NewEditIndex;
            LoadGrid();
        }

        protected void gvItems_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvItems.EditIndex = -1;
            LoadGrid();
        }

        protected void gvItems_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            int itemId = Convert.ToInt32(gvItems.DataKeys[e.RowIndex].Value);
            GridViewRow row = gvItems.Rows[e.RowIndex];

            string itemName = ((TextBox)row.Cells[1].Controls[0]).Text;
            string category = ((TextBox)row.Cells[2].Controls[0]).Text;
            string unit = ((TextBox)row.Cells[3].Controls[0]).Text;
            string price = ((TextBox)row.Cells[4].Controls[0]).Text;
            bool status = ((CheckBox)row.Cells[5].Controls[0]).Checked;
            string description = ((TextBox)row.Cells[6].Controls[0]).Text;

            string query = @"UPDATE IT_PurchaseItems 
                             SET ItemName=@ItemName, Category=@Category, Unit=@Unit, UnitPrice=@UnitPrice, Status=@Status, Description=@Description 
                             WHERE ItemID=@ItemID";

            var parameters = new[] {
                new System.Data.SqlClient.SqlParameter("@ItemID", itemId),
                new System.Data.SqlClient.SqlParameter("@ItemName", itemName),
                new System.Data.SqlClient.SqlParameter("@Category", category),
                new System.Data.SqlClient.SqlParameter("@Unit", unit),
                new System.Data.SqlClient.SqlParameter("@UnitPrice", price),
                new System.Data.SqlClient.SqlParameter("@Status", status),
                new System.Data.SqlClient.SqlParameter("@Description", description)
            };

            _db.ExecuteNonQuery(query, parameters);
            gvItems.EditIndex = -1;
            LoadGrid();
        }

        protected void gvItems_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int itemId = Convert.ToInt32(gvItems.DataKeys[e.RowIndex].Value);
            string query = "DELETE FROM IT_PurchaseItems WHERE ItemID=@ItemID";
            var param = new[] { new System.Data.SqlClient.SqlParameter("@ItemID", itemId) };
            _db.ExecuteNonQuery(query, param);
            LoadGrid();
        }

        private void ClearForm()
        {
            txtItemName.Text = txtCategory.Text = txtUnit.Text = txtUnitPrice.Text = txtDescription.Text = "";
            chkStatus.Checked = true;
        }

        private void SetPageLabels()
        {
            litPageTitle.Text = "IT Purchase Items Management";
            litBreadcrumb.Text = "Home > Management > IT Items";

            //litPageTitle.Text = GetLocalResourceObject("PageTitle")?.ToString() ?? "IT Purchase Items Management";
            //litBreadcrumb.Text = GetLocalResourceObject("Breadcrumb")?.ToString() ?? "Home > Management > IT Items";
        }
    }
}