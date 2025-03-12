<%@ Page Title="Product CBM Maintain" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="PMC_CUS_ProductCBMMaintain.aspx.cs" Inherits="IT_WorkPlant.Pages.PMC_CUS_ProductCBMMaintain" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="text-center">PMC - Product CBM Maintain</h2>

    <!-- 搜尋區域 -->
    <div class="card mb-3">
        <div class="card-header bg-primary text-white">
            <strong>Search Product CBM</strong>
        </div>
        <div class="card-body">
            <div class="row">
                <div class="col-md-6">
                    <label for="tbSearchProdCode">Search by Product Code:</label>
                    <asp:TextBox ID="tbSearchProdCode" runat="server" CssClass="form-control" placeholder="Enter product code"></asp:TextBox>
                </div>
                <div class="col-md-6 mt-4">
                    <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary w-100" OnClick="btnSearch_Click" />
                </div>
            </div>
        </div>
    </div>

    <!-- 顯示數據 -->
    <div class="card mb-3">
        <div class="card-header bg-secondary text-white">
            <strong>Product CBM List</strong>
        </div>
        <div class="card-body">
             <asp:GridView ID="gvProductCBM" runat="server" AutoGenerateColumns="false" CssClass="table table-striped table-hover"
                 AllowPaging="true" PageSize="10" 
                 OnRowEditing="gvProductCBM_RowEditing"
                 OnRowUpdating="gvProductCBM_RowUpdating" 
                 OnRowCancelingEdit="gvProductCBM_RowCancelingEdit"
                 OnPageIndexChanging="gvProductCBM_PageIndexChanging">
                <Columns>
                    <asp:BoundField DataField="Vic_ProdCode" HeaderText="Product Code" ReadOnly="true" />

                    <asp:TemplateField HeaderText="Gross Weight">
                        <ItemTemplate>
                            <asp:Label ID="lblGrossWeight" runat="server" Text='<%# Bind("GrossWeight") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="tbGrossWeight" runat="server" Text='<%# Bind("GrossWeight") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Net Weight">
                        <ItemTemplate>
                            <asp:Label ID="lblNetWeight" runat="server" Text='<%# Bind("NetWeight") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="tbNetWeight" runat="server" Text='<%# Bind("NetWeight") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Volume (CBM)">
                        <ItemTemplate>
                            <asp:Label ID="lblVolCBM" runat="server" Text='<%# Bind("Vol_CBM") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="tbVolCBM" runat="server" Text='<%# Bind("Vol_CBM") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Length (mm)">
                        <ItemTemplate>
                            <asp:Label ID="lblLength" runat="server" Text='<%# Bind("Length") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="tbLength" runat="server" Text='<%# Bind("Length") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Width (mm)">
                        <ItemTemplate>
                            <asp:Label ID="lblWidth" runat="server" Text='<%# Bind("Width") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="tbWidth" runat="server" Text='<%# Bind("Width") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Height (mm)">
                        <ItemTemplate>
                            <asp:Label ID="lblHeight" runat="server" Text='<%# Bind("Height") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="tbHeight" runat="server" Text='<%# Bind("Height") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Units">
                        <ItemTemplate>
                            <asp:Label ID="lblUnits" runat="server" Text='<%# Bind("Units") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="tbUnits" runat="server" Text='<%# Bind("Units") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>

                    <asp:CommandField ShowEditButton="true" />
                </Columns>
            </asp:GridView>            
        </div>
    </div>

    <!-- 新增區域 -->
    <div class="card mb-3">
        <div class="card-header bg-success text-white">
            <strong>Add New Product CBM</strong>
        </div>
        <div class="card-body">
            <div class="row">
                <div class="col-md-6">
                    <label for="tbNewProdCode">Product Code:</label>
                    <asp:TextBox ID="tbNewProdCode" CssClass="form-control" runat="server" placeholder="Enter product code"></asp:TextBox>
                </div>
                <div class="col-md-6">
                    <label for="tbNewGrossWeight">Gross Weight:</label>
                    <asp:TextBox ID="tbNewGrossWeight" CssClass="form-control" runat="server" placeholder="Enter gross weight"></asp:TextBox>
                </div>
                <div class="col-md-6">
                    <label for="tbNewNetWeight">Net Weight:</label>
                    <asp:TextBox ID="tbNewNetWeight" CssClass="form-control" runat="server" placeholder="Enter net weight"></asp:TextBox>
                </div>
                <div class="col-md-6">
                    <label for="tbNewVolCBM">Volume (CBM):</label>
                    <asp:TextBox ID="tbNewVolCBM" CssClass="form-control" runat="server" placeholder="Enter volume"></asp:TextBox>
                </div>
                <div class="col-md-4">
                    <label for="tbNewLength">Length (mm):</label>
                    <asp:TextBox ID="tbNewLength" CssClass="form-control" runat="server" placeholder="Enter length"></asp:TextBox>
                </div>
                <div class="col-md-4">
                    <label for="tbNewWidth">Width (mm):</label>
                    <asp:TextBox ID="tbNewWidth" CssClass="form-control" runat="server" placeholder="Enter width"></asp:TextBox>
                </div>
                <div class="col-md-4">
                    <label for="tbNewHeight">Height (mm):</label>
                    <asp:TextBox ID="tbNewHeight" CssClass="form-control" runat="server" placeholder="Enter height"></asp:TextBox>
                </div>
                <div class="col-md-6">
                    <label for="tbNewUnits">Units:</label>
                    <asp:TextBox ID="tbNewUnits" CssClass="form-control" runat="server" placeholder="Enter units"></asp:TextBox>
                </div>
                <div class="col-md-6 mt-4">
                    <asp:Button ID="btnInsert" runat="server" CssClass="btn btn-success w-100" Text="Add New Product"
                        OnClick="btnInsert_Click" />
                </div>
            </div>
        </div>
    </div>

    <div class="card mb-3">
        <div class="card-body">
            <label for="fileUpload">Upload Excel:</label>
            <asp:FileUpload ID="fileUpload" CssClass="form-control" runat="server" /> 
            <asp:Button ID="btnUpload" runat="server" CssClass="btn btn-success" Text="Batch Update CBM Info." OnClick="btnUpload_Click" />
        </div>
    </div>

    <asp:Label ID="lblStatus" runat="server" CssClass="status-label" Text=""></asp:Label>
</asp:Content>
