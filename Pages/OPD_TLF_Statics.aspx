<%@ Page Title="OPD TLF Statistics" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="OPD_TLF_Statics.aspx.cs" Inherits="IT_WorkPlant.Pages.OPD_TLF_Statics" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
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
        .chart-box { height: 400px; }
        .date-range-container {
            display: flex;
            gap: 10px;
            align-items: center;
            justify-content: center;
            margin-bottom: 20px;
        }
        .date-input {
            background-color: #2c3e50;
            color: white;
            border: 1px solid #444;
            padding: 5px;
            border-radius: 5px;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-4">
        <h1 class="text-center mb-4">Work Order Posting</h1>

        <div class="date-range-container">
            <asp:TextBox ID="txtStartDate" runat="server" TextMode="Date" CssClass="date-input" />
            <span>to</span>
            <asp:TextBox ID="txtEndDate" runat="server" TextMode="Date" CssClass="date-input" />
            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary" OnClick="btnSearch_Click" />
        </div>

        <div class="row mb-4">
            <div class="col-12">
                <div class="dark-card p-3">
                    <h6 class="text-center">Daily Statistics</h6>
                    <canvas id="chartDaily" class="chart-box"></canvas>
                </div>
            </div>
        </div>
    </div>

    <asp:HiddenField ID="hfChartData" runat="server" />

    <script type="text/javascript">
        let chartDaily;

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
                            borderColor: type === 'line' ? '#3498db' : bgColors,
                            borderWidth: type === 'line' ? 2 : 1,
                            fill: type === 'line',
                            tension: 0.3,
                            pointBackgroundColor: type === 'line' ? '#3498db' : undefined,
                            pointBorderColor: type === 'line' ? '#ffffff' : undefined,
                            pointRadius: type === 'line' ? 4 : undefined
                        }]
                    },
                    options: {
                        responsive: true,
                        plugins: {
                            legend: {
                                display: false
                            }
                        },
                        scales: {
                            x: {
                                ticks: { color: 'white' },
                                grid: { color: 'rgba(255,255,255,0.2)' }
                            },
                            y: {
                                ticks: { color: 'white' },
                                grid: { color: 'rgba(255,255,255,0.2)' },
                                beginAtZero: true
                            }
                        }
                    },
                });
            };

            chartDaily = createChart('chartDaily', 'line', chartData.daily?.labels, chartData.daily?.data,
                ['#3498db']);
        };
    </script>
</asp:Content> 