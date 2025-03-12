using System;
using System.Web;
using System.Web.UI;
using IT_WorkPlant.Models;

namespace IT_WorkPlant
{
    public partial class Login : System.Web.UI.Page
    {
        private readonly UserInfo _authService = new UserInfo();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserEmpID"] != null)
            {
                Response.Redirect("Default.aspx");
            }
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string userId = txtUserID.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
            {
                lblMessage.Text = "請輸入使用者工號和密碼。\nPlease Enter onboard ID and Password.";
                return;
            }

            try
            {
                var userInfo = _authService.AuthenticateUser(userId, password);
                if (userInfo != null)
                {
                    SetUserSession(userId, userInfo);
                    Response.Redirect("Default.aspx");
                }
                else
                {
                    lblMessage.Text = $"使用者工號或密碼錯誤。\nUser onboard ID or Password incorrect.";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"系統錯誤，請稍後再試。\nSystem Error, please tyr later.";
                LogError(ex);
            }
        }

        private void SetUserSession(string userId, UserInfo userInfo)
        {
            Session["UserEmpID"] = userId;
            Session["UserName"] = userInfo.UserName;
            Session["UserEmpMail"] = userInfo.UserEmpMail;
            Session["DeptName"] = userInfo.DeptName;
        }

        private void LogError(Exception ex)
        {
            Console.WriteLine($"Error Message: {ex.Message}\nStack: {ex.StackTrace}");
        }
    }
}
