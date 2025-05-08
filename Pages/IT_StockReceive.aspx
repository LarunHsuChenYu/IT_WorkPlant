<%@ Page Title="รับของเข้าคลัง" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="IT_StockReceive.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_StockReceive" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-4">
        <div class="card p-4 shadow-sm border border-danger mb-4">
            <h4 class="fw-bold text-danger mb-4">📦➕ ฟอร์มรับของเข้าคลัง</h4>

            <div class="row g-3">
                <div class="col-md-4">
                    <label class="form-label">เลือกรูปแบบ</label>
                    <asp:DropDownList ID="ddlMode" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlMode_SelectedIndexChanged">
                        <asp:ListItem Text="เลือกรูปแบบ" Value="" />
                        <asp:ListItem Text="รับสินค้าเดิม" Value="existing" />
                        <asp:ListItem Text="เพิ่มรายการใหม่" Value="new" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-4">
                    <label class="form-label">วันที่รับเข้า</label>
                    <asp:TextBox ID="txtReceiveDate" runat="server" CssClass="form-control" TextMode="Date" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">ประเภทการรับเข้า</label>
                    <asp:DropDownList ID="ddlReceiveType" runat="server" CssClass="form-select">
                        <asp:ListItem Text="เลือกประเภท" Value="" />
                        <asp:ListItem Text="สั่งซื้อ" Value="สั่งซื้อ" />
                        <asp:ListItem Text="คืนของ" Value="คืนของ" />
                    </asp:DropDownList>
                </div>
            </div>

            <!-- 🔁 สินค้าเดิม -->
            <asp:Panel ID="pnlSelectExisting" runat="server" CssClass="row g-3 mt-3" Visible="false">
                <div class="col-md-4">
                    <label class="form-label">รหัสสินค้า</label>
                    <asp:DropDownList ID="ddlProductCode" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlProductCode_SelectedIndexChanged" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">ชื่อสินค้า</label>
                    <asp:TextBox ID="txtProductName" runat="server" CssClass="form-control" ReadOnly="true" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">รุ่นสินค้า</label>
                    <asp:TextBox ID="txtModel" runat="server" CssClass="form-control" ReadOnly="true" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">หน่วย</label>
                    <asp:TextBox ID="txtUnit" runat="server" CssClass="form-control" ReadOnly="true" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">จำนวน</label>
                    <asp:TextBox ID="txtQuantity" runat="server" CssClass="form-control" TextMode="Number" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">ราคาต่อชิ้น</label>
                    <asp:TextBox ID="txtUnitCost" runat="server" CssClass="form-control" TextMode="Number" />
                </div>
                <div class="col-md-6">
                    <label class="form-label">แหล่งที่มา</label>
                    <asp:TextBox ID="txtSource" runat="server" CssClass="form-control" />
                </div>
                <div class="col-12">
                    <label class="form-label">หมายเหตุ</label>
                    <asp:TextBox ID="txtRemarks" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" />
                </div>
            </asp:Panel>

            <!-- 🆕 เพิ่มรายการใหม่ -->
            <asp:Panel ID="pnlNewProduct" runat="server" CssClass="row g-3 mt-3" Visible="false">
                <div class="col-md-4">
                    <label class="form-label">ชื่อสินค้าใหม่</label>
                    <asp:TextBox ID="txtNewProductName" runat="server" CssClass="form-control" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">รุ่นสินค้าใหม่</label>
                    <asp:TextBox ID="txtNewModel" runat="server" CssClass="form-control" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">หน่วย</label>
                    <asp:TextBox ID="txtNewUnit" runat="server" CssClass="form-control" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">จำนวน</label>
                    <asp:TextBox ID="txtNewQuantity" runat="server" CssClass="form-control" TextMode="Number" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">ราคาต่อชิ้น</label>
                    <asp:TextBox ID="txtNewUnitCost" runat="server" CssClass="form-control" TextMode="Number" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">จำนวนขั้นต่ำที่ควรมี</label>
                    <asp:TextBox ID="txtMinimumQty" runat="server" CssClass="form-control" TextMode="Number" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">แหล่งที่มา</label>
                    <asp:TextBox ID="txtNewSource" runat="server" CssClass="form-control" />
                </div>
                <div class="col-md-4">
                    <label class="form-label">เลขอ้างอิง</label>
                    <asp:TextBox ID="txtReference" runat="server" CssClass="form-control" />
                </div>
                <div class="col-12">
                    <label class="form-label">หมายเหตุ</label>
                    <asp:TextBox ID="txtNewRemarks" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="2" />
                </div>
            </asp:Panel>

            <!-- ✅ ปุ่มบันทึก -->
            <div class="row mt-3">
                <div class="col-12 d-flex justify-content-center">
                    <asp:Button ID="btnSubmit" runat="server" Text="✅ บันทึกรับของเข้า" CssClass="btn btn-success px-4" OnClick="btnSubmit_Click" />
                </div>
            </div>
        </div>

        <!-- 📜 ปุ่มดูประวัติ -->
        <div class="text-end mb-3">
            <asp:Button ID="btnToggleHistory" runat="server" Text="📜 คลิกดูประวัติการรับของ" CssClass="btn btn-outline-secondary" OnClientClick="toggleHistory(); return false;" />
        </div>

        <!-- 🧾 ตารางประวัติ -->
      <asp:Panel ID="pnlHistory" runat="server" Style="display: none;">
    <div class="card shadow-sm border border-success p-3">
        <h5 class="fw-bold text-success mb-3">📋 ประวัติการรับของล่าสุด</h5>
        <asp:GridView ID="gvHistory" runat="server" CssClass="table table-bordered table-striped" AutoGenerateColumns="False">
            <Columns>
                <asp:BoundField DataField="ReceiveDate" HeaderText="วันที่" DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="ProductName" HeaderText="สินค้า" />
                <asp:BoundField DataField="Model" HeaderText="รุ่น" />
                <asp:BoundField DataField="Quantity" HeaderText="จำนวน" />
                <asp:BoundField DataField="Unit" HeaderText="หน่วย" />
                <asp:BoundField DataField="UnitCost" HeaderText="ราคาต่อหน่วย" DataFormatString="{0:N2}" />
                <asp:BoundField DataField="TotalPrice" HeaderText="ราคารวม" DataFormatString="{0:N2}" />
                <asp:BoundField DataField="Source" HeaderText="แหล่งที่มา" />
                <asp:BoundField DataField="CreatedBy" HeaderText="ผู้รับเข้า" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Panel>

    <script type="text/javascript">
        function toggleHistory() {
            var panel = document.getElementById('<%= pnlHistory.ClientID %>');
            panel.style.display = (panel.style.display === 'none') ? 'block' : 'none';
        }
    </script>
        </div>
</asp:Content>



