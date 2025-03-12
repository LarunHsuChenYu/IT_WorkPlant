<%@ Page Title="User Management" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="IT_UserManagement.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_UserManagement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="text-center">User Account of IT Workplate Management</h2>

    <!-- 搜尋區域 -->
    <div class="card mb-3">
        <div class="card-header bg-primary text-white">
            <strong>Search User</strong>
        </div>
        <div class="form-group">
            <asp:Table ID="tblSearch" runat="server" CssClass="search-table">
                <asp:TableRow>
                    <asp:TableCell>
                        <label for="tbSearchEmpID">Search by onboard ID:</label>
                        <asp:TextBox ID="tbSearchEmpID" CssClass="form-control" runat="server" placeholder="Enter onboard ID"></asp:TextBox>
                    </asp:TableCell>
                    <asp:TableCell>
                        <label for="tbSearchName">Search by Name:</label>
                        <asp:TextBox ID="tbSearchName" CssClass="form-control" runat="server" placeholder="Enter Name"></asp:TextBox>
                    </asp:TableCell>
                    <asp:TableCell>
                        <label for="ddlSearchDept">Search by Department:</label>
                        <asp:DropDownList ID="ddlSearchDept" CssClass="form-control" runat="server">
                            <asp:ListItem Text="All Departments" Value="" />
                        </asp:DropDownList>
                    </asp:TableCell>
                </asp:TableRow>
            </asp:Table>
            <br />
            <asp:Button ID="btnSearch" runat="server" CssClass="btn btn-primary" Text="Search" OnClick="btnSearch_Click" />
        </div>

    </div>

    <!-- 顯示數據 -->
    <div class="card mb-3">
        <div class="card-header bg-secondary text-white">
            <strong>User List</strong>
        </div>
        <div class="card-body">
            <asp:GridView ID="gvUsers" runat="server" AutoGenerateColumns="false" CssClass="table table-striped table-hover"
                AllowPaging="true" PageSize="10" 
                OnRowEditing="gvUsers_RowEditing"
                OnRowUpdating="gvUsers_RowUpdating" 
                OnRowCancelingEdit="gvUsers_RowCancelingEdit"
                OnPageIndexChanging="gvUsers_PageIndexChanging">
                <Columns>
                    <asp:BoundField DataField="UserIndex" HeaderText="Index" ReadOnly="true" />
                    
                    <asp:TemplateField HeaderText="Name">
                        <ItemTemplate>
                            <asp:Label ID="lblUserEmpID" runat="server" Text='<%# Bind("UserEmpID") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:Label ID="lblUserEmpID" runat="server" Text='<%# Bind("UserEmpID") %>'></asp:Label>
                        </EditItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Name">
                        <ItemTemplate>
                            <asp:Label ID="lblUserName" runat="server" Text='<%# Bind("UserName") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="tbUserName" runat="server" Text='<%# Bind("UserName") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Email">
                        <ItemTemplate>
                            <asp:Label ID="lblUserEmpMail" runat="server" Text='<%# Bind("UserEmpMail") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="tbUserEmpMail" runat="server" Text='<%# Bind("UserEmpMail") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Department">
                        <ItemTemplate>
                            <asp:Label ID="lblDeptName" runat="server" Text='<%# Bind("DeptName") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="tbDeptName" runat="server" Text='<%# Bind("DeptName") %>'></asp:TextBox>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:TemplateField HeaderText="Password">
                        <ItemTemplate>
                            <asp:Label ID="lblUserEmpPW" runat="server" Text='<%# Bind("UserEmpPW") %>'></asp:Label>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:TextBox ID="tbUserEmpPW" runat="server" Text='<%# Bind("UserEmpPW") %>'></asp:TextBox>
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
            <strong>Add New User</strong>
        </div>
        <div class="card-body">
            <div class="row">
                <div class="col-md-6">
                    <label for="tbNewEmpID">onboard ID:</label>
                    <asp:TextBox ID="tbNewEmpID" CssClass="form-control" runat="server" placeholder="Enter onboard ID"></asp:TextBox>
                </div>
                <div class="col-md-6">
                    <label for="tbNewName">Name:</label>
                    <asp:TextBox ID="tbNewName" CssClass="form-control" runat="server" placeholder="Enter Name"></asp:TextBox>
                </div>
                <div class="col-md-6">
                    <label for="tbNewEmail">Email:</label>
                    <asp:TextBox ID="tbNewEmail" CssClass="form-control" runat="server" placeholder="Enter Email"></asp:TextBox>
                </div>
                <div class="col-md-6">
                    <label for="ddlNewDept">Department:</label>
                    <asp:DropDownList ID="ddlNewDept" CssClass="form-control" runat="server"></asp:DropDownList>
                </div>
                
                <div class="col-md-6">
                    <label for="tbNewPassword">Password:</label>
                    <asp:TextBox ID="tbNewPassword" CssClass="form-control" runat="server" placeholder="Enter Password"></asp:TextBox>
                </div>
                <div class="col-md-12 mt-4">
                    <asp:Button ID="btnInsert" runat="server" CssClass="btn btn-success w-100" Text="Add New User" OnClick="btnInsert_Click" />
                </div>
                
            </div>
            <asp:Label ID="lblStatus" runat="server" CssClass="status-label" Text=""></asp:Label>
        </div>
    </div>
</asp:Content>
