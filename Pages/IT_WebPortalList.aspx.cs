using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IT_WorkPlant.Pages
{
    public partial class IT_WebPortalList : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                List<LinkItem> links = new List<LinkItem>
                {
                    new LinkItem { No = 1, System = "AVAYA IP PHONE", URL = "https://192.168.30.237:8443/WebMgmtEE/WebManagement.html" },
                    new LinkItem { No = 2, System = "Connon Printer Office A", URL = "http://192.168.30.250:8000" },
                    new LinkItem { No = 3, System = "Connon Printer Office QC", URL = "http://192.168.32.251:8000/" },
                    new LinkItem { No = 4, System = "Enrich NAS Synology", URL = "https://192.168.30.167:5001/#/signin" },
                    new LinkItem { No = 5, System = "H3C Access Point Console", URL = "https://172.10.10.5/web/frame/login.html" },
                    new LinkItem { No = 6, System = "H3C CoreSwitch", URL = "https://192.168.32.1/web/frame/login.html" },
                    new LinkItem { No = 7, System = "H3C Security Firewall", URL = "https://172.10.10.4/web/frame/login.html" },
                    new LinkItem { No = 8, System = "IP Audit Network Report", URL = "http://192.168.32.103/~ipaudit/" },
                    new LinkItem { No = 9, System = "Kodcloud", URL = "http://192.168.12.134/#user/login" },
                    new LinkItem { No = 10, System = "Network Proxy Report", URL = "http://192.168.32.101/" },
                    new LinkItem { No = 11, System = "QC BUISU System", URL = "http://192.168.30.170:13080/" },
                    new LinkItem { No = 12, System = "Server Room Monitoring", URL = "http://192.168.32.213/dashboard/page_NE4MPWyH" },
                    new LinkItem { No = 13, System = "Server Room NVR", URL = "http://192.168.32.211/doc/page/login.asp?_1736471105840" },
                    new LinkItem { No = 14, System = "vmware ESXi 6.0 Host", URL = "https://192.168.32.100/ui/#/login" },
                    new LinkItem { No = 15, System = "vmware ESXi 8.0 Host", URL = "https://192.168.32.111/ui/" },
                    new LinkItem { No = 16, System = "WAN Trafic Report", URL = "https://corp2.ais-idc.com/cacti/index.php" }
                };

                rptLinks.DataSource = links;
                rptLinks.DataBind();
            }
        }
    }

    public class LinkItem
    {
        public int No { get; set; }
        public string System { get; set; }
        public string URL { get; set; }
    }
}