using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Oracle.ManagedDataAccess.Client;
using System.Web.Security;
using System.Linq;
using IT_WorkPlant.Models;


namespace IT_WorkPlant.Pages
{
    public partial class WF_Approve : Page
    {
        private readonly OracleDatabaseHelper _dbHelper;
        private readonly UserInfo _userInfo;

        public WF_Approve()
        {
            _dbHelper = new OracleDatabaseHelper(System.Configuration.ConfigurationManager.ConnectionStrings["OracleDB"].ConnectionString);
            _userInfo = new UserInfo();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string token = Request.QueryString["token"];
                if (string.IsNullOrEmpty(token))
                {
                    Response.Redirect("~/Error.aspx?message=Invalid token");
                    return;
                }

                try
                {
                    LoadApprovalData(token);
                }
                catch (Exception ex)
                {
                    Response.Redirect($"~/Error.aspx?message={Server.UrlEncode(ex.Message)}");
                }
            }
        }

        private void LoadApprovalData(string token)
        {
            // Get form details
            const string getFormSql = @"
                SELECT f.FormID, f.FlowID, f.Status, f.RequestDate,
                       r.RequestUserID, r.DeptID,
                       u.FirstName, u.LastName, d.DeptName_en
                FROM WF_Forms f
                JOIN IT_RequestList r ON f.FormID = r.ReportID
                JOIN Users u ON r.RequestUserID = u.UserID
                JOIN Departments d ON r.DeptID = d.DeptNameID
                WHERE f.Token = :Token";

            var formData = _dbHelper.ExecuteQuery(getFormSql, new[] {
                new OracleParameter(":Token", token)
            }).AsEnumerable().FirstOrDefault();

            if (formData == null)
                throw new Exception("Form not found or token is invalid");

            // Get approval steps
            const string getStepsSql = @"
                SELECT s.StepOrder, s.Role, s.StepDesc,
                       u.FirstName, u.LastName,
                       a.ApproveStatus, a.Comment, a.ApproveDate
                FROM WF_ApprovalSteps s
                LEFT JOIN WF_RequestApproveLog a ON s.FlowID = a.FlowID 
                    AND s.StepOrder = a.StepOrder
                    AND a.FormID = :FormID
                LEFT JOIN Users u ON a.ApproverID = u.UserID
                WHERE s.FlowID = :FlowID
                ORDER BY s.StepOrder";

            var stepsData = _dbHelper.ExecuteQuery(getStepsSql, new[] {
                new OracleParameter(":FormID", formData.Field<int>("FormID")),
                new OracleParameter(":FlowID", formData.Field<int>("FlowID"))
            });

            // Bind data to controls
            lblRequester.Text = $"{formData.Field<string>("FirstName")} {formData.Field<string>("LastName")}";
            lblDepartment.Text = formData.Field<string>("DeptName_en");
            lblRequestDate.Text = formData.Field<DateTime>("RequestDate").ToString("yyyy-MM-dd HH:mm");
            hfFormID.Value = formData.Field<int>("FormID").ToString();

            rptTimeline.DataSource = stepsData;
            rptTimeline.DataBind();
        }

        protected void btnApprove_Click(object sender, EventArgs e)
        {
            ProcessApproval(true);
        }

        protected void btnReject_Click(object sender, EventArgs e)
        {
            ProcessApproval(false);
        }

        private void ProcessApproval(bool isApproved)
        {
            try
            {
                int formId = int.Parse(hfFormID.Value);
                string comment = txtComment.Text.Trim();

                // Get current step
                const string getCurrentStepSql = @"
                    SELECT s.StepOrder, s.Role, s.RoleID
                    FROM WF_Forms f
                    JOIN WF_ApprovalSteps s ON f.FlowID = s.FlowID
                    WHERE f.FormID = :FormID
                    AND s.StepOrder = (
                        SELECT MIN(StepOrder)
                        FROM WF_ApprovalSteps
                        WHERE FlowID = f.FlowID
                        AND Status = 'Current'
                    )";

                var currentStep = _dbHelper.ExecuteQuery(getCurrentStepSql, new[] {
                    new OracleParameter(":FormID", formId)
                }).AsEnumerable().FirstOrDefault();

                if (currentStep == null)
                    throw new Exception("No current step found");

                // Update approval log
                const string updateApprovalSql = @"
                    UPDATE WF_RequestApproveLog
                    SET ApproveStatus = :Status,
                        Comment = :Comment,
                        ApproveDate = SYSDATE
                    WHERE FormID = :FormID
                    AND StepOrder = :StepOrder";

                _dbHelper.ExecuteNonQuery(updateApprovalSql, new[] {
                    new OracleParameter(":Status", isApproved ? 1 : 2),
                    new OracleParameter(":Comment", comment),
                    new OracleParameter(":FormID", formId),
                    new OracleParameter(":StepOrder", currentStep.Field<int>("StepOrder"))
                });

                // Update form status
                const string updateFormSql = @"
                    UPDATE WF_Forms
                    SET Status = :Status,
                        LastUpdateDate = SYSDATE
                    WHERE FormID = :FormID";

                _dbHelper.ExecuteNonQuery(updateFormSql, new[] {
                    new OracleParameter(":Status", isApproved ? 1 : 3),
                    new OracleParameter(":FormID", formId)
                });

                Response.Redirect("~/Pages/WF_FormStatus.aspx");
            }
            catch (Exception ex)
            {
                Response.Redirect($"~/Error.aspx?message={Server.UrlEncode(ex.Message)}");
            }
        }

        protected string GetStepClass(object dataItem)
        {
            if (dataItem == null) return string.Empty;

            var row = (DataRowView)dataItem;
            string approveStatus = row["ApproveStatus"]?.ToString();

            if (string.IsNullOrEmpty(approveStatus))
                return "pending";
            
            switch (approveStatus)
            {
                case "1": // Approved
                    return "completed";
                case "2": // Rejected
                    return "rejected";
                case "3": // Current
                    return "active";
                default:
                    return "pending";
            }
        }
    }
} 