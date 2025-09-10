<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="QMS_QualityIssueSummary.aspx.cs"
    Inherits="IT_WorkPlant.Pages.QMS_QualityIssueSummary"
    MasterPageFile="~/Pages/QMS.Master"
    Title="Issue Summary" %>

<asp:Content ID="Head1" ContentPlaceHolderID="head" runat="server">
    <title>Quality Issue Summary</title>
    
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.css">
    <style>
        
        .qms-card{background:var(--card);border:1px solid var(--border);border-radius:var(--radius)}
        .qms-panel{background:var(--card);border:1px solid var(--border);border-radius:var(--radius)}
        .qms-stat .label{font-size:.9rem;color:var(--muted-foreground)}
        .qms-stat .value{font-size:2rem;font-weight:700;line-height:1}
        .stat-icon{width:32px;height:32px;border-radius:999px;display:inline-grid;place-items:center;background:var(--secondary)}
        .stat-icon.warn{background:rgba(255,196,0,.18)}
        .stat-icon.ok{background:rgba(25,135,84,.18)}
        .stat-icon.qty{background:rgba(134,93,255,.15)}

        
        .pill{border-radius:999px;padding:.25rem .6rem;font-size:.8rem}
        .qty-chip{background:var(--muted);color:var(--muted-foreground)}
        .tag-default{background:var(--secondary)}
        .tag-danger{background:#ef4444;color:#fff}
        .muted{color:var(--muted-foreground)}
        .table > :not(caption) > * > *{vertical-align:middle}
    </style>
</asp:Content>

<asp:Content ID="Body1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="d-flex align-items-center justify-content-between mb-3">
        <h2 class="fw-bold m-0">Quality Issue Summary</h2>
        <asp:Button ID="btnExport" runat="server" CssClass="btn btn-outline-secondary"
            Text="Export Summary" OnClick="btnExport_Click" />
    </div>

    <div class="row g-3 qms-stat mb-4">
        <div class="col-12 col-md-3">
            <div class="qms-card p-3 h-100 d-flex justify-content-between align-items-center">
                <div>
                    <div class="label">Total Issues</div>
                    <div class="value"><asp:Literal ID="litTotalIssues" runat="server" /></div>
                </div>
                <div class="stat-icon qty"><i class="bi bi-building"></i></div>
            </div>
        </div>
        <div class="col-12 col-md-3">
            <div class="qms-card p-3 h-100 d-flex justify-content-between align-items-center">
                <div>
                    <div class="label">Open Issues</div>
                    <div class="value"><asp:Literal ID="litOpenIssues" runat="server" /></div>
                </div>
                <div class="stat-icon warn"><i class="bi bi-exclamation-triangle"></i></div>
            </div>
        </div>
        <div class="col-12 col-md-3">
            <div class="qms-card p-3 h-100 d-flex justify-content-between align-items-center">
                <div>
                    <div class="label">Completed</div>
                    <div class="value"><asp:Literal ID="litCompleted" runat="server" /></div>
                </div>
                <div class="stat-icon ok"><i class="bi bi-record-circle"></i></div>
            </div>
        </div>
        <div class="col-12 col-md-3">
            <div class="qms-card p-3 h-100 d-flex justify-content-between align-items-center">
                <div>
                    <div class="label">Total Quantity</div>
                    <div class="value"><asp:Literal ID="litTotalQty" runat="server" /></div>
                </div>
                <div class="stat-icon qty"><i class="bi bi-clipboard-data"></i></div>
            </div>
        </div>
    </div>

    <div class="qms-panel p-3 mb-3">
        <div class="row g-3 align-items-end">
            <div class="col-12 col-md-2">
                <label class="form-label">Week</label>
                <asp:DropDownList ID="ddlWeek" runat="server" CssClass="form-select" />
            </div>
            <div class="col-12 col-md-2">
                <label class="form-label">Production Unit</label>
                <asp:DropDownList ID="ddlUnit" runat="server" CssClass="form-select" />
            </div>
            <div class="col-12 col-md-2">
                <label class="form-label">Defect Type</label>
                <asp:DropDownList ID="ddlDefectType" runat="server" CssClass="form-select" />
            </div>
            <div class="col-12 col-md-2">
                <label class="form-label">Product</label>
                <asp:DropDownList ID="ddlProduct" runat="server" CssClass="form-select" />
            </div>
            <div class="col-12 col-md-2">
                <label class="form-label">Department</label>
                <asp:DropDownList ID="ddlDept" runat="server" CssClass="form-select" />
            </div>
            <div class="col-12 col-md-2">
                <label class="form-label">Status</label>
                <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select" />
            </div>

            <div class="col-12 col-lg-6 mt-2">
                <div class="input-group">
                    <span class="input-group-text"><i class="bi bi-search"></i></span>
                    <asp:TextBox ID="txtSearchTop" runat="server" CssClass="form-control" placeholder="Search summary..." />
                </div>
            </div>
            <div class="col-12 col-lg-6 d-flex gap-2 mt-2">
                <asp:Button ID="btnSearch" runat="server" CssClass="btn btn-dark" Text="Search" OnClick="btnSearch_Click" />
                <asp:Button ID="btnClear" runat="server" CssClass="btn btn-outline-secondary"
                    Text="Clear Filters" OnClick="btnClear_Click" />
            </div>
        </div>
    </div>

    
    <div class="qms-panel p-3">
        <div class="d-flex justify-content-between align-items-center mb-2">
            <div class="fw-semibold">
                Issue Summary (<asp:Literal ID="litResultCount" runat="server" /> results)
            </div>
        </div>

        <asp:GridView ID="gvSummary" runat="server" CssClass="table"
            AutoGenerateColumns="False" GridLines="None" AllowPaging="false">
            <Columns>
                
                <asp:HyperLinkField HeaderText="ID"
                    DataTextField="Id"
                    DataNavigateUrlFields="Id"
                    DataNavigateUrlFormatString="~/Pages/QMS_QualityIssueDetail.aspx?id={0}" />

                <asp:BoundField DataField="ProductionWeek" HeaderText="Week" />

                <asp:TemplateField HeaderText="Unit/Line">
                    <ItemTemplate><%# Eval("Unit") + " / " + Eval("Line") %></ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField DataField="Product" HeaderText="Product" />
                <asp:BoundField DataField="DefectType" HeaderText="Defect Type" />
                <asp:BoundField DataField="Description" HeaderText="Description" />

                <asp:TemplateField HeaderText="Qty">
                    <ItemTemplate><%# QtyChip(Eval("Qty")) %></ItemTemplate>
                </asp:TemplateField>

                <asp:BoundField DataField="Department" HeaderText="Department" />
                <asp:BoundField DataField="Action" HeaderText="Action" />

                <asp:TemplateField HeaderText="Feedback Date">
                    <ItemTemplate><%# FeedbackDateHtml(Eval("FeedbackDate")) %></ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Status">
                    <ItemTemplate><%# GetStatusBadge(Eval("Status") as string) %></ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Remarks">
                    <ItemTemplate><%# RemarksHtml(Eval("Remarks")) %></ItemTemplate>
                </asp:TemplateField>

               
                <asp:TemplateField HeaderText="Actions">
                    <ItemTemplate>
                        <a class="btn btn-light btn-sm me-1" title="View" aria-label="View"
                           href='<%# ResolveUrl("~/Pages/QMS_QualityIssueDetail.aspx?id=" + Server.UrlEncode(Eval("Id").ToString())) %>'>
                           <i class="bi bi-eye"></i><span class="visually-hidden">View</span>
                        </a>
                        <a class="btn btn-light btn-sm me-1" title="Edit" aria-label="Edit"
                           href='<%# ResolveUrl("~/Pages/QMS_QualityIssueDetail.aspx?id=" + Server.UrlEncode(Eval("Id").ToString()) + "&edit=1") %>'>
                           <i class="bi bi-pencil-square"></i><span class="visually-hidden">Edit</span>
                        </a>
                        <a class="btn btn-light btn-sm" title="Open in new tab" aria-label="Open in new tab"
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
