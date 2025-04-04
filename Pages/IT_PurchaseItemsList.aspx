<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="IT_PurchaseItemsList.aspx.cs" Inherits="IT_WorkPlant.Models.IT_PurchaseItemsList" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <h2><asp:Literal ID="litPageTitle" runat="server" /></h2>
        <asp:Literal ID="litBreadcrumb" runat="server" />
    </div>

    <div class="button-bar">
        <asp:Button ID="btnAddNew" runat="server" CssClass="btn btn-primary" Text="+ Add New Item" OnClick="btnAddNew_Click" />
        <asp:Button ID="btnDeleteSelected" runat="server" CssClass="btn btn-danger" Text="Delete Selected" OnClick="btnDeleteSelected_Click" />
    </div>

    <asp:GridView ID="gvItems" runat="server" AutoGenerateColumns="False" CssClass="table table-striped table-bordered"
        DataKeyNames="ItemID" OnRowEditing="gvItems_RowEditing" OnRowUpdating="gvItems_RowUpdating" OnRowCancelingEdit="gvItems_RowCancelingEdit"
        OnRowDeleting="gvItems_RowDeleting">
        <Columns>
            <asp:TemplateField HeaderText="#">
                <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="ItemName" HeaderText="Item Name" SortExpression="ItemName" />
            <asp:BoundField DataField="Category" HeaderText="Category" SortExpression="Category" />
            <asp:BoundField DataField="Unit" HeaderText="Unit" />
            <asp:BoundField DataField="UnitPrice" HeaderText="Unit Price" DataFormatString="{0:N2}" />
            <asp:CheckBoxField DataField="Status" HeaderText="Active" />
            <asp:BoundField DataField="Description" HeaderText="Description" />
            <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
        </Columns>
    </asp:GridView>

    <asp:Panel ID="pnlAddItem" runat="server" Visible="false" CssClass="panel panel-default">
        <div class="panel-heading">Add New Item</div>
        <div class="panel-body">
            <asp:Label AssociatedControlID="txtItemName" runat="server" Text="Item Name:" /><br />
            <asp:TextBox ID="txtItemName" runat="server" CssClass="form-control" /><br />

            <asp:Label AssociatedControlID="txtCategory" runat="server" Text="Category:" /><br />
            <asp:TextBox ID="txtCategory" runat="server" CssClass="form-control" /><br />

            <asp:Label AssociatedControlID="txtUnit" runat="server" Text="Unit:" /><br />
            <asp:TextBox ID="txtUnit" runat="server" CssClass="form-control" /><br />

            <asp:Label AssociatedControlID="txtUnitPrice" runat="server" Text="Unit Price:" /><br />
            <asp:TextBox ID="txtUnitPrice" runat="server" CssClass="form-control" /><br />

            <asp:Label AssociatedControlID="txtDescription" runat="server" Text="Description:" /><br />
            <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" /><br />

            <asp:CheckBox ID="chkStatus" runat="server" Text="Active" Checked="true" /><br /><br />

            <asp:Button ID="btnSaveItem" runat="server" CssClass="btn btn-success" Text="Save" OnClick="btnSaveItem_Click" />
            <asp:Button ID="btnCancelAdd" runat="server" CssClass="btn btn-default" Text="Cancel" OnClick="btnCancelAdd_Click" />
        </div>
    </asp:Panel>
</asp:Content>
