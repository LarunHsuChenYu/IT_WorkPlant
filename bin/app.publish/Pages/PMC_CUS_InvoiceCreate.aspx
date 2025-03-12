<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" 
    CodeBehind="PMC_CUS_InvoiceCreate.aspx.cs" Inherits="IT_WorkPlant.Pages.PMC_CUS_InvoiceCreate" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        /* Flexbox 容器 */
        .grid-container {
            display: flex;
            justify-content: space-between;
            gap: 20px;
            margin-top: 20px;
        }

        /* Flexbox 項目 */
        .grid-item {
            flex: 1;
            display: flex;
            flex-direction: column;
            align-items: center;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 5px;
            background-color: #f9f9f9;
        }

        /* 標題樣式 */
        .grid-item h3 {
            margin-bottom: 10px;
            color: #333;
            font-weight: bold;
        }

        /* 通用樣式 */
        body {
            font-family: Arial, sans-serif;
            margin: 20px;
            background-color: #f9f9f9;
            color: #333;
        }

        /* 標題樣式 */
        .header-title {
            font-size: 24px;
            font-weight: bold;
            text-align: left;
            margin-bottom: 20px;
            color: black;
        }

        /* 表單容器 */
        .form-container {
            width: 50%;
            margin: 0 auto;
            background: #ffffff;
            padding: 20px;
            border: 1px solid #ddd;
            border-radius: 5px;
            box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
        }

        /* 表單項目 */
        .form-group {
            margin-bottom: 15px;
        }

        label {
            font-weight: bold;
            display: block;
            margin-bottom: 5px;
        }

        .form-control {
            width: 100%;
            padding: 8px;
            border: 1px solid #ccc;
            border-radius: 4px;
            font-size: 14px;
        }

        .calendar-control {
            margin-top: 10px;
            border: 1px solid #ccc;
            border-radius: 4px;
        }

        /* 按鈕樣式 */
        .btn {
            padding: 10px 20px;
            background-color: #007bff;
            color: white;
            border: none;
            border-radius: 4px;
            cursor: pointer;
            font-size: 14px;
            display: inline-block;
        }

        .btn-primary:hover {
            background-color: #0056b3;
        }

        /* 狀態標籤 */
        .status-label {
            display: block;
            margin-top: 10px;
            color: #28a745;
            font-weight: bold;
        }

    </style>

    <h2 class="header-title">PMC - Invoice Creation (關務貨櫃發票與打包清單)</h2>

    <div class="form-container">
        <!-- Invoice No -->
        <div class="form-group">
            <label for="tbInvNo">Invoice No:</label>
            <asp:TextBox ID="tbInvNo" CssClass="form-control" runat="server"></asp:TextBox>
        </div>

        <!-- Date -->
        <div class="form-group">
            <label for="tbDate">Date:</label>
            <asp:TextBox ID="tbDate" CssClass="form-control" runat="server" onclick="showCalendar()"></asp:TextBox>
            <asp:Calendar ID="Calendar1" runat="server" OnSelectionChanged="Calendar1_SelectionChanged" style="display:none;" CssClass="calendar-control"></asp:Calendar>
        </div>

        <!-- Vessel/Voyage -->
        <div class="form-group">
            <label for="tbVessel">Vessel/Voyage:</label>
            <asp:TextBox ID="tbVessel" CssClass="form-control" runat="server"></asp:TextBox>
        </div>

        <!-- File Upload -->
        <div class="form-group">
            <label for="fileUploadPlan">Upload Plan:</label>
            <asp:FileUpload ID="fileUploadPlan" CssClass="form-control" runat="server" />
        </div>

        <!-- Process Button -->
        <div class="form-group">
            <asp:Button ID="btnProcessPlan" runat="server" Text="Process Plan" CssClass="btn btn-primary" OnClick="btnProcessPlan_Click" />
        </div>

        <!-- Status Label -->
        <div class="form-group">
            <asp:Label ID="lblStatus" runat="server" CssClass="status-label" Text=""></asp:Label>
        </div>
    </div>

    <script>
        // 顯示日曆
        function showCalendar() {
            var calendar = document.getElementById('<%= Calendar1.ClientID %>');
            if (calendar) {
                calendar.style.display = 'block'; // 顯示日曆
            }
        }
    </script>


    
    <!-- Flexbox 容器 -->
    <div class="grid-container">
        <!-- Invoice Section -->
        <div class="grid-item">
            <h3>Invoice</h3>
            <asp:GridView ID="gvINV" runat="server" AutoGenerateColumns="true" CssClass="table table-striped" />
            <asp:Button ID="btnGenerate_INV_PDF" runat="server" Text="Generate Invoice" OnClick="btnGenerate_INV_PDF_Click" Visible="false" />
        </div>

        <!-- Packing List Section -->
        <div class="grid-item">
            <h3>Packing List</h3>
            <asp:GridView ID="gvPLS" runat="server" AutoGenerateColumns="true" CssClass="table table-striped" />
            <asp:Button ID="btnGenerate_PLS_PDF" runat="server" Text="Generate Packing List" OnClick="btnGenerate_PLS_PDF_Click" Visible="false" />
        </div>
    </div>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    
</asp:Content>





