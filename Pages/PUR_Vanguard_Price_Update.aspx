
<%@ Page Language="C#" Async="true" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PUR_Vanguard_Price_Update.aspx.cs" Inherits="IT_WorkPlant.Pages.PUR_Vanguard_Price_Update" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .progress-container {
            width: 300px;
            height: 20px;
            background-color: #e0e0e0;
            border-radius: 10px;
            overflow: hidden;
        }
        .progress-bar {
            height: 100%;
            background-color: #4CAF50;
            width: 0%;
            transition: width 0.3s ease-in-out;
        }
        .table-style {
            border-collapse: collapse;
            width: 100%;
            font-family: 'Segoe UI', sans-serif;
        }
        .table-style th {
            background-color: #4CAF50;
            color: white;
            padding: 8px;
            text-align: left;
        }
        .table-style td {
            padding: 8px;
            border-bottom: 1px solid #ddd;
        }
        .table-style tr:nth-child(even) {
            background-color: #f9f9f9;
        }
        .table-style tr:hover {
            background-color: #f1f1f1;
        }
        .upload-box {
    margin: 20px 0;
    padding: 20px;
    border: 2px dashed #ced4da;
    border-radius: 12px;
    background-color: #ffffff;
    text-align: center;
    width: 320px;
}

.custom-file-upload {
    display: inline-block;
    padding: 10px 20px;
    cursor: pointer;
    background-color: #0d6efd;
    color: white;
    border-radius: 8px;
    font-weight: 600;
    margin-bottom: 15px;
    font-size: 16px;
}

.custom-file-upload:hover {
    background-color: #0b5ed7;
}

input[type="file"] {
    display: none;
}

.btn-upload {
    background-color: #198754;
    color: white;
    border: none;
    padding: 10px 20px;
    border-radius: 8px;
    cursor: pointer;
    font-weight: 600;
    font-size: 16px;
}

.btn-upload:hover {
    background-color: #157347;
}

    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div style="display: flex; align-items: center; gap: 16px; margin-bottom: 24px;">
        <img src="/Image/price-up.gif" alt="Price Up Icon" style="height: 60px;" />
        <span style="font-size: 32px; color: #0d6efd; font-weight: 800;">
            PUR Vanguard Price Update
        </span>
    </div>
    <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <div class="upload-box">
                <label class="custom-file-upload">
                    <asp:FileUpload ID="FileUpload1" runat="server" />
                    📁 Choose File
                </label>
                <div id="fileNameDisplay" style="margin-top: 10px; font-weight: 500; color: #333;"></div>
                <br />
                <asp:Button ID="btnUpload" runat="server" Text="⬆ Upload" CssClass="btn-upload" OnClick="btnUpload_Click" />
            </div>

            <br />
            <asp:Label ID="lblStatus" runat="server" Text="" /><br />
            <asp:Label ID="lblProgress" runat="server" Text="" /><br />

            <div class="progress-container">
                <div id="progressBar" class="progress-bar" runat="server"></div>
            </div>

            <br />

            <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="true" CssClass="table-style" />

            <asp:Timer ID="Timer1" runat="server" Interval="500" OnTick="Timer1_Tick" Enabled="false" />

            <asp:Button ID="btnConfirmSubmit" runat="server" 
                Text="✅ Confirm Task Done" 
                OnClick="btnConfirmSubmit_Click" 
                CssClass="btn btn-success mt-3"
                Visible="false" />

        </ContentTemplate>
        <Triggers>
    <asp:PostBackTrigger ControlID="btnUpload" />
    <asp:PostBackTrigger ControlID="btnConfirmSubmit" />
</Triggers>

    </asp:UpdatePanel>
    <script>
        document.addEventListener("DOMContentLoaded", function () {
            const fileInput = document.querySelector('input[type="file"]');
            const fileNameDisplay = document.getElementById('fileNameDisplay');

            fileInput.addEventListener('change', function () {
                if (fileInput.files.length > 0) {
                    fileNameDisplay.textContent = "📄 " + fileInput.files[0].name;
                } else {
                    fileNameDisplay.textContent = "";
                }
            });
        });
    </script>

</asp:Content>

