<%@ Page Title="Stock Dashboard" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="IT_StockItems.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_StockItems" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-4">
        
<div class="d-flex justify-content-end align-items-center mb-3">
    <div class="d-flex align-items-center shadow-sm px-3 py-2 rounded" style="background-color: #f8f9fa;">
        <span style="font-size: 1.6rem; margin-right: 10px;">🧑‍💻</span>
        <span class="fw-bold text-dark">
            <asp:Label ID="lblUsername" runat="server" Text="Guest"></asp:Label>
        </span>
    </div>
</div>
        
<div class="row g-3 mb-4">
    
    <div class="col-md-3">
        <asp:LinkButton ID="btnShowNeeds" runat="server" OnClick="btnShowNeeds_Click" CssClass="text-decoration-none">
            <div class="card text-center shadow-sm border-0 w-100" style="background-color: #fff; padding: 0.75rem;">
                <div class="card-body">
                    <h6 class="text-muted mb-1"><%= GetLabel("needsreplenish") %></h6>
                    <h3 class="fw-bold text-danger">
                        <asp:Label ID="lblNeedsCount" runat="server" Text="0"></asp:Label>
                    </h3>
                    <p class="mb-0">🛒 <%= GetLabel("showneeds") %></p>
                </div>
            </div>
        </asp:LinkButton>
    </div>

    
    <div class="col-md-3">
        <asp:LinkButton ID="btnShowAll" runat="server" OnClick="btnShowAll_Click" CssClass="text-decoration-none">
            <div class="card text-center shadow-sm border-0 w-100" style="background-color: #fff; padding: 0.75rem;">
                <div class="card-body">
                    <h6 class="text-muted mb-1"><%= GetLabel("totalitems") %></h6>
                    <h3 class="fw-bold text-primary">
                        <asp:Label ID="lblTotalItems" runat="server" Text="0"></asp:Label>
                    </h3>
                    <p class="mb-0">📦 <%= GetLabel("showall") %></p>
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
                    <h6 class="fw-bold text-danger mt-2 mb-1"><%= GetLabel("receive") %></h6>
                    <p class="text-muted mb-0" style="font-size: 0.9rem;"><%= GetLabel("receivenote") %></p>
                </div>
            </div>
        </asp:HyperLink>
    </div>

    
    <div class="col-md-3">
        <asp:HyperLink ID="btnGoToIssue" runat="server" NavigateUrl="~/Pages/IT_StockIssue.aspx" CssClass="text-decoration-none">
            <div class="card text-center shadow-lg border border-warning rounded-4 w-100"
                 style="background: linear-gradient(135deg, #fffbe6, #fff1cc); padding: 1rem;">
                <div class="card-body">
                    <div style="font-size: 2.2rem;">📤➖</div>
                    <h6 class="fw-bold text-warning mt-2 mb-1"><%= GetLabel("issue") %></h6>
                    <p class="text-muted mb-0" style="font-size: 0.9rem;"><%= GetLabel("issuenote") %></p>

                </div>
            </div>
        </asp:HyperLink>
    </div>
</div>


<div class="d-flex justify-content-between align-items-center mb-2">
    <h5 class="text-success fw-bold"><%= GetLabel("title") %> – WH</h5>
</div>


<div class="mb-3 d-flex justify-content-end">
    <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control me-2" Width="250px" />
    <asp:Button ID="btnSearch" runat="server" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
</div>



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
    OnRowUpdating="gvStockItems_RowUpdating"
    OnRowCreated="gvStockItems_RowCreated">


    <HeaderStyle CssClass="table-light" />
    <Columns>
        <asp:TemplateField HeaderText="no">
            <ItemTemplate>
                <asp:Label ID="lblRowNumber" runat="server" />
            </ItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="productname">
            <ItemTemplate><%# Eval("ProductName") %></ItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="model">
            <ItemTemplate><%# Eval("Model") %></ItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="unit">
            <ItemTemplate><%# Eval("Unit") %></ItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="minimumqty">
            <ItemTemplate><%# Eval("MinimumQty") %></ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtMinimumQty" runat="server" Text='<%# Bind("MinimumQty") %>' CssClass="form-control" />
            </EditItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="inventoryqty">
            <ItemTemplate><%# Eval("InventoryQty") %></ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtInventoryQty" runat="server" Text='<%# Bind("InventoryQty") %>' CssClass="form-control" />
            </EditItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="replenishqty">
            <ItemTemplate>
                <asp:Label ID="lblReplenishQty" runat="server" Text='<%# Eval("ReplenishQty") %>' />
            </ItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="unitcost">
            <ItemTemplate><%# Eval("UnitCost", "{0:N2}") %></ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtUnitCost" runat="server" Text='<%# Bind("UnitCost") %>' CssClass="form-control" />
            </EditItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="inventoryvalue">
            <ItemTemplate><%# Eval("InventoryValue", "{0:N2}") %></ItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="createdate">
            <ItemTemplate><%# Eval("CreateDate", "{0:yyyy-MM-dd HH:mm}") %></ItemTemplate>
        </asp:TemplateField>

        <asp:TemplateField HeaderText="receivedby">
            <ItemTemplate><%# Eval("ReceivedBy") %></ItemTemplate>
        </asp:TemplateField>

       <asp:TemplateField HeaderText="actions">
    <ItemTemplate>
        <asp:LinkButton ID="btnEdit" runat="server" CommandName="Edit" CssClass="btn btn-sm btn-warning">
            ✏️ <%# GetLabel("edit") %>
        </asp:LinkButton>
    </ItemTemplate>
    <EditItemTemplate>
        <asp:LinkButton ID="btnUpdate" runat="server" CommandName="Update" CssClass="btn btn-sm btn-success">
            💾 <%# GetLabel("save") %>
        </asp:LinkButton>
        <asp:LinkButton ID="btnCancel" runat="server" CommandName="Cancel" CssClass="btn btn-sm btn-secondary">
            ❌ <%# GetLabel("cancel") %>
        </asp:LinkButton>
    </EditItemTemplate>
</asp:TemplateField>

    </Columns>
</asp:GridView>

</div>
</asp:Content>
