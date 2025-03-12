<%@ Page Title="Shipping Price Maintain" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"  
    CodeBehind="PMC_CUS_ShippingPriceMaintain.aspx.cs" Inherits="IT_WorkPlant.Pages.PMC_CUS_ShippingPriceMaintain" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Shipping Price Maintenance</h2>

    <!-- 搜索功能 -->
    <div class="form-group">
        <asp:Table ID="tblSearch" runat="server" CssClass="search-table">
            <asp:TableRow>
                <asp:TableCell>
                    <label for="tbSearchInvCode">Search Inventroy Code:</label>
                    <asp:TextBox ID="tbSearchInvCode" CssClass="form-control" runat="server"></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell>
                    <label for="tbSearchSKU">Search SKU:</label>
                    <asp:TextBox ID="tbSearchSKU" CssClass="form-control" runat="server"></asp:TextBox>
                </asp:TableCell>
                <asp:TableCell>
                    <label for="tbSearchItemName">Search Product Name:</label>
                    <asp:TextBox ID="tbSearchItemName" CssClass="form-control" runat="server"></asp:TextBox>
                </asp:TableCell>
                
            </asp:TableRow>
        </asp:Table>
        <br />
        <asp:Button ID="btnSearch" runat="server" CssClass="btn btn-primary" Text="Search" OnClick="btnSearch_Click" />
    </div>

    <br />

    <!-- 資料顯示 -->
    <asp:GridView ID="gvShippingPrices" runat="server" AutoGenerateColumns="false" CssClass="table table-striped"
        AllowPaging="true" PageSize="10" OnRowEditing="gvShippingPrices_RowEditing"
        OnRowUpdating="gvShippingPrices_RowUpdating" OnRowCancelingEdit="gvShippingPrices_RowCancelingEdit"
        OnPageIndexChanging="gvShippingPrices_PageIndexChanging">
        <Columns>
            <asp:BoundField DataField="Prod_ID" HeaderText="Id" ReadOnly="true" />
            <asp:BoundField DataField="Inventory_Code" HeaderText="Inv. Code" ReadOnly="true" />
            <asp:TemplateField HeaderText="SKU">
                <ItemTemplate>
                    <asp:Label ID="lblSKU" runat="server" Text='<%# Bind("Prod_SKU") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:Label ID="lblEditSKU" runat="server" Text='<%# Bind("Prod_SKU") %>'></asp:Label>
                </EditItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Item_Name" HeaderText="Product Name" ReadOnly="true" />
            <asp:TemplateField HeaderText="Price">
                <ItemTemplate>
                    <asp:Label ID="lblPrice" runat="server" Text='<%# Bind("Update_Price", "{0:N2}") %>'></asp:Label>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="tbPrice" runat="server" Text='<%# Bind("Update_Price") %>'></asp:TextBox>
                </EditItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Update_Date" HeaderText="Update Date" DataFormatString="{0:yyyy-MM-dd}" ReadOnly="true" />
            <asp:CommandField ShowEditButton="true" />
        </Columns>
    </asp:GridView>



    <br />

    <div class="form-group">
        <h3>Insert New One</h3>

        <label for="tbNewInvCode">Inventory Code:</label>
        <asp:TextBox ID="tbNewInvCode" CssClass="form-control" runat="server"></asp:TextBox>

        <label for="tbNewSKU">SKU:</label>
        <asp:TextBox ID="tbNewSKU" CssClass="form-control" runat="server"></asp:TextBox>

        <label for="tbNewItemName">Product Name:</label>
        <asp:TextBox ID="tbNewItemName" CssClass="form-control" runat="server"></asp:TextBox>

        <label for="tbNewPrice">Price:</label>
        <asp:TextBox ID="tbNewPrice" CssClass="form-control" runat="server"></asp:TextBox>
        <br />
        <asp:Button ID="btnInsert" runat="server" CssClass="btn btn-success" Text="Add New One" OnClick="btnInsert_Click" />
    </div>

    <br />

    <div class="form-group">
        <label for="fileUpload">Upload Excel:</label>
        <asp:FileUpload ID="fileUpload" CssClass="form-control" runat="server" /> 
        <asp:Button ID="btnUpload" runat="server" CssClass="btn btn-success" Text="Upload & Update Prices" OnClick="btnUpload_Click" />
    </div>

    <asp:Label ID="lblStatus" runat="server" CssClass="status-label" Text=""></asp:Label>
</asp:Content>
