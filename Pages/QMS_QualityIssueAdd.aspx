<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="QMS_QualityIssueAdd.aspx.cs"
    Inherits="IT_WorkPlant.Pages.QMS_AddQualityIssue"
    MasterPageFile="~/Pages/QMS.Master"
    Title="Add New Issue" %>

<asp:Content ID="Head1" ContentPlaceHolderID="head" runat="server">
    <style>.qms-card{background:var(--card);border:1px solid var(--border);border-radius:var(--radius)}</style>
</asp:Content>

<asp:Content ID="Body1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="d-flex align-items-center justify-content-between mb-3">
        <a href='<%= ResolveUrl("~/Pages/QMS_QualityIssuesList.aspx") %>' class="btn btn-light">
            <i class="bi bi-arrow-left"></i> Back to List
        </a>
        <h2 class="fw-bold m-0">Add New Quality Issue</h2>
        <span></span>
    </div>

    <!-- Alerts -->
    <asp:Panel ID="pnlAlert" runat="server" CssClass="alert alert-danger d-none" Visible="false">
        <asp:Literal ID="litError" runat="server" />
    </asp:Panel>
    <asp:Panel ID="pnlSuccess" runat="server" CssClass="alert alert-success d-none" Visible="false">
        Saved successfully (mock).
    </asp:Panel>

    <div class="qms-card p-3">
        <h6 class="mb-3">Quality Issue Information</h6>
        <div class="row g-3">
            <div class="col-12 col-md-2">
                <label class="form-label">Production Year *</label>
                <asp:DropDownList ID="ddlYear" runat="server" CssClass="form-select" />
            </div>
            <div class="col-12 col-md-2">
                <label class="form-label">Production Week *</label>
                <asp:DropDownList ID="ddlWeek" runat="server" CssClass="form-select" />
            </div>
            <div class="col-12 col-md-2">
                <label class="form-label">Date *</label>
                <asp:TextBox ID="txtDate" runat="server" CssClass="form-control" TextMode="Date" />
            </div>

            <div class="col-12 col-md-3">
                <label class="form-label">Production Unit *</label>
                <asp:DropDownList ID="ddlUnit" runat="server" CssClass="form-select" />
            </div>
            <div class="col-12 col-md-3">
                <label class="form-label">Production Line *</label>
                <asp:DropDownList ID="ddlLine" runat="server" CssClass="form-select" />
            </div>

            <div class="col-12 col-md-3">
                <label class="form-label">Defect Category *</label>
                <asp:DropDownList ID="ddlDefectCategory" runat="server" CssClass="form-select" />
            </div>
            <div class="col-12 col-md-3">
                <label class="form-label">Defect Sub-Type *</label>
                <asp:DropDownList ID="ddlDefectSubType" runat="server" CssClass="form-select" />
            </div>

            <div class="col-12 col-md-3">
                <label class="form-label">Product Series *</label>
                <asp:DropDownList ID="ddlProductSeries" runat="server" CssClass="form-select" />
            </div>

            <div class="col-12">
                <label class="form-label">Issue Description *</label>
                <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
            </div>

            <div class="col-6 col-md-3">
                <label class="form-label">Downtime (Hours)</label>
                <asp:TextBox ID="txtDownH" runat="server" CssClass="form-control" />
            </div>
            <div class="col-6 col-md-3">
                <label class="form-label">Downtime (Minutes)</label>
                <asp:TextBox ID="txtDownM" runat="server" CssClass="form-control" />
            </div>

            <div class="col-12 col-md-4">
                <label class="form-label">Responsible Department *</label>
                <asp:DropDownList ID="ddlDept" runat="server" CssClass="form-select" />
            </div>
            <div class="col-12 col-md-4">
                <label class="form-label">Responsible Person *</label>
                <asp:TextBox ID="txtPerson" runat="server" CssClass="form-control" />
            </div>

            <div class="col-12">
                <label class="form-label">Short-Term Solution</label>
                <asp:TextBox ID="txtShort" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" />
            </div>
            <div class="col-12">
                <label class="form-label">Remarks</label>
                <asp:TextBox ID="txtRemarks" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" />
            </div>
        </div>

        <div class="d-flex gap-2 mt-4">
            <asp:Button ID="btnSave" runat="server" CssClass="btn btn-dark" Text="Save" OnClick="btnSave_Click" />
            <asp:Button ID="btnReset" runat="server" CssClass="btn btn-outline-secondary" Text="Reset" OnClick="btnReset_Click" />
        </div>
    </div>
</asp:Content>
