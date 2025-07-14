<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" 
    CodeBehind="IT_RequestsList.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_RequestsList" %>
<%@ Import Namespace="System.Web" %>


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

        .myGridView th {
            background: linear-gradient(to right, #2e8b57, #3cb371); 
            color: white;
            font-weight: bold;
            text-align: left;
        }

        .myGridView tr:nth-child(odd) {
            background: linear-gradient(to right, #e0fbe8, #c6f3d4); 
        }

        .myGridView tr:nth-child(even) {
            background: linear-gradient(to right, #d4fcdc, #b9efc5);
        }

        .myGridView tr:hover {
            background: linear-gradient(to right, #a4f5af, #80eb9e);
        }

        .myGridView th,
        .myGridView td {
            border: 1px solid black;
            padding: 8px;
            text-align: center;        
            vertical-align: middle;   
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
        <asp:DropDownList ID="ddlIssueMonth" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged" />

         <asp:DropDownList ID="ddlIssueDate" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
        <asp:ListItem Value="" Text="All Dates" />
    </asp:DropDownList>

        <asp:DropDownList ID="ddlDeptName" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
            <asp:ListItem Value="" Text="All Departments" />
        </asp:DropDownList>
        <asp:DropDownList ID="ddlRequestUser" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
            <asp:ListItem Value="" Text="All Request Users" />
        </asp:DropDownList>
        <asp:DropDownList ID="ddlIssueType" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
            <asp:ListItem Value="" Text="All Issue Types" />
        </asp:DropDownList>
        <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
            <asp:ListItem Value="" Text="All Status" />
        </asp:DropDownList>
    </div>

    <!-- GridView Section -->
    <asp:GridView ID="gvRequests" runat="server" AutoGenerateColumns="False" CssClass="myGridView"
    DataKeyNames="ReportID,Department,IssueTypeID,DeptNameID"
    OnRowEditing="gvRequests_RowEditing"
    OnRowCancelingEdit="gvRequests_RowCancelingEdit"
    OnRowUpdating="gvRequests_RowUpdating"
    OnSelectedIndexChanged="gvRequests_SelectedIndexChanged">

        <HeaderStyle BackColor="LightBlue" ForeColor="Black" Font-Bold="True" />
        <FooterStyle BackColor="LightBlue" ForeColor="Black" />
        <RowStyle BackColor="LightCyan" ForeColor="DarkBlue" Font-Italic="True" />
        <AlternatingRowStyle BackColor="PaleTurquoise" ForeColor="DarkBlue" Font-Italic="True" />

        <Columns>
            <asp:BoundField DataField="ReportID" HeaderText="Report ID" ReadOnly="true" />

            <asp:TemplateField HeaderText="Issue Date">
                <ItemTemplate>
                    <%# Eval("IssueDate", "{0:yyyy-MM-dd}") %>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:Label ID="lblIssueDate" runat="server" Text='<%# Eval("IssueDate", "{0:yyyy-MM-dd}") %>' />
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Department">
    <ItemTemplate>
        <%# Eval("Department") %>
    </ItemTemplate>
    <EditItemTemplate>
        <asp:DropDownList ID="ddlDepartment" runat="server" Enabled="false">
            <asp:ListItem Text="Production Control Dept." Value="Production Control Dept." />
            <asp:ListItem Text="EHS Dept." Value="EHS Dept." />
            <asp:ListItem Text="Quality Control Dept." Value="Quality Control Dept." />
            <asp:ListItem Text="Purchase Dept." Value="Purchase Dept." />
            <asp:ListItem Text="Manufacturing Division" Value="Manufacturing Division" />
        </asp:DropDownList>
        <asp:HiddenField ID="hfDeptNameID" runat="server" Value='<%# Eval("DeptNameID") %>' />
    </EditItemTemplate>
</asp:TemplateField>


            <asp:BoundField DataField="RequestUser" HeaderText="Request User" ReadOnly="true" />
            <asp:TemplateField HeaderText="Issue Details"><ItemTemplate> <%# HttpUtility.HtmlDecode(Eval("IssueDetails").ToString()) %></ItemTemplate></asp:TemplateField>


            <asp:TemplateField HeaderText="Issue Type">
                <ItemTemplate>
                    <%# Eval("IssueType") %>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:DropDownList ID="ddlIssueType" runat="server" />
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="DRI User">
                <ItemTemplate>
                    <%# Eval("DRI_UserName") %>
                    <asp:HiddenField ID="hfDRIUserIndex" runat="server" Value='<%# Eval("DRI_UserID") %>' />
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:DropDownList ID="ddlDRIUser" runat="server" CssClass="form-control" />
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
                        <asp:ListItem Text="WIP" Value="0" />
                        <asp:ListItem Text="Done" Value="1" />
                    </asp:DropDownList>
                </EditItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Last Update Date">
                <ItemTemplate>
                    <%# Eval("LastUpdateDate", "{0:yyyy-MM-dd}") %>
                </ItemTemplate>
            </asp:TemplateField>

            <asp:TemplateField HeaderText="Finished Date">
                <ItemTemplate>
                    <asp:Label ID="lblFinishedDate" runat="server" 
                        Text='<%# Eval("FinishedDate") != DBNull.Value ? Eval("FinishedDate", "{0:yyyy-MM-dd}") : " " %>' />
                </ItemTemplate>
            </asp:TemplateField>

            <asp:BoundField DataField="Remark" HeaderText="Remark" />

            <asp:TemplateField HeaderText="Image">
    <ItemTemplate>
        <asp:HyperLink ID="lnkImage" runat="server"
            NavigateUrl='<%# Eval("ImagePath", "~/App_Temp/{0}") %>'
            Target="_blank">
            <asp:Image ID="imgThumb" runat="server"
                ImageUrl='<%# Eval("ImagePath", "~/App_Temp/{0}") %>'
                Width="60px" Height="60px"
                Visible='<%# !string.IsNullOrEmpty(Eval("ImagePath").ToString()) %>' />
        </asp:HyperLink>
    </ItemTemplate>
</asp:TemplateField>


            <asp:TemplateField HeaderText="Action">
                <ItemTemplate>
                    <asp:LinkButton ID="btnEdit" runat="server" CommandName="Edit" Text="Edit" />
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:LinkButton ID="btnUpdate" runat="server" CommandName="Update" Text="Update" />
                    &nbsp;
                    <asp:LinkButton ID="btnCancel" runat="server" CommandName="Cancel" Text="Cancel" />
                    &nbsp;
                    <asp:LinkButton ID="btnDelete" runat="server" Text="Delete"
                        CommandArgument='<%# Eval("ReportID") %>'
                        OnClick="btnDelete_Click"
                        OnClientClick="return confirm('Do you want to delete this data?');"
                        ForeColor="Red" />
                </EditItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
</asp:Content>
