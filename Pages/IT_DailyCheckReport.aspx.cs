using IT_WorkPlant.Models;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace IT_WorkPlant.Pages
{
    public partial class IT_DailyCheckReport : System.Web.UI.Page
    {
        private readonly MssqlDatabaseHelper _dbHelper = new MssqlDatabaseHelper();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // 預設顯示當前月份
                monthPicker.Value = DateTime.Now.ToString("yyyy-MM");
                SetMonthPickerLimits();
                BindGridView();
            }
        }

        private void SetMonthPickerLimits()
        {
            // 設置月份選擇範圍為前後 12 個月
            DateTime currentDate = DateTime.Now;
            DateTime minDate = currentDate.AddMonths(-12);
            DateTime maxDate = currentDate.AddMonths(12);

            monthPicker.Attributes["min"] = minDate.ToString("yyyy-MM");
            monthPicker.Attributes["max"] = maxDate.ToString("yyyy-MM");
        }

        protected void btnLoadReport_Click(object sender, EventArgs e)
        {
            BindGridView();
        }

        private void BindGridView()
        {
            // 清除 GridView 的資料
            gvDailyCheck.DataSource = null;
            gvDailyCheck.DataBind();

            // 清除現有的動態列
            gvDailyCheck.Columns.Clear();

            // 添加固定列
            gvDailyCheck.Columns.Add(new BoundField
            {
                DataField = "ItemID",
                HeaderText = "Item ID",
                HeaderStyle = { CssClass = "text-center" },
                ItemStyle = { CssClass = "text-center" }
            });

            gvDailyCheck.Columns.Add(new BoundField
            {
                DataField = "ItemName",
                HeaderText = "Item Name",
                HeaderStyle = { CssClass = "text-center" },
                ItemStyle = { CssClass = "text-left" }
            });

            gvDailyCheck.Columns.Add(new BoundField
            {
                DataField = "ItemDetail",
                HeaderText = "Item Detail",
                HeaderStyle = { CssClass = "text-center" },
                ItemStyle = { CssClass = "text-left" }
            });

            // 動態生成每天的列
            string selectedMonth = monthPicker.Value;
            DateTime startDate = DateTime.ParseExact(selectedMonth, "yyyy-MM", null);
            int daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                TemplateField dayColumn = new TemplateField
                {
                    HeaderText = $"{day}<br/>{GetDayOfWeekAbbreviation(startDate.Year, startDate.Month, day)}",
                    HeaderStyle = { CssClass = "text-center" },
                    ItemStyle = { CssClass = "text-center" }
                };

                dayColumn.ItemTemplate = new DayTemplate(day);
                gvDailyCheck.Columns.Add(dayColumn);
            }

            // 綁定資料
            var dailyChecks = GetDailyChecks(startDate, startDate.AddMonths(1).AddDays(-1));
            gvDailyCheck.DataSource = dailyChecks;
            gvDailyCheck.DataBind();
        }

        private List<DailyCheck> GetDailyChecks(DateTime startDate, DateTime endDate)
        {
            var dailyChecks = new List<DailyCheck>();

            // 從 IT_DailyCheck_records 表中查詢資料
            string query = @"
                SELECT r.Check_Date, r.ItemID, r.Item_Value, i.Item_Name, i.Item_Detail 
                FROM IT_DailyCheck_records r
                INNER JOIN IT_ServerRoom_CheckItems i ON r.ItemID = i.ItemID
                WHERE r.Check_Date BETWEEN @StartDate AND @EndDate
                ORDER BY r.ItemID, r.Check_Date";

            SqlParameter[] parameters =
            {
                new SqlParameter("@StartDate", startDate),
                new SqlParameter("@EndDate", endDate)
            };

            DataTable dt = _dbHelper.ExecuteQuery(query, parameters);

            // 將資料轉換為 DailyCheck 對象
            foreach (DataRow row in dt.Rows)
            {
                int itemId = Convert.ToInt32(row["ItemID"]);
                string itemName = row["Item_Name"].ToString();
                string itemDetail = row["Item_Detail"].ToString();
                DateTime checkDate = Convert.ToDateTime(row["Check_Date"]);
                string itemValue = row["Item_Value"].ToString();

                var check = dailyChecks.Find(c => c.ItemID == itemId);
                if (check == null)
                {
                    check = new DailyCheck
                    {
                        ItemID = itemId,
                        ItemName = itemName,
                        ItemDetail = itemDetail,
                        DailyChecks = new Dictionary<int, string>()
                    };
                    dailyChecks.Add(check);
                }

                // 將檢查結果填入對應的日期
                int day = checkDate.Day;
                check.DailyChecks[day] = itemValue;
            }

            // 填充空值為 "-"
            foreach (var check in dailyChecks)
            {
                for (int day = 1; day <= 31; day++)
                {
                    if (!check.DailyChecks.ContainsKey(day))
                    {
                        check.DailyChecks[day] = "-";
                    }
                }
            }

            return dailyChecks;
        }

        protected void gvDailyCheck_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                DateTime selectedMonth = DateTime.ParseExact(monthPicker.Value, "yyyy-MM", null);
                int daysInMonth = DateTime.DaysInMonth(selectedMonth.Year, selectedMonth.Month);

                for (int day = 1; day <= daysInMonth; day++)
                {
                    DateTime currentDate = new DateTime(selectedMonth.Year, selectedMonth.Month, day);
                    if (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday)
                    {
                        e.Row.Cells[day + 2].BackColor = System.Drawing.Color.DarkGray;
                    }
                }
            }
        }

        // 獲取星期英文縮寫
        public string GetDayOfWeekAbbreviation(int year, int month, int day)
        {
            DateTime currentDate = new DateTime(year, month, day);
            return currentDate.ToString("ddd");
        }

        protected void btnExportToPDF_Click(object sender, EventArgs e)
        {
            // 創建 PDF 文檔
            Document document = new Document(PageSize.A4.Rotate(), 10f, 10f, 10f, 10f);
            MemoryStream memoryStream = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            // 添加表格
            AddGridToPDF(document, gvDailyCheck, "Daily Check Report");

            document.Close();

            // 將 PDF 發送到瀏覽器
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=IT_DailyCheckReport.pdf");
            Response.BinaryWrite(memoryStream.ToArray());
            Response.End();
        }

        private void AddGridToPDF(Document document, GridView gridView, string title)
        {
            // 添加標題
            document.Add(new Paragraph(title, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14)));

            // 創建表格
            PdfPTable table = new PdfPTable(gridView.Columns.Count);
            table.WidthPercentage = 100;

            // 添加表頭
            foreach (DataControlField column in gridView.Columns)
            {
                table.AddCell(new Phrase(column.HeaderText));
            }

            // 添加數據行
            foreach (GridViewRow row in gridView.Rows)
            {
                foreach (TableCell cell in row.Cells)
                {
                    table.AddCell(new Phrase(cell.Text));
                }
            }

            // 將表格添加到文檔
            document.Add(table);
            document.Add(new Paragraph("\n")); // 添加空行
        }
    }

    public class DailyCheck
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemDetail { get; set; }
        public Dictionary<int, string> DailyChecks { get; set; }
    }

    // 自定義 TemplateField 的 ItemTemplate
    public class DayTemplate : ITemplate
    {
        private int _day;

        public DayTemplate(int day)
        {
            _day = day;
        }

        public void InstantiateIn(Control container)
        {
            // 使用 Label 顯示每日資料
            Label lblDay = new Label
            {
                ID = "lblDay" + _day
            };
            lblDay.DataBinding += (sender, e) =>
            {
                // 在 DataBinding 事件中從容器的資料獲取值
                GridViewRow row = (GridViewRow)container.NamingContainer;
                var dailyChecks = DataBinder.Eval(row.DataItem, "DailyChecks") as Dictionary<int, string>;
                if (dailyChecks != null && dailyChecks.ContainsKey(_day))
                {
                    lblDay.Text = dailyChecks[_day]; // 將對應的值顯示在 Label 上
                }
                else
                {
                    lblDay.Text = "-"; // 如果沒有值，顯示預設值
                }
            };

            container.Controls.Add(lblDay);
        }
    }
}