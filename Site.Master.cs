using IT_WorkPlant.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace IT_WorkPlant
{
    public partial class SiteMaster : MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["UserEmpID"] != null)
                {
                    // 用户已登录，添加登出按钮
                    LinkButton btnLogout = new LinkButton
                    {
                        ID = "btnLogout",
                        CssClass = "nav-link",
                        Text = "Log-out"
                    };
                    btnLogout.Click += Logout_Click;
                    phLoginStatus.Controls.Add(new HtmlGenericControl("li") { Attributes = { ["class"] = "nav-item" }, Controls = { btnLogout } });
                }
                else
                {
                    // 用户未登录，添加登录链接
                    HyperLink loginLink = new HyperLink
                    {
                        NavigateUrl = "Login.aspx",
                        CssClass = "nav-link",
                        Text = "Log-In"
                    };
                    phLoginStatus.Controls.Add(new HtmlGenericControl("li") { Attributes = { ["class"] = "nav-item" }, Controls = { loginLink } });
                }

                litYear.Text = DateTime.Now.Year.ToString();

                // 從 Session 獲取部門 ID
                string deptName = Session["DeptName"]?.ToString();

                // 根據部門顯示導航項目
                BuildNavbar(deptName);
            }
        }

        protected void Logout_Click(object sender, EventArgs e)
        {
            // 清除 Session 資料
            Session.Clear();
            Session.Abandon();

            // 可選：清除所有的認證 Cookie（如果有使用）
            if (Request.Cookies[".ASPXAUTH"] != null)
            {
                Response.Cookies[".ASPXAUTH"].Expires = DateTime.Now.AddDays(-1);
            }

            // 重定向至登入頁面
            Response.Redirect("~/Login.aspx");
        }

        private void BuildNavbar(string deptName)
        {
            // 基本導航項目
            AddNavItem("Meeting Room Booking", "~/Pages/MeetingRoomBooking");

            // IT 部門可以看到所有導航項目
            if (deptName == "IT")
            {
                AddDropdownItem("IT", "IT",
                    new[] {
                        new KeyValuePair<string, string>("User Account", "~/Pages/IT_UserManagement"),
                        new KeyValuePair<string, string>("Daily Check", "~/Pages/IT_DailyCheckList"),
                        new KeyValuePair<string, string>("Web Portal", "~/Pages/IT_WebPortalList")
                    });
                AddDropdownItem("PMC", "PMC",
                    new[] {
                        new KeyValuePair<string, string>("WO Update", "~/Pages/PMC_WO_HeadUpdate"),
                        new KeyValuePair<string, string>("CUS Invoice Create", "~/Pages/PMC_CUS_InvoiceCreate"),
                        new KeyValuePair<string, string>("CUS Maintain Shipping Price", "~/Pages/PMC_CUS_ShippingPriceMaintain"),
                        new KeyValuePair<string, string>("CUS Maintain Product CBM", "~/Pages/PMC_CUS_ProductCBMMaintain")
                    });
                AddDropdownItem("MFG", "MFG",
                    new[] {
                        new KeyValuePair<string, string>("EQ Daily Check", "~/Pages/EQ_Daily_Check")
                    });
                AddDropdownItem("PUR", "PUR",
                    new[] {
                        new KeyValuePair<string, string>("PUR Function", "~/Pages/PUR_Function")
                    });
                AddDropdownItem("ADM", "ADM",
                            new[] {
                                new KeyValuePair<string, string>("G3", "http://192.168.32.129:8015/hrp/login.do"),
                                new KeyValuePair<string, string>("N8", "http://192.168.30.238:8012/#/login"),
                            });
                
            }
            else
            {
                // 部門特定導航
                switch (deptName)
                {
                    case "PC":
                        AddDropdownItem("PMC", "PMC",
                            new[] {
                                new KeyValuePair<string, string>("WO Update", "~/Pages/PMC_WO_HeadUpdate"),
                                new KeyValuePair<string, string>("CUS Invoice Create", "~/Pages/PMC_CUS_InvoiceCreate"),
                                new KeyValuePair<string, string>("CUS Maintain Shipping Price", "~/Pages/PMC_CUS_ShippingPriceMaintain"),
                                new KeyValuePair<string, string>("CUS Maintain Product CBM", "~/Pages/PMC_CUS_ProductCBMMaintain")
                            });
                        break;

                    case "MF":
                        AddDropdownItem("MFG", "MFG",
                            new[] {
                                new KeyValuePair<string, string>("EQ Daily Check", "~/Pages/EQ_Daily_Check")
                            });
                        break;

                    case "PU":
                        AddDropdownItem("PUR", "PUR",
                            new[] {
                                new KeyValuePair<string, string>("PUR Function", "~/Pages/PUR_Function")
                            });
                        break;
                    case "AD":
                        AddDropdownItem("ADM", "ADM",
                            new[] {
                                new KeyValuePair<string, string>("G3", "http://192.168.32.129:8015/hrp/login.do"),
                                new KeyValuePair<string, string>("N8", "http://192.168.30.238:8012/#/login"),
                            });
                        break;

                    default:
                        // 可以加入默認或無權限顯示的處理
                        break;
                }
            }
        }


        private void AddNavItem(string text, string href)
        {
            var li = new HtmlGenericControl("li");
            li.Attributes["class"] = "nav-item";

            var a = new HtmlGenericControl("a");
            a.Attributes["class"] = "nav-link";
            a.Attributes["href"] = ResolveUrl(href);
            a.InnerText = text;

            li.Controls.Add(a);
            navbar.Controls.Add(li);
        }

        private void AddDropdownItem(string text, string dropdownId, KeyValuePair<string, string>[] items)
        {
            // 主導航項
            var li = new HtmlGenericControl("li");
            li.Attributes["class"] = "nav-item dropdown";

            var a = new HtmlGenericControl("a");
            a.Attributes["class"] = "nav-link dropdown-toggle";
            a.Attributes["href"] = "#";
            a.Attributes["id"] = dropdownId;
            a.Attributes["role"] = "button";
            a.Attributes["data-bs-toggle"] = "dropdown";
            a.Attributes["aria-expanded"] = "false";

            // **使用 LiteralControl 避免錯誤**
            a.Controls.Add(new LiteralControl(text));

            li.Controls.Add(a);

            // 下拉選單
            var ul = new HtmlGenericControl("ul");
            ul.Attributes["class"] = "dropdown-menu";
            ul.Attributes["aria-labelledby"] = dropdownId;

            foreach (var item in items)
            {
                var dropdownLi = new HtmlGenericControl("li");
                var dropdownA = new HtmlGenericControl("a");
                dropdownA.Attributes["class"] = "dropdown-item";

                // **檢查 URL 是否為完整網址**
                string url = item.Value;
                if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    dropdownA.Attributes["href"] = url;
                }
                else
                {
                    dropdownA.Attributes["href"] = ResolveUrl(url);
                }

                // **使用 LiteralControl 避免錯誤**
                dropdownA.Controls.Add(new LiteralControl(item.Key));

                dropdownLi.Controls.Add(dropdownA);
                ul.Controls.Add(dropdownLi);
            }

            li.Controls.Add(ul);
            navbar.Controls.Add(li);
        }

    }
}