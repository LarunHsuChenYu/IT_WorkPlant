<%@ Page Language="C#" AutoEventWireup="true"
    CodeBehind="QMS_Dashboard.aspx.cs"
    Inherits="IT_WorkPlant.Pages.QMS_Dashboard"
    MasterPageFile="~/Pages/QMS.Master"
    Title="Dashboard" %>

<asp:Content ID="Head1" ContentPlaceHolderID="head" runat="server">
  <!-- Chart.js (ถ้า QMS.Master ยังไม่ได้อ้าง) -->
  <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.1"></script>

  <style>
    /* การ์ด + เฮดเดอร์ */
    .qms-card{background:var(--card);border:1px solid var(--border);border-radius:var(--radius);box-shadow:0 1px 0 rgba(0,0,0,.02)}
    .qms-card .title{font-weight:700}
    .section-head{font-weight:700;margin-bottom:.5rem}

    /* KPI */
    .kpi .value{font-size:2.2rem;line-height:1;font-weight:800}
    .kpi .sub{color:var(--muted-foreground);font-size:.9rem}
    .kpi .icon{width:36px;height:36px;border-radius:999px;display:inline-grid;place-items:center;background:var(--secondary)}
    .kpi .icon.warn{background:rgba(255,196,0,.18)}
    .kpi .icon.ok{background:rgba(25,135,84,.18)}
    .kpi .icon.avg{background:rgba(134,93,255,.15)}

    /* Filters */
    .filters .label{font-weight:600}

    /* Chart wrapper + ความสูงคงที่ */
    .chart-card .header{font-weight:600;margin-bottom:.5rem}
    .chart-wrap{position:relative;width:100%}
    .h-line{height:320px}
    .h-bar{height:320px}
    .h-pie{height:280px}
    @media (max-width: 1199.98px){
      .h-line,.h-bar{height:280px}
      .h-pie{height:240px}
    }
    @media (max-width: 767.98px){
      .h-line,.h-bar{height:240px}
      .h-pie{height:220px}
    }

    /* ตาราง summary */
    .table.qms-table> :not(caption)>*>*{vertical-align:middle}
    .badge-chip{border-radius:999px;padding:.25rem .6rem;font-size:.8rem}
    .chip-high{background:#fee2e2;color:#b91c1c}
    .chip-med {background:#fef9c3;color:#a16207}
    .chip-low {background:#dcfce7;color:#166534}
  </style>
</asp:Content>

<asp:Content ID="Body1" ContentPlaceHolderID="MainContent" runat="server">

  <h2 class="fw-bold mb-3">Quality Statistics Dashboard</h2>

  <!-- ปุ่มมุมขวา -->
  <div class="d-flex gap-2 justify-content-end mb-2">
    <asp:Button ID="btnExport" runat="server" CssClass="btn btn-outline-secondary" Text="Print / Export" OnClick="btnRefresh_Click" />
    <asp:Button ID="btnRefresh" runat="server" CssClass="btn btn-outline-secondary" Text="Refresh Data" OnClick="btnRefresh_Click" />
  </div>

  <!-- Filters -->
  <div class="qms-card p-3 mb-3 filters">
    <div class="d-flex align-items-center gap-2 mb-3">
      <i class="bi bi-funnel"></i><span class="label">Filters</span>
    </div>
    <div class="row g-3 align-items-end">
      <div class="col-12 col-md-3">
        <label class="form-label">Date From</label>
        <asp:TextBox ID="txtFrom" runat="server" CssClass="form-control" placeholder="yyyy-mm-dd"></asp:TextBox>
      </div>
      <div class="col-12 col-md-3">
        <label class="form-label">Date To</label>
        <asp:TextBox ID="txtTo" runat="server" CssClass="form-control" placeholder="yyyy-mm-dd"></asp:TextBox>
      </div>
      <div class="col-12 col-md-3">
        <label class="form-label">Production Unit</label>
        <asp:DropDownList ID="ddlUnit" runat="server" CssClass="form-select"></asp:DropDownList>
      </div>
      <div class="col-12 col-md-3">
        <label class="form-label">Defect Type</label>
        <asp:DropDownList ID="ddlDefect" runat="server" CssClass="form-select"></asp:DropDownList>
      </div>
      <div class="col-12 d-flex gap-2 mt-1">
        <asp:Button ID="btnApply" runat="server" CssClass="btn btn-dark" Text="Apply" OnClick="btnApply_Click" />
        <asp:Button ID="btnReset" runat="server" CssClass="btn btn-outline-secondary" Text="Reset" OnClick="btnReset_Click" />
      </div>
    </div>
  </div>

  <!-- KPIs -->
  <div class="row g-3 kpi mb-2">
    <div class="col-12 col-lg-3">
      <div class="qms-card p-3 d-flex justify-content-between align-items-center">
        <div>
          <div class="text-muted">Total Issues</div>
          <div class="value"><asp:Literal ID="litTotalIssues" runat="server" /></div>
          <div class="sub"><i class="bi bi-graph-up-arrow text-danger me-1"></i>vs last period</div>
        </div>
        <div class="icon"><i class="bi bi-exclamation-triangle"></i></div>
      </div>
    </div>
    <div class="col-12 col-lg-3">
      <div class="qms-card p-3 d-flex justify-content-between align-items-center">
        <div>
          <div class="text-muted">Open</div>
          <div class="value"><asp:Literal ID="litOpenIssues" runat="server" /></div>
          <div class="sub">Pending resolution</div>
        </div>
        <div class="icon warn"><i class="bi bi-alarm"></i></div>
      </div>
    </div>
    <div class="col-12 col-lg-3">
      <div class="qms-card p-3 d-flex justify-content-between align-items-center">
        <div>
          <div class="text-muted">Completed</div>
          <div class="value"><asp:Literal ID="litCompleted" runat="server" /></div>
          <div class="sub">Resolved items</div>
        </div>
        <div class="icon ok"><i class="bi bi-check2-circle"></i></div>
      </div>
    </div>
    <div class="col-12 col-lg-3">
      <div class="qms-card p-3 d-flex justify-content-between align-items-center">
        <div>
          <div class="text-muted">Avg. Downtime (min)</div>
          <div class="value"><asp:Literal ID="litAvgDowntime" runat="server" /></div>
          <div class="sub">per issue</div>
        </div>
        <div class="icon avg"><i class="bi bi-stopwatch"></i></div>
      </div>
    </div>
  </div>

  <!-- Charts row 1 -->
  <div class="row g-3 mb-3">
    <div class="col-12 col-xl-7">
      <div class="qms-card p-3 chart-card">
        <div class="header">Issues by Week (Trend)</div>
        <div class="chart-wrap h-line"><canvas id="trendChart"></canvas></div>
      </div>
    </div>
    <div class="col-12 col-xl-5">
      <div class="qms-card p-3 chart-card">
        <div class="header">Defect Type Distribution</div>
        <div class="chart-wrap h-pie"><canvas id="defectChart"></canvas></div>
      </div>
    </div>
  </div>

  <!-- Charts row 2 -->
  <div class="row g-3 mb-3">
    <div class="col-12 col-xl-7">
      <div class="qms-card p-3 chart-card">
        <div class="header">Issues by Department</div>
        <div class="chart-wrap h-line"><canvas id="deptLineChart"></canvas></div>
      </div>
    </div>
    <div class="col-12 col-xl-5">
      <div class="qms-card p-3 chart-card">
        <div class="header">Downtime by Department</div>
        <div class="chart-wrap h-bar"><canvas id="downtimeBarChart"></canvas></div>
      </div>
    </div>
  </div>

  <!-- Department Performance Summary -->
  <div class="qms-card p-3 mb-4">
    <div class="title mb-2">Department Performance Summary</div>
    <div class="table-responsive">
      <asp:GridView ID="gvDeptPerf" runat="server" CssClass="table qms-table"
        GridLines="None" AutoGenerateColumns="False" ShowHeaderWhenEmpty="True">
        <Columns>
          <asp:BoundField DataField="Department" HeaderText="Department" />
          <asp:BoundField DataField="TotalIssues" HeaderText="Total Issues" />
          <asp:BoundField DataField="AvgWeek" HeaderText="Avg/Week" />
          <asp:TemplateField HeaderText="Total Downtime">
            <ItemTemplate><%# String.Format("{0:0} min", Eval("TotalDowntimeMin")) %></ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Avg Downtime/Issue">
            <ItemTemplate><%# String.Format("{0:0} min", Eval("AvgDowntimePerIssueMin")) %></ItemTemplate>
          </asp:TemplateField>
          <asp:TemplateField HeaderText="Status">
            <ItemTemplate><%# GetStatusChip(Eval("Status")) %></ItemTemplate>
          </asp:TemplateField>
        </Columns>
      </asp:GridView>
    </div>
  </div>

  <!-- JSON payload (จาก code-behind) -->
  <script id="dashboard-json" type="application/json"><asp:Literal ID="litDashboardJson" runat="server" /></script>
</asp:Content>

<asp:Content ID="Scripts1" ContentPlaceHolderID="BodyScripts" runat="server">
  <script>
      function readPayload() {
          const el = document.getElementById('dashboard-json');
          try { return JSON.parse(el.textContent || '{}'); } catch (e) { return {}; }
      }

      function mkLine(ctx, data, opts) {
          return new Chart(ctx, {
              type: 'line',
              data: data,
              options: Object.assign({
                  responsive: true, maintainAspectRatio: false,
                  interaction: { mode: 'index', intersect: false },
                  plugins: { legend: { display: true } },
                  scales: {
                      x: { grid: { display: false } },
                      y: { grid: { color: 'rgba(0,0,0,.06)' }, ticks: { precision: 0 } }
                  }
              }, opts || {})
          });
      }
      function mkDoughnut(ctx, data) {
          return new Chart(ctx, {
              type: 'doughnut',
              data: data,
              options: {
                  responsive: true, maintainAspectRatio: false,
                  cutout: '60%',
                  plugins: { legend: { position: 'bottom' } }
              }
          });
      }
      function mkBar(ctx, data) {
          return new Chart(ctx, {
              type: 'bar',
              data: data,
              options: {
                  responsive: true, maintainAspectRatio: false,
                  plugins: { legend: { position: 'bottom' } },
                  scales: {
                      x: { grid: { display: false } },
                      y: { grid: { color: 'rgba(0,0,0,.06)' }, ticks: { precision: 0 } }
                  }
              }
          });
      }

      document.addEventListener('DOMContentLoaded', () => {
          const p = readPayload();

          // Trend
          const labels = (p.weekly || []).map(x => x.Week);
          const issues = (p.weekly || []).map(x => x.Issues);
          const target = (p.weekly || []).map(x => x.Target);
          const resolved = (p.weekly || []).map(x => x.Resolved);

          mkLine(document.getElementById('trendChart'), {
              labels,
              datasets: [
                  { label: 'Actual Issues', data: issues, borderColor: '#6f6cf6', backgroundColor: 'rgba(111,108,246,.15)', tension: .35, pointRadius: 2, borderWidth: 2 },
                  { label: 'Target', data: target, borderColor: '#77c3a0', backgroundColor: 'transparent', tension: .35, pointRadius: 2, borderWidth: 2, borderDash: [6, 6] },
                  { label: 'Resolved', data: resolved, borderColor: '#f6a663', backgroundColor: 'rgba(246,166,99,.15)', tension: .35, pointRadius: 2, borderWidth: 2 }
              ]
          });

          // Defect pie
          const dLabels = (p.defects || []).map(x => x.Name);
          const dData = (p.defects || []).map(x => x.Value);
          mkDoughnut(document.getElementById('defectChart'), {
              labels: dLabels,
              datasets: [{
                  data: dData,
                  backgroundColor: ['#6f6cf6', '#6dd6cf', '#f6c453', '#f59e0b', '#22c55e'],
                  borderWidth: 1
              }]
          });

          // Issues by department (multi-line)
          const dl = (p.deptTrend || []);
          const depts = {
              'Quality Control': dl.map(x => x.QualityControl),
              'Production': dl.map(x => x.Production),
              'Assembly': dl.map(x => x.Assembly),
              'Maintenance': dl.map(x => x.Maintenance)
          };
          mkLine(document.getElementById('deptLineChart'), {
              labels: dl.map(x => x.Week),
              datasets: [
                  { label: 'Quality Control', data: depts['Quality Control'], borderColor: '#8b8bf8', backgroundColor: 'transparent', tension: .35, pointRadius: 2, borderWidth: 2 },
                  { label: 'Production', data: depts['Production'], borderColor: '#57c3a7', backgroundColor: 'transparent', tension: .35, pointRadius: 2, borderWidth: 2 },
                  { label: 'Assembly', data: depts['Assembly'], borderColor: '#f6a663', backgroundColor: 'transparent', tension: .35, pointRadius: 2, borderWidth: 2 },
                  { label: 'Maintenance', data: depts['Maintenance'], borderColor: '#f87171', backgroundColor: 'transparent', tension: .35, pointRadius: 2, borderWidth: 2 }
              ]
          });

          // Downtime bar
          mkBar(document.getElementById('downtimeBarChart'), {
              labels: (p.downtime || []).map(x => x.Department),
              datasets: [
                  { label: 'Downtime (min)', data: (p.downtime || []).map(x => x.Downtime), backgroundColor: '#8b8bf8' },
                  { label: 'Issues Count', data: (p.downtime || []).map(x => x.Issues), backgroundColor: '#22c55e' }
              ]
          });
      });
  </script>
</asp:Content>
