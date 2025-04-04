<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" 
    CodeBehind="PMC_WO_HeadUpdate.aspx.cs" Inherits="IT_WorkPlant.Pages.PMC_WO_HeadUpdate" %>

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
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <asp:FileUpload ID="FileUpload1" runat="server" />
    <asp:UpdatePanel ID="UpdatePanel1" runat="server">
        <ContentTemplate>
            <asp:Button ID="btnUpload" runat="server" Text="Upload" OnClick="btnUpload_Click" />
            <br /><br />
            <asp:Label ID="lblStatus" runat="server" Text="" /><br />
            <asp:Label ID="lblProgress" runat="server" Text="" /><br />
            <div class="progress-container">
                <div id="progressBar" class="progress-bar" runat="server"></div>
            </div>
            <br />
            <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="true" CssClass="table-style" />
            <asp:Timer ID="Timer1" runat="server" Interval="500" OnTick="Timer1_Tick" Enabled="false" />
        </ContentTemplate>
        <Triggers>
            <asp:PostBackTrigger ControlID="btnUpload" />
        </Triggers>
    </asp:UpdatePanel>
</asp:Content>