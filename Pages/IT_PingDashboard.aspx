<%@ Page Title="Ping Server Dashboard" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="IT_PingDashboard.aspx.cs" 
    Inherits="IT_WorkPlant.Pages.IT_PingDashboard" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <style>
        /* ปุ่มสรุป + ไฮไลต์ (ใช้ "class" ไม่ผูกกับ ID) */
        .stat-btn { border-radius:.75rem; padding:1rem 1rem; color:#fff; text-decoration:none; display:block; }
        .stat-btn h4 { margin:0; }
        .stat-all    { background:#3b82f6; } /* blue  */
        .stat-online { background:#198754; } /* green */
        .stat-offline{ background:#dc3545; } /* red   */
        .stat-active { outline:3px solid rgba(0,0,0,.25); outline-offset:2px; }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-4">
        <h2 class="mb-4">🌐 Server Status Dashboard</h2>

        <!-- ให้ทุกอย่างอยู่ใน UpdatePanel เดียว -->
        <asp:UpdatePanel ID="updMain" runat="server">
            <ContentTemplate>

                <!-- ฟอร์มเพิ่ม Server -->
                <div class="card p-4 mb-4">
                    <h4 class="mb-3">➕ Add New Server</h4>
                    <div class="row g-3">
                        <div class="col-md-6">
                            <asp:TextBox ID="txtNewName" runat="server" CssClass="form-control" placeholder="Server Name (e.g. Firewall, NAS)" />
                        </div>
                        <div class="col-md-4">
                            <asp:TextBox ID="txtNewIP" runat="server" CssClass="form-control" placeholder="IP Address (e.g. 192.168.1.1)" />
                        </div>
                        <div class="col-md-2">
                            <asp:Button ID="btnAddServer" runat="server" CssClass="btn btn-primary w-100" Text="Add" OnClick="btnAddServer_Click" />
                        </div>
                    </div>
                    <asp:Label ID="lblAddStatus" runat="server" CssClass="mt-3 d-block"></asp:Label>
                </div>

                <!-- ปุ่มสรุป (กดเพื่อฟิลเตอร์) -->
                <div class="row g-3 mb-3 text-center">
                    <div class="col-md-4">
                        <asp:LinkButton ID="btnFilterAll" runat="server"
                            CssClass="stat-btn stat-all d-block w-100"
                            OnClick="btnFilterAll_Click" OnClientClick="this.blur();">
                            <div class="fw-semibold" style="opacity:.9;">Total Servers</div>
                            <h4 class="m-0"><asp:Label ID="lblTotalServers" runat="server" Text="0" /></h4>
                        </asp:LinkButton>
                    </div>
                    <div class="col-md-4">
                        <asp:LinkButton ID="btnFilterOnline" runat="server"
                            CssClass="stat-btn stat-online d-block w-100"
                            OnClick="btnFilterOnline_Click" OnClientClick="this.blur();">
                            <div class="fw-semibold" style="opacity:.9;">Online</div>
                            <h4 class="m-0"><asp:Label ID="lblOnline" runat="server" Text="0" /></h4>
                        </asp:LinkButton>
                    </div>
                    <div class="col-md-4">
                        <asp:LinkButton ID="btnFilterOffline" runat="server"
                            CssClass="stat-btn stat-offline d-block w-100"
                            OnClick="btnFilterOffline_Click" OnClientClick="this.blur();">
                            <div class="fw-semibold" style="opacity:.9;">Offline</div>
                            <h4 class="m-0"><asp:Label ID="lblOffline" runat="server" Text="0" /></h4>
                        </asp:LinkButton>
                    </div>
                </div>

                <!-- ค้นหา -->
                <div class="mb-3">
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control" 
                                 placeholder="🔍 Search for name or IP address" 
                                 AutoPostBack="true" OnTextChanged="txtSearch_TextChanged" />
                </div>

                <!-- รายการการ์ดสถานะ -->
                <asp:Timer ID="PingTimer" runat="server" Interval="30000" OnTick="PingTimer_Tick" />
                <asp:Panel ID="pnlStatus" runat="server" />

            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
</asp:Content>
