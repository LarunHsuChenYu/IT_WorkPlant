<%@ Page Title="IT Service Purchase" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" 
    CodeBehind="~/Pages/IT_Stuff_Purchase.aspx.cs"
    Inherits="IT_WorkPlant.Pages.IT_Stuff_Purchase" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="page-header">
        <h2><asp:Literal ID="litPageTitle" runat="server" /></h2>
    </div>

    <div class="form-section">
        <table class="form-table">
            <tr>
                <td><asp:Label ID="lblEmpName" runat="server" Text="Employee Name:" /></td>
                <td><asp:TextBox ID="txtEmpName" runat="server" CssClass="form-control" ReadOnly="true" /></td>
                <td><asp:Label ID="lblDept" runat="server" Text="Department:" /></td>
                <td><asp:TextBox ID="txtDept" runat="server" CssClass="form-control" ReadOnly="true" /></td>
            </tr>
            <tr>
                <td><asp:Label ID="lblDate" runat="server" Text="Request Date:" /></td>
                <td><asp:TextBox ID="txtDate" runat="server" CssClass="form-control" ReadOnly="true" /></td>
                <td><asp:Label ID="lblReason" runat="server" Text="Purchase Reason:" /></td>
                <td><asp:TextBox ID="txtReason" runat="server" CssClass="form-control" /></td>
            </tr>
        </table>
    </div>

    <asp:GridView ID="gvPurchaseItems" runat="server" AutoGenerateColumns="False"
    CssClass="table table-bordered"
    OnRowDataBound="gvPurchaseItems_RowDataBound">
        <Columns>
            <asp:TemplateField HeaderText="#">
                <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Item Name">
                <ItemTemplate>
                    <asp:DropDownList ID="ddlItem" runat="server" CssClass="form-control" 
                        AutoPostBack="true" OnSelectedIndexChanged="ddlItem_SelectedIndexChanged" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Usage">
                <ItemTemplate>
                    <asp:TextBox ID="txtUsage" runat="server" CssClass="form-control" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Quantity">
                <ItemTemplate>
                    <asp:TextBox ID="txtQty" runat="server" CssClass="form-control" TextMode="Number" 
                        AutoPostBack="true" OnTextChanged="txtQty_TextChanged"/>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Unit">
                <ItemTemplate>
                    <asp:Label ID="lblUnit" runat="server" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Unit Price">
                <ItemTemplate>
                    <asp:Label ID="lblPrice" runat="server" />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Subtotal">
                <ItemTemplate>
                    <asp:Label ID="lblSubtotal" runat="server" />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>

    <div class="form-section text-right">
        <%--<asp:Button ID="btnAddRow" runat="server" CssClass="btn btn-info" Text="+ Add Row" OnClick="btnAddRow_Click" />--%>
        <asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-success" Text="Submit Purchase Request" OnClick="btnSubmit_Click" />
    </div>
</asp:Content>