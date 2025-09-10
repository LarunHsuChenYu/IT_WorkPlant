<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="QMS_QualityIssueDetail.aspx.cs"
    Inherits="IT_WorkPlant.Pages.QMS_QualityIssueDetail"
    MasterPageFile="~/Pages/QMS.Master"
    Title="Quality Issue Details" %>

<asp:Content ID="Head1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* เสริมเล็กน้อยให้เหมือนต้นแบบ */
        .qms-card,.qms-panel{background:var(--card);border:1px solid var(--border);border-radius:var(--radius)}
        .section-title{font-weight:700;font-size:1rem}
        .kv {display:grid;grid-template-columns:180px 1fr;gap:.25rem .75rem}
        .kv > div:nth-child(odd){color:var(--muted-foreground)}
        .pill{border-radius:999px;padding:.25rem .6rem;font-size:.8rem;white-space:nowrap}
        .badge-open{background:#fee2e2;color:#991b1b}
        .badge-progress{background:#fde68a;color:#92400e}
        .badge-done{background:#dcfce7;color:#065f46}
        .alert-icon{margin-right:.5rem}
        .btn-icon{display:inline-flex;align-items:center;gap:.5rem}
        .card-pad{padding:1rem 1.25rem}
    </style>
</asp:Content>

<asp:Content ID="Body1" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Header -->
    <div class="d-flex justify-content-between align-items-center mb-3">
        <div class="d-flex align-items-center gap-2">
            <a href='<%# ResolveUrl("~/Pages/QMS_QualityIssuesList.aspx") %>' class="btn btn-light btn-sm" runat="server">
                <i class="bi bi-arrow-left"></i> Back to List
            </a>
            <h2 class="fw-bold m-0">
                <asp:Literal ID="litIssueId" runat="server" />
            </h2>
            <div class="ms-2">
                <asp:Literal ID="litStatusBadgeTop" runat="server" />
            </div>
        </div>

        <div class="d-flex align-items-center gap-2">
            <asp:Button ID="btnEdit" runat="server" CssClass="btn btn-dark btn-icon"
                Text="Edit Issue" OnClick="btnEdit_Click" />
            <asp:Button ID="btnSave" runat="server" CssClass="btn btn-success btn-icon"
                Text="Save Changes" OnClick="btnSave_Click" />
            <asp:Button ID="btnCancel" runat="server" CssClass="btn btn-outline-secondary"
                Text="Cancel" OnClick="btnCancel_Click" />
        </div>
    </div>

    <!-- Alerts -->
    <asp:Panel ID="pnlAlert" runat="server" Visible="false" CssClass="alert alert-danger d-flex align-items-start">
        <i class="bi bi-exclamation-triangle alert-icon"></i>
        <div><strong>Error.</strong> <asp:Literal ID="litError" runat="server" /></div>
    </asp:Panel>
    <asp:Panel ID="pnlSuccess" runat="server" Visible="false" CssClass="alert alert-success d-flex align-items-start">
        <i class="bi bi-check2-circle alert-icon"></i>
        <div>Saved successfully.</div>
    </asp:Panel>

    <!-- ===== VIEW MODE ===== -->
    <asp:Panel ID="pnlView" runat="server">
        <div class="row g-3">
            <!-- LEFT -->
            <div class="col-lg-8">
                <!-- Issue Information -->
                <div class="qms-panel card-pad mb-3">
                    <div class="fw-semibold mb-2">Issue Information</div>
                    <div class="kv">
                        <div>Production Week</div><div><asp:Literal ID="vWeek" runat="server" /></div>
                        <div>Date</div><div><asp:Literal ID="vDate" runat="server" /></div>
                        <div>Production Unit</div><div><asp:Literal ID="vUnit" runat="server" /> / <asp:Literal ID="vLine" runat="server" /></div>
                        <div>Product</div><div><asp:Literal ID="vProduct" runat="server" /></div>
                        <div>Defect Type</div><div><asp:Literal ID="vDefectType" runat="server" /></div>
                        <div>Downtime</div><div><asp:Literal ID="vDowntime" runat="server" /> minutes</div>
                    </div>
                </div>

                <div class="qms-panel card-pad mb-3">
                    <div class="fw-semibold mb-2">Issue Description</div>
                    <div class="form-control-plaintext"><asp:Literal ID="vDescription" runat="server" /></div>
                </div>

                <div class="qms-panel card-pad mb-3">
                    <div class="fw-semibold mb-2">Short-term Solution</div>
                    <div class="form-control-plaintext"><asp:Literal ID="vShort" runat="server" /></div>
                </div>

                <div class="qms-panel card-pad mb-3">
                    <div class="fw-semibold mb-2">Corrective Action</div>
                    <div class="form-control-plaintext"><asp:Literal ID="vCorrective" runat="server" /></div>
                </div>

                <div class="qms-panel card-pad mb-3">
                    <div class="fw-semibold mb-2">Remarks</div>
                    <div class="form-control-plaintext"><asp:Literal ID="vRemarks" runat="server" /></div>
                </div>
            </div>

            <!-- RIGHT -->
            <div class="col-lg-4">
                <div class="qms-panel card-pad mb-3">
                    <div class="fw-semibold mb-2">Responsibility</div>
                    <div class="kv">
                        <div>Responsible Department</div><div><asp:Literal ID="vDept" runat="server" /></div>
                        <div>Responsible Person</div><div><asp:Literal ID="vPerson" runat="server" /></div>
                        <div>Status</div><div><asp:Literal ID="vStatus" runat="server" /></div>
                        <div>Completion Date</div><div><asp:Literal ID="vCompletedDate" runat="server" /></div>
                    </div>
                </div>

                <div class="qms-panel card-pad">
                    <div class="fw-semibold mb-2">Audit Trail</div>
                    <div class="mb-3">
                        <div class="text-muted small">Created By</div>
                        <div><asp:Literal ID="litCreatedBy" runat="server" /></div>
                    </div>
                    <div>
                        <div class="text-muted small">Last Modified By</div>
                        <div><asp:Literal ID="litModifiedBy" runat="server" /></div>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>

    <!-- ===== EDIT MODE ===== -->
    <asp:Panel ID="pnlEdit" runat="server">
        <div class="row g-3">
            <!-- LEFT -->
            <div class="col-lg-8">
                <div class="qms-panel card-pad mb-3">
                    <div class="fw-semibold mb-3">Issue Information</div>

                    <div class="row g-3">
                        <div class="col-6 col-md-4">
                            <label class="form-label">Production Week *</label>
                            <asp:TextBox ID="txtWeek" runat="server" CssClass="form-control" />
                        </div>
                        <div class="col-6 col-md-4">
                            <label class="form-label">Date *</label>
                            <asp:TextBox ID="txtDate" runat="server" CssClass="form-control" TextMode="Date" />
                        </div>
                        <div class="col-6 col-md-4">
                            <label class="form-label">Downtime (minutes)</label>
                            <asp:TextBox ID="txtDowntime" runat="server" CssClass="form-control" />
                        </div>

                        <div class="col-6 col-md-4">
                            <label class="form-label">Production Unit</label>
                            <asp:DropDownList ID="ddlUnit" runat="server" CssClass="form-select" />
                        </div>
                        <div class="col-6 col-md-4">
                            <label class="form-label">Production Line</label>
                            <asp:DropDownList ID="ddlLine" runat="server" CssClass="form-select" />
                        </div>
                        <div class="col-6 col-md-4">
                            <label class="form-label">Product</label>
                            <asp:DropDownList ID="ddlProduct" runat="server" CssClass="form-select" />
                        </div>

                        <div class="col-6 col-md-4">
                            <label class="form-label">Defect Type</label>
                            <asp:DropDownList ID="ddlDefectType" runat="server" CssClass="form-select" />
                        </div>
                    </div>
                </div>

                <div class="qms-panel card-pad mb-3">
                    <label class="form-label fw-semibold">Issue Description *</label>
                    <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                </div>

                <div class="qms-panel card-pad mb-3">
                    <label class="form-label fw-semibold">Short-term Solution</label>
                    <asp:TextBox ID="txtShort" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                </div>

                <div class="qms-panel card-pad mb-3">
                    <label class="form-label fw-semibold">Corrective Action</label>
                    <asp:TextBox ID="txtCorrective" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                </div>

                <div class="qms-panel card-pad mb-3">
                    <label class="form-label fw-semibold">Remarks</label>
                    <asp:TextBox ID="txtRemarks" runat="server" CssClass="form-control" TextMode="MultiLine" Rows="3" />
                </div>
            </div>

            <!-- RIGHT -->
            <div class="col-lg-4">
                <div class="qms-panel card-pad mb-3">
                    <div class="fw-semibold mb-3">Responsibility</div>
                    <div class="mb-3">
                        <label class="form-label">Responsible Department</label>
                        <asp:DropDownList ID="ddlDept" runat="server" CssClass="form-select" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Responsible Person *</label>
                        <asp:TextBox ID="txtPerson" runat="server" CssClass="form-control" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Status</label>
                        <asp:DropDownList ID="ddlStatus" runat="server" CssClass="form-select" />
                    </div>
                    <div class="mb-0">
                        <label class="form-label">Completion Date</label>
                        <asp:TextBox ID="txtCompletedDate" runat="server" CssClass="form-control" TextMode="Date" />
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>

</asp:Content>
