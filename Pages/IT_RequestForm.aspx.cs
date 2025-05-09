﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using IT_WorkPlant.Models;

namespace IT_WorkPlant.Pages
{
    public partial class IT_RequestForm : System.Web.UI.Page
    {
        private readonly MssqlDatabaseHelper _dbHelper = new MssqlDatabaseHelper();
        private readonly UserInfo _ui = new UserInfo();

        private string UserName => Session["UserName"]?.ToString() ?? "";
        private string UserEmpID => Session["UserEmpID"]?.ToString() ?? "";
        private string DeptName => Session["DeptName"]?.ToString() ?? "";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (string.IsNullOrEmpty(UserEmpID))
                {
                    Response.Redirect("../Login.aspx");
                }

                txtDate.Text = DateTime.Now.ToString("yyyy/MM/dd");
                txtName.Text = UserName;
                txtDept.Text = DeptName;

                BindEmptyRows(); // เติมแถวว่าง 5 แถว
            }
        }

        private void BindEmptyRows()
        {
            var emptyRows = new List<object>();
            for (int i = 0; i < 5; i++)
                emptyRows.Add(new object());

            rptRequestItems.DataSource = emptyRows;
            rptRequestItems.DataBind();
        }

        protected void rptRequestItems_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var ddl = e.Item.FindControl("ddlIssueType") as DropDownList;
                if (ddl != null)
                {
                    string query = "SELECT IssueTypeID, IssueTypeCode, IssueTypeName FROM IssueType WHERE IssueTypeID <= 4 ORDER BY IssueTypeID";
                    DataTable dt = _dbHelper.ExecuteQuery(query, null);
                    dt.Columns.Add("DisplayText", typeof(string), "IssueTypeCode + ' ' + IssueTypeName");

                    ddl.DataSource = dt;
                    ddl.DataTextField = "DisplayText";
                    ddl.DataValueField = "IssueTypeID";
                    ddl.DataBind();
                    ddl.Items.Insert(0, new ListItem("Select Issue Type", ""));
                }
            }
        }

        protected async void SubmitForm(object sender, EventArgs e)
        {
            int? requestUserID = _ui.GetRequestUserID(UserName, UserEmpID);
            if (requestUserID == null)
            {
                ShowAlert("Unable to identify user");
                return;
            }

            bool hasValidItem = false;

            foreach (RepeaterItem item in rptRequestItems.Items)
            {
                var ddl = item.FindControl("ddlIssueType") as DropDownList;
                var txt = item.FindControl("txtDescription") as TextBox;
                var fileUpload = item.FindControl("fileUploadImage") as FileUpload;

                if (ddl == null || txt == null)
                    continue;

                if (string.IsNullOrWhiteSpace(txt.Text) && string.IsNullOrEmpty(ddl.SelectedValue))
                    continue;

                hasValidItem = true;

                string imagePath = null;
                if (fileUpload != null && fileUpload.HasFile)
                {
                    string ext = Path.GetExtension(fileUpload.FileName).ToLower();
                    if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                    {
                        string fileName = Guid.NewGuid().ToString() + ext;
                        string savePath = Server.MapPath("~/App_Temp/" + fileName);
                        fileUpload.SaveAs(savePath);
                        imagePath = fileName;
                    }
                }

                var columnValues = new Dictionary<string, object>
                {
                    { "IssueDate", DateTime.Parse(txtDate.Text) },
                    { "DeptNameID", DeptName },
                    { "CompanyID", "ENR" },
                    { "RequestUserID", requestUserID },
                    { "IssueDetails", HttpUtility.HtmlEncode(txt.Text) },
                    { "IssueTypeID", ddl.SelectedValue },
                    { "Status", false },
                    { "LastUpdateDate", DateTime.Now },
                    { "DRI_UserID", DBNull.Value },
                    { "Solution", DBNull.Value },
                    { "FinishedDate", DBNull.Value },
                    { "Remark", DBNull.Value },
                    { "ImagePath", imagePath ?? (object)DBNull.Value }
                };

                try
                {
                    _dbHelper.InsertData("IT_RequestList", columnValues);
                }
                catch (Exception ex)
                {
                    ShowAlert($"Error saving data: {ex.Message}");
                    return;
                }

                string sNotifyMsg =
                    $"Date: {DateTime.Now:yyyy/MM/dd}\n" +
                    $"Department: {DeptName}\n" +
                    $"Name: {UserName}\n" +
                    $"Category: {ddl.SelectedItem.Text}\n" +
                    $"Description: {txt.Text}";

                try
                {
                    var notifier = new LineNotificationModel();
                    string lineGroupId = ConfigurationManager.AppSettings["LineGroupID"];
                    await notifier.SendLineGroupMessageAsync(lineGroupId, sNotifyMsg);
                }
                catch (Exception ex)
                {
                    ShowAlert($"Failed to send LINE notification: {ex.Message}");
                }
            }

            if (!hasValidItem)
            {
                ShowAlert("Please enter at least 1 issue");
                return;
            }

            ShowAlertAndRedirect("Request submitted successfully.", ResolveUrl("~/Pages/IT_RequestsList.aspx"));

        }
        protected void CancelForm(object sender, EventArgs e)
        {
            ShowAlertAndRedirect("Request cancelled.", ResolveUrl("~/Pages/IT_RequestsList.aspx"));
        }

        private void ShowAlertAndRedirect(string message, string redirectUrl)
        {
            string safeMessage = HttpUtility.JavaScriptStringEncode(message);
            string script = $@"
                alert('{safeMessage}');
                window.location.href = '{redirectUrl}';
            ";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alertRedirect", script, true);
        }

        private void ShowAlert(string message)
        {
            string safeMessage = HttpUtility.JavaScriptStringEncode(message);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", $"alert('{safeMessage}');", true);
        }
    }
}