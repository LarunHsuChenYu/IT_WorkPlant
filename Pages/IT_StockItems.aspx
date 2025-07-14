<%@ Page Title="Stock Dashboard" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="IT_StockItems.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_StockItems" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-4">
        <!-- Top Right Username & Avatar -->
<div class="d-flex justify-content-end align-items-center mb-3">
    <div class="d-flex align-items-center shadow-sm px-3 py-2 rounded" style="background-color: #f8f9fa;">
        <span style="font-size: 1.6rem; margin-right: 10px;">🧑‍💻</span>
        <span class="fw-bold text-dark">
            <asp:Label ID="lblUsername" runat="server" Text="Guest"></asp:Label>
        </span>
    </div>
</div>
        <!-- Dashboard Cards -->
<div class="row g-3 mb-4">
    
    <div class="col-md-3">
        <asp:LinkButton ID="btnShowNeeds" runat="server" OnClick="btnShowNeeds_Click" CssClass="text-decoration-none">
            <div class="card text-center shadow-sm border-0 w-100" style="background-color: #fff; padding: 0.75rem;">
                <div class="card-body">
                    <h6 class="text-muted mb-1">จำนวนสินค้าที่ต้องซื้อเพิ่ม</h6>
                    <h3 class="fw-bold text-danger">
                        <asp:Label ID="lblNeedsCount" runat="server" Text="0"></asp:Label>
                    </h3>
                    <p class="mb-0">🛒 ต้องซื้อ</p>
                </div>
            </div>
        </asp:LinkButton>
    </div>

    
    <div class="col-md-3">
        <asp:LinkButton ID="btnShowAll" runat="server" OnClick="btnShowAll_Click" CssClass="text-decoration-none">
            <div class="card text-center shadow-sm border-0 w-100" style="background-color: #fff; padding: 0.75rem;">
                <div class="card-body">
                    <h6 class="text-muted mb-1">จำนวนสินค้าทั้งหมด</h6>
                    <h3 class="fw-bold text-primary">
                        <asp:Label ID="lblTotalItems" runat="server" Text="0"></asp:Label>
                    </h3>
                    <p class="mb-0">📦 รายการ</p>
                </div>
            </div>
        </asp:LinkButton>
    </div>

    
    <div class="col-md-3">
        <asp:HyperLink ID="HyperLink1" runat="server" NavigateUrl="~/Pages/IT_StockReceive.aspx" CssClass="text-decoration-none">
            <div class="card text-center shadow-lg border border-danger rounded-4 w-100"
                 style="background: linear-gradient(135deg, #fff0f0, #ffe6e6); padding: 1rem;">
                <div class="card-body">
                    <div style="font-size: 2.2rem;">📦➕</div>
                    <h6 class="fw-bold text-danger mt-2 mb-1">รับของเข้า</h6>
                    <p class="text-muted mb-0" style="font-size: 0.9rem;">เพิ่มสินค้าลงคลัง</p>
                </div>
            </div>
        </asp:HyperLink>
    </div>

    <!-- กล่อง: เบิกของออก -->
    <div class="col-md-3">
        <asp:HyperLink ID="btnGoToIssue" runat="server" NavigateUrl="~/Pages/IT_StockIssue.aspx" CssClass="text-decoration-none">
            <div class="card text-center shadow-lg border border-warning rounded-4 w-100"
                 style="background: linear-gradient(135deg, #fffbe6, #fff1cc); padding: 1rem;">
                <div class="card-body">
                    <div style="font-size: 2.2rem;">📤➖</div>
                    <h6 class="fw-bold text-warning mt-2 mb-1">เบิกของออก</h6>
                    <p class="text-muted mb-0" style="font-size: 0.9rem;">จ่ายออกจากคลัง</p>
                </div>
            </div>
        </asp:HyperLink>
    </div>
</div>

 <!-- Table Title -->
        <div class="d-flex justify-content-between align-items-center mb-2">
            <h5 class="text-success fw-bold">รายการสินค้าในคลัง – WH</h5>
        </div>
        <!-- Search Box -->
<div class="mb-3 d-flex justify-content-end">
    <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control me-2" placeholder="ค้นหารายการสินค้า..." Width="250px" />
    <asp:Button ID="btnSearch" runat="server" Text="🔍 ค้นหา" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
</div>

        <!-- Stock Table -->
<asp:GridView ID="gvStockItems" runat="server"
    CssClass="table table-bordered"
    AutoGenerateColumns="False"
    GridLines="None"
    ShowHeader="true"
    AllowPaging="true"
    PageSize="10"
    DataKeyNames="ItemID"
    OnPageIndexChanging="gvStockItems_PageIndexChanging"
    OnRowDataBound="gvStockItems_RowDataBound"
    OnRowEditing="gvStockItems_RowEditing"
    OnRowCancelingEdit="gvStockItems_RowCancelingEdit"
    OnRowUpdating="gvStockItems_RowUpdating">
    
    <HeaderStyle CssClass="table-light" />
    <Columns>

         <asp:TemplateField HeaderText="ลำดับ">
        <ItemTemplate>
            <asp:Label ID="lblRowNumber" runat="server" />
        </ItemTemplate>
    </asp:TemplateField>

        <asp:BoundField DataField="ProductName" HeaderText="รายการ" ReadOnly="true" />
        <asp:BoundField DataField="Model" HeaderText="รุ่น" ReadOnly="true" />
        <asp:BoundField DataField="Unit" HeaderText="หน่วยนับ" ReadOnly="true" />

       
        <asp:TemplateField HeaderText="ขั้นต่ำ">
            <ItemTemplate><%# Eval("MinimumQty") %></ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtMinimumQty" runat="server" Text='<%# Bind("MinimumQty") %>' CssClass="form-control" />
            </EditItemTemplate>
        </asp:TemplateField>

        
        <asp:TemplateField HeaderText="คงเหลือในคลัง">
            <ItemTemplate><%# Eval("InventoryQty") %></ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtInventoryQty" runat="server" Text='<%# Bind("InventoryQty") %>' CssClass="form-control" />
            </EditItemTemplate>
        </asp:TemplateField>

       
       <asp:TemplateField HeaderText="ต้องเติมเพิ่ม">
    <ItemTemplate>
        <asp:Label ID="lblReplenishQty" runat="server" Text='<%# Eval("ReplenishQty") %>' />
    </ItemTemplate>
</asp:TemplateField>


        
        <asp:TemplateField HeaderText="ราคาต่อหน่วย">
            <ItemTemplate><%# Eval("UnitCost", "{0:N2}") %></ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtUnitCost" runat="server" Text='<%# Bind("UnitCost") %>' CssClass="form-control" />
            </EditItemTemplate>
        </asp:TemplateField>

        <asp:BoundField DataField="InventoryValue" HeaderText="มูลค่ารวม" DataFormatString="{0:N2}" ReadOnly="true" />
        <asp:BoundField DataField="CreateDate" HeaderText="วันที่เพิ่ม" DataFormatString="{0:yyyy-MM-dd HH:mm}" ReadOnly="true" />
        <asp:BoundField DataField="ReceivedBy" HeaderText="ผู้รับเข้า" ReadOnly="true" />

        
        <asp:CommandField ShowEditButton="true" EditText="✏️ แก้ไข" UpdateText="💾 บันทึก" CancelText="❌ ยกเลิก" />
    </Columns>
</asp:GridView>

</div>
</asp:Content>
