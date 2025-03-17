<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" 
    CodeBehind="IT_RequestsList.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_RequestsList" %>

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

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>IT Requests List</h2>
    
    <!-- Filters Section -->
    <div class="filter-container">
        <asp:DropDownList ID="ddlIssueMonth" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged"></asp:DropDownList>
        <asp:DropDownList ID="ddlDeptName" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
            <asp:ListItem Value="" Text="All Departments"></asp:ListItem>
        </asp:DropDownList>
        <asp:DropDownList ID="ddlRequestUser" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
            <asp:ListItem Value="" Text="All Request Users"></asp:ListItem>
        </asp:DropDownList>
        <asp:DropDownList ID="ddlIssueType" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
            <asp:ListItem Value="" Text="All Issue Types"></asp:ListItem>
        </asp:DropDownList>
        <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
            <asp:ListItem Value="" Text="All Statuses"></asp:ListItem>
        </asp:DropDownList>
    </div>

    <!-- GridView Section -->
    <asp:GridView ID="gvRequests" runat="server" AutoGenerateColumns="False" CssClass="myGridView"
        DataKeyNames="ReportID,Department"
        OnRowEditing="gvRequests_RowEditing"
        OnRowCancelingEdit="gvRequests_RowCancelingEdit"
        OnRowUpdating="gvRequests_RowUpdating">
        
        <HeaderStyle BackColor="LightBlue" ForeColor="Black" Font-Bold="True" />
        <FooterStyle BackColor="LightBlue" ForeColor="Black" />
        <RowStyle BackColor="LightCyan" ForeColor="DarkBlue" Font-Italic="True" />
        <AlternatingRowStyle BackColor="PaleTurquoise" ForeColor="DarkBlue" Font-Italic="True" />
        
        <Columns>
            <asp:BoundField DataField="ReportID" HeaderText="Report ID" ReadOnly="true" />
            <asp:BoundField DataField="IssueDate" HeaderText="Issue Date" DataFormatString="{0:yyyy-MM-dd}" />
            
            <asp:TemplateField HeaderText="Department">
                <ItemTemplate>
                    <%# Eval("Department") %>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:DropDownList ID="ddlDepartment" runat="server">
                        <asp:ListItem Text="Production Control Dept." Value="Production Control Dept." />
                        <asp:ListItem Text="EHS Dept." Value="EHS Dept." />
                        <asp:ListItem Text="Quality Control Dept." Value="Quality Control Dept." />
                        <asp:ListItem Text="Purchase Dept." Value="Purchase Dept." />
                        <asp:ListItem Text="Manufacturing Division" Value="Manufacturing Division" />
                    </asp:DropDownList>
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
                    <asp:DropDownList ID="DropDownList1" runat="server" CssClass="form-control">
                        <asp:ListItem Text="WIP" Value="0"></asp:ListItem>
                        <asp:ListItem Text="Done" Value="1"></asp:ListItem>
                    </asp:DropDownList>
                </EditItemTemplate>
            </asp:TemplateField>
            
            <asp:BoundField DataField="LastUpdateDate" HeaderText="Last Update Date" ReadOnly="true" />
            <asp:TemplateField HeaderText="Finished Date">
                <ItemTemplate>
                    <asp:Label ID="lblFinishedDate" runat="server" 
                        Text='<%# Eval("FinishedDate") != DBNull.Value ? Eval("FinishedDate", "{0:yyyy-MM-dd HH:mm:ss}") : "⏳ Done = Date!" %>'>
                    </asp:Label>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Remark" HeaderText="Remark" />
            <asp:CommandField ShowEditButton="True" />
        </Columns>
    </asp:GridView>
</asp:Content>
