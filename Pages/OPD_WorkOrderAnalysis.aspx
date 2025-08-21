<%@ Page Title="OPD Work Order Analysis" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="OPD_WorkOrderAnalysis.aspx.cs" Inherits="IT_WorkPlant.Pages.OPD_WorkOrderAnalysis" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <style>
        body { background-color: #1e1e2f; color: white; }
        .dark-card { background-color: #3b4f63; border: 1px solid #3c4d5f; border-radius: 10px; color: white; margin-bottom: 20px; }
        h1 { color: #448cd4; text-align: center; margin-bottom: 30px; }
        .chart-container { height: 400px; position: relative; }
        .date-range-container { display: flex; gap: 15px; align-items: center; justify-content: center; margin-bottom: 30px; flex-wrap: wrap; }
        .date-input { background-color: #2c3e50; color: white; border: 1px solid #444; padding: 8px 12px; border-radius: 5px; }
        .btn-custom { background-color: #3498db; border: none; color: white; padding: 8px 20px; border-radius: 5px; cursor: pointer; }
        .btn-custom:hover { background-color: #2980b9; }
        .section-title { color: #f1c40f; margin-bottom: 10px; }
        canvas { font-size: 14px !important; color: white !important; }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container-fluid py-4">
        <h1>Work Order Analysis</h1>

        <!-- Cards -->
        <div class="row mb-4">
            <div class="col-md-3">
                <div class="card text-white bg-primary mb-3">
                    <div class="card-body">
                        <h6 class="card-title">Avg. Completion Rate</h6>
                        <h3 id="cardAvgRate">--</h3>
                    </div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card text-white bg-danger mb-3">
                    <div class="card-body">
                        <h6 class="card-title">Unfinished WO</h6>
                        <h3 id="cardUnfinishedWO">--</h3>
                    </div>
                </div>
            </div>
            <div class="col-md-6">
                <div class="card text-white bg-warning mb-3">
                    <div class="card-body">
                        <h6 class="card-title">Top Risk PO</h6>
                        <h5 id="cardTopRiskPO">--</h5>
                        <div id="cardTopRiskDetail"></div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Search Filters -->
        <div class="dark-card p-3 mb-4">
            <div class="section-title">Search Filters</div>
            <div class="row">
                <div class="col-md-2">
                    <label>Start Date</label>
                    <asp:TextBox ID="txtStartDate" runat="server" TextMode="Date" CssClass="form-control" />
                </div>
                <div class="col-md-2">
                    <label>End Date</label>
                    <asp:TextBox ID="txtEndDate" runat="server" TextMode="Date" CssClass="form-control" />
                </div>
                <div class="col-md-2">
                    <label>Top N</label>
                    <asp:TextBox ID="txtTopN" runat="server" CssClass="form-control" Text="20" />
                </div>
                <div class="col-md-2">
                    <label>Max Completion %</label>
                    <asp:TextBox ID="txtMaxRate" runat="server" CssClass="form-control" Text="100" />
                </div>
                <div class="col-md-2">
                    <label>PO Keyword</label>
                    <asp:TextBox ID="txtPOKeyword" runat="server" CssClass="form-control" />
                </div>
                <div class="col-md-2">
                    <label>Top N Sort</label>
                    <asp:DropDownList ID="ddlTopNSort" runat="server" CssClass="form-control">
                        <asp:ListItem Value="unfinished">Max Unfinished Qty</asp:ListItem>
                        <asp:ListItem Value="lowrate">Lowest Completion Rate</asp:ListItem>
                    </asp:DropDownList>
                </div>
            </div>
            <div class="row mt-3">
                <div class="col-md-2">
                    <asp:Button ID="btnSearchPO" runat="server" Text="Search" CssClass="btn btn-primary w-100" OnClick="btnSearchPO_Click" />
                </div>
            </div>
        </div>

        <!-- Tabs -->
        <ul class="nav nav-tabs mb-3" id="mainTab" role="tablist">
            <li class="nav-item" role="presentation">
                <button class="nav-link active" id="tab-distribution" data-bs-toggle="tab" data-bs-target="#tabDist" type="button" role="tab">Distribution</button>
            </li>
            <li class="nav-item" role="presentation">
                <button class="nav-link" id="tab-topn" data-bs-toggle="tab" data-bs-target="#tabTopN" type="button" role="tab">Top/Bottom N</button>
            </li>
        </ul>
        <div class="tab-content mb-4">
            <div class="tab-pane fade show active" id="tabDist" role="tabpanel">
                <div class="dark-card p-3 mb-4">
                    <div class="section-title">PO Completion Rate Distribution</div>
                    <div class="chart-container">
                        <canvas id="chartBucketDist"></canvas>
                    </div>
                </div>
            </div>
            <div class="tab-pane fade" id="tabTopN" role="tabpanel">
                <div class="dark-card p-3 mb-4">
                    <div class="section-title">Top/Bottom N PO</div>
                    <div class="chart-container">
                        <canvas id="chartTopN"></canvas>
                    </div>
                </div>
            </div>
        </div>

        <!-- All PO Data -->
        <div class="dark-card p-3 mb-4">
            <div class="section-title">All PO Data</div>
            <asp:GridView ID="gvAllPO" runat="server" CssClass="table table-bordered table-striped" AutoGenerateColumns="true" AllowPaging="true" PageSize="10" OnPageIndexChanging="gvAllPO_PageIndexChanging"></asp:GridView>
        </div>

        <!-- WO Detail -->
        <div class="dark-card p-3 mb-4">
            <div class="section-title">Work Order Detail Search</div>
            <div class="row">
                <div class="col-md-4">
                    <label>Work Order No</label>
                    <asp:TextBox ID="txtWONo" runat="server" CssClass="form-control" placeholder="Enter WO number to search"></asp:TextBox>
                </div>
                <div class="col-md-2 d-flex align-items-end">
                    <asp:Button ID="btnSearchWO" runat="server" Text="Search WO" CssClass="btn btn-primary w-100" OnClick="btnSearchWO_Click" />
                </div>
            </div>
            <div class="row mt-3">
                <div class="col-md-6">
                    <div class="chart-container">
                        <canvas id="chartWOGauge"></canvas>
                    </div>
                </div>
                <div class="col-md-6">
                    <div id="woDetailText" style="margin-top: 20px;"></div>
                </div>
            </div>
        </div>

        <!-- PO WO List -->
        <div class="dark-card p-3 mb-4">
            <div class="section-title">PO Work Order List</div>
            <div class="row mb-3">
                <div class="col-md-4">
                    <label>PO Number</label>
                    <asp:TextBox ID="txtPONo" runat="server" CssClass="form-control" placeholder="Enter PO number to search" />
                </div>
                <div class="col-md-2 d-flex align-items-end">
                    <asp:Button ID="btnSearchPOWO" runat="server" Text="Search PO WO" CssClass="btn btn-primary w-100" OnClick="btnSearchPOWO_Click" />
                </div>
            </div>

            <!-- กราฟโดนัทสรุป -->
            <div class="chart-container" style="height:260px;">
                <canvas id="chartPOWOList"></canvas>
            </div>

            <!-- Legend แบบกดได้ -->
            <div id="poLegend" class="text-center mt-3"></div>

            <!-- Inline fallback (กรณีไม่มี Bootstrap JS) -->
            <div id="woInlinePanel" class="mt-3 d-none">
                <div class="table-responsive" style="max-height:420px;overflow:auto;">
                    <table class="table table-dark table-striped table-sm align-middle mb-0">
                        <thead>
                            <tr>
                                <th style="min-width:120px;">WO No</th>
                                <th>Item</th>
                                <th class="text-end">Plan</th>
                                <th class="text-end">Done</th>
                                <th class="text-end">Rate %</th>
                                <th style="min-width:120px;">Last Update</th>
                            </tr>
                        </thead>
                        <tbody id="woInlineBody"></tbody>
                    </table>
                </div>
            </div>

            <!-- Modal แสดงรายการ WO -->
            <div class="modal fade" id="woListModal" tabindex="-1" aria-hidden="true">
              <div class="modal-dialog modal-lg modal-dialog-centered">
                <div class="modal-content" style="background:#2f3e50;color:#fff;">
                  <div class="modal-header">
                    <h5 class="modal-title" id="woListTitle">WO List</h5>
                    <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal" aria-label="Close"></button>
                  </div>
                  <div class="modal-body">
                    <div id="woCounts" class="mb-2 small text-secondary"></div>
                    <div class="table-responsive" style="max-height:420px;overflow:auto;">
                      <table class="table table-dark table-striped table-sm align-middle mb-0">
                        <thead>
                          <tr>
                            <th style="min-width:120px;">WO No</th>
                            <th>Item</th>
                            <th class="text-end">Plan</th>
                            <th class="text-end">Done</th>
                            <th class="text-end">Rate %</th>
                            <th style="min-width:120px;">Last Update</th>
                          </tr>
                        </thead>
                        <tbody id="woListBody"></tbody>
                      </table>
                    </div>
                  </div>
                  <div class="modal-footer">
                    <span class="me-auto small text-secondary">* คลิกปุ่มสีที่ด้านบนเพื่อสลับรายการ</span>
                    <button type="button" class="btn btn-outline-light" data-bs-dismiss="modal">Close</button>
                  </div>
                </div>
              </div>
            </div>
        </div>

        <!-- Hidden fields -->
        <asp:HiddenField ID="hfSummaryData" runat="server" />
        <asp:HiddenField ID="hfBucketDistData" runat="server" />
        <asp:HiddenField ID="hfTopNData" runat="server" />
        <asp:HiddenField ID="hfAllPOData" runat="server" />
        <asp:HiddenField ID="hfWODetailData" runat="server" />
        <asp:HiddenField ID="hfPOWOListData" runat="server" />
        <asp:HiddenField ID="hfDrillPONo" runat="server" />
    </div>

    <script type="text/javascript">
        window.onload = function () {
            // ===== Summary cards =====
            var summaryRaw = document.getElementById('<%= hfSummaryData.ClientID %>').value;
            var summary = {};
            try { summary = JSON.parse(summaryRaw); } catch (e) { summary = {}; }
            document.getElementById('cardAvgRate').innerText = summary.avgRate !== undefined ? summary.avgRate + '%' : '--';
            document.getElementById('cardUnfinishedWO').innerText = summary.unfinishedWO !== undefined ? summary.unfinishedWO : '--';
            document.getElementById('cardTopRiskPO').innerText = summary.topRiskPO || '--';
            document.getElementById('cardTopRiskDetail').innerText = summary.topRiskDetail || '';

            // ===== Distribution doughnut =====
            var bucketRaw = document.getElementById('<%= hfBucketDistData.ClientID %>').value;
            var bucketData = [];
            try { bucketData = JSON.parse(bucketRaw); } catch (e) { bucketData = []; }
            var distLabels = bucketData.map(x => x.bucket);
            var distValues = bucketData.map(x => x.count);
            var distColors = ['#e74c3c', '#f1c40f', '#3498db', '#27ae60'];
            var ctxDist = document.getElementById('chartBucketDist').getContext('2d');
            new Chart(ctxDist, {
                type: 'doughnut',
                data: { labels: distLabels, datasets: [{ data: distValues, backgroundColor: distColors }] },
                options: { plugins: { legend: { display: true }, tooltip: { enabled: true } } }
            });

            // ===== Top/Bottom N bar =====
            var topNRaw = document.getElementById('<%= hfTopNData.ClientID %>').value;
            var topNData = [];
            try { topNData = JSON.parse(topNRaw); } catch (e) { topNData = []; }
            var topNLabels = topNData.map(x => x.PO_NO);
            var topNValues = topNData.map(x => x.COMPLETION_RATE);
            var topNColors = topNValues.map(v => v >= 85 ? '#27ae60' : (v >= 60 ? '#f1c40f' : '#e74c3c'));
            var ctxTopN = document.getElementById('chartTopN').getContext('2d');
            var chartTopN = new Chart(ctxTopN, {
                type: 'bar',
                data: { labels: topNLabels, datasets: [{ label: 'Completion Rate (%)', data: topNValues, backgroundColor: topNColors }] },
                options: {
                    onClick: function (evt, elements) {
                        if (elements.length > 0) {
                            var idx = elements[0].index;
                            var poNo = topNLabels[idx];
                            document.getElementById('<%= hfDrillPONo.ClientID %>').value = poNo;
                            document.getElementById('<%= txtPONo.ClientID %>').value = poNo;
                            document.getElementById('<%= btnSearchPOWO.ClientID %>').click();
                        }
                    },
                    plugins: { legend: { display: false }, tooltip: { callbacks: { label: c => 'Completion: ' + c.parsed.y + '%' } } },
                    scales: { y: { beginAtZero: true, max: 100 } }
                }
            });

            // ===== WO gauge (half doughnut) =====
            var rawWO = document.getElementById('<%= hfWODetailData.ClientID %>').value;
            var dataWO = [];
            try { dataWO = JSON.parse(rawWO); } catch (e) { dataWO = []; }
            if (dataWO && dataWO.length > 0) {
                var wo = dataWO[0];
                var ctxGauge = document.getElementById('chartWOGauge').getContext('2d');
                new Chart(ctxGauge, {
                    type: 'doughnut',
                    data: {
                        labels: ['Completed', 'Remaining'],
                        datasets: [{
                            data: [wo.COMPLETION_RATE, 100 - wo.COMPLETION_RATE],
                            backgroundColor: [ wo.COMPLETION_RATE >= 85 ? '#27ae60' : (wo.COMPLETION_RATE >= 60 ? '#f1c40f' : '#e74c3c'), '#e0e0e0' ],
                            borderWidth: 0
                        }]
                    },
                    options: {
                        rotation: -90, circumference: 180, cutout: '70%',
                        plugins: { legend: { display: false }, tooltip: { callbacks: { label: c => c.label + ': ' + c.parsed + '%' } } }
                    }
                });
                var detail =
                    `<b>WO No:</b> ${wo.WO_NO}<br>` +
                    `<b>PO No:</b> ${wo.PO_NO}<br>` +
                    `<b>Product No:</b> ${wo.PRODUCT_NO}<br>` +
                    `<b>Product Name:</b> ${wo.PRODUCT_NAME}<br>` +
                    `<b>Plan Qty:</b> ${wo.PLAN_QTY}<br>` +
                    `<b>Finish Qty:</b> ${wo.FINISH_QTY}<br>` +
                    `<b>Completion Rate:</b> ${wo.COMPLETION_RATE}%<br>` +
                    `<b>Plan Finish Date:</b> ${wo.PLAN_FINISH_DATE}<br>` +
                    `<b>Actual Finish Date:</b> ${wo.ACTUAL_FINISH_DATE}`;
                document.getElementById('woDetailText').innerHTML = detail;
            } else {
                document.getElementById('woDetailText').innerHTML = '';
            }

            // ===== PO Work Order List → Doughnut summary + รายการ WO แยกสถานะ =====
            var rawPOWO = document.getElementById('<%= hfPOWOListData.ClientID %>').value;
            var dataPOWO = [];
            try { dataPOWO = JSON.parse(rawPOWO); } catch (e) { dataPOWO = []; }

            // helpers
            function V(r, keys, d) { for (let k of keys) if (r[k] !== undefined && r[k] !== null && r[k] !== '') return r[k]; return d; }
            function isCompletedRow(r) {
                var status = String(V(r, ['STATUS','WoStatus','wo_status'], '')).toLowerCase();
                if (status) return status.includes('complete') || ['done','closed','finish','finished'].includes(status);
                var rate = Number(V(r, ['COMPLETION_RATE','CompletionRate','Rate'], NaN));
                if (!isNaN(rate)) return rate >= 100;
                var plan = Number(V(r, ['PLAN_QTY','PlanQty','PLAN'], 0));
                var done = Number(V(r, ['FINISH_QTY','FinishQty','DONE','FINISH'], 0));
                return plan > 0 && done >= plan;
            }
            function esc(s){ return (s==null?'':String(s)).replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;').replace(/'/g,'&#039;'); }
            function num(n){ return (n===null||n===undefined||n==='')?'':Number(n).toLocaleString(); }
            function rateNum(r){ if (r===null||r===undefined||isNaN(Number(r))) return ''; return Math.round(Number(r)) + '%'; }

            if (dataPOWO && dataPOWO.length > 0) {
                var valuesPOWO = dataPOWO.map(function (r) {
                    var v = Number(r.COMPLETION_RATE || r.completion_rate || r.CompletionRate || 0);
                    if (isNaN(v)) v = 0;
                    return Math.max(0, Math.min(100, v));
                });

                var completedCount  = dataPOWO.filter(isCompletedRow).length;
                var inprogressCount = dataPOWO.length - completedCount;

                var avg = valuesPOWO.length ? Math.round(valuesPOWO.reduce(function (a, b) { return a + b; }, 0) / valuesPOWO.length) : 0;

                var canvasPOWO = document.getElementById('chartPOWOList');
                canvasPOWO.height = 260;
                var ctxPOWO = canvasPOWO.getContext('2d');

                if (window._powoChart) { window._powoChart.destroy(); }
                window._powoChart = new Chart(ctxPOWO, {
                    type: 'doughnut',
                    data: { labels: ['Completed', 'In Progress'], datasets: [{ data: [completedCount, inprogressCount], backgroundColor: ['#2ecc71', '#f39c12'], borderWidth: 0 }] },
                    options: {
                        responsive: true, maintainAspectRatio: false, cutout: '60%',
                        plugins: {
                            legend: { display: false },
                            title: { display: true, text: 'Avg Completion: ' + avg + '%' },
                            tooltip: { callbacks: { label: ctx => ctx.label + ': ' + ctx.parsed + ' WO' } }
                        }
                    }
                });

                // Legend ปุ่มคลิกเพื่อเปิดลิสต์
                var legend = document.getElementById('poLegend');
                var btnsHtml = `
                    <div class="d-inline-flex gap-3">
                      <button type="button" class="btn btn-sm" id="btnShowCompleted"
                              style="background:#2ecc71;color:#0b2e13;border:1px solid #1d9f57;">
                        Completed (${completedCount} WO)
                      </button>
                      <button type="button" class="btn btn-sm" id="btnShowProgress"
                              style="background:#f39c12;color:#3a2500;border:1px solid #c5810f;">
                        In&nbsp;Progress (${inprogressCount} WO)
                      </button>
                    </div>`;
                legend.innerHTML = btnsHtml;

                var completedRows  = dataPOWO.filter(isCompletedRow);
                var inprogressRows = dataPOWO.filter(function(r){ return !isCompletedRow(r); });

                function fillRows(tbody, rows){
                    tbody.innerHTML = '';
                    rows.forEach(function(r){
                        var wo   = V(r, ['WO_NO','WONO','WO','WorkOrder','SFB01'], '-');
                        var item = V(r, ['ITEM','ITEM_NO','ItemName','IMA01','ITEM_NAME','PRODUCT_NAME'], '-');
                        var plan = Number(V(r, ['PLAN_QTY','PlanQty','PLAN'], ''));
                        var done = Number(V(r, ['FINISH_QTY','FinishQty','DONE','FINISH'], ''));
                        var rate = (Number.isFinite(plan) && plan>0) ? ((Number(done)/Number(plan))*100) : Number(V(r, ['COMPLETION_RATE','CompletionRate','Rate'], NaN));
                        var upd  = V(r, ['LAST_UPDATE','UpdateTime','UPDATED_AT','MODI_DATE'], '');
                        var tr = document.createElement('tr');
                        tr.innerHTML =
                          `<td>${esc(wo)}</td>
                           <td>${esc(item)}</td>
                           <td class="text-end">${num(plan)}</td>
                           <td class="text-end">${num(done)}</td>
                           <td class="text-end">${rateNum(rate)}</td>
                           <td>${esc(upd)}</td>`;
                        /* ★★★ เพิ่ม 3 บรรทัดให้คลิกแถวแล้วเปิด WO Detail ★★★ */
                        tr.style.cursor = 'pointer';
                        tr.setAttribute('role','button');
                        tr.addEventListener('click', function(){ document.getElementById('<%= txtWONo.ClientID %>').value = String(wo).trim(); document.getElementById('<%= btnSearchWO.ClientID %>').click(); (window.bootstrap && bootstrap.Modal.getInstance(document.getElementById('woListModal')))?.hide(); });
                        /* ★★★ จบ 3 บรรทัด ★★★ */
                        tbody.appendChild(tr);
                    });
                }

                function openList(title, rows){
                    var modalEl = document.getElementById('woListModal');
                    var hasBootstrap = window.bootstrap && bootstrap.Modal;
                    if (hasBootstrap) {
                        document.getElementById('woListTitle').textContent = 'WO – ' + title;
                        document.getElementById('woCounts').textContent = 'Total ' + rows.length + ' WO';
                        fillRows(document.getElementById('woListBody'), rows);
                        bootstrap.Modal.getOrCreateInstance(modalEl).show();
                    } else {
                        // fallback inline
                        document.getElementById('woInlinePanel').classList.remove('d-none');
                        fillRows(document.getElementById('woInlineBody'), rows);
                        var inline = document.getElementById('woInlinePanel');
                        window.scrollTo({ top: inline.offsetTop - 80, behavior: 'smooth' });
                    }
                }

                // กันโพสต์แบ็ก
                document.getElementById('btnShowCompleted')?.addEventListener('click', function(e){
                    e.preventDefault(); e.stopPropagation();
                    openList('Completed', completedRows);
                });
                document.getElementById('btnShowProgress')?.addEventListener('click', function(e){
                    e.preventDefault(); e.stopPropagation();
                    openList('In Progress', inprogressRows);
                });

            } else {
                if (window._powoChart) { window._powoChart.destroy(); window._powoChart = null; }
                document.getElementById('poLegend').innerHTML = '';
                document.getElementById('woInlinePanel').classList.add('d-none');
            }
        };

        if (window.jQuery) {
            $(document).ready(function () {
                var allPORaw = document.getElementById('<%= hfAllPOData.ClientID %>').value;
                var allPOData = [];
                try { allPOData = JSON.parse(allPORaw); } catch (e) { allPOData = []; }
                console.log('AllPOData:', allPOData);
                if (allPOData.length > 0) console.log('First row:', allPOData[0]);
                else console.log('AllPOData is empty!');
            });
        }
    </script>
</asp:Content>
