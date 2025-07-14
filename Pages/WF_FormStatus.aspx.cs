using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using IT_WorkPlant.Models;
using System.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Web.UI.HtmlControls;

namespace IT_WorkPlant.Pages
{
    public partial class WF_FormStatus : Page
    {
        private OracleDatabaseHelper _dbHelper;
        private readonly UserInfo _userInfo = new UserInfo();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["UserEmpID"] == null)
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                LoadUserWorkflows();
            }
        }

        private void LoadUserWorkflows()
        {
            try
            {
                _dbHelper = new OracleDatabaseHelper(ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString);

                var userID = Session["UserEmpID"]?.ToString();
                if (string.IsNullOrEmpty(userID))
                {
                    Response.Redirect("~/Login.aspx");
                    return;
                }

                var sql = @"
                    SELECT f.FormID, f.FlowName, f.RequestDate, f.CurrentStep, f.Status, f.LastUpdateDate
                    FROM WorkflowForms f
                    JOIN WF_FlowDefinitions fd ON f.FlowID = fd.FlowID
                    WHERE f.RequesterID = :UserID
                    ORDER BY f.RequestDate DESC";

                var parameters = new[] {
                    new OracleParameter(":UserID", OracleDbType.Varchar2) { Value = userID }
                };

                DataTable dt = _dbHelper.ExecuteQuery(sql, parameters);
                gvWorkflows.DataSource = dt;
                gvWorkflows.DataBind();
            }
            catch (Exception ex)
            {
                
                System.Diagnostics.Debug.WriteLine($"[WF_FormStatus Error] {ex}");

                Response.Redirect("~/Error.aspx");
            }
        }


        protected void gvWorkflows_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(gvWorkflows, "Select$" + e.Row.RowIndex);
                e.Row.Style["cursor"] = "pointer";

                
                string status = DataBinder.Eval(e.Row.DataItem, "Status").ToString();
                Label lblStatus = (Label)e.Row.FindControl("lblStatus");
                if (lblStatus != null)
                {
                    lblStatus.CssClass = $"status-badge status-{status.ToLower()}";
                }
            }
        }

        protected void gvWorkflows_SelectedIndexChanged(object sender, EventArgs e)
        {
            int formId = Convert.ToInt32(gvWorkflows.SelectedDataKey.Value);
            LoadFormStatus(formId);
            divFormDetail.Visible = true;
        }

        private void LoadFormStatus(int formID)
        {
            try
            {
                _dbHelper = new OracleDatabaseHelper(ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString);

                
                string formQuery = @"
                    SELECT f.FormID, f.RequesterName, f.RequesterDept, f.RequestDate, f.FormContent,
                           f.Status, f.CompletionDate, f.RejectionDate
                    FROM WorkflowForms f
                    WHERE f.FormID = :FormID";

                OracleParameter[] formParams = {
                    new OracleParameter(":FormID", OracleDbType.Int32) { Value = formID }
                };

                DataTable formDt = _dbHelper.ExecuteQuery(formQuery, formParams);

                if (formDt.Rows.Count > 0)
                {
                    DataRow formRow = formDt.Rows[0];
                    lblRequester.Text = formRow["RequesterName"].ToString();
                    lblDepartment.Text = formRow["RequesterDept"].ToString();
                    lblRequestDate.Text = Convert.ToDateTime(formRow["RequestDate"]).ToString("yyyy/MM/dd HH:mm");
                    lblFormContent.Text = formRow["FormContent"].ToString();
                    lblCurrentStatus.Text = formRow["Status"].ToString();

                    if (formRow["CompletionDate"] != DBNull.Value)
                    {
                        lblCompletionDate.Text = Convert.ToDateTime(formRow["CompletionDate"]).ToString("yyyy/MM/dd HH:mm");
                    }
                    else if (formRow["RejectionDate"] != DBNull.Value)
                    {
                        lblCompletionDate.Text = Convert.ToDateTime(formRow["RejectionDate"]).ToString("yyyy/MM/dd HH:mm");
                    }
                }

                
                string timelineQuery = @"
                    SELECT s.StepOrder, s.PositionName_EN, s.Status, s.Comment, s.ApprovalDate
                    FROM WF_ApprovalSteps s
                    WHERE s.FormID = :FormID
                    ORDER BY s.StepOrder";

                DataTable timelineDt = _dbHelper.ExecuteQuery(timelineQuery, formParams);
                rptTimeline.DataSource = timelineDt;
                rptTimeline.DataBind();
            }
            catch (Exception ex)
            {
                
                System.Diagnostics.Debug.WriteLine($"[WF_FormStatus Error] {ex}");

                Response.Redirect("~/Error.aspx");
            }


        }

        protected string GetStatusClass(object status)
        {
            string statusStr = status.ToString();
            switch (statusStr)
            {
                case "Completed":
                    return "completed";
                case "Rejected":
                    return "rejected";
                case "Current":
                    return "current";
                default:
                    return "pending";
            }
        }

        protected string FormatDate(object date)
        {
            if (date == DBNull.Value)
                return "";

            return Convert.ToDateTime(date).ToString("yyyy/MM/dd HH:mm");
        }
    }
} 