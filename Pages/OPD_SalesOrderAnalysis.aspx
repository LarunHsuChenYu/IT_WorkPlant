<%@ Page Title="OPD Sales Order Analysis" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="OPD_SalesOrderAnalysis.aspx.cs" Inherits="IT_WorkPlant.Pages.OPD_SalesOrderAnalysis" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/chartjs-adapter-date-fns/dist/chartjs-adapter-date-fns.bundle.min.js"></script>
    <style>
        body {
            background-color: #1e1e2f;
            color: white;
        }
        .dark-card {
            background-color: #3b4f63;
            border: 1px solid #3c4d5f;
            border-radius: 10px;
            color: white;
            margin-bottom: 20px;
        }
        .dark-card h6 { 
            color: white; 
            margin-bottom: 15px;
        }
        h1 { 
            color: #448cd4; 
            text-align: center;
            margin-bottom: 30px;
        }
        .chart-container { 
            height: 400px; 
            position: relative;
        }
        .date-range-container {
            display: flex;
            gap: 15px;
            align-items: center;
            justify-content: center;
            margin-bottom: 30px;
            flex-wrap: wrap;
        }
        .date-input {
            background-color: #2c3e50;
            color: white;
            border: 1px solid #444;
            padding: 8px 12px;
            border-radius: 5px;
        }
        .btn-custom {
            background-color: #3498db;
            border: none;
            color: white;
            padding: 8px 20px;
            border-radius: 5px;
            cursor: pointer;
        }
        .btn-custom:hover {
            background-color: #2980b9;
        }
        .btn-export {
            background-color: #27ae60;
            border: none;
            color: white;
            padding: 8px 20px;
            border-radius: 5px;
            cursor: pointer;
            margin-left: 10px;
        }
        .btn-export:hover {
            background-color: #229954;
        }
        .filter-section {
            background-color: #2c3e50;
            padding: 15px;
            border-radius: 8px;
            margin-bottom: 20px;
        }
        .filter-label {
            color: #bdc3c7;
            font-size: 14px;
            margin-bottom: 5px;
        }
        .filter-value {
            color: #ecf0f1;
            font-weight: bold;
        }
        .heatmap-cell {
            display: inline-block;
            width: 30px;
            height: 30px;
            margin: 2px;
            border-radius: 4px;
            text-align: center;
            line-height: 30px;
            font-size: 10px;
            color: white;
            font-weight: bold;
        }
        .table-container {
            max-height: 400px;
            overflow-y: auto;
            background-color: #2c3e50;
            border-radius: 8px;
        }
        .table-dark {
            background-color: transparent;
            color: white;
        }
        .table-dark th {
            background-color: #34495e;
            border-color: #3c4d5f;
            color: #ecf0f1;
        }
        .table-dark td {
            border-color: #3c4d5f;
        }
        .loading {
            text-align: center;
            padding: 20px;
            color: #bdc3c7;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid py-4">
        <h1>Sales Order Analysis</h1>

       
        <div class="date-range-container">
            <div>
                <div class="filter-label">Start Date</div>
                <asp:TextBox ID="txtStartDate" runat="server" TextMode="Date" CssClass="date-input" />
            </div>
            <div>
                <div class="filter-label">End Date</div>
                <asp:TextBox ID="txtEndDate" runat="server" TextMode="Date" CssClass="date-input" />
            </div>
            <div>
                <div class="filter-label">&nbsp;</div>
                <asp:Button ID="btnSearch" runat="server" Text="Query" CssClass="btn-custom" OnClick="btnSearch_Click" />
            </div>
        </div>

       
        <div class="filter-section" id="filterSection" runat="server" visible="false">
            <div class="row">
                <div class="col-md-3">
                    <div class="filter-label">Selected Model</div>
                    <div class="filter-value" id="selectedModels" runat="server"></div>
                </div>
                <div class="col-md-3">
                    <div class="filter-label">Customer Quantity</div>
                    <div class="filter-value" id="customerCount" runat="server"></div>
                </div>
                <div class="col-md-3">
                    <div class="filter-label">Data Period</div>
                    <div class="filter-value" id="dateRange" runat="server"></div>
                </div>
                <div class="col-md-3">
                    <div class="filter-label">Operation</div>
                    <asp:Button ID="btnClearFilter" runat="server" Text="Clear Filters" CssClass="btn-custom" OnClick="btnClearFilter_Click" />
                </div>
            </div>
        </div>

        <div class="row">
          
            <div class="col-lg-6">
                <div class="dark-card p-3">
                    <h6>Top 5 SPO Model Sales</h6>
                    <div class="chart-container">
                        <canvas id="chartTop5SPO"></canvas>
                    </div>
                </div>
            </div>

           
            <div class="col-lg-6">
                <div class="dark-card p-3">
                    <h6>Top 5 SPO Model Sales</h6>
                    <div class="chart-container">
                        <canvas id="chartTop5STO"></canvas>
                    </div>
                </div>
            </div>
        </div>

        <div class="row">
          
            <div class="col-lg-6">
                <div class="dark-card p-3">
                    <h6>SO_TYPE Distribution</h6>
                    <div class="chart-container">
                        <canvas id="chartSoType"></canvas>
                    </div>
                </div>
            </div>

           
            <div class="col-lg-8">
                <div class="dark-card p-3">
                    <h6>Customer Hotspot Analysis</h6>
                    <div class="chart-container">
                        <canvas id="chartHeatmap"></canvas>
                    </div>
                </div>
            </div>

           
            <div class="col-lg-4">
                <div class="dark-card p-3">
                    <h6>Sales Detail Report</h6>
                    <div class="mb-3">
                        <asp:Button ID="btnExportCSV" runat="server" Text="Export CSV CSV" CssClass="btn-export" OnClick="btnExportCSV_Click" />
                    </div>
                    <div class="table-container">
                        <asp:GridView ID="gvSalesDetail" runat="server" CssClass="table table-dark table-sm" 
                                      AutoGenerateColumns="false" ShowHeader="true" GridLines="Horizontal">
                            <Columns>
                                <asp:BoundField DataField="model_pn" HeaderText="Product Model" />
                                <asp:BoundField DataField="sales_qty" HeaderText="Sales Volume" />
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <asp:HiddenField ID="hfTop5Data" runat="server" />
    <asp:HiddenField ID="hfSoTypeData" runat="server" />
    <asp:HiddenField ID="hfHeatmapData" runat="server" />
    <asp:HiddenField ID="hfSelectedModels" runat="server" />

    <script type="text/javascript">
        let chartTop5SPO, chartTop5STO, chartSoType, chartHeatmap;

        window.onload = function () {
            initializeCharts();
        };

        function initializeCharts() {
           
            let top5Data = parseJsonData('<%= hfTop5Data.Value %>');
            let soTypeData = parseJsonData('<%= hfSoTypeData.Value %>');
            let heatmapData = parseJsonData('<%= hfHeatmapData.Value %>');

            if (top5Data && top5Data.spoModel) {
                createTop5Chart(top5Data.spoModel, 'chartTop5SPO', 'SPO');
            }

            if (top5Data && top5Data.stoModel) {
                createTop5Chart(top5Data.stoModel, 'chartTop5STO', 'STO');
            }

            if (soTypeData && soTypeData.labels && soTypeData.datasets) {
                createSoTypeChart(soTypeData);
            }


            if (heatmapData && heatmapData.customers && heatmapData.models && heatmapData.data) {
                createHeatmapChart(heatmapData);
            }
        }

        function parseJsonData(jsonString) {
            try {
                return JSON.parse(jsonString || '{}');
            } catch (e) {
                console.error("JSON parse error:", e);
                return {};
            }
        }

        function createTop5Chart(chartModel, canvasId, type) {
            const ctx = document.getElementById(canvasId);
            if (!ctx) return;

            let chartInstance;
            if (type === 'SPO') {
                if (chartTop5SPO) chartTop5SPO.destroy();
                chartInstance = chartTop5SPO = new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: chartModel.labels,
                        datasets: [{
                            label: 'Sales Volume',
                            data: chartModel.data,
                            backgroundColor: '#3498db',
                            borderColor: '#2980b9',
                            borderWidth: 1
                        }]
                    },
                    options: {
                        indexAxis: 'y',
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: { display: false },
                            tooltip: {
                                callbacks: {
                                    label: function (context) {
                                        const idx = context.dataIndex;
                                        const qty = context.parsed.x;
                                        const fullNames = chartModel.fullNames && chartModel.fullNames[idx] ? chartModel.fullNames[idx] : '';
                                       
                                        let lines = fullNames.split(/;\s*|\n/).filter(x => x);
                                        let result = ['Sales Volume: ' + qty.toLocaleString(), 'Assembled Product:'];
                                        result = result.concat(lines);
                                        return result;
                                    }
                                }
                            }
                        },
                        scales: {
                            x: {
                                ticks: { color: 'white' },
                                grid: { color: 'rgba(255,255,255,0.2)' },
                                beginAtZero: true
                            },
                            y: {
                                ticks: { color: 'white' },
                                grid: { color: 'rgba(255,255,255,0.2)' }
                            }
                        },
                        onClick: function (event, elements) {
                            if (elements.length > 0) {
                                const index = elements[0].index;
                                const modelName = chartModel.labels[index];
                                selectModel(modelName);
                            }
                        }
                    }
                });
            } else { 
                if (chartTop5STO) chartTop5STO.destroy();
                chartInstance = chartTop5STO = new Chart(ctx, {
                    type: 'bar',
                    data: {
                        labels: chartModel.labels,
                        datasets: [{
                            label: 'Sales Volume',
                            data: chartModel.data,
                            backgroundColor: '#e74c3c',
                            borderColor: '#c0392b',
                            borderWidth: 1
                        }]
                    },
                    options: {
                        indexAxis: 'y',
                        responsive: true,
                        maintainAspectRatio: false,
                        plugins: {
                            legend: { display: false },
                            tooltip: {
                                callbacks: {
                                    label: function (context) {
                                        const idx = context.dataIndex;
                                        const qty = context.parsed.x;
                                        const fullNames = chartModel.fullNames && chartModel.fullNames[idx] ? chartModel.fullNames[idx] : '';
                                        
                                        let lines = fullNames.split(/;\s*|\n/).filter(x => x);
                                        let result = ['Sales Volume: ' + qty.toLocaleString(), 'Composite Product:'];
                                        result = result.concat(lines);
                                        return result;
                                    }
                                }
                            }
                        },
                        scales: {
                            x: {
                                ticks: { color: 'white' },
                                grid: { color: 'rgba(255,255,255,0.2)' },
                                beginAtZero: true
                            },
                            y: {
                                ticks: { color: 'white' },
                                grid: { color: 'rgba(255,255,255,0.2)' }
                            }
                        },
                        onClick: function (event, elements) {
                            if (elements.length > 0) {
                                const index = elements[0].index;
                                const modelName = chartModel.labels[index];
                                selectModel(modelName);
                            }
                        }
                    }
                });
            }
        }

        function createSoTypeChart(data) {
            const ctx = document.getElementById('chartSoType');
            if (!ctx) return;

            chartSoType = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: data.labels,
                    datasets: data.datasets
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: true,
                            labels: { color: 'white' }
                        },
                        tooltip: {
                            callbacks: {
                                label: function(context) {
                                    const total = context.dataset.data.reduce((a, b) => a + b, 0);
                                    const percentage = ((context.parsed.y / total) * 100).toFixed(1);
                                    return context.dataset.label + ': ' + context.parsed.y + ' (' + percentage + '%)';
                                }
                            }
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
                            beginAtZero: true,
                            stacked: true
                        }
                    }
                }
            });
        }

        function createHeatmapChart(data) {
            const ctx = document.getElementById('chartHeatmap');
            if (!ctx) return;

            
            const heatmapData = {
                labels: data.customers,
                datasets: data.models.map((model, index) => ({
                    label: model,
                    data: data.data[index] || [],
                    backgroundColor: getColorForIndex(index),
                    borderColor: '#2c3e50',
                    borderWidth: 1
                }))
            };

            chartHeatmap = new Chart(ctx, {
                type: 'bar',
                data: heatmapData,
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: true,
                            labels: { color: 'white' }
                        },
                        tooltip: {
                            callbacks: {
                                label: function(context) {
                                    return context.dataset.label + ': ' + context.parsed.y;
                                }
                            }
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
                }
            });
        }

        function getColorForIndex(index) {
            const colors = ['#e74c3c', '#3498db', '#2ecc71', '#f39c12', '#9b59b6', '#1abc9c', '#e67e22'];
            return colors[index % colors.length];
        }

        function selectModel(modelName) {
            
            document.getElementById('<%= hfSelectedModels.ClientID %>').value = modelName;
            
            
            __doPostBack('<%= btnSearch.UniqueID %>', '');
        }

        function refreshCharts() {
            if (chartTop5SPO) chartTop5SPO.destroy();
            if (chartTop5STO) chartTop5STO.destroy();
            if (chartSoType) chartSoType.destroy();
            if (chartHeatmap) chartHeatmap.destroy();
            
            setTimeout(initializeCharts, 100);
        }
    </script>
</asp:Content> 