<%@ Page Title="IT Daily Check Report" Language="C#" MasterPageFile="~/Site.Master" 
    AutoEventWireup="true" CodeBehind="IT_DailyCheckReport.aspx.cs" 
    Inherits="IT_WorkPlant.Pages.IT_DailyCheckReport" %>

<asp:Content ID="HeadStyle" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .grid-view {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
        }
        .grid-view th, .grid-view td {
            border: 1px solid #ccc;
            padding: 8px;
            text-align: center;
        }
        .grid-view th {
            background-color: #f2f2f2;
        }
        .btn-export {
            margin-top: 20px;
            text-align: center;
        }
        .date-picker-container {
            margin-bottom: 20px;
            text-align: center;
        }
    </style>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <!-- ✅ ห่อ HTML เดิมไว้ใน Content แต่ไม่ลบโครงสร้างเดิม -->
    <!DOCTYPE html>
    <html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>IT Daily Check Report</title>
    </head>
    <body>
        <form id="form1" runat="server">
            <h2>Enrich Industrial Thai Co. Ltd</h2>
            <h2 class="text-center">IT Daily Check Report</h2>
            <h4>FY-FM-WI-064-07-AO</h4>

            <div class="date-picker-container">
                <label for="monthPicker">Please select a month to view the report.</label>
                <input type="month" id="monthPicker" name="monthPicker" runat="server"/>
                <asp:Button ID="btnLoadReport" runat="server" Text="Load Report" CssClass="btn btn-primary" OnClick="btnLoadReport_Click" />
            </div>

            <asp:GridView ID="gvDailyCheck" runat="server" AutoGenerateColumns="false" CssClass="grid-view" OnRowDataBound="gvDailyCheck_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="ItemID" HeaderText="Item ID" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" />
                    <asp:BoundField DataField="ItemName" HeaderText="Item Name" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-left" />
                    <asp:BoundField DataField="ItemDetail" HeaderText="Item Detail" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-left" />
                </Columns>
            </asp:GridView>

            <div class="btn-export">
                <asp:Button ID="btnExportToPDF" runat="server" Text="Export to PDF" CssClass="btn btn-primary" OnClick="btnExportToPDF_Click" />
            </div>
        </form>
    </body>
    </html>

</asp:Content>
