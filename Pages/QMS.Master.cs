using System;
using System.Web.UI.WebControls;

namespace IT_WorkPlant.Pages
{
    public partial class QMSMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // ชื่อหน้า = Page.Title
            litPageTitle.Text = string.IsNullOrWhiteSpace(Page.Title) ? "Quality" : Page.Title;

            // ไฮไลท์เมนูซ้ายตามหน้า
            string path = (Request.AppRelativeCurrentExecutionFilePath ?? "").ToLowerInvariant();
            SetActive(lnkIssueList, "qms_qualityissueslist.aspx", path);
            SetActive(lnkAddIssue, "qms_qualityissueadd.aspx", path);
            SetActive(lnkIssueSummary, "qms_qualityissuesummary.aspx", path);
            SetActive(lnkAddException, "qms_addexception.aspx", path);
            SetActive(lnkDashboard, "qms_dashboard.aspx", path);
        }

        private void SetActive(HyperLink link, string endsWithFile, string currentPath)
        {
            if (currentPath.EndsWith(endsWithFile))
                link.CssClass = (link.CssClass + " active").Trim();
        }
    }
}
