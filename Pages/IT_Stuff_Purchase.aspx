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

        <!-- ปุ่ม -->
       <!-- 🔻 ฟอร์มกรอกข้อมูล MEMO -->
<div class="card p-3 mt-4 mb-3 shadow-sm border">
    <h5 class="text-danger fw-bold mb-3">📝 กรอกข้อมูล MEMO</h5>

    <div class="row mb-3">
        <div class="col-md-3">
            <label class="form-label">เลขที่ MEMO</label>
            <asp:TextBox ID="txtMemoNo" runat="server" CssClass="form-control" />
        </div>
        <div class="col-md-3">
            <label class="form-label">ถึง (TO:)</label>
            <asp:TextBox ID="txtMemoTo" runat="server" CssClass="form-control" Text="總經理" />
        </div>
        <div class="col-md-3">
            <label class="form-label">ผู้อนุมัติ (Approved By)</label>
            <asp:TextBox ID="txtApprovedBy" runat="server" CssClass="form-control" />
        </div>
        <div class="col-md-3">
            <label class="form-label">ผู้ตรวจสอบ (Reviewed By)</label>
            <asp:TextBox ID="txtReviewedBy" runat="server" CssClass="form-control" />
        </div>
    </div>

    <div class="row mb-3">
        <div class="col-md-6">
            <label class="form-label">หัวข้อ Subject</label>
            <asp:TextBox ID="txtMemoSubject" runat="server" CssClass="form-control" />
        </div>
        <div class="col-md-6">
            <label class="form-label">รายละเอียด MEMO</label>
            <asp:TextBox ID="txtMemoDetail" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
        </div>
    </div>

    <div class="mb-3">
        <label class="form-label fw-bold">Requester Department</label><br />
        <asp:CheckBox ID="chkDeptHardware" runat="server" Text="Hardware" CssClass="me-3" />
        <asp:CheckBox ID="chkDeptSoftware" runat="server" Text="Software" CssClass="me-3" />
    </div>

    <div class="mb-3">
        <label class="form-label fw-bold">Requester Type</label><br />
        <asp:CheckBox ID="chkUseERPComputer" runat="server" Text="ERP, use computer" CssClass="me-3" />
        <asp:CheckBox ID="chkMonitorNo" runat="server" Text="Monitor not included" CssClass="me-3" />
        <asp:CheckBox ID="chkERPLaptop" runat="server" Text="ERP, use a laptop" CssClass="me-3" />
        <asp:CheckBox ID="chkProdLine" runat="server" Text="Computer production line" CssClass="me-3" />
        <asp:CheckBox ID="chkMonitorYes" runat="server" Text="Monitor included" CssClass="me-1" />
        Size: <asp:TextBox ID="txtMonitorSize" runat="server" Width="80px" CssClass="form-control d-inline-block ms-1 me-3" />
        <asp:CheckBox ID="chkLaptopProd" runat="server" Text="Laptop for production line" CssClass="me-3" />
        <asp:CheckBox ID="chkDocWorkPC" runat="server" Text="Computer document work" CssClass="me-3" />
        <asp:CheckBox ID="chkDocWorkLaptop" runat="server" Text="Laptop document work" CssClass="me-3" />
        <asp:CheckBox ID="chkAppSys" runat="server" Text="Application System" CssClass="me-3" />
        <asp:CheckBox ID="chkSoftwareSuite" runat="server" Text="Software Suite" CssClass="me-3" />
        <asp:CheckBox ID="chkLicensedSW" runat="server" Text="Licensed software" CssClass="me-3" />
        <asp:CheckBox ID="chkOtherSW" runat="server" Text="Other software" CssClass="me-3" />
        <asp:CheckBox ID="chkOtherHW" runat="server" Text="Other hardware" CssClass="me-3" />
    </div>

    <div class="text-center">
        <asp:Button ID="btnGenerateMemo" runat="server" Text="🔄 แสดง MEMO" CssClass="btn btn-primary"
            OnClick="btnGenerateMemo_Click" />
    </div>
</div>





        <!-- 🔻 แสดง MEMO ที่กรอกแล้ว -->
<asp:Panel ID="pnlMemo" runat="server" CssClass="mt-4 p-4 border border-secondary bg-light shadow-sm" Visible="false">
    <h5 class="text-center fw-bold">恩泰實業（泰國）有限公司<br />Enrich Industrial (Thai) Co., Ltd.</h5>
    <h5 class="text-center border-bottom border-dark pb-2">內部聯絡單 MEMORANDUM</h5>

    <div class="row mb-2">
        <div class="col-md-6">編號 No.: <asp:Label ID="lblMemoNo" runat="server" CssClass="fw-bold" /></div>
        <div class="col-md-6 text-end">日期 Date: <asp:Label ID="lblMemoDate" runat="server" /></div>
    </div>

    <div>呈 TO: <asp:Label ID="lblMemoTo" runat="server" /></div>
    <div>CC: 信息</div>
    <div>主旨 Subject: <asp:Label ID="lblMemoSubject" runat="server" /></div>

    <div class="mt-3 border p-2 bg-white">
        <asp:Label ID="lblMemoDetail" runat="server" />
    </div>

    <div class="mt-3 border p-2 bg-white">
        <b>Requester Department:</b><br />
        <asp:Label ID="lblMemoDept" runat="server" /><br /><br />
        <b>Requester Type:</b><br />
        <asp:Label ID="lblMemoType" runat="server" />
    </div>

    <asp:GridView ID="gvMemo" runat="server" CssClass="table table-bordered text-center mt-3" AutoGenerateColumns="False">
        <Columns>
            <asp:BoundField HeaderText="Budget Description" DataField="ItemName" />
            <asp:BoundField HeaderText="Dept. Code" DataField="DeptCode" />
            <asp:BoundField HeaderText="Expense Code" DataField="ExpenseCode" />
            <asp:BoundField HeaderText="Reason" DataField="Reason" />
            <asp:BoundField HeaderText="Currency" DataField="Currency" />
            <asp:BoundField HeaderText="Qty" DataField="Qty" />
            <asp:BoundField HeaderText="Amount" DataField="Amount" />
        </Columns>
    </asp:GridView>

    <div class="row mt-4">
        <div class="col text-center">核准:<br />Approved by:<br /><asp:Label ID="lblApprovedBy" runat="server" /></div>
        <div class="col text-center">覆核:<br />Reviewed by:<br /><asp:Label ID="lblReviewedBy" runat="server" /></div>
        <div class="col text-center">製表:<br />Prepared by:<br /><asp:Label ID="lblPreparedBy" runat="server" /></div>
    </div>
</asp:Panel>


    </div>
</asp:Content>
