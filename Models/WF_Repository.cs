using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;

namespace IT_WorkPlant.Models
{
    #region Models
    public class Workflow
    {
        public int FlowID { get; set; }
        public string FlowName { get; set; }
        public string DeptID { get; set; }
        public string DeptName { get; set; }
    }

    public class ApprovalStep
    {
        public int FlowID { get; set; }
        public int StepOrder { get; set; }
        public int RoleID { get; set; }
        public string Role { get; set; }
        public string StepDesc { get; set; }
        public string PositionName_EN { get; set; }
    }

    [Serializable]
    public class Department
    {
        public string DeptID { get; set; }   // DeptIndex
        public string DeptName { get; set; }   // DeptName_en 或 DeptName_zh
    }

    public class FormInstance
    {
        public int FormID { get; set; }
        public int FlowID { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
    }

    public class FormStatusLog
    {
        public int FormID { get; set; }
        public int Status { get; set; }
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
    }

    public class ApproveLog
    {
        public int FormID { get; set; }
        public int StepOrder { get; set; }
        public int ApproverID { get; set; }
        public int ApproveStatus { get; set; }
        public string Comment { get; set; }
        public DateTime? ApproveDate { get; set; }
    }

    #endregion
    public class WF_Repository
    {
        private readonly MssqlDatabaseHelper _db;
        public WF_Repository(MssqlDatabaseHelper dbHelper)
        {
            _db = dbHelper; 
        }
        private const string FLOW_NOT_FOUND = "Workflow not found";
        private const string STEP_NOT_FOUND = "Step not found";
        private readonly UserInfo _userInfo = new UserInfo();

        #region ── Constants ────────────────────────────────────────
        // Form Status Types
        public const int FORM_STATUS_SUBMITTED = 0;
        public const int FORM_STATUS_IN_PROCESS = 1;
        public const int FORM_STATUS_COMPLETED = 2;
        public const int FORM_STATUS_REJECTED = 3;

        // Approval Status Types
        public const int APPROVE_STATUS_PENDING = 0;
        public const int APPROVE_STATUS_APPROVED = 1;
        public const int APPROVE_STATUS_REJECTED = 2;
        #endregion

        #region ── Helper Methods ────────────────────────────────────────
        private int? GetCurrentUserID()
        {
            var userName = HttpContext.Current.Session["UserName"]?.ToString();
            var userEmpId = HttpContext.Current.Session["UserEmpID"]?.ToString();
            return _userInfo.GetRequestUserID(userName, userEmpId);
        }

        private int ResolveApproverID(int roleId, string role, int formId, SqlTransaction transaction)
        {
            // Get the current flow and step information
            const string getFlowStepSql = @"
                SELECT f.FlowID, s.StepOrder, s.UseDynamicManager
                FROM WF_Forms f
                JOIN WF_ApprovalSteps s ON f.FlowID = s.FlowID
                WHERE f.FormID = @FormID";

            var flowStep = _db.ExecuteQuery(getFlowStepSql, new[] {
                new SqlParameter("@FormID", formId)
            }, transaction).AsEnumerable().FirstOrDefault();

            if (flowStep != null)
            {
                int flowId = flowStep.Field<int>("FlowID");
                int stepOrder = flowStep.Field<int>("StepOrder");
                bool useDynamicManager = flowStep.Field<bool>("UseDynamicManager");

                // If this step uses dynamic manager
                if (useDynamicManager && role.ToLower() == "manager")
                {
                    // Get the user's EmpID from the form
                    const string getUserEmpIdSql = @"
                        SELECT r.RequestUserID
                        FROM WF_Forms f
                        JOIN IT_RequestList r ON f.FormID = r.ReportID
                        WHERE f.FormID = @FormID";

                    var requestUserId = _db.ExecuteScalar(getUserEmpIdSql, new[] {
                        new SqlParameter("@FormID", formId)
                    }, transaction);

                    if (requestUserId == null || requestUserId == DBNull.Value)
                        throw new Exception($"No request user found for form {formId}");

                    // Get the user's EmpID
                    const string getUserEmpIdSql2 = @"
                        SELECT UserEmpID
                        FROM Users
                        WHERE UserID = @UserID";

                    var userEmpId = _db.ExecuteScalar(getUserEmpIdSql2, new[] {
                        new SqlParameter("@UserID", requestUserId)
                    }, transaction);

                    if (userEmpId == null || userEmpId == DBNull.Value)
                        throw new Exception($"No EmpID found for user {requestUserId}");

                    // Get the manager's ID
                    const string getManagerSql = @"
                        SELECT ReportsTo
                        FROM Users
                        WHERE UserEmpID = @UserEmpID";

                    var managerId = _db.ExecuteScalar(getManagerSql, new[] {
                        new SqlParameter("@UserEmpID", userEmpId)
                    }, transaction);

                    if (managerId == null || managerId == DBNull.Value)
                        throw new Exception($"No manager found for user {userEmpId}");

                    return Convert.ToInt32(managerId);
                }
            }

            // For fixed roles, return the role ID directly
            return roleId;
        }

        private void ValidateWorkflow(int flowId)
        {
            const string checkFlowSql = @"
                SELECT COUNT(1) 
                FROM WF_FlowDefinitions 
                WHERE FlowID = @FlowID";

            var exists = (int)_db.ExecuteScalar(checkFlowSql, new[] {
                new SqlParameter("@FlowID", flowId)
            }) > 0;

            if (!exists)
                throw new Exception($"Workflow {flowId} does not exist");
        }
        #endregion

        #region ── Flow CRUD ────────────────────────────────────────
        public IEnumerable<Workflow> GetAllFlows()
        {
            const string sql =
                "SELECT f.FlowID, f.FlowName, f.DeptID, " +
                "       d.DeptName_en AS DeptName " +
                "FROM   WF_FlowDefinitions f " +
                "LEFT JOIN Departments d ON f.DeptID = d.DeptNameID " + 
                "ORDER BY f.FlowID";

            try
            {
                return _db.ExecuteQuery(sql, null)
                          .AsEnumerable()
                          .Select(r => new Workflow
                          {
                              FlowID = r.Field<int>("FlowID"),
                              FlowName = r.Field<string>("FlowName"),
                              DeptID = r.Field<string>("DeptID"),
                              DeptName = r.Field<string>("DeptName")
                          });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve workflows", ex);
            }
        }

        public IEnumerable<Department> GetDepartments()
        {
            const string sql =
                "SELECT " +
                "DeptNameID, " +
                "DeptName_en AS DeptName " +
                "FROM Departments " +
                "ORDER BY DeptIndex";

            try
            {
                return _db.ExecuteQuery(sql, null)
                          .AsEnumerable()
                          .Select(r => new Department
                          {
                              DeptID = r.Field<string>("DeptNameID"), 
                              DeptName = r.Field<string>("DeptName") 
                          });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve departments", ex);
            }
        }

        public int InsertFlow(Workflow w)   
        {
            if (w == null)
                throw new ArgumentNullException(nameof(w));

            var userName = HttpContext.Current.Session["UserName"]?.ToString();
            var userEmpId = HttpContext.Current.Session["UserEmpID"]?.ToString();
            var userIndex = _userInfo.GetRequestUserID(userName, userEmpId) ?? (object)DBNull.Value;

            const string sql = @"
                INSERT INTO WF_FlowDefinitions (FlowName, DeptID, CreatedBy, CreatedDate)
                OUTPUT INSERTED.FlowID 
                VALUES (@FlowName, @DeptID, @CreatedBy, GETDATE())";

            try
            {
                return 
                    (int)_db.ExecuteScalar(sql, new[]{ 
                        new SqlParameter("@FlowName", w.FlowName),
                        new SqlParameter("@DeptID", w.DeptID),
                        new SqlParameter("@CreatedBy", userIndex)
                    });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to insert workflow", ex);
            }
        }

        public void UpdateFlow(Workflow w)
        {
            if (w == null)
                throw new ArgumentNullException(nameof(w));

            var userName = HttpContext.Current.Session["UserName"]?.ToString();
            var userEmpId = HttpContext.Current.Session["UserEmpID"]?.ToString();
            var userIndex = _userInfo.GetRequestUserID(userName, userEmpId) ?? (object)DBNull.Value;

            const string sql = @"
                UPDATE WF_FlowDefinitions
                SET FlowName = @FlowName,
                    DeptID = @DeptID,
                    ModifiedBy = @ModifiedBy,
                    ModifiedDate = GETDATE()
                WHERE FlowID = @FlowID";

            try
            {
                var result = _db.ExecuteNonQuery(sql, new[]{
                    new SqlParameter("@FlowName", w.FlowName),
                    new SqlParameter("@DeptID", w.DeptID),
                    new SqlParameter("@FlowID", w.FlowID),
                    new SqlParameter("@ModifiedBy", userIndex)
                });

                if (result == 0)
                    throw new Exception(FLOW_NOT_FOUND);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update workflow", ex);
            }
        }

        public void DeleteFlow(int flowID)
        {
            const string sql = @"
            DELETE WF_ApprovalSteps   WHERE FlowID=@FlowID;
            DELETE WF_FlowDefinitions WHERE FlowID=@FlowID;";
            try
            {
                var result = _db.ExecuteNonQuery(sql, new[] { new SqlParameter("@FlowID", flowID) });

                if (result == 0)
                    throw new Exception(FLOW_NOT_FOUND);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete workflow", ex);
            }
        }
        #endregion

        #region ── Step CRUD ────────────────────────────────────────
        public IEnumerable<ApprovalStep> GetSteps(int flowID)
        {
            const string sql = @"
                SELECT s.FlowID, s.StepOrder, s.RoleID, s.Role, s.StepDesc, p.PositionName_EN
                FROM WF_ApprovalSteps s
                LEFT JOIN Positions p ON s.RoleID = p.PositionID
                WHERE s.FlowID = @FlowID
                ORDER BY s.StepOrder";
            try
            {
                return _db.ExecuteQuery(sql, new[] { new SqlParameter("@FlowID", flowID) })
                          .AsEnumerable()
                          .Select(r => new ApprovalStep
                          {
                              FlowID = r.Field<int>("FlowID"),
                              StepOrder = r.Field<int>("StepOrder"),
                              RoleID = r.Field<int>("RoleID"),
                              Role = r.Field<string>("Role"),
                              StepDesc = r.Field<string>("StepDesc"),
                              PositionName_EN = r.Field<string>("PositionName_EN")
                          });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve steps", ex);
            }
        }

        public void InsertStep(ApprovalStep s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            var userName = HttpContext.Current.Session["UserName"]?.ToString();
            var userEmpId = HttpContext.Current.Session["UserEmpID"]?.ToString();
            var userIndex = _userInfo.GetRequestUserID(userName, userEmpId) ?? (object)DBNull.Value;

            const string sql = @"
                INSERT INTO WF_ApprovalSteps (
                    FlowID, StepOrder, Role, RoleID, StepDesc,
                    CreatedBy, CreatedDate
                )
                VALUES (
                    @FlowID, @StepOrder, @Role, @RoleID, @StepDesc,
                    @CreatedBy, GETDATE()
                )";
            try
            {
                _db.ExecuteNonQuery(sql, new[]{
                    new SqlParameter("@FlowID", s.FlowID),
                    new SqlParameter("@StepOrder", s.StepOrder),
                    new SqlParameter("@Role", s.Role),
                    new SqlParameter("@RoleID", s.RoleID),
                    new SqlParameter("@StepDesc", (object)s.StepDesc ?? DBNull.Value),
                    new SqlParameter("@CreatedBy", userIndex)
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to insert step", ex);
            }
        }

        public void UpdateStep(ApprovalStep s)
        {
            if (s == null)
                throw new ArgumentNullException(nameof(s));

            const string sql = @"
                UPDATE WF_ApprovalSteps
                SET Role = @Role,
                    RoleID = @RoleID,
                    StepDesc = @StepDesc
                WHERE FlowID = @FlowID 
                AND StepOrder = @StepOrder";
            try
            {
                var result = _db.ExecuteNonQuery(sql, new[]{
                    new SqlParameter("@Role", s.Role),
                    new SqlParameter("@RoleID", s.RoleID),
                    new SqlParameter("@StepDesc", (object)s.StepDesc ?? DBNull.Value),
                    new SqlParameter("@FlowID", s.FlowID),
                    new SqlParameter("@StepOrder", s.StepOrder)
                });

                if (result == 0)
                    throw new Exception(STEP_NOT_FOUND);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update step", ex);
            }
        }

        public void DeleteStep(int flowID, int stepOrder)
        {
            const string sql = @"
                DELETE WF_ApprovalSteps
                WHERE FlowID = @FlowID 
                AND StepOrder = @StepOrder";
            try
            {
                var result = _db.ExecuteNonQuery(sql, new[]{
                    new SqlParameter("@FlowID", flowID),
                    new SqlParameter("@StepOrder", stepOrder)
                });

                if (result == 0)
                    throw new Exception(STEP_NOT_FOUND);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete step", ex);
            }
        }

        public void UpdateStepOrder(int flowID, int orderA, int orderB)
        {
            // 交換兩個步驟的 StepOrder
            const string sql = @"
                UPDATE WF_ApprovalSteps SET StepOrder = -1 WHERE FlowID = @FlowID AND StepOrder = @OrderA;
                UPDATE WF_ApprovalSteps SET StepOrder = @OrderA WHERE FlowID = @FlowID AND StepOrder = @OrderB;
                UPDATE WF_ApprovalSteps SET StepOrder = @OrderB WHERE FlowID = @FlowID AND StepOrder = -1;";
            _db.ExecuteNonQuery(sql, new[] {
                new SqlParameter("@FlowID", flowID),
                new SqlParameter("@OrderA", orderA),
                new SqlParameter("@OrderB", orderB)
            });
        }
        #endregion

        #region ── Workflow Submission ────────────────────────────────────────
        public void SubmitWorkflow(int requestId, int flowId, string comment = "Form submitted", SqlTransaction transaction = null)
        {
            if (requestId <= 0)
                throw new ArgumentException("Invalid request ID", nameof(requestId));

            if (string.IsNullOrWhiteSpace(comment))
                throw new ArgumentException("Comment cannot be empty", nameof(comment));

            // Validate workflow exists
            ValidateWorkflow(flowId);

            // If no transaction provided, create a new one
            bool isNewTransaction = transaction == null;
            if (isNewTransaction)
            {
                transaction = _db.BeginTransaction();
            }

            try
            {
                // Check if form already exists
                const string checkFormSql = @"
                    SELECT COUNT(1) 
                    FROM WF_Forms 
                    WHERE FormID = @FormID";

                var formExists = (int)_db.ExecuteScalar(checkFormSql, new[] {
                    new SqlParameter("@FormID", requestId)
                }, transaction) > 0;

                if (formExists)
                    throw new Exception($"Form {requestId} already exists in workflow");

                // Get request details
                const string getRequestSql = @"
                    SELECT ReportID, IssueTypeID, DeptNameID
                    FROM IT_RequestList
                    WHERE ReportID = @ReportID";

                var requestDetails = _db.ExecuteQuery(getRequestSql, new[] {
                    new SqlParameter("@ReportID", requestId)
                }, transaction).AsEnumerable().FirstOrDefault();

                if (requestDetails == null)
                    throw new Exception($"Request {requestId} not found");

                // 1. Create form instance
                const string insertFormSql = @"
                    INSERT INTO WF_Forms (
                        FormID, FlowID, Status, 
                        ReportID, IssueTypeID, DeptID,
                        CreatedDate, CreatedBy
                    )
                    VALUES (
                        @FormID, @FlowID, @Status,
                        @ReportID, @IssueTypeID, @DeptID,
                        GETDATE(), @CreatedBy
                    )";

                var userIndex = GetCurrentUserID() ?? (object)DBNull.Value;

                _db.ExecuteNonQuery(insertFormSql, new[] {
                    new SqlParameter("@FormID", requestId),
                    new SqlParameter("@FlowID", flowId),
                    new SqlParameter("@Status", FORM_STATUS_SUBMITTED),
                    new SqlParameter("@ReportID", requestDetails.Field<int>("ReportID")),
                    new SqlParameter("@IssueTypeID", requestDetails.Field<int>("IssueTypeID")),
                    new SqlParameter("@DeptID", requestDetails.Field<string>("DeptID")),
                    new SqlParameter("@CreatedBy", userIndex)
                }, transaction);

                // 2. Create initial status log
                const string insertStatusLogSql = @"
                    INSERT INTO WF_FormStatusLog (
                        FormID, Status, Comment, 
                        ChangeDate, ChangedBy
                    )
                    VALUES (
                        @FormID, @Status, @Comment,
                        GETDATE(), @ChangedBy
                    )";

                _db.ExecuteNonQuery(insertStatusLogSql, new[] {
                    new SqlParameter("@FormID", requestId),
                    new SqlParameter("@Status", FORM_STATUS_SUBMITTED),
                    new SqlParameter("@Comment", comment),
                    new SqlParameter("@ChangedBy", userIndex)
                }, transaction);

                // 3. Get first approval step
                const string getFirstStepSql = @"
                    SELECT RoleID, Role
                    FROM WF_ApprovalSteps
                    WHERE FlowID = @FlowID 
                    AND StepOrder = 1";

                var firstStep = _db.ExecuteQuery(getFirstStepSql, new[] {
                    new SqlParameter("@FlowID", flowId)
                }, transaction).AsEnumerable().FirstOrDefault();

                if (firstStep == null)
                    throw new Exception($"No approval steps found for workflow {flowId}");

                // 4. Resolve approver ID
                int roleId = firstStep.Field<int>("RoleID");
                string role = firstStep.Field<string>("Role");
                int approverId = ResolveApproverID(roleId, role, requestId, transaction);

                // 5. Create pending approval log
                const string insertApproveLogSql = @"
                    INSERT INTO WF_RequestApproveLog (
                        FormID, FlowID, StepOrder, ApproverID, 
                        ApproveStatus, Remark, ApproveDate
                    )
                    VALUES (
                        @FormID, @FlowID, 1, @ApproverID,
                        @ApproveStatus, NULL, NULL
                    )";

                _db.ExecuteNonQuery(insertApproveLogSql, new[] {
                    new SqlParameter("@FormID", requestId),
                    new SqlParameter("@FlowID", flowId),
                    new SqlParameter("@ApproverID", approverId),
                    new SqlParameter("@ApproveStatus", APPROVE_STATUS_PENDING)
                }, transaction);

                if (isNewTransaction)
                {
                    _db.CommitTransaction();
                }
            }
            catch (Exception)
            {
                if (isNewTransaction)
                {
                    _db.RollbackTransaction();
                }
                throw;
            }
        }
        #endregion
    }
}