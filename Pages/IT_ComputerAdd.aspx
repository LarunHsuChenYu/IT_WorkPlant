<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="IT_ComputerAdd.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_ComputerAdd" %>
<%@ Import Namespace="System.Web" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .card-shadow {
            box-shadow: 0 0 25px rgba(0, 0, 0, 0.07);
            border-radius: 16px;
        }

        .form-title {
            font-size: 24px;
            font-weight: 600;
            margin-bottom: 20px;
            color: #dc3545;
        }

        .form-label {
            font-weight: 500;
            margin-bottom: 5px;
            display: block;
        }

        .section-divider {
            border-top: 2px solid #eee;
            margin: 30px 0 20px;
        }

        .btn-lg {
            padding: 12px 30px;
            font-size: 16px;
            border-radius: 8px;
        }
    </style>

    <div class="container mt-4">
        <h3 class="mb-4 text-danger fw-bold">Add Computer</h3>
        <div class="row g-4">
            <!-- Column 1 -->
            <div class="col-md-6">
                <asp:TextBox ID="txtUserID" runat="server" CssClass="form-control mb-3" placeholder="User ID" AutoPostBack="true" OnTextChanged="txtUserID_TextChanged" />
                <asp:TextBox ID="txtUserName" runat="server" CssClass="form-control mb-3" placeholder="Employee Name" />
                <asp:TextBox ID="txtDeptName" runat="server" CssClass="form-control mb-3" placeholder="Department" />

                <label class="form-label">Type</label>
                <div class="mb-3">
                    <asp:RadioButton ID="rdbPC" GroupName="Type" runat="server" Text="PC" CssClass="form-check-input me-1" />
                    <asp:RadioButton ID="rdbNB" GroupName="Type" runat="server" Text="NB" CssClass="form-check-input ms-3" />
                </div>

                <asp:TextBox ID="txtNamePC" runat="server" CssClass="form-control mb-3" placeholder="Computer Name" />
                <asp:TextBox ID="txtBrand" runat="server" CssClass="form-control mb-3" placeholder="Brand" />
                <asp:TextBox ID="txtModel" runat="server" CssClass="form-control mb-3" placeholder="Model" />
                <asp:TextBox ID="txtSerialNumber" runat="server" CssClass="form-control mb-3" placeholder="Serial Number" />
            </div>

            <!-- Column 2 -->
            <div class="col-md-6">
                <asp:TextBox ID="txtColumnCode" runat="server" CssClass="form-control mb-3" placeholder="ERP Column Code" />

                <asp:DropDownList ID="ddlSystem" runat="server" CssClass="form-control mb-3">
                    <asp:ListItem Text="-- เลือก System --" Value="" />
                    <asp:ListItem Text="Windows 10 Pro" />
                    <asp:ListItem Text="Windows 11 Pro" />
                </asp:DropDownList>

                <asp:DropDownList ID="ddlWarranty" runat="server" AutoPostBack="true" OnSelectedIndexChanged="ddlWarranty_SelectedIndexChanged" CssClass="form-control mb-3">
                    <asp:ListItem Text="-- เลือก Warranty --" Value="" />
                    <asp:ListItem Text="Yes" Value="Yes" />
                    <asp:ListItem Text="No" Value="No" />
                </asp:DropDownList>

                <asp:Panel ID="pnlWarrantyPeriod" runat="server" Visible="false" CssClass="mb-3">
                    <asp:TextBox ID="txtWarrantyYears" runat="server" CssClass="form-control" placeholder="Warranty Period (Years)" />
                </asp:Panel>

                <asp:TextBox ID="txtLocation" runat="server" CssClass="form-control mb-3" placeholder="Location" />
                <asp:TextBox ID="txtAssetCode" runat="server" CssClass="form-control mb-3" placeholder="Asset Code" />
                <asp:TextBox ID="txtPrice" runat="server" CssClass="form-control mb-3" placeholder="Price" />

                <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-control mb-3">
                    <asp:ListItem Text="-- Select Status --" Value="" />
                    <asp:ListItem Text="ใช้งาน" />
                    <asp:ListItem Text="ไม่ได้ใช้งาน" />
                </asp:DropDownList>

                <asp:TextBox ID="txtPurchaseDate" runat="server" CssClass="form-control mb-3" placeholder="Purchase Date (yyyy-MM-dd)" />
            </div>
        </div>

        <div class="text-end mt-4">
            <asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-danger btn-lg" Text="Save Data" OnClick="btnSubmit_Click" />
            <asp:Button ID="btnReset" runat="server" CssClass="btn btn-outline-secondary btn-lg ms-2" Text="Reset" OnClientClick="return confirm('Clear form?');" CausesValidation="false" />
        </div>

        <div class="mt-3">
            <asp:HyperLink ID="lnkViewComputerList" runat="server" NavigateUrl="~/Pages/IT_ComputerListView.aspx" CssClass="btn btn-outline-primary">
                📋 View All Computers
            </asp:HyperLink>
        </div>
    </div>
</asp:Content>
