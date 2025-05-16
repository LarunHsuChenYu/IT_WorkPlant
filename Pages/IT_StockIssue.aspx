<%@ Page Title="เบิกของออกจากคลัง" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="IT_StockIssue.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_StockIssue" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card p-4 shadow-sm border border-danger mb-4">
    <h4 class="fw-bold text-danger mb-4">📤 <%= GetLabel("title") %></h4>

    <div class="row g-3">
        <div class="col-md-4">
            <label class="form-label"><%# GetLabel("issuedate") %></label>
            <asp:TextBox ID="txtIssueDate" runat="server" CssClass="form-control" TextMode="Date" />
        </div>
        <div class="col-md-4">
            <label class="form-label"><%# GetLabel("issuedby") %></label>
            <asp:DropDownList ID="ddlIssuedBy" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlIssuedBy_SelectedIndexChanged" />
        </div>
        <div class="col-md-4">
            <label class="form-label"><%# GetLabel("department") %></label>
            <asp:TextBox ID="txtDepartment" runat="server" CssClass="form-control" ReadOnly="true" />
        </div>

        <div class="col-md-4">
            <label class="form-label"><%# GetLabel("productcode") %></label>
            <asp:DropDownList ID="ddlProductCode" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlProductCode_SelectedIndexChanged" />
        </div>
        <div class="col-md-4">
            <label class="form-label"><%# GetLabel("productname") %></label>
            <asp:TextBox ID="txtProductName" runat="server" CssClass="form-control" ReadOnly="true" />
        </div>
        <div class="col-md-4">
            <label class="form-label"><%# GetLabel("model") %></label>
            <asp:TextBox ID="txtModel" runat="server" CssClass="form-control" ReadOnly="true" />
        </div>

        <div class="col-md-4">
            <label class="form-label"><%# GetLabel("unit") %></label>
            <asp:TextBox ID="txtUnit" runat="server" CssClass="form-control" ReadOnly="true" />
        </div>
        <div class="col-md-4">
            <label class="form-label"><%# GetLabel("quantity") %></label>
            <asp:TextBox ID="txtQuantity" runat="server" CssClass="form-control" TextMode="Number" />
        </div>
        <div class="col-md-4">
            <label class="form-label"><%# GetLabel("purpose") %></label>
            <asp:TextBox ID="txtPurpose" runat="server" CssClass="form-control" />
        </div>

        <div class="col-md-4">
            <label class="form-label"><%# GetLabel("issuetype") %></label>
            <asp:DropDownList ID="ddlIssueType" runat="server" CssClass="form-select">
                <asp:ListItem Text="Select Type" Value="" />
                <asp:ListItem Text="Used" Value="Used" />
                <asp:ListItem Text="Borrowed" Value="Borrowed" />
            </asp:DropDownList>
        </div>
        <div class="col-md-4">
            <label class="form-label"><%# GetLabel("approvedby") %></label>
            <asp:TextBox ID="txtApprovedBy" runat="server" CssClass="form-control" ReadOnly="true" />
        </div>
        <div class="col-md-4">
            <label class="form-label"><%# GetLabel("status") %></label>
            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                <asp:ListItem Text="รออนุมัติ" Value="รออนุมัติ" />
                <asp:ListItem Text="อนุมัติ" Value="อนุมัติ" />
                <asp:ListItem Text="ไม่อนุมัติ" Value="ไม่อนุมัติ" />
            </asp:DropDownList>
        </div>
    </div>

    <!-- ✅ ปุ่ม -->
    <div class="row mt-3">
        <div class="col-12 d-flex justify-content-center">
            <asp:Button ID="Button1" runat="server" Text='<%# GetLabel("submit") %>' CssClass="btn btn-danger px-4" OnClick="btnSubmit_Click" />
        </div>
    </div>
</div>

    
            <!-- 📋 ประวัติการเบิก --><!-- 📋 ประวัติการเบิก -->
<div class="card shadow-sm border border-success p-4 mt-4">
    <h5 class="fw-bold text-success mb-3">📋 <%# GetLabel("history") %></h5>

    <!-- ปุ่ม filter -->
    <div class="mb-3 d-flex gap-2">
        <asp:Button ID="btnShowAll" runat="server" Text='<%# GetLabel("filter_all") %>' CssClass="btn btn-outline-secondary" OnClick="btnShowAll_Click" />
        <asp:Button ID="btnShowUsed" runat="server" Text='<%# GetLabel("filter_used") %>' CssClass="btn btn-danger" OnClick="btnShowUsed_Click" />
        <asp:Button ID="btnShowBorrowed" runat="server" Text='<%# GetLabel("filter_borrowed") %>' CssClass="btn btn-outline-primary" OnClick="btnShowBorrowed_Click" />\
    </div>

    <asp:Panel ID="pnlUsed" runat="server" Visible="false">
    <asp:GridView ID="gvUsed" runat="server" CssClass="table table-bordered table-striped" AutoGenerateColumns="False" OnRowCreated="gvUsed_RowCreated">
        <Columns>
            <asp:BoundField DataField="IssueDate" HeaderText="issuedate" DataFormatString="{0:yyyy-MM-dd}" />
            <asp:BoundField DataField="IssuedBy" HeaderText="issuedby" />
            <asp:BoundField DataField="Department" HeaderText="department" />
            <asp:BoundField DataField="ProductName" HeaderText="productname" />
            <asp:BoundField DataField="Model" HeaderText="model" />
            <asp:BoundField DataField="Quantity" HeaderText="quantity" />
            <asp:BoundField DataField="Purpose" HeaderText="purpose" />
            <asp:BoundField DataField="Status" HeaderText="status" />
            <asp:BoundField DataField="IssueType" HeaderText="issuetype" />
            <asp:BoundField DataField="ApprovedBy" HeaderText="approvedby" />
        </Columns>
    </asp:GridView>
</asp:Panel>

<!-- Grid สำหรับยืมของ -->
<asp:Panel ID="pnlBorrowed" runat="server" Visible="false">
    <asp:GridView ID="gvBorrowed" runat="server" CssClass="table table-bordered table-striped" AutoGenerateColumns="False" OnRowCreated="gvBorrowed_RowCreated">
        <Columns>
            <asp:BoundField DataField="IssueDate" HeaderText="issuedate" DataFormatString="{0:yyyy-MM-dd}" />
            <asp:BoundField DataField="IssuedBy" HeaderText="issuedby" />
            <asp:BoundField DataField="Department" HeaderText="department" />
            <asp:BoundField DataField="ProductName" HeaderText="productname" />
            <asp:BoundField DataField="Model" HeaderText="model" />
            <asp:BoundField DataField="Quantity" HeaderText="quantity" />
            <asp:BoundField DataField="Purpose" HeaderText="purpose" />
            <asp:BoundField DataField="Status" HeaderText="status" />
            <asp:BoundField DataField="IssueType" HeaderText="issuetype" />
            <asp:BoundField DataField="ApprovedBy" HeaderText="approvedby" />
            <asp:BoundField DataField="IsReturned" HeaderText="isreturned" />
            <asp:BoundField DataField="ReturnDate" HeaderText="returndate" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
            <asp:TemplateField HeaderText="return">
                <ItemTemplate>
                    <asp:Button ID="btnReturn" runat="server" Text='<%# GetLabel("return") %>'
                        CommandArgument='<%# Eval("IssueID") %>'
                        OnClick="btnReturn_Click"
                        Visible='<%# Eval("IssueType").ToString() == "Borrowed" && (Eval("IsReturned") == DBNull.Value || !(bool)Eval("IsReturned")) %>'
                        CssClass="btn btn-success btn-sm" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
</asp:Panel>
</div>
    </asp:Content>
