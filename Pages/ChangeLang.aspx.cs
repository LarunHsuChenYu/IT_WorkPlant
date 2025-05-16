using System;

namespace IT_WorkPlant.Pages
{
    public partial class ChangeLang : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string lang = Request.QueryString["lang"];

            if (!string.IsNullOrEmpty(lang))
            {
                // เซฟค่าไว้ใน session
                Session["lang"] = lang;
            }

            // ถ้ามี referrer (หน้าที่มาจาก) ก็กลับไปหน้านั้น
            // ถ้าไม่มี ให้กลับไปหน้า default (กันไว้ก่อน)
            string returnUrl = Request.UrlReferrer?.ToString() ?? "~/Default.aspx";
            Response.Redirect(returnUrl);
        }
    }
}
