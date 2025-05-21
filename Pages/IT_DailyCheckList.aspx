<%@ Page Title="IT Daily Check List" Language="C#" MasterPageFile="~/Site.Master" 
    AutoEventWireup="true" CodeBehind="IT_DailyCheckList.aspx.cs" 
    Inherits="IT_WorkPlant.Pages.IT_DailyCheckList" %>

    
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="https://code.jquery.com/ui/1.13.2/themes/base/jquery-ui.css">
    <script src="https://code.jquery.com/jquery-3.7.0.js"></script>
    <script src="https://code.jquery.com/ui/1.13.2/jquery-ui.js"></script>
    <title>IT Daily Check List</title>
    <script>
        function showTab(tabId) {
            document.querySelectorAll('.tab-content').forEach(tab => { tab.classList.remove('active'); });
            document.getElementById(tabId).classList.add('active');
            document.getElementById('<%= hfActiveTab.ClientID %>').value = tabId;
        }

    </script>
    <style>
        .tabs {
            display: flex;
            border: 1px solid #ccc; 
            border-radius: 10px; 
            overflow: hidden; 
        }
        .tab {
            flex-grow: 1;
            text-align: center;
            padding: 10px;
            background-color: #aaa2a2e7;
            cursor: pointer;
            border: none; 
            box-sizing: border-box;
            border-right: 1px solid #ccc; 
        }
        .tab:last-child {
            border-right: none;
        }
        .tab:hover { 
            background-color: #808080;
            color: white;
        }
        .tab.active { 
            background-color: #888282e7;
        }
        .tab-content { display: none; padding: 20px; border: 1px solid #ccc; margin-top: 10px; border-radius: 5px;}
        .active { display: block; }

    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2 class="text-center">
        IT Daily Check List - <%= DateTime.Now.ToString("yyyy/MM/dd") %>
    </h2>
    <p class="text-muted text-center">Please complete the checklist below and save your results.</p>
    <asp:Panel ID="pIT_DailyCheck" runat="server">
        <div class="tabs">
            <div class="tab" onclick="showTab('divServerRoom')">Server Room Check</div>
            <div class="tab" onclick="showTab('divUPSStatus')">UPS Status Check</div>
        </div>
        <div id="divServerRoom" class="tab-content active">
            <asp:GridView ID="gvServerRoomCheck" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered table-striped table-hover" ShowFooter="true">
                <Columns>
                    <asp:BoundField DataField="Id" HeaderText="No." HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" />
                    <asp:BoundField DataField="Item" HeaderText="Item" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-left" />
                    <asp:BoundField DataField="Details" HeaderText="Details" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-left" />
                    <asp:TemplateField HeaderText="Status">
                        <HeaderStyle CssClass="text-center" />
                        <ItemTemplate>
                            <!-- 如果 IsNumericInput 是 true，顯示 TextBox -->
                            <asp:PlaceHolder ID="phTextBox" runat="server" Visible='<%# Eval("IsNumericInput") %>'>
                                <asp:TextBox ID="txtNumericInput" runat="server" CssClass="form-control" />
                            </asp:PlaceHolder>

                            <!-- 如果 IsNumericInput 是 false，顯示 RadioButtonList -->
                            <asp:PlaceHolder ID="phRadioButtonList" runat="server" Visible='<%# !(Convert.ToBoolean(Eval("IsNumericInput"))) %>'>
                                <asp:RadioButtonList ID="rblStatus" runat="server" RepeatDirection="Horizontal" CssClass="form-check-inline">
                                    <asp:ListItem Text="PASS" Value="1" />
                                    <asp:ListItem Text="FAIL" Value="0" />
                                </asp:RadioButtonList>
                            </asp:PlaceHolder>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Remarks">
                        <HeaderStyle CssClass="text-center" />
                        <ItemTemplate>
                            <asp:TextBox ID="txtRemarks" runat="server" CssClass="form-control" Placeholder="Add remarks here..." />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
        <div id="divUPSStatus" class="tab-content">
            <asp:GridView ID="gvUPSCheck" runat="server" AutoGenerateColumns="false" CssClass="table table-bordered table-striped table-hover" ShowFooter="true">
                <Columns>
                    <asp:BoundField DataField="Id" HeaderText="No." HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-center" />
                    <asp:BoundField DataField="Item" HeaderText="Item" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-left" />
                    <asp:BoundField DataField="Details" HeaderText="Details" HeaderStyle-CssClass="text-center" ItemStyle-CssClass="text-left" />
                    <asp:TemplateField HeaderText="Status">
                        <HeaderStyle CssClass="text-center" />
                        <ItemTemplate>
                            <!-- 如果 IsNumericInput 是 true，顯示 TextBox -->
                            <asp:PlaceHolder ID="phTextBox" runat="server" Visible='<%# Eval("IsNumericInput") %>'>
                                <asp:TextBox ID="txtNumericInput" runat="server" CssClass="form-control" />
                            </asp:PlaceHolder>

                            <!-- 如果 IsNumericInput 是 false，顯示 RadioButtonList -->
                            <asp:PlaceHolder ID="phRadioButtonList" runat="server" Visible='<%# !(Convert.ToBoolean(Eval("IsNumericInput"))) %>'>
                                <asp:RadioButtonList ID="rblStatus" runat="server" RepeatDirection="Horizontal" CssClass="form-check-inline">
                                    <asp:ListItem Text="PASS" Value="1" />
                                    <asp:ListItem Text="FAIL" Value="0" />
                                </asp:RadioButtonList>
                            </asp:PlaceHolder>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Remarks">
                        <HeaderStyle CssClass="text-center" />
                        <ItemTemplate>
                            <asp:TextBox ID="txtRemarks" runat="server" CssClass="form-control" Placeholder="Add remarks here..." />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <asp:HiddenField ID="hfActiveTab" runat="server" />
        
    </asp:Panel>
    
    <div class="text-center mt-3">
        <asp:Button ID="btnSave" runat="server" Text="Save Results" CssClass="btn btn-success mx-2" OnClick="btnSave_Click"/>
    </div>

    <div class="text-center mt-3">
        <asp:Button ID="btnOpenReport" runat="server" Text="Open Daily Check Report" 
            OnClientClick="window.open('IT_DailyCheckReport.aspx', '_blank', 'width=800,height=600'); return false;" 
            CssClass="btn btn-info" />
    </div>
</asp:Content>
