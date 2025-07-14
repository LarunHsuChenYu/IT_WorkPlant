<%@ Page Title="IT Service Purchase" Language="C#" 
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="IT_Stuff_Purchase.aspx.cs"
    Inherits="IT_WorkPlant.Pages.IT_Stuff_Purchase"
    Async="true"
    ValidateRequest="false" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container bg-white p-4 rounded shadow-sm">
        <!-- ข้อมูลผู้ใช้ -->
        <div class="row mb-4">
            <div class="col-md-4">
                <label class="form-label text-danger">👤 Employee Name:</label>
                <asp:TextBox ID="txtEmpName" runat="server" CssClass="form-control border-danger-subtle bg-light-subtle" ReadOnly="true" />
            </div>
            <div class="col-md-4">
                <label class="form-label text-danger">🏢 Department:</label>
                <asp:TextBox ID="txtDept" runat="server" CssClass="form-control border-danger-subtle bg-light-subtle" ReadOnly="true" />
            </div>
            <div class="col-md-4">
                <label class="form-label text-danger">📅 Request Date:</label>
                <asp:TextBox ID="txtDate" runat="server" CssClass="form-control border-danger-subtle bg-light-subtle" ReadOnly="true" />
            </div>
        </div>

        <!-- ตารางกรอกสินค้า -->
        <asp:GridView ID="gvPurchaseItems" runat="server" AutoGenerateColumns="False"
            CssClass="table table-hover border border-1 rounded shadow-sm"
            OnRowDataBound="gvPurchaseItems_RowDataBound">
            <Columns>
                <asp:TemplateField HeaderText="#" ItemStyle-Width="5%">
                    <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Category" ItemStyle-Width="15%"> 
                    <ItemTemplate>
                        <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-select form-select-sm"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlCategory_SelectedIndexChanged" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Item Name" ItemStyle-Width="20%">
                    <ItemTemplate>
                        <asp:DropDownList ID="ddlItem" runat="server" CssClass="form-select form-select-sm"
                            AutoPostBack="true" OnSelectedIndexChanged="ddlItem_SelectedIndexChanged" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Purchase Reason" ItemStyle-Width="25%">
                    <ItemTemplate>
                        <asp:TextBox ID="txtUsage" runat="server" CssClass="form-control form-control-sm" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Qty" ItemStyle-Width="8%">
                    <ItemTemplate>
                        <asp:TextBox ID="txtQty" runat="server" CssClass="form-control form-control-sm" TextMode="Number"
                            AutoPostBack="true" OnTextChanged="txtQty_TextChanged" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Unit" ItemStyle-Width="7%">
                    <ItemTemplate>
                        <asp:Label ID="lblUnit" runat="server" CssClass="text-muted" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Price" ItemStyle-Width="10%">
                    <ItemTemplate>
                        <asp:Label ID="lblPrice" runat="server" CssClass="text-muted" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Subtotal" ItemStyle-Width="10%">
                    <ItemTemplate>
                        <asp:Label ID="lblSubtotal" runat="server" CssClass="fw-semibold text-danger" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <!-- ปุ่ม Submit -->
        <div class="text-center mt-4">
            <asp:Button ID="btnSubmit" runat="server" Text="🛒 Submit Purchase Request" CssClass="btn btn-danger px-4 py-2"
                OnClick="btnSubmit_Click" />
        </div>
    </div>
</asp:Content>
