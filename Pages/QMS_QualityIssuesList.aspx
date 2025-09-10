<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="QMS_QualityIssuesList.aspx.cs"
    Inherits="IT_WorkPlant.Pages.QMS_QualityIssuesList"
    MasterPageFile="~/Pages/QMS.Master"
    Title="Issue List" %>

<asp:Content ID="Head1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* ปรับสไตล์ให้เข้าธีม QMS */
        .qms-panel{background:var(--card);border:1px solid var(--border);border-radius:var(--radius)}
        .qms-card {background:var(--card);border:1px solid var(--border);border-radius:var(--radius)}
        .table > :not(caption) > * > *{vertical-align:middle}
        /* ป้ายสถานะที่ code-behind ส่งมา */
        .badge-open{background:#fee2e2;color:#b91c1c;border-radius:999px;padding:.2rem .55rem;font-size:.8rem}
        .badge-progress{background:#111827;color:#fef3c7;border-radius:999px;padding:.2rem .55rem;font-size:.8rem}
        .badge-done{background:#e5fbe6;color:#166534;border-radius:999px;padding:.2rem .55rem;font-size:.8rem}
    </style>
</asp:Content>

<asp:Content ID="Body1" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="d-flex align-items-center justify-content-between mb-3">
        <h2 class="fw-bold m-0">Quality Issues</h2>
        <div class="d-flex gap-2">
            <a href='<%= ResolveUrl("~/Pages/QMS_QualityIssueAdd.aspx") %>' class="btn btn-dark">
                <i class="bi bi-plus-lg me-1"></i>Add New Issue
            </a>
        </div>
    </div>

   
    <div class="qms-panel p-3 mb-3">
        <div class="d-flex align-items-center gap-2 mb-2">
            <i class="bi bi-funnel"></i><span class="fw-semibold">Filters</span>
        </div>

        <div class="row g-3">
            <div class="col-12 col-md-3">
                <label class="form-label">Production Week</label>
                <asp:DropDownList ID="ddlFWeek" runat="server" CssClass="form-select" />
            </div>

            <div class="col-12 col-md-3">
                <label class="form-label">Date From</label>
                <div class="input-group">
                    <asp:TextBox ID="txtFDateFrom" runat="server" CssClass="form-control" placeholder="yyyy/mm/dd" />
                    <span class="input-group-text"><i class="bi bi-calendar2"></i></span>
                </div>
            </div>

            <div class="col-12 col-md-3">
                <label class="form-label">Date To</label>
                <div class="input-group">
                    <asp:TextBox ID="txtFDateTo" runat="server" CssClass="form-control" placeholder="yyyy/mm/dd" />
                    <span class="input-group-text"><i class="bi bi-calendar2"></i></span>
                </div>
            </div>

            <div class="col-12 col-md-3">
                <label class="form-label">Defect Type</label>
                <asp:DropDownList ID="ddlFDefect" runat="server" CssClass="form-select" />
            </div>

            <div class="col-12 col-md-3">
                <label class="form-label">Product</label>
                <asp:DropDownList ID="ddlFProduct" runat="server" CssClass="form-select" />
            </div>

            <div class="col-12 col-md-3">
                <label class="form-label">Production Unit</label>
                <asp:DropDownList ID="ddlFUnit" runat="server" CssClass="form-select" />
            </div>

            <div class="col-12 col-md-3">
                <label class="form-label">Responsible Unit</label>
                <asp:DropDownList ID="ddlFRespUnit" runat="server" CssClass="form-select" />
            </div>

            <div class="col-12 col-md-3">
                <label class="form-label">Status</label>
                <asp:DropDownList ID="ddlFStatus" runat="server" CssClass="form-select" />
            </div>

            <div class="col-12 d-flex gap-2 mt-2">
                <asp:Button ID="btnSearch" runat="server" CssClass="btn btn-dark" Text="Search" OnClick="btnSearch_Click" />
                <asp:Button ID="btnClear" runat="server" CssClass="btn btn-outline-secondary" Text="Clear" OnClick="btnClear_Click" />
            </div>
        </div>
    </div>

    
    <div class="qms-card p-0">
        <asp:GridView ID="gvIssues" runat="server" CssClass="table mb-0"
            AutoGenerateColumns="False" GridLines="None" AllowPaging="false">
            <Columns>
                
                <asp:HyperLinkField HeaderText="ID"
                    DataTextField="Id"
                    DataNavigateUrlFields="Id"
                    DataNavigateUrlFormatString="~/Pages/QMS_QualityIssueDetail.aspx?id={0}" />

                <asp:BoundField DataField="ProductionWeek" HeaderText="Week" />
                <asp:BoundField DataField="Date" HeaderText="Date" DataFormatString="{0:yyyy-MM-dd}" HtmlEncode="false" />

                <asp:TemplateField HeaderText="Unit/Line">
                    <ItemTemplate>
                        <%# Eval("ProductionUnit") + " / " + Eval("Line") %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField DataField="DefectType" HeaderText="Defect Type" />
                <asp:BoundField DataField="Product" HeaderText="Product" />
                <asp:BoundField DataField="Description" HeaderText="Description" />

                <asp:BoundField DataField="Downtime" HeaderText="Downtime (min)" />

                <asp:BoundField DataField="ResponsibleUnit" HeaderText="Responsible" />

                <asp:TemplateField HeaderText="Status">
                    <ItemTemplate>
                        <%# GetStatusBadge(Eval("Status") as string) %>
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Actions">
                    <ItemTemplate>
                        
                        <a class="btn btn-light btn-sm me-1" title="View"
                           href='<%# ResolveUrl("~/Pages/QMS_QualityIssueDetail.aspx?id=" + Server.UrlEncode(Eval("Id").ToString())) %>'>
                           <i class="bi bi-eye"></i><span class="visually-hidden">View</span>
                        </a>
                        
                        <a class="btn btn-light btn-sm me-1" title="Edit"
                           href='<%# ResolveUrl("~/Pages/QMS_QualityIssueDetail.aspx?id=" + Server.UrlEncode(Eval("Id").ToString()) + "&edit=1") %>'>
                           <i class="bi bi-pencil-square"></i><span class="visually-hidden">Edit</span>
                        </a>
                        
                        <a class="btn btn-light btn-sm" title="Open"
                           target="_blank"
                           href='<%# ResolveUrl("~/Pages/QMS_QualityIssueDetail.aspx?id=" + Server.UrlEncode(Eval("Id").ToString())) %>'>
                           <i class="bi bi-box-arrow-up-right"></i><span class="visually-hidden">Open</span>
                        </a>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>
