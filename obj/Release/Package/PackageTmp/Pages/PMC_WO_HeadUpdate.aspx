<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" 
    CodeBehind="PMC_WO_HeadUpdate.aspx.cs" Inherits="IT_WorkPlant.Pages.PMC_WO_HeadUpdate" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>PMC - WO Head Update(單頭刷新)</h2>
    
    <!-- 引入 ScriptManagerProxy -->
    <asp:ScriptManagerProxy ID="ScriptManagerProxy1" runat="server" />
    
    <!-- 進度條 -->
    <div class="progress" style="width: 50%;">
        <div id="progressBar" class="progress-bar progress-bar-striped" role="progressbar" aria-valuemin="0" aria-valuemax="100" style="width: 0%;">
            <span id="progressLabel">0%</span>
        </div>
    </div>
    <br />

    <!-- 文件上傳 -->
    <asp:FileUpload ID="FileUpload1" runat="server" />
    <asp:Button ID="btnUpload" runat="server" Text="Upload" OnClick="btnUpload_Click" CssClass="btn btn-primary" />
            
    <!-- 狀態標籤 -->
    <asp:Label ID="lblStatus" runat="server" Text="" CssClass="text-info"></asp:Label>

    <!-- 資料顯示 -->
    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="true" CssClass="table table-striped" />
       
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- 引入 jQuery & Bootstrap -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" />

    <script type="text/javascript">
        function updateProgressBar(percentage) {
            $('#progressBar').css('width', percentage + '%');
            $('#progressLabel').text(percentage + '%');
        }
    </script>
</asp:Content>