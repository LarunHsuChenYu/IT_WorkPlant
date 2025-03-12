<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="IT_RequestsList.aspx.cs" 
    Inherits="IT_WorkPlant.Pages.IT_RequestsList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>IT Requests List</title>
    <style>
        .filter-container {
            display: flex;
            flex-wrap: wrap;
            gap: 10px;
            margin-bottom: 20px;
        }

        .filter-container select {
            padding: 5px;
            font-size: 14px;
            width: 200px;
        }

        .myGridView {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }

        .myGridView th, .myGridView td {
            border: 1px solid #ddd;
            padding: 8px;
            text-align: left;
        }

        .myGridView th {
            background-color: #007BFF;
            color: white;
            font-weight: bold;
        }

        .myGridView tr:nth-child(even) {
            background-color: #f2f2f2;
        }

        .myGridView tr:hover {
            background-color: #ddd;
        }

        .myGridView td {
            color: #333;
        }

        h2 {
            margin-bottom: 20px;
            color: #007BFF;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="Server">
    <h2>IT Requests List</h2>

    <!-- Filters Section -->
    <div class="filter-container">
        <asp:DropDownList ID="ddlIssueDate" runat="server" AutoPostBack="True" OnSelectedIndexChanged="FilterChanged">
            <asp:ListItem Value="" Text="All Issue Dates"></asp:ListItem>
        </asp:DropDownList>

        <asp:DropDownList ID="ddlDeptName" runat="server" AutoPostBack="True" OnSelectedIndexChanged="FilterChanged">
            <asp:ListItem Value="" Text="All Departments"></asp:ListItem>
        </asp:DropDownList>

        <asp:DropDownList ID="ddlRequestUser" runat="server" AutoPostBack="True" OnSelectedIndexChanged="FilterChanged">
            <asp:ListItem Value="" Text="All Request Users"></asp:ListItem>
        </asp:DropDownList>

        <asp:DropDownList ID="ddlIssueType" runat="server" AutoPostBack="True" OnSelectedIndexChanged="FilterChanged">
            <asp:ListItem Value="" Text="All Issue Types"></asp:ListItem>
        </asp:DropDownList>

        <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="True" OnSelectedIndexChanged="FilterChanged">
            <asp:ListItem Value="" Text="All Statuses"></asp:ListItem>
        </asp:DropDownList>
    </div>

   <asp:GridView ID="gvRequests" runat="server" AutoGenerateColumns="False" CssClass="myGridView"
        OnRowEditing="gvRequests_RowEditing"
        OnRowCancelingEdit="gvRequests_RowCancelingEdit"
        OnRowUpdating="gvRequests_RowUpdating">
        <HeaderStyle BackColor="LightBlue" ForeColor="Black" Font-Bold="True" />
        <FooterStyle BackColor="LightBlue" ForeColor="Black" />
        <RowStyle BackColor="LightCyan" ForeColor="DarkBlue" Font-Italic="True" />
        <AlternatingRowStyle BackColor="PaleTurquoise" ForeColor="DarkBlue" Font-Italic="True" />

        <Columns>
            <asp:BoundField DataField="ReportID" HeaderText="Report ID" ReadOnly="true" />
            <asp:BoundField DataField="IssueDate" HeaderText="Issue Date" ReadOnly="true" />
            <asp:TemplateField HeaderText="Department">
                <ItemTemplate>
                    <%# Eval("Department") %>
                    <asp:HiddenField ID="hfDeptIndex" runat="server" Value='<%# Eval("Department") %>' />
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:DropDownList ID="ddlDept" runat="server" CssClass="form-control"></asp:DropDownList>
                </EditItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="RequestUser" HeaderText="Request User" ReadOnly="true" />
            <asp:BoundField DataField="IssueDetails" HeaderText="Issue Details" ReadOnly="true" />
            <asp:BoundField DataField="IssueType" HeaderText="Issue Type" ReadOnly="true" />
            <asp:TemplateField HeaderText="DRI User">
                <ItemTemplate>
                    <%# Eval("DRI_UserName") %>
                    <asp:HiddenField ID="hfDRIUserIndex" runat="server" Value='<%# Eval("DRI_UserID") %>' />
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:DropDownList ID="ddlDRIUser" runat="server" CssClass="form-control"></asp:DropDownList>
                </EditItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Solution" HeaderText="Solution" />
            <asp:TemplateField HeaderText="Status">
                <ItemTemplate>
                    <%# Eval("Status") %>
                    <asp:HiddenField ID="hfStatus" runat="server" Value='<%# Eval("StatusValue") %>' />
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control">
                        <asp:ListItem Text="WIP" Value="0"></asp:ListItem>
                        <asp:ListItem Text="Done" Value="1"></asp:ListItem>
                    </asp:DropDownList>
                </EditItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="LastUpdateDate" HeaderText="Last Update Date" ReadOnly="true" />
            <asp:BoundField DataField="FinishedDate" HeaderText="Finished Date" />
            <asp:BoundField DataField="Remark" HeaderText="Remark" />
            <asp:CommandField ShowEditButton="True" />
        </Columns>
    </asp:GridView>

</asp:Content>
