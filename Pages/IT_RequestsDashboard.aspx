<%@ Page Title="IT Requests Dashboard" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="IT_RequestsDashboard.aspx.cs"
    Inherits="IT_WorkPlant.Pages.IT_RequestsDashboard" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <!-- ยังโหลดปลั๊กอินไว้ แต่ปิดการใช้งาน datalabels ใน config -->
    <script src="https://cdn.jsdelivr.net/npm/chartjs-plugin-datalabels@2.2.0"></script>
    <script src="https://cdn.jsdelivr.net/npm/chartjs-plugin-annotation@1.1.0"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/html2canvas/1.4.1/html2canvas.min.js"></script>
    <link href="https://cdn.jsdelivr.net/npm/flatpickr/dist/flatpickr.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/flatpickr"></script>

    <script type="text/javascript">
        window.onload = function () {
            flatpickr("#<%= txtFromDate.ClientID %>", {
                dateFormat: "Y/m/d",
                altInput: true,
                altFormat: "d M Y",
                allowInput: false,
                defaultDate: "today"
            });

            flatpickr("#<%= txtToDate.ClientID %>", {
                dateFormat: "Y/m/d",
                altInput: true,
                altFormat: "d M Y",
                allowInput: false,
                defaultDate: "today"
            });

            let raw = document.getElementById('<%= hfChartData.ClientID %>').value || '{}';
            let chartData = {};
            try { chartData = JSON.parse(raw); } catch (e) { console.error("Invalid JSON", e); }

            const createChart = (id, type, labels, data, bgColors) => {
                const isPie = (type === 'pie' || type === 'doughnut');

                const config = {
                    type: type,
                    data: {
                        labels: labels || [],
                        datasets: [{
                            data: data || [],
                            backgroundColor: bgColors,
                            borderColor: type === 'line' ? '#4aa3ff' : '#ffffff',
                            borderWidth: type === 'line' ? 2 : 1,
                            fill: type === 'line',
                            tension: 0.35,
                            pointRadius: type === 'line' ? 3 : 0
                        }]
                    },
                    options: {
                        responsive: true,
                        maintainAspectRatio: false,      // << สำคัญ! ให้ canvas สูงตามกล่อง
                        plugins: {
                            legend: {
                                display: isPie,            // โชว์เฉพาะกราฟวงกลม
                                position: 'bottom',
                                labels: { color: 'white' }
                            },
                            datalabels: { display: false } // ปิดตัวเลขบนกราฟไว้ก่อน
                        },
                        scales: isPie ? {} : {
                            x: {
                                ticks: { color: 'white', autoSkip: true, maxTicksLimit: 7 },
                                grid: { color: 'rgba(255,255,255,0.15)' }
                            },
                            y: {
                                beginAtZero: true,
                                ticks: { color: 'white', precision: 0 },
                                grid: { color: 'rgba(255,255,255,0.15)' }
                            }
                        }
                    }
                };

                return new Chart(document.getElementById(id), config);
            };

            // ===== สร้างเฉพาะกราฟที่ต้องใช้ =====
            const chartType = createChart('chartType', 'bar', chartData.type?.labels, chartData.type?.data,
                ['#f1c40f', '#2ecc71', '#e74c3c', '#3498db', '#9b59b6', '#1abc9c']);
            const chartDept = createChart('chartDept', 'bar', chartData.dept?.labels, chartData.dept?.data,
                ['#e67e22', '#f39c12', '#16a085', '#27ae60', '#8e44ad', '#c0392b', '#2980b9']);
            const chartTrend = createChart('chartTrend', 'line', chartData.trend?.labels, chartData.trend?.data,
                ['rgba(52,152,219,0.35)']);
            const chartDRI = createChart('chartDRI', 'pie', chartData.dri?.labels, chartData.dri?.data,
                ['#e74c3c', '#f1c40f', '#2ecc71', '#3498db', '#9b59b6']);
        };

        function captureDashboard() {
            const captureTarget = document.querySelector(".container.py-4");
            if (!captureTarget) {
                alert("ไม่พบ Dashboard ที่จะ capture");
                return;
            }
            html2canvas(captureTarget).then(canvas => {
                const image = canvas.toDataURL("image/png");
                const link = document.createElement("a");
                link.href = image;
                link.download = "IT_Dashboard_Snapshot.png";
                link.click();
            });
        }
    </script>

    <style>
        body { background-color: #1e1e2f; color: white; }

        .card-summary{
            text-align:center; padding:20px;
            background-color:#40566b; border:1px solid #3c4d5f; border-radius:10px;
        }

        .dark-card{
            background-color:#3b4f63; border:1px solid #3c4d5f; border-radius:10px; color:white;
        }
        .dark-card h6{ color:white; }

        h1{ color:#448cd4; }
        h3{ color:white; }

        /* 🔧 คุมสัดส่วนกราฟให้เท่ากันทุกใบ */
        .chart-card{ min-height: 420px; }
        .chart-body{ height: 360px; }
        @media (max-width: 991.98px){ .chart-body{ height: 320px; } }
        .chart-body canvas{
            width:100% !important;
            height:100% !important;
            display:block;
        }

        .summary-table{ font-size:0.9rem; }
        .summary-table th, .summary-table td{
            background-color:#34495e !important; color:white !important;
        }

        .form-select{
            background-color:#2c3e50; color:white; border:1px solid #444;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-4">
        <h1 class="text-center mb-4">IT Requests Dashboard</h1>

        <div class="text-center mb-3">
            <button type="button" class="btn btn-success" onclick="captureDashboard()">📸 Capture Dashboard</button>
        </div>

        <div class="text-center mb-3">
            <asp:DropDownList ID="ddlTimeFilter" runat="server" AutoPostBack="true"
                OnSelectedIndexChanged="ddlTimeFilter_SelectedIndexChanged"
                CssClass="form-select w-auto d-inline">
                <asp:ListItem Text="All Time" Value="all" />
                <asp:ListItem Text="This Month" Value="month" />
                <asp:ListItem Text="Last Month" Value="lastmonth" />
                <asp:ListItem Text="This Week" Value="week" />
                <asp:ListItem Text="Last Week" Value="lastweek" />
                <asp:ListItem Text="Today" Value="today" />
            </asp:DropDownList>
        </div>

        <div class="row justify-content-center mb-4">
            <div class="col-md-2 text-center">
                <label class="d-block">Start Date (Y/m/d)</label>
                <asp:TextBox ID="txtFromDate" runat="server" CssClass="form-control flatpickr-input" />
            </div>
            <div class="col-md-2 text-center">
                <label class="d-block">End Date (Y/m/d)</label>
                <asp:TextBox ID="txtToDate" runat="server" CssClass="form-control flatpickr-input" />
            </div>
            <div class="col-md-1 text-center d-flex align-items-end">
                <asp:Button ID="btnSearchRange" runat="server" Text="Query" CssClass="btn btn-primary w-100"
                    OnClick="btnSearchRange_Click" />
            </div>
        </div>

        <div class="row mb-4 text-white text-center">
            <div class="col-md-3">
                <div class="card-summary">
                    <h6>Total Requests</h6>
                    <h3><asp:Label ID="lblTotal" runat="server" Text="0" /></h3>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card-summary" style="background-color:#f1c40f; color:black;">
                    <h6>WIP</h6>
                    <h3><asp:Label ID="lblWIP" runat="server" Text="0" /></h3>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card-summary" style="background-color:#2ecc71;">
                    <h6>Completed</h6>
                    <h3><asp:Label ID="lblDone" runat="server" Text="0" /></h3>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card-summary" style="background-color:#3498db;">
                    <h6>Completed Today</h6>
                    <h3><asp:Label ID="lblDoneToday" runat="server" Text="0" /></h3>
                </div>
            </div>
        </div>

        <!-- 🔥 DASHBOARD GRAPHS -->
        <div class="row mb-4">
            <div class="col-md-6">
                <div class="dark-card chart-card p-3 h-100 d-flex flex-column">
                    <h6 class="text-center">Requests by Type</h6>
                    <div class="chart-body"><canvas id="chartType"></canvas></div>
                    <div class="pt-2"><asp:Literal ID="ltTableType" runat="server" /></div>
                </div>
            </div>
            <div class="col-md-6">
                <div class="dark-card chart-card p-3 h-100 d-flex flex-column">
                    <h6 class="text-center">Requests by Department</h6>
                    <div class="chart-body"><canvas id="chartDept"></canvas></div>
                    <div class="pt-2"><asp:Literal ID="ltTableDept" runat="server" /></div>
                </div>
            </div>
        </div>

        <div class="row mb-4">
            <div class="col-md-6">
                <div class="dark-card chart-card p-3 h-100 d-flex flex-column">
                    <h6 class="text-center">Requests Trend</h6>
                    <div class="chart-body"><canvas id="chartTrend"></canvas></div>
                    <div class="pt-2"><asp:Literal ID="ltTableTrend" runat="server" /></div>
                </div>
            </div>
            <div class="col-md-6">
                <div class="dark-card chart-card p-3 h-100 d-flex flex-column">
                    <h6 class="text-center">Requests by Responsible Person</h6>
                    <div class="chart-body"><canvas id="chartDRI"></canvas></div>
                    <div class="pt-2"><asp:Literal ID="ltTableDRI" runat="server" /></div>
                </div>
            </div>
        </div>

        <!-- ❌ ลบ Average Days to Close Request ทั้งบล็อกออก -->
    </div>

    <asp:HiddenField ID="hfChartData" runat="server" />
</asp:Content>
