using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IT_WorkPlant.Pages
{
    public partial class IT_Stuff_Purchase : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // 檢查是否已登入
            if (Session["UserEmpID"] == null)
            {
                // 未登入，重定向至登入頁面
                Response.Redirect("../Login.aspx");
            }
        }

        protected void btnPick_Click(object sender, EventArgs e)
        {
            // Implement functionality to pick the item (store in session, etc.)
            Response.Write("<script>alert('Item picked!');</script>");
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            // Implement form submission logic here
            Response.Write("<script>alert('Submit Completed');</script>");
            ClientScript.RegisterStartupScript(this.GetType(), "popup", "showPopup();", true);
        }
    }
}