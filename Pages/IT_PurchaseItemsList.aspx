<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="IT_PurchaseItemsList.aspx.cs" Inherits="IT_WorkPlant.Models.IT_PurchaseItemsList" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container bg-white p-4 rounded shadow-sm">

        <!-- Header -->
        <div class="mb-2 text-center">
            <h2 class="fw-bold text-danger">🧾 <asp:Literal ID="litPageTitle" runat="server" /></h2>
        </div>

        <!-- Breadcrumb -->
        <div class="mb-4">
            <small class="text-muted"><asp:Literal ID="litBreadcrumb" runat="server" /></small>
        </div>

        <!-- Button Group -->
        <div class="mb-3 d-flex justify-content-center gap-2">
            <asp:Button ID="btnAddNew" runat="server" CssClass="btn btn-danger" Text="➕ Add New Item" OnClick="btnAddNew_Click" />
            <asp:Button ID="btnDeleteSelected" runat="server" CssClass="btn btn-outline-secondary" Text="🗑️ Delete Selected" OnClick="btnDeleteSelected_Click" />
        </div>

        <!-- GridView -->
        <asp:GridView ID="gvItems" runat="server" AutoGenerateColumns="False"
            CssClass="table table-hover border border-1 rounded shadow-sm"
            DataKeyNames="ItemID"
            OnRowEditing="gvItems_RowEditing" OnRowUpdating="gvItems_RowUpdating"
            OnRowCancelingEdit="gvItems_RowCancelingEdit" OnRowDeleting="gvItems_RowDeleting"
            OnRowDataBound="gvItems_RowDataBound">
            <Columns>
                <asp:TemplateField HeaderText="#">
                    <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="ItemName" HeaderText="Item Name" SortExpression="ItemName" />
                <asp:BoundField DataField="Category" HeaderText="Category" SortExpression="Category" />
                <asp:BoundField DataField="Unit" HeaderText="Unit" />
                <asp:BoundField DataField="UnitPrice" HeaderText="Unit Price" DataFormatString="{0:N2}" />
                <asp:TemplateField HeaderText="Active">
    <ItemTemplate>
        <asp:CheckBox ID="chkStatusRow" runat="server" Checked='<%# Convert.ToBoolean(Eval("Status")) %>' Enabled="false" />
    </ItemTemplate>
</asp:TemplateField>
                <asp:BoundField DataField="Description" HeaderText="Description" />
                <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
            </Columns>
        </asp:GridView>

        <!-- Add New Panel -->
       <asp:Panel ID="pnlAddItem" runat="server" Visible="false" CssClass="mt-4 border rounded p-4 bg-light-subtle">
    <h5 class="text-danger fw-bold mb-3">🛠️ Add New Item</h5>
    <div class="row g-3">
        <!-- Row 1 -->
        <div class="col-md-6">
            <label class="form-label text-danger fw-semibold">Item Name</label>
            <asp:TextBox ID="txtItemName" runat="server" CssClass="form-control" />
        </div>
        <div class="col-md-6">
            <label class="form-label text-danger fw-semibold">Category</label>
            <asp:TextBox ID="txtCategory" runat="server" CssClass="form-control" />
        </div>

        <!-- Row 2 -->
        <div class="col-md-6">
            <label class="form-label text-danger fw-semibold">Unit</label>
            <asp:TextBox ID="txtUnit" runat="server" CssClass="form-control" />
        </div>
        <div class="col-md-6">
            <label class="form-label text-danger fw-semibold">Unit Price</label>
            <asp:TextBox ID="txtUnitPrice" runat="server" CssClass="form-control" />
        </div>

        <!-- Row 3 -->
        <div class="col-md-6">
            <label class="form-label text-danger fw-semibold">Description</label>
            <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" />
        </div>
        <div class="col-md-6 d-flex align-items-end">
            <div class="form-check">
                <asp:CheckBox ID="chkStatus" runat="server" CssClass="form-check-input" Checked="true" />
                <label class="form-check-label text-danger fw-semibold ms-1">Active</label>
            </div>
        </div>
    </div>

    <div class="mt-4 d-flex gap-2">
        <asp:Button ID="btnSaveItem" runat="server" CssClass="btn btn-success" Text="💾 Save" OnClick="btnSaveItem_Click" />
        <asp:Button ID="btnCancelAdd" runat="server" CssClass="btn btn-outline-secondary" Text="Cancel" OnClick="btnCancelAdd_Click" />
    </div>
</asp:Panel>


    </div>
</asp:Content>
