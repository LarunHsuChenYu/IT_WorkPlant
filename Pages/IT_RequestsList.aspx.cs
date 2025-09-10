using IT_WorkPlant.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IT_WorkPlant.Pages
{
    public partial class IT_RequestsList : System.Web.UI.Page
    {
        private IT_RequestModel _model;

        // เก็บชุดข้อมูลล่าสุดไว้ (หน้า List)
        private DataTable RequestsData
        {
            get => ViewState["RequestsData"] as DataTable;
            set => ViewState["RequestsData"] = value;
        }

        // เก็บไอดีที่กำลังแสดงในแผง Detail
        private int? SelectedReportID
        {
            get => ViewState["SelectedReportID"] as int?;
            set => ViewState["SelectedReportID"] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (_model == null) _model = new IT_RequestModel();

            // เตรียมโฟลเดอร์รูป
            string uploadFolder = Server.MapPath("~/App_Temp");
            if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

            if (!IsPostBack)
            {
                if (Session["UserEmpID"] == null)
                {
                    Response.Redirect("../Login.aspx");
                    return;
                }

                BindFilters();

                // 🔰 ตั้งค่าเริ่มต้นให้เป็น WIP
                var wipItem = ddlStatus.Items.FindByValue("WIP") ?? ddlStatus.Items.FindByText("WIP");
                ddlStatus.ClearSelection();
                if (wipItem != null)
                {
                    wipItem.Selected = true;       // dropdown โชว์ WIP
                    ViewState["Status"] = "WIP";   // ตัวกรองฝั่งโค้ดเป็น WIP
                }
                else
                {
                    // สำรอง: ถ้าไม่มีตัวเลือก WIP ให้เป็น All เหมือนเดิม
                    if (ddlStatus.Items.FindByValue("") != null)
                        ddlStatus.SelectedValue = "";
                    ViewState["Status"] = "";
                }

                BindRequestData();

                // ถ้ามี ?id= ให้เปิดแผง Detail ทันที (ดึงแบบไม่ติดฟิลเตอร์)
                if (int.TryParse(Request.QueryString["id"], out int rid))
                    ShowDetail(rid);
                else
                    ToggleDetail(false);
            }
        }

        private void BindRequestData()
        {
            string issueMonth = ViewState["IssueMonth"]?.ToString();
            string issueDate = ViewState["IssueDate"]?.ToString();
            string deptName = ViewState["Department"]?.ToString();
            string requestUser = ViewState["RequestUser"]?.ToString();
            string issueType = ViewState["IssueType"]?.ToString();
            string status = ViewState["Status"]?.ToString();

            DataTable dt = _model.GetFilteredRequests(deptName, requestUser, issueType, status, issueMonth, issueDate);
            RequestsData = dt;

            // ===== สรุปตัวเลข Dashboard ตามข้อมูลที่ฟิลเตอร์แล้ว =====
            ComputeSummaries(dt);

            DataView dv = dt.DefaultView;
            dv.Sort = "ReportID ASC";
            gvRequests.DataSource = dv;
            gvRequests.DataBind();
        }

        private void BindFilters()
        {
            // เดือน
            DataTable m = _model.GetFilterOptions("FORMAT(IssueDate, 'yyyy-MM')");
            ddlIssueMonth.DataSource = m;
            ddlIssueMonth.DataTextField = "Value";
            ddlIssueMonth.DataValueField = "Value";
            ddlIssueMonth.DataBind();
            ddlIssueMonth.Items.Insert(0, new ListItem("All Months", ""));

            // แผนก
            DataTable d = _model.GetDepartments();
            ddlDeptName.DataSource = d;
            ddlDeptName.DataTextField = "DeptName_en";
            ddlDeptName.DataValueField = "DeptNameID";
            ddlDeptName.DataBind();
            ddlDeptName.Items.Insert(0, new ListItem("All Departments", ""));

            BindFilterDropdown(ddlRequestUser, "RequestUser", "All Request Users");
            BindFilterDropdown(ddlIssueType, "IssueType", "All Issue Types");
            BindFilterDropdown(ddlStatus, "Status", "All Statuses");
        }

        private void BindFilterDropdown(DropDownList ddl, string col, string def)
        {
            DataTable dt = _model.GetFilterOptions(col);
            ddl.DataSource = dt;
            ddl.DataTextField = "Value";
            ddl.DataValueField = "Value";
            ddl.DataBind();
            ddl.Items.Insert(0, new ListItem(def, ""));
            if (col == "Status") ddl.Items.Insert(1, new ListItem("Finished Today", "Today"));
        }

        private void BindIssueDateFilter(string month)
        {
            ddlIssueDate.Items.Clear();
            ddlIssueDate.Items.Insert(0, new ListItem("All Dates", ""));
            if (string.IsNullOrEmpty(month)) return;

            if (DateTime.TryParse($"{month}-01", out DateTime first))
            {
                int days = DateTime.DaysInMonth(first.Year, first.Month);
                for (int i = 1; i <= days; i++)
                {
                    var d = new DateTime(first.Year, first.Month, i);
                    ddlIssueDate.Items.Add(new ListItem(d.ToString("dd"), d.ToString("yyyy-MM-dd")));
                }
            }
        }

        protected void FilterChanged(object sender, EventArgs e)
        {
            ViewState["IssueMonth"] = ddlIssueMonth.SelectedValue;
            ViewState["Department"] = ddlDeptName.SelectedValue;
            ViewState["RequestUser"] = ddlRequestUser.SelectedValue;
            ViewState["IssueType"] = ddlIssueType.SelectedValue;
            ViewState["Status"] = ddlStatus.SelectedValue;
            ViewState["IssueDate"] = ddlIssueDate.SelectedValue;

            BindIssueDateFilter(ddlIssueMonth.SelectedValue);
            if (!string.IsNullOrEmpty(ViewState["IssueDate"]?.ToString()))
            {
                var v = ViewState["IssueDate"].ToString();
                if (ddlIssueDate.Items.FindByValue(v) != null) ddlIssueDate.SelectedValue = v;
            }

            BindRequestData();
            ToggleDetail(false);
        }

        protected void gvRequests_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvRequests.PageIndex = e.NewPageIndex;
            BindRequestData();
        }

        protected void gvRequests_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            var drv = (DataRowView)e.Row.DataItem;
            e.Row.Attributes["id"] = $"row_{drv["ReportID"]}";
        }

        /* ===== DETAIL ===== */
        private void ShowDetail(int reportId)
        {
            // ดึง record จากฐานข้อมูลแบบไม่ติดฟิลเตอร์ เพื่อกันเคสแถวถูกกรองออก
            DataTable all = _model.GetFilteredRequests(null, null, null, null, null, null);
            DataRow row = all.AsEnumerable().FirstOrDefault(r => Convert.ToInt32(r["ReportID"]) == reportId);
            if (row == null) { ToggleDetail(false); return; }

            SelectedReportID = reportId;

            SetText(lblReportID, row["ReportID"].ToString());
            SetText(lblIssueDate, SafeDate(row["IssueDate"]));
            SetText(lblRequestUser, row["RequestUser"]?.ToString());
            SetText(lblFinishedDate, SafeDate(row["FinishedDate"]));

            BindDetailDropdowns(row);

            txtIssueDetail.Text = row["IssueDetails"]?.ToString();
            txtSolution.Text = row["Solution"]?.ToString();
            txtRemark.Text = row["Remark"]?.ToString();

            string statusText = row["Status"]?.ToString();
            if (string.IsNullOrWhiteSpace(statusText))
                statusText = (row["StatusValue"] != DBNull.Value && row["StatusValue"].ToString() == "1") ? "Done" : "WIP";
            lblStatusText.Text = statusText;

            // แสดงรูป
            BindImagesFromDbOrFolder(row, reportId);

            bool isIT = (Session["DeptName"]?.ToString() == "IT");
            btnEdit.Visible = isIT;
            btnDeleteDetail.Visible = isIT;

            SetDetailEditMode(false);
            ToggleDetail(true);
        }

        // แปลงพาธไฟล์จริงบนดิสก์ -> URL แบบ ~/relative
        private string ToAppRelativeUrl(string fullPath)
        {
            var webRoot = Server.MapPath("~").TrimEnd('\\', '/');
            var rel = fullPath.Replace(webRoot, "").TrimStart('\\', '/').Replace("\\", "/");
            return ResolveUrl("~/" + rel);
        }

        // หาไฟล์รูปแบบฉลาด จากค่าใน DB และ/หรือ ReportID
        private IEnumerable<string> FindCandidateFiles(string root, string dbFile, int reportId, string[] allowExt)
        {
            IEnumerable<string> all = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                .Where(p => allowExt.Contains(Path.GetExtension(p).ToLower()));

            var results = new List<string>();

            string baseName = string.IsNullOrWhiteSpace(dbFile) ? null : Path.GetFileName(dbFile);

            // 1) exact match
            if (!string.IsNullOrEmpty(baseName))
                results.AddRange(all.Where(p => string.Equals(Path.GetFileName(p), baseName, StringComparison.OrdinalIgnoreCase)));

            // 2) ends-with (รองรับกรณีมี prefix GUID_ แล้วตามด้วยชื่อเดิม)
            if (!string.IsNullOrEmpty(baseName))
                results.AddRange(all.Where(p => Path.GetFileName(p).EndsWith(baseName, StringComparison.OrdinalIgnoreCase)));

            // 3) contains ReportID (รองรับไฟล์เก่า)
            string rid = reportId.ToString();
            results.AddRange(all.Where(p => Path.GetFileNameWithoutExtension(p).IndexOf(rid, StringComparison.OrdinalIgnoreCase) >= 0));

            return results.Distinct().Take(20);
        }

        // ใช้รูปจาก DB (ImagePath) ก่อน แล้ว fallback ค้นจากชื่อไฟล์ที่มี ReportID
        private void BindImagesFromDbOrFolder(DataRow row, int reportId)
        {
            try
            {
                if (rptImages == null || phNoImage == null) return;

                string root = Server.MapPath("~/App_Temp");
                if (!Directory.Exists(root))
                {
                    rptImages.DataSource = null; rptImages.DataBind(); phNoImage.Visible = true;
                    return;
                }

                var allowExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };

                // 1) ดึงจาก DB ก่อน (แมตช์ชื่อไฟล์ตรง/ลงท้ายด้วย)
                string dbFile = row.Table.Columns.Contains("ImagePath") && row["ImagePath"] != DBNull.Value
                                ? (row["ImagePath"] + "").Trim()
                                : null;

                List<string> candidates = new List<string>();

                if (!string.IsNullOrEmpty(dbFile))
                {
                    string baseName = Path.GetFileName(dbFile);
                    var all = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                                       .Where(p => allowExt.Contains(Path.GetExtension(p).ToLower()));

                    candidates = all.Where(p =>
                                    string.Equals(Path.GetFileName(p), baseName, StringComparison.OrdinalIgnoreCase) ||
                                    Path.GetFileName(p).EndsWith(baseName, StringComparison.OrdinalIgnoreCase))
                                    .ToList();
                }

                // 2) ไม่มีใน DB หรือหาไม่เจอ → ค้นจาก ReportID แต่ "เลือกแค่ 1 ไฟล์" ที่ใหม่สุด
                if (candidates.Count == 0)
                {
                    var all = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                                       .Where(p => allowExt.Contains(Path.GetExtension(p).ToLower()));

                    string rid = reportId.ToString();
                    candidates = all.Where(p =>
                                    Path.GetFileNameWithoutExtension(p)
                                        .IndexOf(rid, StringComparison.OrdinalIgnoreCase) >= 0)
                                    .ToList();
                }

                // เลือกมาแค่ไฟล์เดียว: ล่าสุดสุดตามเวลาแก้ไข
                string pick = candidates
                                .OrderByDescending(p => System.IO.File.GetLastWriteTime(p))
                                .FirstOrDefault();

                if (!string.IsNullOrEmpty(pick))
                {
                    rptImages.DataSource = new[] { new { Url = ToAppRelativeUrl(pick) } };
                    rptImages.DataBind();
                    phNoImage.Visible = false;
                }
                else
                {
                    rptImages.DataSource = null;
                    rptImages.DataBind();
                    phNoImage.Visible = true;
                }
            }
            catch
            {
                rptImages.DataSource = null;
                rptImages.DataBind();
                phNoImage.Visible = true;
            }
        }

        private void BindDetailDropdowns(DataRow row)
        {
            // Dept
            var dtDepartments = _model.GetDepartments();
            ddlDetailDept.DataSource = dtDepartments;
            ddlDetailDept.DataTextField = "DeptName_en";
            ddlDetailDept.DataValueField = "DeptNameID";
            ddlDetailDept.DataBind();
            var deptId = row["DeptNameID"]?.ToString();
            if (!string.IsNullOrEmpty(deptId) && ddlDetailDept.Items.FindByValue(deptId) != null)
                ddlDetailDept.SelectedValue = deptId;

            // IssueType
            var dtIssueTypes = _model.GetAllIssueTypes();
            ddlDetailIssueType.DataSource = dtIssueTypes;
            ddlDetailIssueType.DataTextField = "IssueTypeCode";
            ddlDetailIssueType.DataValueField = "IssueTypeID";
            ddlDetailIssueType.DataBind();
            var issueTypeId = row["IssueTypeID"]?.ToString();
            if (!string.IsNullOrEmpty(issueTypeId) && ddlDetailIssueType.Items.FindByValue(issueTypeId) != null)
                ddlDetailIssueType.SelectedValue = issueTypeId;

            // DRI User
            var dtUsers = _model.GetUsersByDepartment("IT");
            ddlDetailDRI.DataSource = dtUsers;
            ddlDetailDRI.DataTextField = "UserName";
            ddlDetailDRI.DataValueField = "UserIndex";
            ddlDetailDRI.DataBind();
            var driId = row["DRI_UserID"]?.ToString();
            if (!string.IsNullOrEmpty(driId) && ddlDetailDRI.Items.FindByValue(driId) != null)
                ddlDetailDRI.SelectedValue = driId;

            // Status value (รับได้ทั้ง text/ตัวเลข และ select แบบปลอดภัย)
            string statusValue = null;
            if (row.Table.Columns.Contains("StatusValue") && row["StatusValue"] != DBNull.Value)
                statusValue = row["StatusValue"].ToString().Trim();

            if (string.IsNullOrEmpty(statusValue))
            {
                var statusText = row.Table.Columns.Contains("Status") && row["Status"] != DBNull.Value
                    ? row["Status"].ToString().Trim()
                    : null;
                statusValue = string.Equals(statusText, "Done", StringComparison.OrdinalIgnoreCase) ? "1" : "0";
            }

            ddlDetailStatus.ClearSelection();
            var item = ddlDetailStatus.Items.FindByValue(statusValue);
            if (item != null) item.Selected = true;
            else
            {
                var fallback = ddlDetailStatus.Items.FindByValue("0");
                if (fallback != null) fallback.Selected = true;
                else if (ddlDetailStatus.Items.Count > 0) ddlDetailStatus.Items[0].Selected = true;
            }
        }

        private void SetDetailEditMode(bool edit)
        {
            ddlDetailDept.Enabled = edit;
            ddlDetailIssueType.Enabled = edit;
            ddlDetailDRI.Enabled = edit;
            ddlDetailStatus.Enabled = edit;

            txtIssueDetail.ReadOnly = !edit;
            txtSolution.ReadOnly = !edit;
            txtRemark.ReadOnly = !edit;

            btnSave.Visible = edit;
            btnCancelEdit.Visible = edit;
            btnEdit.Visible = !edit;
        }

        protected void btnEdit_Click(object sender, EventArgs e)
        {
            if (SelectedReportID.HasValue) SetDetailEditMode(true);
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            if (SelectedReportID.HasValue) ShowDetail(SelectedReportID.Value);
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!SelectedReportID.HasValue) return;
            int reportID = SelectedReportID.Value;

            string query = @"
                UPDATE IT_RequestList
                SET DRI_UserID   = @DRIUserID,
                    DeptNameID   = @DeptNameID,
                    IssueTypeID  = @IssueTypeID,
                    IssueDetails = @IssueDetails,
                    Solution     = @Solution,
                    Status       = @Status,
                    FinishedDate = CASE WHEN @Status = 1 THEN ISNULL(FinishedDate, GETDATE()) ELSE NULL END,
                    Remark       = @Remark,
                    LastUpdateDate = GETDATE()
                WHERE ReportID   = @ReportID";

            SqlParameter[] parameters =
            {
                new SqlParameter("@DRIUserID",   string.IsNullOrEmpty(ddlDetailDRI.SelectedValue) ? (object)DBNull.Value : ddlDetailDRI.SelectedValue),
                new SqlParameter("@DeptNameID",  string.IsNullOrEmpty(ddlDetailDept.SelectedValue) ? (object)DBNull.Value : ddlDetailDept.SelectedValue),
                new SqlParameter("@IssueTypeID", string.IsNullOrEmpty(ddlDetailIssueType.SelectedValue) ? (object)DBNull.Value : ddlDetailIssueType.SelectedValue),
                new SqlParameter("@IssueDetails", (object)txtIssueDetail.Text ?? DBNull.Value),
                new SqlParameter("@Solution",     (object)txtSolution.Text ?? DBNull.Value),
                new SqlParameter("@Status",       ddlDetailStatus.SelectedValue == "1" ? 1 : 0),
                new SqlParameter("@Remark",       (object)txtRemark.Text ?? DBNull.Value),
                new SqlParameter("@ReportID",     reportID)
            };

            _model.UpdateRequest(query, parameters);

            BindRequestData();     // เผื่อรายการเปลี่ยนสถานะ
            ShowDetail(reportID);  // กลับมาโหมดอ่าน
        }

        protected void btnDeleteDetail_Click(object sender, EventArgs e)
        {
            if (Session["DeptName"]?.ToString() != "IT")
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('คุณไม่มีสิทธิ์ลบรายการนี้');", true);
                return;
            }
            if (!SelectedReportID.HasValue) return;

            string deleteQuery = "DELETE FROM IT_RequestList WHERE ReportID = @ReportID";
            _model.UpdateRequest(deleteQuery, new[] { new SqlParameter("@ReportID", SelectedReportID.Value) });

            SelectedReportID = null;
            ToggleDetail(false);
            BindRequestData();
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            SelectedReportID = null;
            ToggleDetail(false);
            BindRequestData();
        }

        /* ===== Helpers ===== */

        // คำนวณสรุปตัวเลข Dashboard จากชุดข้อมูลที่ฟิลเตอร์แล้ว
        private void ComputeSummaries(DataTable dt)
        {
            int total = 0, finished = 0, wip = 0, assigned = 0, unassigned = 0, today = 0, thisMonth = 0;

            if (dt != null && dt.Rows.Count > 0)
            {
                var rows = dt.AsEnumerable();

                Func<DataRow, string, string> get = (r, col) =>
                    r.Table.Columns.Contains(col) && r[col] != DBNull.Value ? r[col].ToString().Trim() : null;

                Func<DataRow, bool> isFinished = r =>
                {
                    var sv = get(r, "StatusValue");
                    var st = get(r, "Status");
                    if (!string.IsNullOrEmpty(sv) && sv == "1") return true;
                    if (!string.IsNullOrEmpty(st) && st.Equals("Done", StringComparison.OrdinalIgnoreCase)) return true;
                    return false;
                };

                Func<DataRow, bool> hasDRI = r => !string.IsNullOrEmpty(get(r, "DRI_UserID"));

                Func<DataRow, DateTime?> issueDate = r =>
                {
                    var raw = get(r, "IssueDate");
                    if (DateTime.TryParse(raw, out var d)) return d;
                    return null;
                };

                total = rows.Count();
                finished = rows.Count(isFinished);
                wip = rows.Count(r => !isFinished(r));

                // ต้องยังไม่เสร็จด้วย
                assigned = rows.Count(r => !isFinished(r) && hasDRI(r));
                unassigned = rows.Count(r => !isFinished(r) && !hasDRI(r));

                var todayDate = DateTime.Today;
                today = rows.Count(r => issueDate(r)?.Date == todayDate);

                var now = DateTime.Today;
                thisMonth = rows.Count(r =>
                {
                    var d = issueDate(r);
                    return d.HasValue && d.Value.Year == now.Year && d.Value.Month == now.Month;
                });
            }

            // เก็บไว้ใน ViewState เผื่อใช้ต่อ
            ViewState["Summary_Total"] = total;
            ViewState["Summary_Finished"] = finished;
            ViewState["Summary_WIP"] = wip;
            ViewState["Summary_Assigned"] = assigned;
            ViewState["Summary_Unassigned"] = unassigned;
            ViewState["Summary_Today"] = today;
            ViewState["Summary_ThisMonth"] = thisMonth;

            // ตั้งค่าลง Label (ไม่มีไม่พัง)
            TrySetText("lblTotal", total.ToString());
            TrySetText("lblFinished", finished.ToString());
            TrySetText("lblWIP", wip.ToString());
            TrySetText("lblAssigned", assigned.ToString());
            TrySetText("lblUnassigned", unassigned.ToString());
            TrySetText("lblToday", today.ToString());
            TrySetText("lblThisMonth", thisMonth.ToString());
        }

        // หา Label ให้เจาะเข้า ContentPlaceHolder ก่อน
        private void TrySetText(string controlId, string text)
        {
            ITextControl ctrl = null;

            // 1) ใน ContentPlaceHolder (MainContent)
            var cph = Master?.FindControl("MainContent");
            if (cph != null)
                ctrl = cph.FindControl(controlId) as ITextControl;

            // 2) Fallback หาในเพจ
            if (ctrl == null)
                ctrl = FindControl(controlId) as ITextControl;

            if (ctrl != null)
                ctrl.Text = text;
        }

        private static string SafeDate(object o)
        {
            if (o == DBNull.Value || o == null) return "-";
            return DateTime.TryParse(o.ToString(), out var d) ? d.ToString("yyyy-MM-dd") : "-";
        }

        private static void SetText(Label lbl, string text)
        {
            if (lbl != null) lbl.Text = string.IsNullOrWhiteSpace(text) ? "-" : text;
        }

        private void ToggleDetail(bool showDetail)
        {
            pnlList.Visible = !showDetail;
            pnlDetail.Visible = showDetail;
        }

        // (ยังคงไว้สำหรับรองรับไฟล์เก่า ๆ ที่ตั้งชื่ออิง ReportID)
        private void BindImagesFromFolder(int reportId)
        {
            try
            {
                if (rptImages == null || phNoImage == null) return;

                string root = Server.MapPath("~/App_Temp");
                if (!Directory.Exists(root))
                {
                    rptImages.DataSource = null;
                    rptImages.DataBind();
                    phNoImage.Visible = true;
                    return;
                }

                var allowExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
                var files = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                    .Where(p => p.IndexOf(reportId.ToString(), StringComparison.OrdinalIgnoreCase) >= 0
                             && allowExt.Contains(Path.GetExtension(p).ToLower()))
                    .Take(20)
                    .Select(p => new { Url = ToAppRelativeUrl(p) })
                    .ToList();

                if (files.Count > 0)
                {
                    rptImages.DataSource = files;
                    rptImages.DataBind();
                    phNoImage.Visible = false;
                }
                else
                {
                    rptImages.DataSource = null;
                    rptImages.DataBind();
                    phNoImage.Visible = true;
                }
            }
            catch
            {
                rptImages.DataSource = null;
                rptImages.DataBind();
                phNoImage.Visible = true;
            }
        }
    }
}
