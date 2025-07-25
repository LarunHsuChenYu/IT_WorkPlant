﻿using IT_WorkPlant.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IT_WorkPlant.Pages
{
    public partial class IT_Stuff_Purchase : Page
    {
        private readonly MssqlDatabaseHelper _db = new MssqlDatabaseHelper();
        private readonly UserInfo _ui = new UserInfo();

        private DataTable PurchaseItemsTable
        {
            get
            {
                if (ViewState["PurchaseItems"] == null)
                {
                    var dt = new DataTable();
                    dt.Columns.Add("ItemID");
                    dt.Columns.Add("Usage");
                    dt.Columns.Add("Qty");
                    ViewState["PurchaseItems"] = dt;
                }
                return (DataTable)ViewState["PurchaseItems"];
            }
            set
            {
                ViewState["PurchaseItems"] = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserEmpID"] == null)
            {
                Response.Redirect("../Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                txtEmpName.Text = Session["UserName"]?.ToString();
                txtDept.Text = Session["DeptName"]?.ToString();
                txtDate.Text = DateTime.Now.ToString("yyyy/MM/dd");

                var dt = PurchaseItemsTable;
                for (int i = dt.Rows.Count; i < 5; i++)
                {
                    dt.Rows.Add(dt.NewRow());
                }
                BindGrid();
            }
        }

        private void BindGrid()
        {
            var dt = PurchaseItemsTable;
            gvPurchaseItems.DataSource = dt;
            gvPurchaseItems.DataBind();
        }

        protected void gvPurchaseItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DropDownList ddlCategory = (DropDownList)e.Row.FindControl("ddlCategory");
                if (ddlCategory != null)
                {
                    ddlCategory.AutoPostBack = true;
                    ddlCategory.SelectedIndexChanged += ddlCategory_SelectedIndexChanged;

                    string categoryQuery = "SELECT DISTINCT Category FROM IT_PurchaseItems WHERE Status = 1 ORDER BY Category";
                    DataTable categories = _db.ExecuteQuery(categoryQuery, null);

                    ddlCategory.DataSource = categories;
                    ddlCategory.DataTextField = "Category";
                    ddlCategory.DataValueField = "Category";
                    ddlCategory.DataBind();
                    ddlCategory.Items.Insert(0, new ListItem("-- Select --", ""));
                }

                DropDownList ddlItem = (DropDownList)e.Row.FindControl("ddlItem");
                if (ddlItem != null)
                {
                    ddlItem.AutoPostBack = true;
                    ddlItem.SelectedIndexChanged += ddlItem_SelectedIndexChanged;

                    string query = "SELECT ItemID, ItemName FROM IT_PurchaseItems WHERE Status = 1 ORDER BY ItemName";
                    DataTable items = _db.ExecuteQuery(query, null);

                    ddlItem.DataSource = items;
                    ddlItem.DataTextField = "ItemName";
                    ddlItem.DataValueField = "ItemID";
                    ddlItem.DataBind();
                    ddlItem.Items.Insert(0, new ListItem("-- Select --", ""));
                }

                TextBox txtQty = (TextBox)e.Row.FindControl("txtQty");
                if (txtQty != null)
                {
                    txtQty.AutoPostBack = true;
                    txtQty.TextChanged += txtQty_TextChanged;
                }
            }
        }

        protected void ddlCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddlCategory = (DropDownList)sender;
            GridViewRow row = (GridViewRow)ddlCategory.NamingContainer;
            DropDownList ddlItem = (DropDownList)row.FindControl("ddlItem");
            if (ddlItem == null) return;

            string selectedCategory = ddlCategory.SelectedValue;
            if (string.IsNullOrEmpty(selectedCategory)) return;

            string query = "SELECT ItemID, ItemName FROM IT_PurchaseItems WHERE Category = @Category AND Status = 1 ORDER BY ItemName";
            var param = new[] {
                new System.Data.SqlClient.SqlParameter("@Category", selectedCategory)
            };

            DataTable items = _db.ExecuteQuery(query, param);
            ddlItem.DataSource = items;
            ddlItem.DataTextField = "ItemName";
            ddlItem.DataValueField = "ItemID";
            ddlItem.DataBind();
            ddlItem.Items.Insert(0, new ListItem("-- Select --", ""));
        }

        protected void ddlItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            DropDownList ddl = (DropDownList)sender;
            GridViewRow row = (GridViewRow)ddl.NamingContainer;

            Label lblUnit = (Label)row.FindControl("lblUnit");
            Label lblPrice = (Label)row.FindControl("lblPrice");
            Label lblSubtotal = (Label)row.FindControl("lblSubtotal");
            TextBox txtQty = (TextBox)row.FindControl("txtQty");

            if (!int.TryParse(ddl.SelectedValue, out int itemId)) return;

            DataTable itemDt = _db.ExecuteQuery("SELECT Unit, UnitPrice FROM IT_PurchaseItems WHERE ItemID = @ItemID",
                new[] { new System.Data.SqlClient.SqlParameter("@ItemID", itemId) });

            if (itemDt.Rows.Count == 0) return;

            var item = itemDt.Rows[0];
            lblUnit.Text = item["Unit"].ToString();
            lblPrice.Text = Convert.ToDecimal(item["UnitPrice"]).ToString("N2");

            if (int.TryParse(txtQty.Text, out int qty))
            {
                decimal price = Convert.ToDecimal(item["UnitPrice"]);
                lblSubtotal.Text = (price * qty).ToString("N2");
            }
        }

        protected void txtQty_TextChanged(object sender, EventArgs e)
        {
            TextBox txtQty = (TextBox)sender;
            GridViewRow row = (GridViewRow)txtQty.NamingContainer;

            DropDownList ddlItem = (DropDownList)row.FindControl("ddlItem");
            Label lblPrice = (Label)row.FindControl("lblPrice");
            Label lblSubtotal = (Label)row.FindControl("lblSubtotal");

            if (!int.TryParse(ddlItem.SelectedValue, out int itemId)) return;
            if (!int.TryParse(txtQty.Text, out int qty)) qty = 0;

            DataTable itemDt = _db.ExecuteQuery("SELECT UnitPrice FROM IT_PurchaseItems WHERE ItemID = @ItemID",
                new[] { new System.Data.SqlClient.SqlParameter("@ItemID", itemId) });

            if (itemDt.Rows.Count == 0) return;
            decimal price = Convert.ToDecimal(itemDt.Rows[0]["UnitPrice"]);
            lblPrice.Text = price.ToString("N2");
            lblSubtotal.Text = (price * qty).ToString("N2");
        }

        protected void btnAddRow_Click(object sender, EventArgs e)
        {
            var dt = PurchaseItemsTable;

            foreach (GridViewRow row in gvPurchaseItems.Rows)
            {
                DropDownList ddlItem = (DropDownList)row.FindControl("ddlItem");
                TextBox txtUsage = (TextBox)row.FindControl("txtUsage");
                TextBox txtQty = (TextBox)row.FindControl("txtQty");

                if (row.RowIndex >= dt.Rows.Count)
                    continue;

                DataRow dr = dt.Rows[row.RowIndex];
                dr["ItemID"] = ddlItem?.SelectedValue ?? "";
                dr["Usage"] = txtUsage?.Text ?? "";
                dr["Qty"] = txtQty?.Text ?? "";
            }

            dt.Rows.Add(dt.NewRow());
            PurchaseItemsTable = dt;
            BindGrid();
        }

        protected async void btnSubmit_Click(object sender, EventArgs e)
        {
            string userName = Session["UserName"]?.ToString();
            string empId = Session["UserEmpID"]?.ToString();
            int? userId = _ui.GetRequestUserID(userName, empId);
            if (userId == null)
            {
                ShowAlert("Cannot identify current user.");
                return;
            }

            string reason = "";
            string summary = "";
            decimal total = 0;

            foreach (GridViewRow row in gvPurchaseItems.Rows)
            {
                DropDownList ddlItem = (DropDownList)row.FindControl("ddlItem");
                TextBox txtUsage = (TextBox)row.FindControl("txtUsage");
                TextBox txtQty = (TextBox)row.FindControl("txtQty");

                if (!int.TryParse(ddlItem.SelectedValue, out int itemId) || itemId == 0)
                    continue;

                DataTable itemDt = _db.ExecuteQuery("SELECT * FROM IT_PurchaseItems WHERE ItemID = @ItemID",
                    new[] { new System.Data.SqlClient.SqlParameter("@ItemID", itemId) });

                if (itemDt.Rows.Count == 0) continue;
                var item = itemDt.Rows[0];
                string itemName = item["ItemName"].ToString();
                string unit = item["Unit"].ToString();
                decimal price = Convert.ToDecimal(item["UnitPrice"]);
                int qty = int.TryParse(txtQty.Text, out int q) && q > 0 ? q : 1;
                txtQty.Text = qty.ToString();

                decimal subtotal = qty * price;
                total += subtotal;

                summary += $"- {itemName} ({unit}) x {qty} = {subtotal:N0}\n usage: {txtUsage.Text}\n";
            }

            if (string.IsNullOrWhiteSpace(summary))
            {
                ShowAlert("No items selected.");
                return;
            }

            var values = new Dictionary<string, object>
            {
                { "IssueDate", DateTime.Now },
                { "DeptNameID", Session["DeptName"].ToString() },
                { "CompanyID", "ENR" },
                { "RequestUserID", userId },
                { "IssueDetails", reason },
                { "IssueTypeID", 6 },
                { "Status", false },
                { "LastUpdateDate", DateTime.Now },
                { "DRI_UserID", DBNull.Value },
                { "Solution", summary },
                { "FinishedDate", DBNull.Value },
                { "Remark", DBNull.Value }
            };

            _db.InsertData("IT_RequestList", values);
            ShowAlert("Request submitted successfully.");

            try
            {
                var notifier = new LineNotificationModel();
                string lineGroupId = ConfigurationManager.AppSettings["LineGroupID"];
                string lineMessage = "【IT Purchase Request】\r\n"
                    + $"Date: {DateTime.Now:yyyy / MM / dd} \r\n"
                    + $"Dept: {Session["DeptName"]} \r\n"
                    + $"User: {Session["UserName"]} \r\n"
                    + $"Reason: {reason} \r\n"
                    + $"Total: {total:N0}฿/unit \r\n"
                    + $"Items: {summary}";

                await notifier.SendLineGroupMessageAsync(lineGroupId, lineMessage);
            }
            catch (Exception ex)
            {
                ShowAlert("LINE notification failed: " + ex.Message);
            }

            ShowAlertAndRedirect("Request submitted successfully.", ResolveUrl("~/Default.aspx"));
        }

        private void ShowAlert(string msg)
        {
            string safeMsg = System.Web.HttpUtility.JavaScriptStringEncode(msg);
            string script = "alert('" + safeMsg + "');";
            ClientScript.RegisterStartupScript(this.GetType(), "alert", script, true);
        }

        private void ShowAlertAndRedirect(string message, string redirectUrl)
        {
            string safeMessage = System.Web.HttpUtility.JavaScriptStringEncode(message);
            string script = $"alert('{safeMessage}'); window.location='{redirectUrl}';";
            ClientScript.RegisterStartupScript(this.GetType(), "alertRedirect", script, true);
        }
    }
}
