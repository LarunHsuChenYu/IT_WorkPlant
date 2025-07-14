using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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
            {
                string sql = @"INSERT INTO IT_ComputerList
               (NamePC, UserID, UserName, DeptName, Tyb, Brand, Model, SerialNumber, Warranty, System, ColumnCode, Location, Status, AssetCode, PurchaseDate, Price)
               VALUES
               (@NamePC, @UserID, @UserName, @DeptName, @Tyb, @Brand, @Model, @SerialNumber, @Warranty, @System, @ColumnCode, @Location, @Status, @AssetCode, @PurchaseDate, @Price)";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@NamePC", txtNamePC.Text.Trim());
                cmd.Parameters.AddWithValue("@UserID", string.IsNullOrWhiteSpace(txtUserID.Text) ? DBNull.Value : (object)txtUserID.Text.Trim());
                cmd.Parameters.AddWithValue("@UserName", txtUserName.Text.Trim());
                cmd.Parameters.AddWithValue("@DeptName", txtDeptName.Text.Trim());
                cmd.Parameters.AddWithValue("@Tyb", type);
                cmd.Parameters.AddWithValue("@Brand", txtBrand.Text.Trim());
                cmd.Parameters.AddWithValue("@Model", txtModel.Text.Trim());
                cmd.Parameters.AddWithValue("@SerialNumber", txtSerialNumber.Text.Trim());
                string warranty = ddlWarranty.SelectedValue == "Yes" ? txtWarrantyYears.Text.Trim() : "No";
                cmd.Parameters.AddWithValue("@Warranty", warranty);
                cmd.Parameters.AddWithValue("@System", ddlSystem.SelectedValue);
                cmd.Parameters.AddWithValue("@Status", ddlStatus.SelectedValue);
                cmd.Parameters.AddWithValue("@ColumnCode", txtColumnCode.Text.Trim());
                cmd.Parameters.AddWithValue("@Location", txtLocation.Text.Trim());   
                cmd.Parameters.AddWithValue("@AssetCode", txtAssetCode.Text.Trim()); 

                DateTime purchaseDate;
                if (DateTime.TryParse(txtPurchaseDate.Text.Trim(), out purchaseDate))
                {
                    cmd.Parameters.AddWithValue("@PurchaseDate", purchaseDate);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@PurchaseDate", DBNull.Value);
                }

                decimal price;
                if (decimal.TryParse(txtPrice.Text.Trim(), out price))
                {
                    cmd.Parameters.AddWithValue("@Price", price);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Price", DBNull.Value);
                }

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();

                    // แจ้งเตือนสำเร็จ
                    string script = "Swal.fire('สำเร็จ!','บันทึกข้อมูลเรียบร้อยแล้ว','success')";
                    ScriptManager.RegisterStartupScript(this, GetType(), "SuccessAlert", script, true);
                }
                catch (Exception ex)
                {
                    string script = $"Swal.fire('Error','{ex.Message}','error')";
                    ScriptManager.RegisterStartupScript(this, GetType(), "ErrorAlert", script, true);
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
            {
                string sql = "SELECT UserName, DeptName FROM Users WHERE UserEmpID = @UserID";

                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@UserID", userId);

                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        txtUserName.Text = reader["UserName"].ToString();
                        txtDeptName.Text = reader["DeptName"].ToString();
                    }
                    else
                    {
                        txtUserName.Text = "";
                        txtDeptName.Text = "";

                        string script = "Swal.fire('ไม่พบข้อมูล','ไม่พบรหัสพนักงานนี้ในระบบ','warning')";
                        ScriptManager.RegisterStartupScript(this, GetType(), "NotFound", script, true);
                    }
                }
                catch (Exception ex)
                {
                    string script = $"Swal.fire('Error','{ex.Message}','error')";
                    ScriptManager.RegisterStartupScript(this, GetType(), "ErrorLookup", script, true);
                }
            }
        }
        protected void ddlWarranty_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlWarranty.SelectedValue == "Yes")
            {
                pnlWarrantyPeriod.Visible = true;
            }
            else
            {
                pnlWarrantyPeriod.Visible = false;
                txtWarrantyYears.Text = ""; // เคลียร์ข้อมูล
            }
        }

    }
}