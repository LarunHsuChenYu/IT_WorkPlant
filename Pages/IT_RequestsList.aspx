<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" 
    CodeBehind="IT_RequestsList.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_RequestsList" %>
<%@ Import Namespace="System.Web" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>IT Requests List</title>

    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.3/font/bootstrap-icons.css" />

    <style>
        .filter-container{display:flex;flex-wrap:wrap;gap:10px;margin-bottom:20px}
        .filter-container select{padding:6px 8px;font-size:14px;width:200px;border:1px solid #dee2e6;border-radius:8px}
        .myGridView{width:100%;border-collapse:collapse;margin-top:10px}
        .myGridView th{background:#2e8b57;color:#fff;font-weight:600;text-align:center}
        .myGridView tr:nth-child(odd){background:#f5fff8}
        .myGridView tr:nth-child(even){background:#ecfff1}
        .myGridView tr:hover{background:#dff7e6}
        .myGridView th,.myGridView td{border:1px solid #d9e2de;padding:8px 10px;text-align:center;vertical-align:middle}

        .btn-icon{display:inline-flex;align-items:center;justify-content:center;width:36px;height:36px;padding:0;border-radius:999px}

        .dash-row{display:grid;grid-template-columns:repeat(7,minmax(120px,1fr));gap:12px;margin-bottom:16px}
        @media (max-width:1400px){.dash-row{grid-template-columns:repeat(4,1fr)}}
        @media (max-width:768px){.dash-row{grid-template-columns:repeat(2,1fr)}}
        .dash-card{border-radius:14px;border:1px solid #e9ecef;background:#fff;padding:12px 14px;box-shadow:0 6px 18px rgba(0,0,0,.06)}
        .dash-title{font-size:12px;font-weight:700;letter-spacing:.3px;text-transform:uppercase;color:#6b7280;margin-bottom:6px;display:flex;justify-content:space-between}
        .dash-value{font-size:26px;font-weight:800;line-height:1}
        .dash-pill{font-size:11px;padding:2px 8px;border-radius:999px;border:1px solid rgba(255,255,255,.45)}
        .bg-primary-soft{background:linear-gradient(180deg,#e8f1ff,#ffffff)}
        .bg-success-soft{background:linear-gradient(180deg,#e8f8ef,#ffffff)}
        .bg-warning-soft{background:linear-gradient(180deg,#fff7e6,#ffffff)}
        .bg-secondary-soft{background:linear-gradient(180deg,#f1f5f9,#ffffff)}
        .text-primary-700{color:#1d4ed8}
        .text-success-700{color:#0f766e}
        .text-warning-700{color:#b45309}
        .text-secondary-700{color:#334155}

        /* ===== DETAIL ===== */
        #detail-root{max-width:1140px;margin:0 auto}
        #detail-root .card{background:#fff;border:1px solid #e9ecef;border-radius:16px;padding:18px 18px 22px}
        #detail-root .label{font-size:12px;color:#6c757d;margin-bottom:6px}

        #detail-root select,
        #detail-root input[type="text"],
        #detail-root .field {
            width:100% !important;height:44px !important;border:1px solid #dee2e6 !important;
            border-radius:10px !important;padding:8px 10px !important;background:#fff !important;
            box-sizing:border-box !important;line-height:24px !important;
        }
        #detail-root .read{
            background:#f8f9fa !important;display:flex;align-items:center;
            width:100% !important;height:44px !important;border:1px solid #dee2e6 !important;border-radius:10px !important;
            padding:8px 10px !important;box-sizing:border-box !important;
        }
        #detail-root textarea,#detail-root .ta{
            width:100% !important;border:1px solid #dee2e6 !important;border-radius:10px !important;
            padding:10px !important;box-sizing:border-box !important;resize:vertical;
        }
        #detail-wrap .section-line{display:none;}

        /* กล่องภาพ */
        #detail-root .img-box{
            width:100%;border:1px dashed #cbd5e1;border-radius:14px;display:flex;align-items:center;justify-content:center;
            background:#f8fafc;min-height:360px;padding:10px;
        }
        .req-img-thumb img{cursor:zoom-in}
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="mb-2">IT Requests List</h2>

    <asp:Panel ID="pnlList" runat="server">
        <div class="dash-row">
            <div class="dash-card bg-primary-soft">
                <div class="dash-title"><span>Total</span><span class="dash-pill text-primary-700">All</span></div>
                <div class="dash-value text-primary-700"><asp:Label ID="lblTotal" runat="server" Text="0" /></div>
            </div>
            <div class="dash-card bg-success-soft">
                <div class="dash-title"><span>Finished</span><span class="dash-pill text-success-700">Done</span></div>
                <div class="dash-value text-success-700"><asp:Label ID="lblFinished" runat="server" Text="0" /></div>
            </div>
            <div class="dash-card bg-warning-soft">
                <div class="dash-title"><span>WIP</span><span class="dash-pill text-warning-700">In progress</span></div>
                <div class="dash-value text-warning-700"><asp:Label ID="lblWIP" runat="server" Text="0" /></div>
            </div>
            <div class="dash-card bg-secondary-soft">
                <div class="dash-title"><span>Unassigned</span><span class="dash-pill text-secondary-700">No DRI</span></div>
                <div class="dash-value text-secondary-700"><asp:Label ID="lblUnassigned" runat="server" Text="0" /></div>
            </div>
            <div class="dash-card bg-secondary-soft">
                <div class="dash-title"><span>Assigned</span><span class="dash-pill text-secondary-700">Has DRI</span></div>
                <div class="dash-value text-secondary-700"><asp:Label ID="lblAssigned" runat="server" Text="0" /></div>
            </div>
            <div class="dash-card bg-primary-soft">
                <div class="dash-title"><span>Today</span><span class="dash-pill text-primary-700"><%: DateTime.Today.ToString("dd MMM") %></span></div>
                <div class="dash-value text-primary-700"><asp:Label ID="lblToday" runat="server" Text="0" /></div>
            </div>
            <div class="dash-card bg-primary-soft">
                <div class="dash-title"><span>This Month</span><span class="dash-pill text-primary-700"><%: DateTime.Today.ToString("MMM yyyy") %></span></div>
                <div class="dash-value text-primary-700"><asp:Label ID="lblThisMonth" runat="server" Text="0" /></div>
            </div>
        </div>

        <div class="filter-container">
            <asp:DropDownList ID="ddlIssueMonth" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged" />
            <asp:DropDownList ID="ddlIssueDate" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
                <asp:ListItem Value="" Text="All Dates" />
            </asp:DropDownList>
            <asp:DropDownList ID="ddlDeptName" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
                <asp:ListItem Value="" Text="All Departments" />
            </asp:DropDownList>
            <asp:DropDownList ID="ddlRequestUser" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
                <asp:ListItem Value="" Text="All Request Users" />
            </asp:DropDownList>
            <asp:DropDownList ID="ddlIssueType" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
                <asp:ListItem Value="" Text="All Issue Types" />
            </asp:DropDownList>
            <asp:DropDownList ID="ddlStatus" runat="server" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
                <asp:ListItem Value="" Text="All Statuses" />
            </asp:DropDownList>
        </div>

        <asp:GridView ID="gvRequests" runat="server" AutoGenerateColumns="False" CssClass="myGridView"
            DataKeyNames="ReportID"
            OnRowDataBound="gvRequests_RowDataBound"
            OnPageIndexChanging="gvRequests_PageIndexChanging">
            <Columns>
                <asp:BoundField DataField="ReportID" HeaderText="Report ID" ReadOnly="true" />
                <asp:TemplateField HeaderText="Issue Date"><ItemTemplate><%# Eval("IssueDate","{0:yyyy-MM-dd}") %></ItemTemplate></asp:TemplateField>
                <asp:TemplateField HeaderText="Department"><ItemTemplate><%# Eval("Department") %></ItemTemplate></asp:TemplateField>
                <asp:BoundField DataField="RequestUser" HeaderText="Request User" ReadOnly="true" />
                <asp:TemplateField HeaderText="Issue Details"><ItemTemplate><%# HttpUtility.HtmlDecode(Eval("IssueDetails").ToString()) %></ItemTemplate></asp:TemplateField>
                <asp:TemplateField HeaderText="Issue Type"><ItemTemplate><%# Eval("IssueType") %></ItemTemplate></asp:TemplateField>
                <asp:TemplateField HeaderText="DRI User"><ItemTemplate><%# Eval("DRI_UserName") %></ItemTemplate></asp:TemplateField>
                <asp:TemplateField HeaderText="Status"><ItemTemplate><%# Eval("Status") %></ItemTemplate></asp:TemplateField>

                <asp:TemplateField HeaderText="Detail">
                    <ItemTemplate>
                        <a class="btn btn-sm btn-outline-primary btn-icon"
                           href='<%# Eval("ReportID","IT_RequestsList.aspx?id={0}") %>'
                           title="ดูรายละเอียด" aria-label="ดูรายละเอียด">
                            <i class="bi bi-eye"></i>
                        </a>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </asp:Panel>

    <asp:Panel ID="pnlDetail" runat="server" Visible="false">
        <style>
            #detail-wrap .card{ border:0; box-shadow: 0 8px 28px rgba(0,0,0,.08) }
            #detail-wrap .card-header{ background:#2e8b57; color:#fff }
            #detail-wrap .form-plain{ display:block; width:100%; background:#f4f6f9; border:1px solid #e5e7eb; border-radius:.5rem; padding:.5rem .75rem; min-height:38px; line-height:1.6; }
            #detail-wrap .label{ font-weight:600; color:#334155; margin-bottom:.25rem }
            #detail-wrap .form-select, #detail-wrap .form-control:not(textarea){ min-height:44px; }
            #detail-wrap textarea{ width:100% !important; max-width:none !important; }
            #detail-wrap #txtIssueDetail{ min-height:240px; resize:vertical; }
            #detail-wrap #txtSolution{ min-height:200px; resize:vertical; }
            #detail-wrap #txtRemark{ min-height:200px; resize:vertical; }
        </style>

        <div id="detail-wrap" class="container my-4">
            <div class="card">
                <div class="card-header d-flex justify-content-between align-items-center">
                    <h5 class="mb-0">
                        <i class="bi bi-info-circle"></i> Request Detail
                        <span class="ms-2" style="background:#2e8b57;color:#fff;border-radius:999px;padding:.1rem .6rem;">
                            <strong>(#</strong><asp:Label ID="lblReportID" runat="server" /><strong>)</strong>
                        </span>
                    </h5>
                    <asp:Button ID="btnBack" runat="server" Text="← Back to List"
                        CssClass="btn btn-light btn-sm fw-bold" OnClick="btnBack_Click" />
                </div>

                <div class="card-body">
                    <div class="row g-3 mb-1">
                        <div class="col-lg-3 col-md-6">
                            <div class="label">Issue Date</div>
                            <span class="form-plain"><asp:Label ID="lblIssueDate" runat="server" /></span>
                        </div>
                        <div class="col-lg-3 col-md-6">
                            <div class="label">Request User</div>
                            <span class="form-plain"><asp:Label ID="lblRequestUser" runat="server" /></span>
                        </div>
                        <div class="col-lg-3 col-md-6">
                            <div class="label">Department</div>
                            <asp:DropDownList ID="ddlDetailDept" runat="server" CssClass="form-select"></asp:DropDownList>
                        </div>
                        <div class="col-lg-3 col-md-6">
                            <div class="label">Issue Type</div>
                            <asp:DropDownList ID="ddlDetailIssueType" runat="server" CssClass="form-select"></asp:DropDownList>
                        </div>
                    </div>

                    <div class="mb-3">
                        <div class="label">Issue Details</div>
                        <asp:TextBox ID="txtIssueDetail" runat="server"
                            TextMode="MultiLine" Columns="200"
                            CssClass="form-control"
                            Style="width:100% !important; max-width:none !important;">
                        </asp:TextBox>
                    </div>

                    <hr class="section-line" />

                    <div class="row g-4 align-items-start">
                        <div class="col-lg-8">
                            <div class="row g-3">
                                <div class="col-md-6">
                                    <div class="label">DRI User</div>
                                    <asp:DropDownList ID="ddlDetailDRI" runat="server" CssClass="form-select"></asp:DropDownList>
                                </div>
                                <div class="col-md-6">
                                    <div class="label">Status</div>
                                    <asp:Label ID="lblStatusText" runat="server" Visible="false" />
                                    <asp:DropDownList ID="ddlDetailStatus" runat="server" CssClass="form-select">
                                        <asp:ListItem Text="WIP" Value="0"></asp:ListItem>
                                        <asp:ListItem Text="Done" Value="1"></asp:ListItem>
                                    </asp:DropDownList>
                                </div>

                                <div class="col-12">
                                    <div class="label">Solution</div>
                                    <asp:TextBox ID="txtSolution" runat="server"
                                        TextMode="MultiLine" Columns="200"
                                        CssClass="form-control"
                                        Style="width:100% !important; max-width:none !important;">
                                    </asp:TextBox>
                                </div>

                                <div class="col-12">
                                    <div class="label">Remark</div>
                                    <asp:TextBox ID="txtRemark" runat="server"
                                        TextMode="MultiLine" Columns="200"
                                        CssClass="form-control"
                                        Style="width:100% !important; max-width:none !important;">
                                    </asp:TextBox>
                                </div>

                                <div class="col-12">
                                    <div class="label">Finished Date</div>
                                    <span class="form-plain"><asp:Label ID="lblFinishedDate" runat="server" /></span>
                                </div>
                            </div>
                        </div>

                        <div class="col-lg-4">
                            <div class="label mb-2">Image</div>

                            <asp:PlaceHolder ID="phNoImage" runat="server">
                                <div class="img-box">
                                    <div class="text-center">
                                        <i class="bi bi-image" style="font-size:2rem;"></i>
                                        <div class="text-muted mt-2">No image 🖼️</div>
                                    </div>
                                </div>
                            </asp:PlaceHolder>

                            <!-- แสดงรูปบนหน้าเดียวกัน + เปิด modal (encode attribute กันเตือน HTML5) -->
                            <asp:Repeater ID="rptImages" runat="server">
                                <HeaderTemplate><div class="row g-2"></HeaderTemplate>
                                <ItemTemplate>
                                    <div class="col-6">
                                        <a href="javascript:void(0)" class="d-block req-img-thumb"
                                           data-img='<%# HttpUtility.HtmlAttributeEncode((Eval("Url") ?? "").ToString()) %>'>
                                            <img src='<%# ResolveUrl((Eval("Url") ?? "").ToString()) %>'
                                                 class="img-fluid rounded border" alt="attachment" />
                                        </a>
                                    </div>
                                </ItemTemplate>
                                <FooterTemplate></div></FooterTemplate>
                            </asp:Repeater>
                        </div>
                    </div>

                    <div class="row mt-4">
                        <div class="col-12 d-flex gap-2 flex-wrap actions justify-content-end">
                            <asp:Button ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-warning" OnClick="btnEdit_Click" />
                            <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-success" OnClick="btnSave_Click" Visible="false" />
                            <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel" CssClass="btn btn-secondary" OnClick="btnCancelEdit_Click" Visible="false" />
                            <asp:Button ID="btnDeleteDetail" runat="server" Text="Delete" CssClass="btn btn-danger" OnClick="btnDeleteDetail_Click" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </asp:Panel>

    <!-- Modal แสดงรูปบนหน้าเดียวกัน -->
    <div class="modal fade" id="imgModal" tabindex="-1" aria-hidden="true">
      <div class="modal-dialog modal-dialog-centered modal-xl">
        <div class="modal-content bg-dark">
          <div class="modal-header border-0">
            <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
          </div>
          <div class="modal-body p-0 d-flex justify-content-center">
            <img id="imgPreview"
     src="data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///ywAAAAAAQABAAACAUwAOw=="
     alt=""
     style="max-width:100%;max-height:85vh" />
          </div>
        </div>
      </div>
    </div>

    <script>
        (function () {
            function showModal(src) {
                var img = document.getElementById('imgPreview');
                if (img) img.setAttribute('src', src || '');
                if (window.bootstrap && bootstrap.Modal) {
                    new bootstrap.Modal(document.getElementById('imgModal')).show();
                } else if (window.jQuery) {
                    $('#imgModal').modal('show'); // fallback BS4
                }
            }

            document.addEventListener('click', function (e) {
                var a = e.target.closest('.req-img-thumb');
                if (!a) return;
                e.preventDefault();
                showModal(a.getAttribute('data-img'));
            }, false);
        })();
    </script>
</asp:Content>
