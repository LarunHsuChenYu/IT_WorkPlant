using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;


namespace IT_WorkPlant.Pages
{
    public partial class IT_PingDashboard : System.Web.UI.Page
    {
        private static Dictionary<string, bool> lastStatus = new Dictionary<string, bool>();

        // ====== ฟิลเตอร์โหมด ======
        private enum FilterMode { All, Online, Offline }
        private FilterMode CurrentFilter
        {
            get => ViewState["Filter"] == null ? FilterMode.All : (FilterMode)ViewState["Filter"];
            set => ViewState["Filter"] = value;
        }
        // ===========================

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                RenderDashboard();
        }

        protected void PingTimer_Tick(object sender, EventArgs e)
        {
            RenderDashboard();
        }

        private void RenderDashboard()
        {
            pnlStatus.Controls.Clear();

            // 1) โหลดทั้งหมด
            var serversAll = LoadServersFromDB();

            // 2) Ping แบบขนาน (เร็ว ไม่ค้างหน้า)
            var pingMap = new ConcurrentDictionary<string, bool>();
            int onlineAll = 0, offlineAll = 0;

            Parallel.ForEach(
                serversAll,
                new ParallelOptions { MaxDegreeOfParallelism = 12 },   // ปรับตามกำลังเครื่อง
                s =>
                {
                    bool ok = IsPingSuccessful(s.IP);
                    pingMap[s.IP] = ok;
                    if (ok) Interlocked.Increment(ref onlineAll);
                    else Interlocked.Increment(ref offlineAll);

                    // แจ้งเตือนตอนสลับสถานะ (ถ้าจะใช้)
                    if (lastStatus.TryGetValue(s.IP, out bool wasOn) && wasOn && !ok)
                    {
                        // SendLineAlert(s.Name, s.IP);
                    }
                    lastStatus[s.IP] = ok;
                }
            );

            int total = serversAll.Count;

            // 3) กรองสำหรับ "แสดงผล" (keyword + โหมด)
            var servers = serversAll;
            string keyword = txtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(keyword))
            {
                servers = servers.FindAll(s =>
                    s.Name.ToLower().Contains(keyword) ||
                    s.IP.ToLower().Contains(keyword));
            }

            var html = new StringBuilder();
            html.Append("<div class='row'>");

            foreach (var server in servers)
            {
                bool isOnline = pingMap.TryGetValue(server.IP, out bool ok) && ok;

                bool shouldRender =
                    CurrentFilter == FilterMode.All ||
                    (CurrentFilter == FilterMode.Online && isOnline) ||
                    (CurrentFilter == FilterMode.Offline && !isOnline);

                if (!shouldRender) continue;

                string color = isOnline ? "success" : "danger";
                string status = isOnline ? "ONLINE" : "OFFLINE";
                string bgColor = isOnline ? "#e0f8e0" : "#949494";

                html.Append($@"
<div class='col-md-3 mb-3'>
  <div class='card border-{color}' style='background-color:{bgColor};'>
    <div class='card-body text-center' style='min-height:180px;display:flex;flex-direction:column;justify-content:center;'>
      <h5 class='card-title'>{server.Name}</h5>
      <p class='card-text'><strong>{server.IP}</strong></p>
      <span class='badge bg-{color}'>{status}</span>
    </div>
  </div>
</div>");
            }

            html.Append("</div>");
            pnlStatus.Controls.Add(new Literal { Text = html.ToString() });

            // 4) ตัวเลขสรุป (จากข้อมูลทั้งหมด)
            lblTotalServers.Text = total.ToString();
            lblOnline.Text = onlineAll.ToString();
            lblOffline.Text = offlineAll.ToString();

            // กันข้อความในปุ่มหายหลัง PostBack (ถ้าใช้ปุ่มเป็นตัวกล่อง)
            btnFilterAll.Text = $"Total Servers: {total}";
            btnFilterOnline.Text = $"Online: {onlineAll}";
            btnFilterOffline.Text = $"Offline: {offlineAll}";

            // 5) ไฮไลต์ปุ่ม
            UpdateActiveButtons();
        }

        private void UpdateActiveButtons()
        {
            btnFilterAll.CssClass = AddActiveCss(btnFilterAll.CssClass, CurrentFilter == FilterMode.All);
            btnFilterOnline.CssClass = AddActiveCss(btnFilterOnline.CssClass, CurrentFilter == FilterMode.Online);
            btnFilterOffline.CssClass = AddActiveCss(btnFilterOffline.CssClass, CurrentFilter == FilterMode.Offline);
        }

        private string AddActiveCss(string css, bool active)
        {
            css = (css ?? string.Empty).Replace(" stat-active", "");
            return css + (active ? " stat-active" : "");
        }

        private bool IsPingSuccessful(string ip)
        {
            try
            {
                using (var ping = new Ping())
                {
                    // จาก 2000 → 1000ms พอ (ไม่งั้นรวมๆจะช้า)
                    var reply = ping.Send(ip, 1000);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        private List<ServerInfo> LoadServersFromDB()
        {
            List<ServerInfo> list = new List<ServerInfo>();
            string connStr = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();
                string query = "SELECT Name, IP FROM IT_ServerList";
                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new ServerInfo
                        {
                            Name = reader["Name"].ToString(),
                            IP = reader["IP"].ToString()
                        });
                    }
                }
            }
            return list;
        }

        protected void btnAddServer_Click(object sender, EventArgs e)
        {
            string name = txtNewName.Text.Trim();
            string ip = txtNewIP.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(ip))
            {
                lblAddStatus.Text = "❌ กรุณากรอกชื่อและ IP ให้ครบ";
                lblAddStatus.CssClass = "text-danger";
                return;
            }

            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "INSERT INTO IT_ServerList (Name, IP) VALUES (@Name, @IP)";
                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@IP", ip);
                        cmd.ExecuteNonQuery();
                    }
                }

                lblAddStatus.Text = "✅ เพิ่ม Server สำเร็จ!";
                lblAddStatus.CssClass = "text-success";

                txtNewName.Text = "";
                txtNewIP.Text = "";

                RenderDashboard(); // refresh
            }
            catch (Exception ex)
            {
                lblAddStatus.Text = "❌ Error: " + ex.Message;
                lblAddStatus.CssClass = "text-danger";
            }
        }

        public class ServerInfo
        {
            public string Name { get; set; }
            public string IP { get; set; }
        }

        protected void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RenderDashboard();
        }

        // ====== ปุ่มฟิลเตอร์ 3 ปุ่ม ======
        protected void btnFilterAll_Click(object sender, EventArgs e)
        {
            CurrentFilter = FilterMode.All;
            RenderDashboard();
        }

        protected void btnFilterOnline_Click(object sender, EventArgs e)
        {
            CurrentFilter = FilterMode.Online;
            RenderDashboard();
        }

        protected void btnFilterOffline_Click(object sender, EventArgs e)
        {
            CurrentFilter = FilterMode.Offline;
            RenderDashboard();
        }
        
    }
}
