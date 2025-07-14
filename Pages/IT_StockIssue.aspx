<%@ Page Title="เบิกของออกจากคลัง" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="IT_StockIssue.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_StockIssue" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card p-4 shadow-sm border border-danger mb-4">
    <h4 class="fw-bold text-danger mb-4">📤 เบิกของออกจากคลัง</h4>

    <div class="row g-3">
        <div class="col-md-4">
            <label class="form-label">วันที่เบิก</label>
            <asp:TextBox ID="txtIssueDate" runat="server" CssClass="form-control" TextMode="Date" />
        </div>
        <div class="col-md-4">
            <label class="form-label">ผู้เบิก</label>
            <asp:DropDownList ID="ddlIssuedBy" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlIssuedBy_SelectedIndexChanged" />
        </div>
        <div class="col-md-4">
            <label class="form-label">แผนก</label>
            <asp:TextBox ID="txtDepartment" runat="server" CssClass="form-control" ReadOnly="true" />
        </div>

        <div class="col-md-4">
            <label class="form-label">รหัสสินค้า</label>
            <asp:DropDownList ID="ddlProductCode" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlProductCode_SelectedIndexChanged" />
        </div>
        <div class="col-md-4">
            <label class="form-label">ชื่อสินค้า</label>
            <asp:TextBox ID="txtProductName" runat="server" CssClass="form-control" ReadOnly="true" />
        </div>
        <div class="col-md-4">
            <label class="form-label">รุ่น</label>
            <asp:TextBox ID="txtModel" runat="server" CssClass="form-control" ReadOnly="true" />
        </div>

        <div class="col-md-4">
            <label class="form-label">หน่วย</label>
            <asp:TextBox ID="txtUnit" runat="server" CssClass="form-control" ReadOnly="true" />
        </div>
        <div class="col-md-4">
            <label class="form-label">จำนวนเบิก</label>
            <asp:TextBox ID="txtQuantity" runat="server" CssClass="form-control" TextMode="Number" />
        </div>
        <div class="col-md-4">
            <label class="form-label">ใช้ทำอะไร</label>
            <asp:TextBox ID="txtPurpose" runat="server" CssClass="form-control" />
        </div>

        <div class="col-md-4">
            <label class="form-label">สถานะการเบิก</label>
            <asp:DropDownList ID="ddlIssueType" runat="server" CssClass="form-select">
                <asp:ListItem Text="Select Type" Value="" />
                <asp:ListItem Text="Used" Value="Used" />
                <asp:ListItem Text="Borrowed" Value="Borrowed" />
            </asp:DropDownList>
        </div>
        <div class="col-md-4">
            <label class="form-label">ผู้รับเรื่อง</label>
            <asp:TextBox ID="txtApprovedBy" runat="server" CssClass="form-control" ReadOnly="true" />
        </div>
        <div class="col-md-4">
            <label class="form-label">สถานะอนุมัติ</label>
            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select">
                <asp:ListItem Text="รออนุมัติ" Value="รออนุมัติ" />
                <asp:ListItem Text="อนุมัติ" Value="อนุมัติ" />
                <asp:ListItem Text="ไม่อนุมัติ" Value="ไม่อนุมัติ" />
            </asp:DropDownList>
        </div>
    </div>

    <!-- ✅ ปุ่ม -->
    <div class="row mt-3">
        <div class="col-12 d-flex justify-content-center">
            <asp:Button ID="Button1" runat="server" Text="🚀 เบิกของออก" CssClass="btn btn-danger px-4" OnClick="btnSubmit_Click" />
        </div>
    </div>
</div>

    
            <!-- 📋 ประวัติการเบิก --><!-- 📋 ประวัติการเบิก -->
<div class="card shadow-sm border border-success p-4 mt-4">
    <h5 class="fw-bold text-success mb-3">📋 ประวัติการเบิกของล่าสุด</h5>

    <!-- ปุ่ม filter -->
    <div class="mb-3 d-flex gap-2">
        <asp:Button ID="btnShowAll" runat="server" Text="ทั้งหมด" CssClass="btn btn-outline-secondary" OnClick="btnShowAll_Click" />
        <asp:Button ID="btnShowUsed" runat="server" Text="เบิกใช้" CssClass="btn btn-danger" OnClick="btnShowUsed_Click" />
        <asp:Button ID="btnShowBorrowed" runat="server" Text="ยืมของ" CssClass="btn btn-outline-primary" OnClick="btnShowBorrowed_Click" />
    </div>

    <!-- Grid สำหรับเบิกใช้ -->
    <asp:Panel ID="pnlUsed" runat="server" Visible="false">
        <asp:GridView ID="gvUsed" runat="server" CssClass="table table-bordered table-striped" AutoGenerateColumns="False">
            <Columns>
                <asp:BoundField DataField="IssueDate" HeaderText="วันที่เบิก" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="IssuedBy" HeaderText="ผู้เบิก" />
                <asp:BoundField DataField="Department" HeaderText="แผนก" />
                <asp:BoundField DataField="ProductName" HeaderText="สินค้า" />
                <asp:BoundField DataField="Model" HeaderText="รุ่น" />
                <asp:BoundField DataField="Quantity" HeaderText="จำนวน" />
                <asp:BoundField DataField="Purpose" HeaderText="ใช้ทำอะไร" />
                <asp:BoundField DataField="Status" HeaderText="สถานะการเบิก" />
                <asp:BoundField DataField="IssueType" HeaderText="ประเภท" />
                <asp:BoundField DataField="ApprovedBy" HeaderText="ผู้รับเรื่อง" />
            </Columns>
        </asp:GridView>
    </asp:Panel>

    <!-- Grid สำหรับยืมของ -->
    <asp:Panel ID="pnlBorrowed" runat="server" Visible="false">
        <asp:GridView ID="gvBorrowed" runat="server" CssClass="table table-bordered table-striped" AutoGenerateColumns="False">
            <Columns>
                <asp:BoundField DataField="IssueDate" HeaderText="วันที่เบิก" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="IssuedBy" HeaderText="ผู้เบิก" />
                <asp:BoundField DataField="Department" HeaderText="แผนก" />
                <asp:BoundField DataField="ProductName" HeaderText="สินค้า" />
                <asp:BoundField DataField="Model" HeaderText="รุ่น" />
                <asp:BoundField DataField="Quantity" HeaderText="จำนวน" />
                <asp:BoundField DataField="Purpose" HeaderText="ใช้ทำอะไร" />
                <asp:BoundField DataField="Status" HeaderText="สถานะการเบิก" />
                <asp:BoundField DataField="IssueType" HeaderText="ประเภท" />
                <asp:BoundField DataField="ApprovedBy" HeaderText="ผู้รับเรื่อง" />
                <asp:BoundField DataField="IsReturned" HeaderText="คืนของแล้ว?" />
                <asp:BoundField DataField="ReturnDate" HeaderText="วันที่คืนของ" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                <asp:TemplateField HeaderText="คืนของ">
                    <ItemTemplate>
                        <asp:Button ID="btnReturn" runat="server" Text="✅ คืนของ"
                            CommandArgument='<%# Eval("IssueID") %>'
                            OnClick="btnReturn_Click"
                            Visible='<%# Eval("IssueType").ToString() == "Borrowed" && (Eval("IsReturned") == DBNull.Value || !(bool)Eval("IsReturned")) %>'
                            CssClass="btn btn-success btn-sm" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </asp:Panel>
</div>
    </asp:Content>
