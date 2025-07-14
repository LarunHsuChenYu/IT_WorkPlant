using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IT_WorkPlant
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // 檢查是否已登入
            if (Session["UserEmpID"] == null)
            {
                // 未登入，重定向至登入頁面
                Response.Redirect("~/Login.aspx");
            }

            // 若已登入，可以顯示使用者名稱
            lblWelcome.Text = "Welcome to use Enrich IT Workplant, " + Session["UserName"];
        }
        protected string GetLabel(string key)
        {
            string lang = Session["lang"]?.ToString() ?? "en";

            var th = new Dictionary<string, string> {
        { "meetingroom", "จองห้องประชุม" },
        { "helpdesk", "แจ้งปัญหาไอที" },
        { "wifirequest", "ขอใช้งาน WiFi" },
        { "emailrequest", "ขอใช้งานอีเมล" },
        { "borrowrequest", "ขอยืมอุปกรณ์" },
        { "erprequest", "ขอเปิดใช้งาน ERP" },
        { "itmission", "เช็คงาน IT" },
        { "itdashboard", "แดชบอร์ด IT" },
        { "itstuff", "ขอซื้อของไอที" },
        { "warroom", "แดชบอร์ดวิเคราะห์การผลิต" }
    };

            var en = new Dictionary<string, string> {
        { "meetingroom", "Meeting Room Booking" },
        { "helpdesk", "IT HelpDesk" },
        { "wifirequest", "WIFI Request" },
        { "emailrequest", "EMAIL Request" },
        { "borrowrequest", "Borrow Request" },
        { "erprequest", "ERP Register Request" },
        { "itmission", "IT Mission Check" },
        { "itdashboard", "IT Dashboard" },
        { "itstuff", "IT Stuff Purchase" },
        { "warroom", "War Room" }
    };

            var zh = new Dictionary<string, string> {
                { "meetingroom", "會議室預定" },
                { "helpdesk", "IT協助" },
                { "wifirequest", "WiFi申請" },
                { "emailrequest", "電子信箱申請" },
                { "borrowrequest", "IT設備借用" },
                { "erprequest", "ERP帳號申請" },
                { "itmission", "IT任務瀏覽" },
                { "itdashboard", "IT任務看板" },
                { "itstuff", "IT物品請購" },
                { "warroom", "戰情室" }
};

            Dictionary<string, string> dict;
            if (lang == "zh") dict = zh;
            else if (lang == "th") dict = th;
            else dict = en;

            return dict.ContainsKey(key.ToLower()) ? dict[key.ToLower()] : key;
        }

    }
}