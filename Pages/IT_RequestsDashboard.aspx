<%@ Page Title="IT Requests Dashboard" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="IT_RequestsDashboard.aspx.cs"
    Inherits="IT_WorkPlant.Pages.IT_RequestsDashboard" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/html2canvas/1.4.1/html2canvas.min.js"></script>

    <style>
        body {
            background-color: #1e1e2f;
            color: white;
        }
        .card-summary {
            text-align: center;
            padding: 20px;
            background-color: #40566b;
            border: 1px solid #3c4d5f;
            border-radius: 10px;
        }
        .dark-card {
            background-color: #3b4f63;
            border: 1px solid #3c4d5f;
            border-radius: 10px;
            color: white;
        }
        .dark-card h6 { color: white; }
        h1 { color: #448cd4; }
        h3 { color: white; }
        .chart-box { height: 300px; }
        .summary-table { font-size: 0.9rem; }
        .summary-table th, .summary-table td {
            background-color: #34495e !important;
            color: white !important;
        }
        .form-select {
            background-color: #2c3e50;
            color: white;
            border: 1px solid #444;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-4">
        <h1 class="text-center mb-4">IT Requests Dashboard</h1>

        <div class="text-center mb-3">
            <asp:DropDownList ID="ddlTimeFilter" runat="server" AutoPostBack="true"
                OnSelectedIndexChanged="ddlTimeFilter_SelectedIndexChanged"
                CssClass="form-select w-auto d-inline">
                <asp:ListItem Text="All Time" Value="all" />
                <asp:ListItem Text="This Month" Value="month" />
                <asp:ListItem Text="This Week" Value="week" />
                <asp:ListItem Text="Today" Value="today" />
            </asp:DropDownList>
        </div>

        <div class="row mb-4 text-white text-center">
            <div class="col-md-3">
                <div class="card-summary">
                    <h6>Total Requests</h6>
                    <h3><asp:Label ID="lblTotal" runat="server" Text="0" /></h3>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card-summary" style="background-color: #f1c40f; color: black;">
                    <h6>In Progress</h6>
                    <h3><asp:Label ID="lblWIP" runat="server" Text="0" /></h3>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card-summary" style="background-color: #2ecc71;">
                    <h6>Completed</h6>
                    <h3><asp:Label ID="lblDone" runat="server" Text="0" /></h3>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card-summary" style="background-color: #3498db;">
                    <h6>Completed Today</h6>
                    <h3><asp:Label ID="lblDoneToday" runat="server" Text="0" /></h3>
                </div>
            </div>
        </div>

        <div class="row mb-4">
            <div class="col-md-6">
                <div class="dark-card p-3">
                    <h6 class="text-center">Requests by Type</h6>
                    <canvas id="chartType" class="chart-box"></canvas>
                    <asp:Literal ID="ltTableType" runat="server" />
                </div>
            </div>
            <div class="col-md-6">
                <div class="dark-card p-3">
                    <h6 class="text-center">Requests by Department</h6>
                    <canvas id="chartDept" class="chart-box"></canvas>
                    <asp:Literal ID="ltTableDept" runat="server" />
                </div>
            </div>
        </div>

        <div class="row">
            <div class="col-md-6">
                <div class="dark-card p-3">
                    <h6 class="text-center">Requests Trend</h6>
                    <canvas id="chartTrend" class="chart-box"></canvas>
                    <asp:Literal ID="ltTableTrend" runat="server" />
                </div>
            </div>
            <div class="col-md-6">
                <div class="dark-card p-3">
                    <h6 class="text-center">Requests by Responsible Person</h6>
                    <canvas id="chartDRI" class="chart-box"></canvas>
                    <asp:Literal ID="ltTableDRI" runat="server" />
                </div>
            </div>
        </div>
    </div>

    <asp:HiddenField ID="hfChartData" runat="server" />

    <script type="text/javascript">
        let chartType, chartDept, chartTrend, chartDRI;

        window.onload = function () {
            let raw = document.getElementById('<%= hfChartData.ClientID %>').value || '{}';
            let chartData = {};
            try { chartData = JSON.parse(raw); } catch (e) { console.error("Invalid JSON", e); }

            const createChart = (id, type, labels, data, bgColors) => {
                return new Chart(document.getElementById(id), {
                    type: type,
                    data: {
                        labels: labels || [],
                        datasets: [{
                            data: data || [],
                            backgroundColor: bgColors,
                            borderColor: '#ffffff',
                            borderWidth: type === 'line' ? 2 : 1,
                            fill: type === 'line',
                            tension: 0.3
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: {
                            legend: {
                                display: type === 'pie',
                                labels: { color: 'white' }
                            }
                        },
                        scales: (type === 'pie') ? {} : {
                            x: {
                                ticks: { color: 'white' },
                                grid: { color: 'rgba(255,255,255,0.2)' }
                            },
                            y: {
                                ticks: { color: 'white' },
                                grid: { color: 'rgba(255,255,255,0.2)' }
                            }
                        }
                    },
                });
            };

            chartType = createChart('chartType', 'bar', chartData.type?.labels, chartData.type?.data,
                ['#f1c40f', '#2ecc71', '#e74c3c', '#3498db', '#9b59b6', '#1abc9c']);
            chartDept = createChart('chartDept', 'bar', chartData.dept?.labels, chartData.dept?.data,
                ['#e67e22', '#f39c12', '#16a085', '#27ae60', '#8e44ad', '#c0392b', '#2980b9']);
            chartTrend = createChart('chartTrend', 'line', chartData.trend?.labels, chartData.trend?.data,
                ['#3498db']);
            chartDRI = createChart('chartDRI', 'pie', chartData.dri?.labels, chartData.dri?.data,
                ['#e74c3c', '#f1c40f', '#2ecc71', '#3498db', '#9b59b6']);
        };
    </script>
</asp:Content>
