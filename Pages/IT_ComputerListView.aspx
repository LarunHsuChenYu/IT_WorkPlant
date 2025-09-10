<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="IT_ComputerListView.aspx.cs"
    Inherits="IT_WorkPlant.Pages.IT_ComputerListView" %>

<asp:Content ID="Head" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        .pill { border-radius: 999px; margin-right:6px }
        .dept-btn.active { color:#fff !important; background:#0d6efd !important }
        .table > :not(caption) > * > * { vertical-align: middle }

        /* toolbar = แถวเดียว */
        .toolbar{display:flex; align-items:center; justify-content:space-between; gap:12px; flex-wrap:nowrap}
        .search-wrap{display:flex; gap:8px; flex:0 0 auto}
        .dept-wrap{display:flex; align-items:center; gap:8px; white-space:nowrap; overflow-x:auto; padding-bottom:4px; flex:1 1 auto}
        .dept-wrap .muted{color:#6c757d}
        .dept-wrap .count-badge{margin-left:4px}

        .w-90{width:90px}
        .w-110{width:110px}
        .w-140{width:140px}
    </style>
</asp:Content>

<asp:Content ID="Body" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid py-3">
        <h3 class="mb-3">Computer &amp; Notebook List</h3>

        <!-- SUMMARY CARDS (CLICKABLE) -->
<div class="row mb-3 align-items-stretch">
    <div class="col-md-3 col-sm-6 mb-2">
        <asp:LinkButton ID="btnCardTotal" runat="server" OnClick="btnCardTotal_Click" CssClass="text-decoration-none">
            <div class="card text-bg-primary shadow-sm h-100" style="cursor:pointer;">
                <div class="card-body text-center">
                    <h6 class="card-title mb-1 text-white">🗂️Total</h6>
                    <h3 class="mb-0 text-white"><asp:Label ID="lblTotalComputers" runat="server" Text="0" /></h3>
                </div>
            </div>
        </asp:LinkButton>
    </div>

    <div class="col-md-3 col-sm-6 mb-2">
        <asp:LinkButton ID="btnCardPC" runat="server" OnClick="btnCardPC_Click" CssClass="text-decoration-none">
            <div class="card text-bg-success shadow-sm h-100" style="cursor:pointer;">
                <div class="card-body text-center">
                    <h6 class="card-title mb-1 text-white">🖥️PC</h6>
                    <h3 class="mb-0 text-white"><asp:Label ID="lblTotalPC" runat="server" Text="0" /></h3>
                </div>
            </div>
        </asp:LinkButton>
    </div>

    <div class="col-md-3 col-sm-6 mb-2">
        <asp:LinkButton ID="btnCardNB" runat="server" OnClick="btnCardNB_Click" CssClass="text-decoration-none">
            <div class="card text-bg-info shadow-sm h-100" style="cursor:pointer;">
                <div class="card-body text-center">
                    <h6 class="card-title mb-1 text-white">💻Notebook</h6>
                    <h3 class="mb-0 text-white"><asp:Label ID="lblTotalNB" runat="server" Text="0" /></h3>
                </div>
            </div>
        </asp:LinkButton>
    </div>

    <div class="col-md-3 col-sm-6 mb-2">
        <asp:LinkButton ID="btnCardWarranty" runat="server" OnClick="btnCardWarranty_Click" CssClass="text-decoration-none">
            <div class="card text-bg-warning shadow-sm h-100" style="cursor:pointer;">
                <div class="card-body text-center">
                    <h6 class="card-title mb-1">💼Warranty</h6>
                    <h3 class="mb-0"><asp:Label ID="lblWarrantyCount" runat="server" Text="0" /></h3>
                </div>
            </div>
        </asp:LinkButton>
    </div>
</div>

<!-- ปุ่ม Add Device -->
<div class="mb-3 text-end">
    <asp:HyperLink ID="btnAddDevice" runat="server"
        NavigateUrl="~/Pages/IT_ComputerAdd.aspx"
        CssClass="btn btn-success btn-lg shadow-sm">
        <i class="bi bi-plus-circle"></i> ➕Add New Device
    </asp:HyperLink>
</div>


        <!-- SINGLE ROW TOOLBAR -->
        <div class="toolbar mb-2">
            <div class="search-wrap">
                <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" Width="280"
                    placeholder="Search anything..." AutoPostBack="true"
                    OnTextChanged="txtSearch_TextChanged" />
                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-outline-secondary"
                    OnClick="btnClear_Click" />
            </div>

            <div class="dept-wrap">
                <!-- ตัวกรองปัจจุบัน + Total -->
                <span class="muted">
                    <asp:Label ID="lblActiveFilter" runat="server" />
                    <span class="count-badge">
                        • Total:
                        <span class="badge bg-secondary"><asp:Label ID="lblCount" runat="server" /></span>
                    </span>
                </span>

                <!-- ปุ่มแผนก -->
                <asp:PlaceHolder ID="phDeptButtons" runat="server" />
            </div>
        </div>

        <!-- GRIDVIEW -->
        <asp:GridView ID="gvDevices" runat="server" CssClass="table table-striped table-bordered"
            AllowPaging="true" PageSize="20"
            AllowSorting="true"
            AutoGenerateColumns="False"
            DataKeyNames="ComputerID"
            OnPageIndexChanging="gvDevices_PageIndexChanging"
            OnSorting="gvDevices_Sorting"
            OnRowDataBound="gvDevices_RowDataBound"
            OnRowEditing="gvDevices_RowEditing"
            OnRowCancelingEdit="gvDevices_RowCancelingEdit"
            OnRowUpdating="gvDevices_RowUpdating"
            OnRowCommand="gvDevices_RowCommand">

            <Columns>
                <%-- No. --%>
                <asp:TemplateField HeaderText="No." ItemStyle-Width="60">
                    <ItemTemplate><asp:Label ID="lblNo" runat="server" /></ItemTemplate>
                </asp:TemplateField>

                <%-- Computer --%>
                <asp:TemplateField HeaderText="Computer" SortExpression="ComputerName">
                    <ItemTemplate><%# Eval("ComputerName") %></ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtComputerName" runat="server" CssClass="form-control"
                            Text='<%# Bind("ComputerName") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>

                <%-- User (แก้ได้เฉพาะรหัส) --%>
                <asp:TemplateField HeaderText="User">
                    <ItemTemplate><%# Eval("EmpId") %></ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtUserID" runat="server" CssClass="form-control w-110"
                            Text='<%# Bind("EmpId") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>

                <%-- User Name (ReadOnly) --%>
                <asp:TemplateField HeaderText="User Name" SortExpression="UserName">
                    <ItemTemplate><%# Eval("UserName") %></ItemTemplate>
                    <EditItemTemplate>
                        <asp:Label ID="lblUserNameRO" runat="server" CssClass="form-control-plaintext"
                            Text='<%# Eval("UserName") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>

                <%-- Dept (ReadOnly) --%>
                <asp:TemplateField HeaderText="Dept" SortExpression="Dept">
                    <ItemTemplate><%# Eval("Dept") %></ItemTemplate>
                    <EditItemTemplate>
                        <asp:Label ID="lblDeptRO" runat="server" CssClass="form-control-plaintext"
                            Text='<%# Eval("Dept") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>

                <%-- Type --%>
                <asp:TemplateField HeaderText="Type" SortExpression="Type">
                    <ItemTemplate><%# Eval("Type") %></ItemTemplate>
                    <EditItemTemplate>
                        <asp:DropDownList ID="ddlType" runat="server" CssClass="form-select w-90"
                            SelectedValue='<%# Bind("Type") %>'>
                            <asp:ListItem Text="PC" Value="PC" />
                            <asp:ListItem Text="NB" Value="NB" />
                        </asp:DropDownList>
                    </EditItemTemplate>
                </asp:TemplateField>

                <%-- Brand --%>
                <asp:TemplateField HeaderText="Brand" SortExpression="Brand">
                    <ItemTemplate><%# Eval("Brand") %></ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtBrand" runat="server" CssClass="form-control"
                            Text='<%# Bind("Brand") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>

                <%-- Model --%>
                <asp:TemplateField HeaderText="Model" SortExpression="Model">
                    <ItemTemplate><%# Eval("Model") %></ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtModel" runat="server" CssClass="form-control"
                            Text='<%# Bind("Model") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>

                <%-- Serial --%>
                <asp:TemplateField HeaderText="Serial" SortExpression="Serial">
                    <ItemTemplate><%# Eval("Serial") %></ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtSerial" runat="server" CssClass="form-control"
                            Text='<%# Bind("Serial") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>

                <%-- Warranty --%>
                <asp:TemplateField HeaderText="Warranty" SortExpression="Warranty">
                    <ItemTemplate><%# Eval("Warranty") %></ItemTemplate>
                    <EditItemTemplate>
                        <asp:TextBox ID="txtWarranty" runat="server" CssClass="form-control"
                            Text='<%# Bind("Warranty") %>' />
                    </EditItemTemplate>
                </asp:TemplateField>

                <%-- Status (DropDown) --%>
                <asp:TemplateField HeaderText="Status" SortExpression="Status">
                    <ItemTemplate><%# Eval("Status") %></ItemTemplate>
                    <EditItemTemplate>
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                            <asp:ListItem Text="-- Select Status --" Value="" />
                            <asp:ListItem Text="Active" Value="Active" />
                            <asp:ListItem Text="Inactive" Value="Inactive" />
                        </asp:DropDownList>
                    </EditItemTemplate>
                </asp:TemplateField>

                <%-- Action (ย้ายไปท้าย) --%>
                <asp:TemplateField HeaderText="Action" ItemStyle-CssClass="w-140">
                    <ItemTemplate>
                        <asp:LinkButton ID="btnEdit" runat="server" CssClass="btn btn-sm btn-outline-primary me-1"
                            CommandName="Edit" Text="Edit" />
                        <asp:LinkButton ID="btnDelete" runat="server" CssClass="btn btn-sm btn-outline-danger"
                            CommandName="DeleteRow" CommandArgument='<%# Eval("ComputerID") %>' Text="Delete" />
                    </ItemTemplate>
                    <EditItemTemplate>
                        <asp:LinkButton ID="btnUpdate" runat="server" CssClass="btn btn-sm btn-success me-1"
                            CommandName="Update" Text="Save" />
                        <asp:LinkButton ID="btnCancel" runat="server" CssClass="btn btn-sm btn-secondary"
                            CommandName="Cancel" Text="Cancel" />
                    </EditItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>
