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
                Response.Redirect("Login.aspx");
            }

            // 若已登入，可以顯示使用者名稱
            lblWelcome.Text = "Welcome to use Enrich IT Workplant, " + Session["UserName"];
        }
    }
}