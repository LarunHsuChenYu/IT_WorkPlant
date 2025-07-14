using IT_WorkPlant.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IT_WorkPlant.Pages
{
	public partial class IT_erpNewUserCreate : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
            if (!IsPostBack)
            {
                // 初次進入頁面時，如果 Role 預設是 "normal"，則隱藏環境選擇面板
                pnlEnv.Visible = ddlRole.SelectedValue == "IT";

                // Set text for ListItems programmatically
                ddlRole.Items.FindByValue("normal").Text = GetLabel("normal");
                ddlRole.Items.FindByValue("IT").Text = GetLabel("IT");

                ddlEnv.Items.FindByValue("prod").Text = GetLabel("env_production");
                ddlEnv.Items.FindByValue("test").Text = GetLabel("env_testing");
            }
            // Trigger data binding for <%# %> expressions on other controls
            Page.DataBind();
        }
        protected void ddlRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 當選擇 IT Staff 時，顯示 Environment；否則隱藏
            pnlEnv.Visible = ddlRole.SelectedValue == "IT";
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            // 建立 Service 同時也當作參數物件
            var svc = new LinuxUserService
            {
                UserName = txtUserName.Text.Trim(),
                Role = ddlRole.SelectedValue,
                Env = ddlEnv.SelectedValue,
                AddDba = cbAddDba.Checked
            };

            // 呼叫執行，並將結果顯示在 lblMsg
            string result = svc.CreateUser();
            lblMsg.Text = Server.HtmlEncode(result);
        }

        protected string GetLabel(string key)
        {
            string lang = Session["lang"]?.ToString() ?? "en";

            var th = new Dictionary<string, string> {
                { "title", "สร้างบัญชี ERP ใหม่" },
                { "validation_header", "กรุณาแก้ไขข้อผิดพลาดต่อไปนี้:" },
                { "username", "ชื่อผู้ใช้:" },
                { "username_required", "กรุณาระบุชื่อผู้ใช้" },
                { "role", "บทบาท:" },
                { "normal", "ผู้ใช้ทั่วไป" },
                { "IT", "เจ้าหน้าที่ IT" },
                { "environment", "สภาพแวดล้อม:" },
                { "env_production", "ระบบจริง" },
                { "env_testing", "ระบบทดสอบ" },
                { "add_dba_group", "เพิ่มในกลุ่ม dba" },
                { "create_account", "สร้างบัญชี" }
            };

            var en = new Dictionary<string, string> {
                { "title", "Create New ERP Account" },
                { "validation_header", "Please fix the following errors:" },
                { "username", "User Name:" },
                { "username_required", "User Name is required." },
                { "role", "Role:" },
                { "normal", "General User" },
                { "IT", "IT Staff" },
                { "environment", "Environment:" },
                { "env_production", "Production" },
                { "env_testing", "Testing" },
                { "add_dba_group", "Add to dba group" },
                { "create_account", "Create Account" }
            };

            var zh = new Dictionary<string, string> {
                { "title", "建立新 ERP 帳號" },
                { "validation_header", "請修正以下錯誤：" },
                { "username", "使用者名稱：" },
                { "username_required", "請輸入使用者名稱" },
                { "role", "角色：" },
                { "normal", "一般使用者" },
                { "IT", "IT 人員" },
                { "environment", "環境：" },
                { "env_production", "正式環境" },
                { "env_testing", "測試環境" },
                { "add_dba_group", "加入 dba 群組" },
                { "create_account", "建立帳號" }
            };

            Dictionary<string, string> dict;
            if (lang == "zh") dict = zh;
            else if (lang == "th") dict = th;
            else dict = en;

            return dict.ContainsKey(key) ? dict[key] : key;
        }
    }
}