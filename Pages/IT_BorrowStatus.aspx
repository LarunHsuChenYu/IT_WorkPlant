<%@ Page Title="Borrow Status" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="IT_BorrowStatus.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_BorrowStatus" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Device Borrow Status</title>
    <script>
        function validateSerial() {
            var ddl = document.getElementById('<%= ddlSerial.ClientID %>');
            var pnl = document.getElementById('<%= pnlSerial.ClientID %>');

            if (ddl && pnl && pnl.style.display !== "none" && (ddl.selectedIndex === 0 || ddl.value === "")) {
                alert("Please select a serial number before loading.");
                return false;
            }
            return true;
        }
    </script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
      <div class="p-4" style="background: linear-gradient(to right, #dbeafe, #fdf2f8);">
    <div class="container py-4">
        <div class="mb-4">
            <h3>📅 Device Borrow Status</h3>
            <asp:Label ID="ltItemName" runat="server" CssClass="h5 text-primary"></asp:Label>
        </div>

        <div class="row g-3 align-items-end mb-3">
            <div class="col-md-3">
                <label class="form-label">Select Date:</label>
                <asp:TextBox ID="txtDate" runat="server" CssClass="form-control" TextMode="Date"></asp:TextBox>
            </div>

            <div class="col-md-3">
                <label class="form-label">View Type:</label>
                <asp:DropDownList ID="ddlViewType" runat="server" CssClass="form-control">
                    <asp:ListItem Text="Daily" Value="daily" />
                    <asp:ListItem Text="Weekly" Value="weekly" />
                    <asp:ListItem Text="Monthly" Value="monthly" />
                </asp:DropDownList>
            </div>

            <asp:Panel ID="pnlSerial" runat="server" CssClass="col-md-3">
                <label class="form-label">Select Serial:</label>
                <asp:DropDownList ID="ddlSerial" runat="server" CssClass="form-select"
                    AutoPostBack="True" OnSelectedIndexChanged="ddlSerial_SelectedIndexChanged">
                </asp:DropDownList>
            </asp:Panel>

            <div class="col-md-3">
                <label class="form-label d-block invisible">.</label>
                <asp:Button ID="btnLoad" runat="server" Text="View Status"
                    CssClass="btn btn-primary w-100"
                    OnClick="btnLoad_Click" OnClientClick="return validateSerial();" />
            </div>
        </div>

        <div class="mt-4">
            <asp:Literal ID="ltMatrixTable" runat="server"
                Text="<p class='text-muted'>Please select a serial and date, then click 'View Status' to see availability.</p>">
            </asp:Literal>
        </div>
    </div>
          </div>
</asp:Content>
