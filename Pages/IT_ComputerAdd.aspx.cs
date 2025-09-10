using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace IT_WorkPlant.Pages
{
    public partial class IT_ComputerAdd : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string connStr = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
            string type = rdbPC.Checked ? "PC" : "NB";

            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                // *** ตัด System, Location, PurchaseDate, Price ออก ***
                cmd.CommandText = @"
INSERT INTO IT_ComputerList
    (NamePC, UserID, UserName, DeptName, Tyb, Brand, Model, SerialNumber, Warranty, ColumnCode, Status, AssetCode)
VALUES
    (@NamePC, @UserID, @UserName, @DeptName, @Tyb, @Brand, @Model, @SerialNumber, @Warranty, @ColumnCode, @Status, @AssetCode);";

                // NamePC
                cmd.Parameters.Add(new SqlParameter("@NamePC", SqlDbType.NVarChar, 100)
                { Value = (object)txtNamePC.Text.Trim() ?? DBNull.Value });

                // UserID ใน DB เป็น int → แปลง, ถ้าไม่ใช่ตัวเลขให้ NULL
                var pUserId = new SqlParameter("@UserID", SqlDbType.Int);
                if (int.TryParse(txtUserID.Text.Trim(), out int uid))
                    pUserId.Value = uid;
                else
                    pUserId.Value = DBNull.Value;
                cmd.Parameters.Add(pUserId);

                cmd.Parameters.Add(new SqlParameter("@UserName", SqlDbType.NVarChar, 100)
                { Value = (object)txtUserName.Text.Trim() ?? DBNull.Value });

                cmd.Parameters.Add(new SqlParameter("@DeptName", SqlDbType.NVarChar, 100)
                { Value = (object)txtDeptName.Text.Trim() ?? DBNull.Value });

                cmd.Parameters.Add(new SqlParameter("@Tyb", SqlDbType.NVarChar, 10) { Value = type });

                cmd.Parameters.Add(new SqlParameter("@Brand", SqlDbType.NVarChar, 100)
                { Value = (object)txtBrand.Text.Trim() ?? DBNull.Value });

                cmd.Parameters.Add(new SqlParameter("@Model", SqlDbType.NVarChar, 100)
                { Value = (object)txtModel.Text.Trim() ?? DBNull.Value });

                cmd.Parameters.Add(new SqlParameter("@SerialNumber", SqlDbType.NVarChar, 100)
                { Value = (object)txtSerialNumber.Text.Trim() ?? DBNull.Value });

                // Warranty: ถ้าเลือก Yes เก็บจำนวนปี, ไม่ใช่ → "No"
                string warranty = ddlWarranty.SelectedValue == "Yes" ? txtWarrantyYears.Text.Trim() : "No";
                cmd.Parameters.Add(new SqlParameter("@Warranty", SqlDbType.NVarChar, 50) { Value = warranty });

                cmd.Parameters.Add(new SqlParameter("@ColumnCode", SqlDbType.NVarChar, 50)
                { Value = (object)txtColumnCode.Text.Trim() ?? DBNull.Value });

                cmd.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 50)
                { Value = (object)ddlStatus.SelectedValue ?? DBNull.Value });

                cmd.Parameters.Add(new SqlParameter("@AssetCode", SqlDbType.NVarChar, 100)
                { Value = (object)txtAssetCode.Text.Trim() ?? DBNull.Value });

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    ScriptManager.RegisterStartupScript(this, GetType(), "SuccessAlert",
                        "Swal.fire('สำเร็จ!','บันทึกข้อมูลเรียบร้อยแล้ว','success');", true);
                }
                catch (Exception ex)
                {
                    string msg = ex.Message.Replace("'", "\\'");
                    ScriptManager.RegisterStartupScript(this, GetType(), "ErrorAlert",
                        $"Swal.fire('Error','{msg}','error');", true);
                }
            }
        }

        protected void txtUserID_TextChanged(object sender, EventArgs e)
        {
            string userId = txtUserID.Text.Trim();
            if (string.IsNullOrEmpty(userId))
            {
                txtUserName.Text = "";
                txtDeptName.Text = "";
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["EnrichDB"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            using (SqlCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT UserName, DeptName FROM Users WHERE UserEmpID = @UserID";
                cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.NVarChar, 50) { Value = userId });

                try
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            txtUserName.Text = reader["UserName"].ToString();
                            txtDeptName.Text = reader["DeptName"].ToString();
                        }
                        else
                        {
                            txtUserName.Text = "";
                            txtDeptName.Text = "";
                            ScriptManager.RegisterStartupScript(this, GetType(), "NotFound",
                                "Swal.fire('ไม่พบข้อมูล','ไม่พบรหัสพนักงานนี้ในระบบ','warning');", true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string msg = ex.Message.Replace("'", "\\'");
                    ScriptManager.RegisterStartupScript(this, GetType(), "ErrorLookup",
                        $"Swal.fire('Error','{msg}','error');", true);
                }
            }
        }

        protected void ddlWarranty_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool visible = ddlWarranty.SelectedValue == "Yes";
            pnlWarrantyPeriod.Visible = visible;
            if (!visible) txtWarrantyYears.Text = "";
        }
    }
}
