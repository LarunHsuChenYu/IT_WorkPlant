<%@ Page Title="WarRoom Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" 
    CodeBehind="WarRoom.aspx.cs" Inherits="IT_WorkPlant.Pages.WarRoom" %>
<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        html, body {
            margin: 0;
            padding: 0;
            width: 100%;
            height: 100%;
            overflow: hidden;
            background-color: #111;
        }
        body { color: white; }
        .dashboard-grid {
            display: grid;
            grid-template-columns: 1fr 2fr 1fr;
            grid-template-rows: 1fr 1fr;
            height: 100%;
            width: 100%;
            gap: 1rem;
            padding: 1rem;
            box-sizing: border-box;
        }
        .dashboard-card {
            background: #3b4f63;
            color: white;
            box-shadow: 0 2px 8px rgba(0,0,0,0.15);
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            font-size: 1rem;
            transition: grid-area 0.5s ease-in-out;
            overflow: hidden;
            cursor: pointer;
            border-radius: 8px;
            padding: 15px;
        }
        .pos-tl { grid-area: 1 / 1 / 2 / 2; }
        .pos-lb { grid-area: 2 / 1 / 3 / 2; }
        .pos-center { grid-area: 1 / 2 / 3 / 3; }
        .pos-tr { grid-area: 1 / 3 / 2 / 4; }
        .pos-rb { grid-area: 2 / 3 / 3 / 4; }
        .chart-box {
            width: 100% !important;
            max-height: 75%;
            margin: 0 auto;
        }
        h4 {
            text-align: center;
            margin-bottom: 1rem;
            font-size: 1.25rem;
        }
        .progress {
            height: 1.2rem;
            font-size: 0.8rem;
        }
        #trialOrderProgress {
            width: 90%;
            font-size: 0.8rem;
        }
    </style>
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div style="display: flex; flex-direction: column; height: 80vh;">
        <h1 class="text-center" style="flex-shrink: 0; padding: 1rem 0;">War Room Dashboard</h1>
        <div class="d-flex justify-content-center align-items-center mb-3" style="gap: 1rem;">
            <label for="monthSelect" class="form-label mb-0">Select Month</label>
            <asp:HiddenField ID="hfSelectedMonth" runat="server" />
            <input type="month" id="monthSelect" class="form-control" style="width: 160px;">
            <asp:TextBox ID="txtStartDate" runat="server" Text='<%# txtStartDate.Text %>' 
    TextMode="Date" CssClass="form-control" style="width: 160px; display:none;" EnableViewState="true" />
<asp:TextBox ID="txtEndDate" runat="server" Text='<%# txtEndDate.Text %>' 
    TextMode="Date" CssClass="form-control" style="width: 160px; display:none;" EnableViewState="true" />
            <asp:Button ID="btnQuery" runat="server" Text="Query" CssClass="btn btn-primary" OnClick="btnQuery_Click" />
        </div>
        <div class="dashboard-grid" style="flex-grow: 1;">
            <div id="card1" class="dashboard-card">
                <h4>Top 5 Product Orders</h4>
                <div class="btn-group btn-group-sm mb-2" role="group" aria-label="SO Type Toggle">
                    <input type="radio" class="btn-check" name="soTypeToggle" id="btnShowSPO" autocomplete="off" checked>
                    <label class="btn btn-outline-light" for="btnShowSPO">SPO</label>

                    <input type="radio" class="btn-check" name="soTypeToggle" id="btnShowSTO" autocomplete="off">
                    <label class="btn btn-outline-light" for="btnShowSTO">STO</label>
                </div>
                <canvas id="orderModelChart" class="chart-box"></canvas>
            </div>
            <div id="card2" class="dashboard-card">
                <h4>Trial Order Completion Rate</h4>
                <div id="trialOrderProgress"></div>
            </div>
            <div id="card3" class="dashboard-card">
                <h4>WO Posting Daily Static</h4>
                <canvas id="opdTlfDailyChart" class="chart-box"></canvas>
            </div>
            <div id="card4" class="dashboard-card">
                <h4>Top 5 Product Shipment</h4>
                <div class="btn-group btn-group-sm mb-2" role="group" aria-label="SO Type Toggle">
                    <input type="radio" class="btn-check" name="soTypeToggleShip" id="btnShowSPOShip" autocomplete="off" checked>
                    <label class="btn btn-outline-light" for="btnShowSPOShip">SPO</label>

                    <input type="radio" class="btn-check" name="soTypeToggleShip" id="btnShowSTOShip" autocomplete="off">
                    <label class="btn btn-outline-light" for="btnShowSTOShip">STO</label>
                </div>
                <canvas id="bookingTimelineChart" class="chart-box"></canvas>
            </div>
            <div id="card5" class="dashboard-card">
                <h4>Department Capacity Statistics</h4>
                <div class="btn-group btn-group-sm mb-2" role="group" aria-label="WO Type Toggle">
                    <input type="radio" class="btn-check" name="woTypeToggle" id="btnShowSTODept" autocomplete="off" checked>
                    <label class="btn btn-outline-light" for="btnShowSTODept">STO</label>
                    <input type="radio" class="btn-check" name="woTypeToggle" id="btnShowSPODept" autocomplete="off">
                    <label class="btn btn-outline-light" for="btnShowSPODept">SPO</label>
                </div>
                <canvas id="deptCapacityChart" class="chart-box"></canvas>
            </div>
        </div>
    </div>
    <asp:HiddenField ID="hfDashboardData" runat="server" />
    <script>
        window.onload = function () {
            let raw = document.getElementById('<%= hfDashboardData.ClientID %>').value || '{}';
            let data = {};
            try { data = JSON.parse(raw); } catch (e) { console.error("Invalid JSON", e); }
            const chartInstances = {};


            function renderDashboard(data) {

                if (chartInstances.orderModelChart) { chartInstances.orderModelChart.destroy(); }
                if (chartInstances.bookingTimelineChart) { chartInstances.bookingTimelineChart.destroy(); }
                if (chartInstances.deptCapacityChart) { chartInstances.deptCapacityChart.destroy(); }
                if (chartInstances.opdTlfDailyChart) { chartInstances.opdTlfDailyChart.destroy(); }

                document.getElementById('trialOrderProgress').innerHTML = '';

                if (data.orderModel && (data.orderModel.spoModel || data.orderModel.stoModel)) {
                    function renderOrderChart(type) {
                        const chartData = type === 'SPO' ? data.orderModel.spoModel : data.orderModel.stoModel;
                        if (chartInstances.orderModelChart) {
                            chartInstances.orderModelChart.destroy();
                        }
                        chartInstances.orderModelChart = new Chart(document.getElementById('orderModelChart'), {
                            type: 'bar',
                            data: {
                                labels: chartData.labels,
                                datasets: chartData.datasets
                            },
                            options: {
                                maintainAspectRatio: false,
                                plugins: {
                                    legend: {
                                        display: true,
                                        position: 'top',
                                        labels: { color: 'white' }
                                    }
                                },
                                scales: {
                                    x: {
                                        stacked: true,
                                        ticks: { color: 'white' },
                                        grid: { color: 'rgba(255,255,255,0.2)' }
                                    },
                                    y: {
                                        stacked: true,
                                        beginAtZero: true,
                                        ticks: { color: 'white' },
                                        grid: { color: 'rgba(255,255,255,0.2)' }
                                    }
                                }
                            }
                        });
                    }

                    
                    renderOrderChart('SPO');

                    
                    document.getElementById('btnShowSPO').addEventListener('change', () => renderOrderChart('SPO'));
                    document.getElementById('btnShowSTO').addEventListener('change', () => renderOrderChart('STO'));
                }
                if (data.trialOrder && data.trialOrder.length > 0) {
                    let html = '';
                    data.trialOrder.slice(0, 5).forEach(row => {
                        let percent = parseFloat(row.CompletionRate) || 0;
                        html += `
                          <div class="mb-2 w-100">
                            <div style="margin-bottom: 0.25rem;"><b>${row.TrialBatchNo}</b></div>
                            <div class="progress">
                              <div class="progress-bar" role="progressbar" style="width: ${percent}%" aria-valuenow="${percent}" aria-valuemin="0" aria-valuemax="100">${percent}%</div>
                            </div>
                          </div>
                        `;
                    });
                    document.getElementById('trialOrderProgress').innerHTML = html;
                }
                if (data.booking && (data.booking.spoModel || data.booking.stoModel)) {
                    function renderShipmentChart(type) {
                        const chartData = type === 'SPO' ? data.booking.spoModel : data.booking.stoModel;
                        if (chartInstances.bookingTimelineChart) {
                            chartInstances.bookingTimelineChart.destroy();
                        }
                        chartInstances.bookingTimelineChart = new Chart(document.getElementById('bookingTimelineChart'), {
                            type: 'bar',
                            data: {
                                labels: chartData.labels,
                                datasets: chartData.datasets
                            },
                            options: {
                                maintainAspectRatio: false,
                                indexAxis: 'x',
                                responsive: true,
                                plugins: {
                                    legend: {
                                        display: true,
                                        position: 'top',
                                        labels: { color: 'white' }
                                    }
                                },
                                scales: {
                                    x: {
                                        title: { display: false },
                                        stacked: true,
                                        ticks: { color: 'white' },
                                        grid: { color: 'rgba(255,255,255,0.2)' }
                                    },
                                    y: {
                                        title: { display: false },
                                        beginAtZero: true,
                                        stacked: true,
                                        ticks: { color: 'white' },
                                        grid: { color: 'rgba(255,255,255,0.2)' }
                                    }
                                }
                            }
                        });
                    }
                    
                    document.getElementById('btnShowSPOShip').checked = false;
                    document.getElementById('btnShowSTOShip').checked = true;
                    renderShipmentChart('STO');
                    
                    document.getElementById('btnShowSPOShip').addEventListener('change', () => renderShipmentChart('SPO'));
                    document.getElementById('btnShowSTOShip').addEventListener('change', () => renderShipmentChart('STO'));
                }
                if (data.deptCapacity && data.deptCapacity.deptName && data.deptCapacity.deptName.length > 0) {
                    
                    function getDeptDataByType(type) {
                        const idx = data.deptCapacity.woType.map(x => (x || '').toUpperCase());
                        const filterIdx = idx.map((v, i) => v === type.toUpperCase() ? i : -1).filter(i => i !== -1);
                        return {
                            deptName: filterIdx.map(i => data.deptCapacity.deptName[i]),
                            woQty: filterIdx.map(i => data.deptCapacity.woQty[i]),
                            finishQty: filterIdx.map(i => data.deptCapacity.finishQty[i]),
                            woType: filterIdx.map(i => data.deptCapacity.woType[i]),
                            deptCode: filterIdx.map(i => data.deptCapacity.deptCode[i]),
                            woCount: filterIdx.map(i => data.deptCapacity.woCount[i])
                        };
                    }
                    
                    function renderDeptCapacity(type) {
                        const filtered = getDeptDataByType(type);
                        if (chartInstances.deptCapacityChart) chartInstances.deptCapacityChart.destroy();
                        chartInstances.deptCapacityChart = new Chart(document.getElementById('deptCapacityChart'), {
                            type: 'bar',
                            data: {
                                labels: filtered.deptName,
                                datasets: [
                                    { label: 'WO_QTY', data: filtered.woQty, backgroundColor: '#e67e22' },
                                    { label: 'FINISH_QTY', data: filtered.finishQty, backgroundColor: '#27ae60' }
                                ]
                            },
                            options: { maintainAspectRatio: false, plugins: { legend: { labels: { color: 'white' } } }, scales: { x: { ticks: { color: 'white' } }, y: { beginAtZero: true, ticks: { color: 'white' } } } }
                        });
                        
                        let tableHtml = `<div class="table-responsive"><table class="table table-sm table-dark table-bordered mt-2"><thead><tr><th>WO_TYPE</th><th>DEPT_CODE</th><th>DEPT_NAME</th><th>WO_COUNT</th><th>WO_QTY</th><th>FINISH_QTY</th></tr></thead><tbody>`;
                        for (let i = 0; i < filtered.deptName.length; i++) {
                            tableHtml += `<tr>` +
                                `<td>${filtered.woType[i]}</td>` +
                                `<td>${filtered.deptCode[i]}</td>` +
                                `<td>${filtered.deptName[i]}</td>` +
                                `<td>${filtered.woCount[i]}</td>` +
                                `<td>${filtered.woQty[i]}</td>` +
                                `<td>${filtered.finishQty[i]}</td>` +
                                `</tr>`;
                        }
                        tableHtml += `</tbody></table></div>`;
                        
                        const card5 = document.getElementById('card5');
                        let oldTable = card5.querySelector('table');
                        if (oldTable) oldTable.parentElement.remove();
                        card5.insertAdjacentHTML('beforeend', tableHtml);
                    }
                    
                    renderDeptCapacity('STO');
                    document.getElementById('btnShowSTODept').addEventListener('change', () => renderDeptCapacity('STO'));
                    document.getElementById('btnShowSPODept').addEventListener('change', () => renderDeptCapacity('SPO'));
                }
                if (data.opdTlfDaily && data.opdTlfDaily.labels && data.opdTlfDaily.labels.length > 0) {
                    chartInstances.opdTlfDailyChart = new Chart(document.getElementById('opdTlfDailyChart'), {
                        type: 'line',
                        data: {
                            labels: data.opdTlfDaily.labels,
                            datasets: [{ data: data.opdTlfDaily.data, backgroundColor: 'rgba(52,152,219,0.2)', borderColor: '#3498db', borderWidth: 2, fill: true, tension: 0.3, pointRadius: 4 }]
                        },
                        options: { maintainAspectRatio: false, responsive: true, plugins: { legend: { display: false } }, scales: { x: { ticks: { color: 'white' }, grid: { color: 'rgba(255,255,255,0.2)' } }, y: { beginAtZero: true, ticks: { color: 'white' }, grid: { color: 'rgba(255,255,255,0.2)' } } } }
                    });
                }
            }


            renderDashboard(data);

            
            const monthInput = document.getElementById('monthSelect');
            const txtStartDate = document.getElementById('<%= txtStartDate.ClientID %>');
            const txtEndDate = document.getElementById('<%= txtEndDate.ClientID %>');
            
            if (monthInput) {
                const today = new Date();
                const yyyy = today.getFullYear();
                const mm = (today.getMonth() + 1).toString().padStart(2, '0');
                //monthInput.value = `${yyyy}-${mm}`;
                setMonthRange(monthInput.value);
                monthInput.addEventListener('change', function () {
                    setMonthRange(this.value);
                });
            }
            function setMonthRange(ym) {
                if (!ym) return;
                const [y, m] = ym.split('-');
                const first = `${y}-${m}-01`;
                const last = new Date(y, m, 0).toISOString().slice(0, 10);
                if (txtStartDate) txtStartDate.value = first;
                if (txtEndDate) txtEndDate.value = last;
            }
            
            const btnQuery = document.getElementById('<%= btnQuery.ClientID %>');
            if (btnQuery) {
                btnQuery.addEventListener('click', function (e) {
                    setMonthRange(monthInput.value);
                });
            }

            const posClasses = ['pos-tl', 'pos-lb', 'pos-center', 'pos-tr', 'pos-rb'];
            const cards = [
                document.getElementById('card1'),
                document.getElementById('card2'),
                document.getElementById('card3'),
                document.getElementById('card4'),
                document.getElementById('card5')
            ];
            let currentPositions = ['pos-tl', 'pos-lb', 'pos-center', 'pos-tr', 'pos-rb'];
            function setLayout() {
                cards.forEach((card, i) => {
                    
                    posClasses.forEach(c => card.classList.remove(c));
                    
                    card.classList.add(currentPositions[i]);
                });
                setTimeout(() => {
                    for (const key in chartInstances) {
                        if (chartInstances[key]) { chartInstances[key].resize(); }
                    }
                }, 510);
            }
            cards.forEach((card, i) => {
                card.onclick = () => {
                    const clickedPos = currentPositions[i];
                    if (clickedPos === 'pos-center') return;
                    const centerIndex = currentPositions.indexOf('pos-center');
                    currentPositions[centerIndex] = clickedPos;
                    currentPositions[i] = 'pos-center';
                    setLayout();
                };
            });            
            const initialContentOrder = ['card1', 'card2', 'card3', 'card4', 'card5'];
            
            currentPositions[initialContentOrder.indexOf('card1')] = 'pos-tl';
            currentPositions[initialContentOrder.indexOf('card2')] = 'pos-lb';
            currentPositions[initialContentOrder.indexOf('card3')] = 'pos-center';
            currentPositions[initialContentOrder.indexOf('card4')] = 'pos-tr';
            currentPositions[initialContentOrder.indexOf('card5')] = 'pos-rb';
            setLayout();
        };

        document.addEventListener('DOMContentLoaded', function () {
            var someElementInside = document.getElementById('<%= hfDashboardData.ClientID %>');
            if (someElementInside) {
                var container = someElementInside.closest('.container.body-content');
                if (container) {
                    container.style.width = '100%';
                    container.style.maxWidth = '100%';
                    container.style.padding = '0';
                    container.style.margin = '0';
                    var hr = container.querySelector('hr');
                    var footer = container.querySelector('footer');
                    if (hr) hr.style.display = 'none';
                    if (footer) footer.style.display = 'none';
                }
            }
        });
    </script>
 <script>
     document.addEventListener("DOMContentLoaded", function () {
         const monthInput = document.getElementById("monthSelect");
         if (monthInput) {
             monthInput.value = '<%= DateTime.Parse(txtStartDate.Text).ToString("yyyy-MM") %>';
        }
    });
    </script>

</asp:Content> 