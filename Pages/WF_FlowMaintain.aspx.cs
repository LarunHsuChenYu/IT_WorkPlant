using IT_WorkPlant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Security.Principal;
using System.Data;

namespace IT_WorkPlant.Pages
{
    using IT_WorkPlant.Models;

    public partial class WF_FlowMaintain : System.Web.UI.Page
    {
        private const string SUCCESS_MESSAGE = "Done.";
        private WF_Repository _repo;
        private MssqlDatabaseHelper _db;

        private int CurrentFlowID
        {
            get => ViewState["FlowID"] == null ? 0 : (int)ViewState["FlowID"];
            set => ViewState["FlowID"] = value;
        }

        private int CurrentStepOrder
        {
            get => ViewState["StepOrder"] == null ? 0 : (int)ViewState["StepOrder"];
            set => ViewState["StepOrder"] = value;
        }

        private IList<Department> DeptList
        {
            get => (IList<Department>)Session["DeptList"];
            set => Session["DeptList"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserEmpID"] == null)
            {
                Response.Redirect("~/Login.aspx");
                return;
            }

            _db = new MssqlDatabaseHelper();
            _repo = new WF_Repository(_db);

            if (!IsPostBack)
            {
                DeptList = _repo.GetDepartments().ToList();
                BindFlows();
                BindDepartmentDropdowns();
                BindPositionDropDowns();
            }
        }
        private void BindDepartmentDropdowns()
        {
            ddlNewDept.DataSource = DeptList;
            ddlNewDept.DataTextField = "DeptName";
            ddlNewDept.DataValueField = "DeptID";
            ddlNewDept.DataBind();
            ddlNewDept.Items.Insert(0, new ListItem("-- Select --", ""));

            ddlEditDept.DataSource = DeptList;
            ddlEditDept.DataTextField = "DeptName";
            ddlEditDept.DataValueField = "DeptID";
            ddlEditDept.DataBind();
        }

        private void BindPositionDropDowns()
        {
            var dt = GetPositions();
            ddlNewRole.DataSource = dt;
            ddlNewRole.DataTextField = "PositionName_EN";
            ddlNewRole.DataValueField = "PositionID";
            ddlNewRole.DataBind();
            ddlNewRole.Items.Insert(0, new ListItem("Select Position", ""));

            ddlEditRole.DataSource = dt;
            ddlEditRole.DataTextField = "PositionName_EN";
            ddlEditRole.DataValueField = "PositionID";
            ddlEditRole.DataBind();
            ddlEditRole.Items.Insert(0, new ListItem("Select Position", ""));
        }

        private DataTable GetPositions()
        {
            string query = "SELECT PositionID, PositionName_EN FROM Positions WHERE IsActive = 1";
            var db = new MssqlDatabaseHelper();
            return db.ExecuteQuery(query, null);
        }

        private void BindFlows()
        {
            var data = _repo.GetAllFlows().ToList();
            gvFlows.DataSource = data;
            gvFlows.DataBind();
        }

        protected void gvFlows_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(gvFlows, "Select$" + e.Row.RowIndex);
                e.Row.Style["cursor"] = "pointer";
            }
        }

        protected void gvFlows_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentFlowID = (int)gvFlows.SelectedDataKey.Value;
            lblCurrentFlow.Text = CurrentFlowID.ToString();
            btnAddStep.Enabled = true;
            btnEditFlow.Enabled = true;
            btnDeleteFlow.Enabled = true;
            BindSteps();
        }

        protected void btnAddFlow_Click(object sender, EventArgs e)
        {
            var name = txtNewFlowName.Text?.Trim() ?? string.Empty;
            var dept = ddlNewDept.SelectedValue;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(dept))
            {
                ShowMsg("Please enter flow name and select department.", false);
                return;
            }

            var newFlow = new Workflow { FlowName = name, DeptID = dept };
            _repo.InsertFlow(newFlow);

            txtNewFlowName.Text = string.Empty;
            ddlNewDept.SelectedIndex = 0;

            BindFlows();
            ShowMsg(SUCCESS_MESSAGE, true);
        }

        protected void btnEditFlow_Click(object sender, EventArgs e)
        {
            if (CurrentFlowID == 0) return;

            var flow = _repo.GetAllFlows().FirstOrDefault(f => f.FlowID == CurrentFlowID);
            if (flow != null)
            {
                hdnEditFlowID.Value = flow.FlowID.ToString();
                txtEditFlowName.Text = flow.FlowName;
                ddlEditDept.SelectedValue = flow.DeptID;

                ScriptManager.RegisterStartupScript(this, GetType(), "ShowModal",
                    "var modal = new bootstrap.Modal(document.getElementById('editFlowModal')); modal.show();", true);
            }
        }

        protected void btnSaveFlow_Click(object sender, EventArgs e)
        {
            var id = int.Parse(hdnEditFlowID.Value);
            var name = txtEditFlowName.Text?.Trim() ?? string.Empty;
            var dept = ddlEditDept.SelectedValue;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrEmpty(dept))
            {
                ShowMsg("Please enter flow name and select department.", false);
                return;
            }

            _repo.UpdateFlow(new Workflow { FlowID = id, FlowName = name, DeptID = dept });
            BindFlows();
            ShowMsg(SUCCESS_MESSAGE, true);
        }

        protected void btnDeleteFlow_Click(object sender, EventArgs e)
        {
            if (CurrentFlowID == 0) return;

            var steps = _repo.GetSteps(CurrentFlowID);
            foreach (var step in steps)
            {
                _repo.DeleteStep(CurrentFlowID, step.StepOrder);
            }

            _repo.DeleteFlow(CurrentFlowID);

            CurrentFlowID = 0;
            lblCurrentFlow.Text = "";
            btnAddStep.Enabled = false;
            btnEditFlow.Enabled = false;
            btnDeleteFlow.Enabled = false;

            BindFlows();
            ClearStepArea();
            ShowMsg(SUCCESS_MESSAGE, true);
        }

        private void BindSteps()
        {
            var data = _repo.GetSteps(CurrentFlowID).ToList();
            gvSteps.DataSource = data;
            gvSteps.DataBind();
        }

        private void ClearStepArea()
        {
            CurrentFlowID = 0;
            CurrentStepOrder = 0;
            lblCurrentFlow.Text = "";
            btnAddStep.Enabled = false;
            btnEditStep.Enabled = false;
            btnDeleteStep.Enabled = false;
            gvSteps.DataSource = null;
            gvSteps.DataBind();
        }

        protected void gvSteps_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                e.Row.Attributes["onclick"] = Page.ClientScript.GetPostBackClientHyperlink(gvSteps, "Select$" + e.Row.RowIndex);
                e.Row.Style["cursor"] = "pointer";
            }
        }

        protected void gvSteps_SelectedIndexChanged(object sender, EventArgs e)
        {
            CurrentStepOrder = (int)gvSteps.SelectedDataKey.Value;
            btnEditStep.Enabled = true;
            btnDeleteStep.Enabled = true;
        }

        protected void gvSteps_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (CurrentFlowID == 0) return;
            int stepOrder = int.Parse(e.CommandArgument.ToString());
            var steps = _repo.GetSteps(CurrentFlowID).OrderBy(s => s.StepOrder).ToList();

            if (e.CommandName == "MoveUp" && stepOrder > 1)
            {
                var current = steps.First(s => s.StepOrder == stepOrder);
                var above = steps.First(s => s.StepOrder == stepOrder - 1);
                _repo.UpdateStepOrder(CurrentFlowID, current.StepOrder, above.StepOrder);
            }
            else if (e.CommandName == "MoveDown" && stepOrder < steps.Count)
            {
                var current = steps.First(s => s.StepOrder == stepOrder);
                var below = steps.First(s => s.StepOrder == stepOrder + 1);
                _repo.UpdateStepOrder(CurrentFlowID, current.StepOrder, below.StepOrder);
            }
            BindSteps();
        }

        protected void btnAddStep_Click(object sender, EventArgs e)
        {
            if (CurrentFlowID == 0) return;

            var roleIdStr = ddlNewRole.SelectedValue;
            var desc = txtNewDesc.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(roleIdStr))
            {
                ShowMsg("Please select a position.", false);
                return;
            }

            int roleId = int.Parse(roleIdStr);
            string roleName = ddlNewRole.SelectedItem.Text;

            int next = _repo.GetSteps(CurrentFlowID).Count() + 1;
            _repo.InsertStep(new ApprovalStep
            {
                FlowID = CurrentFlowID,
                StepOrder = next,
                Role = roleName,
                RoleID = roleId,
                StepDesc = desc
            });

            ddlNewRole.SelectedIndex = 0;
            txtNewDesc.Text = string.Empty;

            BindSteps();
            ShowMsg(SUCCESS_MESSAGE, true);
        }

        protected void btnEditStep_Click(object sender, EventArgs e)
        {
            if (CurrentFlowID == 0 || CurrentStepOrder == 0) return;

            var step = _repo.GetSteps(CurrentFlowID).FirstOrDefault(s => s.StepOrder == CurrentStepOrder);
            if (step != null)
            {
                hdnEditStepOrder.Value = step.StepOrder.ToString();
                ddlEditRole.SelectedValue = step.RoleID.ToString();
                txtEditDesc.Text = step.StepDesc;

                ScriptManager.RegisterStartupScript(this, GetType(), "ShowStepModal",
                    "var modal = new bootstrap.Modal(document.getElementById('editStepModal')); modal.show();", true);
            }
        }

        protected void btnSaveStep_Click(object sender, EventArgs e)
        {
            if (CurrentFlowID == 0 || CurrentStepOrder == 0) return;

            var roleIdStr = ddlEditRole.SelectedValue;
            var desc = txtEditDesc.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrEmpty(roleIdStr))
            {
                ShowMsg("Please select a position.", false);
                return;
            }

            int roleId = int.Parse(roleIdStr);
            string roleName = ddlEditRole.SelectedItem.Text;

            _repo.UpdateStep(new ApprovalStep
            {
                FlowID = CurrentFlowID,
                StepOrder = CurrentStepOrder,
                Role = roleName,
                RoleID = roleId,
                StepDesc = desc
            });

            BindSteps();
            ShowMsg(SUCCESS_MESSAGE, true);
        }

        protected void btnDeleteStep_Click(object sender, EventArgs e)
        {
            if (CurrentFlowID == 0 || CurrentStepOrder == 0) return;

            _repo.DeleteStep(CurrentFlowID, CurrentStepOrder);

            CurrentStepOrder = 0;
            btnEditStep.Enabled = false;
            btnDeleteStep.Enabled = false;

            BindSteps();
            ShowMsg(SUCCESS_MESSAGE, true);
        }

        private void ShowMsg(string msg, bool success)
        {
            lblMessage.Text = msg;
            lblMessage.CssClass = success ? "alert alert-success" : "alert alert-danger";
            lblMessage.Visible = true;
        }
    }
}
