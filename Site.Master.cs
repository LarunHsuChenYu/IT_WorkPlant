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
            // ✅ ตั้งค่าภาษาเริ่มต้น
            if (Session["lang"] == null)
                Session["lang"] = "en";

            // เคลียร์ที่วางปุ่มมุมขวา
            phLoginStatus.Controls.Clear();

            if (Session["UserEmpID"] != null)
            {
                // 🔓 Logout link (ไปหน้า Logout.aspx ให้จัดการล้าง session)
                var li = new HtmlGenericControl("li");
                li.Attributes["class"] = "nav-item";

                var logoutLink = new HyperLink
                {
                    NavigateUrl = ResolveUrl("~/Pages/Logout.aspx"),
                    CssClass = "nav-link",
                    Text = "Log-out"
                };
                li.Controls.Add(logoutLink);
                phLoginStatus.Controls.Add(li);

                // เมนูหลัก
                string deptName = Session["DeptName"] as string;
                AddNavItem("Home", "~/Default.aspx");
                BuildNavbar(deptName);
            }
            else
            {
                // 🔐 Login link
                var li = new HtmlGenericControl("li");
                li.Attributes["class"] = "nav-item";

                var loginLink = new HyperLink
                {
                    NavigateUrl = ResolveUrl("~/Login.aspx"),
                    CssClass = "nav-link",
                    Text = "Log-In"
                };
                li.Controls.Add(loginLink);
                phLoginStatus.Controls.Add(li);
            }

            if (!IsPostBack)
            {
                litYear.Text = DateTime.Now.Year.ToString();
            }
        }

        protected void Logout_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Session.Abandon();

            if (Request.Cookies[".ASPXAUTH"] != null)
            {
                Response.Cookies[".ASPXAUTH"].Expires = DateTime.Now.AddDays(-1);
            }
            ScriptManager.RegisterStartupScript(this, this.GetType(), "logoutRedirect",
                "window.location.href='../Login.aspx';", true);
        }

        private void BuildNavbar(string deptName)
        {
            int positionID = Session["Position"] != null ? Convert.ToInt32(Session["Position"]) : 0;

            // 基本導航項目
            //AddNavItem("Meeting Room Booking", "~/Pages/MeetingRoomBooking");

            // IT 部門可以看到所有導航項目
            if (deptName == "IT")
            {
                AddDropdownItem("IT", "IT",
                    new[] {
                new KeyValuePair<string, string>("ERP Account", "~/Pages/IT_erpNewUserCreate.aspx"),
                new KeyValuePair<string, string>("User Account", "~/Pages/IT_UserManagement"),
                new KeyValuePair<string, string>("Daily Check", "~/Pages/IT_DailyCheckList"),
                new KeyValuePair<string, string>("Web Portal", "~/Pages/IT_WebPortalList"),
                new KeyValuePair<string, string>("IT Purchase Items Maintain", "~/Pages/IT_PurchaseItemsList"),
                new KeyValuePair<string, string>("IT Stock", "~/Pages/IT_StockItems.aspx"),
                new KeyValuePair<string, string>("IT Borrow Approval", "~/Pages/IT_BorrowApproval.aspx"),
                new KeyValuePair<string, string>("IT ComputerList", "~/Pages/IT_ComputerAdd.aspx"),
                new KeyValuePair<string, string>("Working Flow", "~/Pages/WF_FlowMaintain.aspx"),
                new KeyValuePair<string, string>("IT PingDashboard", "~/Pages/IT_PingDashboard.aspx"),
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
                new KeyValuePair<string, string>("Vanguard Price Update", "~/Pages/PUR_Vanguard_Price_Update"),
                new KeyValuePair<string, string>("InvoicePriceUpdate", "~/Pages/PUR_InvoicePriceUpdate")
                    });
                AddDropdownItem("ADM", "ADM",
                    new[] {
                new KeyValuePair<string, string>("G3", "http://192.168.32.129:8015/hrp/login.do"),
                new KeyValuePair<string, string>("N8", "http://192.168.32.129:8012/#/login"),
                    });

                // ✅ เพิ่มเมนู Dashboard สำหรับ IT ที่นี่
                AddDropdownItem("Dashboard", "OPD",
                    new[] {
                new KeyValuePair<string, string>("WO Entry Posting", "~/Pages/OPD_TLF_Statics"),
                new KeyValuePair<string, string>("Sale Order Entry", "~/Pages/OPD_SalesOrderAnalysis"),
                new KeyValuePair<string, string>("Work Order Analysis", "~/Pages/OPD_WorkOrderAnalysis")
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
                new KeyValuePair<string, string>("Vanguard Price Update", "~/Pages/PUR_Vanguard_Price_Update"),
                new KeyValuePair<string, string>("InvoicePriceUpdate", "~/Pages/PUR_InvoicePriceUpdate")
                            });
                        break;

                    case "AD":
                        AddDropdownItem("ADM", "ADM",
                            new[] {
                new KeyValuePair<string, string>("G3", "http://192.168.32.129:8015/hrp/login.do"),
                new KeyValuePair<string, string>("N8", "http://192.168.30.238:8012/#/login"),
                            });
                        break;

                    case "GO": // ✅ เพิ่มเคสสำหรับ GM หรือแผนกที่ต้องการให้เห็น Dashboard
                        AddDropdownItem("Dashboard", "OPD",
                            new[] {
                new KeyValuePair<string, string>("WO Entry Posting", "~/Pages/OPD_TLF_Statics"),
                new KeyValuePair<string, string>("Sale Order Entry", "~/Pages/OPD_SalesOrderAnalysis"),
                new KeyValuePair<string, string>("Work Order Analysis", "~/Pages/OPD_WorkOrderAnalysis")
                            });
                        break;

                    default:
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