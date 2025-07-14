<%@ Page Title="Workflow Maintain" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="WF_FlowMaintain.aspx.cs"
    Inherits="IT_WorkPlant.Pages.WF_FlowMaintain" %>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <h3>Workflow Management</h3>

    <asp:Label ID="lblMessage" runat="server" Visible="false" />

    <div class="mb-3">
        <div class="row">
            <div class="col-md-4">
    <a href="IT_WebPortalList.aspx">IT_WebPortalList.aspx</a>
                <asp:TextBox ID="txtNewFlowName" runat="server" CssClass="form-control" placeholder="New Flow Name" />
            </div>
            <div class="col-md-4">
                <asp:DropDownList ID="ddlNewDept" runat="server" CssClass="form-control" />
            </div>
            <div class="col-md-4">
                <asp:Button ID="btnAddFlow" runat="server" Text="Add Flow" CssClass="btn btn-primary" OnClick="btnAddFlow_Click" />
            </div>
        </div>
    </div>

    <div class="mb-3">
        <asp:Button ID="btnEditFlow" runat="server" Text="Edit Flow" CssClass="btn btn-warning" OnClick="btnEditFlow_Click" Enabled="false" />
        <asp:Button ID="btnDeleteFlow" runat="server" Text="Delete Flow" CssClass="btn btn-danger" OnClick="btnDeleteFlow_Click" Enabled="false" />
    </div>

    <asp:GridView ID="gvFlows" runat="server"
        CssClass="table table-bordered"
        AutoGenerateColumns="False"
        DataKeyNames="FlowID"
        OnSelectedIndexChanged="gvFlows_SelectedIndexChanged"
        OnRowDataBound="gvFlows_RowDataBound">
        <Columns>
            <asp:CommandField ShowSelectButton="true" HeaderText="Sel" />
            <asp:BoundField DataField="FlowID" HeaderText="ID" ReadOnly="true" />
            <asp:BoundField DataField="FlowName" HeaderText="Flow Name" />
            <asp:BoundField DataField="DeptName" HeaderText="Department" />
        </Columns>
        <SelectedRowStyle BackColor="#007bff" ForeColor="White" />
    </asp:GridView>

    <hr/>

    <h4>
        Steps for Flow ID : <asp:Label ID="lblCurrentFlow" runat="server" />
    </h4>

    <div class="mb-3">
        <div class="row">
            <div class="col-md-4">
                <asp:TextBox ID="txtNewStep" runat="server" CssClass="form-control" placeholder="New Step Order" />
            </div>
            <div class="col-md-4">
                <asp:TextBox ID="txtNewDesc" runat="server" CssClass="form-control" placeholder="Description" />
            </div>
            <div class="col-md-4">
                <asp:DropDownList ID="ddlNewRole" runat="server" CssClass="form-control" />
            </div>
            <div class="col-md-4">
                <asp:Button ID="btnAddStep" runat="server" Text="Add Step" CssClass="btn btn-primary" OnClick="btnAddStep_Click" Enabled="false" />
            </div>
        </div>
    </div>

    <div class="mb-3">
        <asp:Button ID="btnEditStep" runat="server" Text="Edit Step" CssClass="btn btn-warning" OnClick="btnEditStep_Click" Enabled="false" />
        <asp:Button ID="btnDeleteStep" runat="server" Text="Delete Step" CssClass="btn btn-danger" OnClick="btnDeleteStep_Click" Enabled="false" />
    </div>

    <asp:GridView ID="gvSteps" runat="server"
        CssClass="table table-striped"
        AutoGenerateColumns="False"
        DataKeyNames="StepOrder"
        OnSelectedIndexChanged="gvSteps_SelectedIndexChanged"
        OnRowDataBound="gvSteps_RowDataBound"
        OnRowCommand="gvSteps_RowCommand"
        EmptyDataText="No steps yet.">
        <Columns>
            <asp:TemplateField HeaderText="Move">
                <ItemTemplate>
                    <asp:Button ID="btnMoveUp" runat="server" Text="↑" CommandName="MoveUp" CommandArgument='<%# Eval("StepOrder") %>' Enabled='<%# (int)Eval("StepOrder") > 1 %>' />
                    <asp:Button ID="btnMoveDown" runat="server" Text="↓" CommandName="MoveDown" CommandArgument='<%# Eval("StepOrder") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:CommandField ShowSelectButton="true" HeaderText="Sel" />
            <asp:BoundField DataField="StepOrder" HeaderText="Order" ReadOnly="true"/>
            <asp:BoundField DataField="PositionName_EN" HeaderText="Position" />
            <asp:BoundField DataField="StepDesc" HeaderText="Description" />
        </Columns>
        <SelectedRowStyle BackColor="#007bff" ForeColor="White" />
    </asp:GridView>

    
    <div class="modal fade" id="editFlowModal" tabindex="-1" role="dialog" aria-hidden="true">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Edit Flow</h5>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hdnEditFlowID" runat="server" />
                    <div class="form-group">
                        <label>Flow Name</label>
                        <asp:TextBox ID="txtEditFlowName" runat="server" CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>Department</label>
                        <asp:DropDownList ID="ddlEditDept" runat="server" CssClass="form-control" />
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>
                    <asp:Button ID="btnSaveFlow" runat="server" Text="Save Changes" CssClass="btn btn-primary" OnClick="btnSaveFlow_Click" />
                </div>
            </div>
        </div>
    </div>

    
    <div class="modal fade" id="editStepModal" tabindex="-1" role="dialog" aria-hidden="true">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Edit Step</h5>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body">
                    <asp:HiddenField ID="hdnEditStepOrder" runat="server" />
                    <div class="form-group">
                        <label>Role</label>
                        <asp:DropDownList ID="ddlEditRole" runat="server" CssClass="form-control" />
                    </div>
                    <div class="form-group">
                        <label>Description</label>
                        <asp:TextBox ID="txtEditDesc" runat="server" CssClass="form-control" />
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>
                    <asp:Button ID="btnSaveStep" runat="server" Text="Save Changes" CssClass="btn btn-primary" OnClick="btnSaveStep_Click" />
                </div>
            </div>
        </div>
    </div>
</asp:Content>
