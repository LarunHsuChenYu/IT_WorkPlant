<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true"
    CodeBehind="IT_erpNewUserCreate.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_erpNewUserCreate" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    
    <script type="text/javascript">
        function toggleEnv(val)
        {
            document.getElementById('<%= pnlEnv.ClientID %>').style.display = (val === "IT" ? "block" : "none");
        }
    </script>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <div class="title-header">
        <img src="/Image/erp_color.png" alt="form Icon" style="width: 90px; height: 90px;" />
        <asp:Label ID="lblTitle" runat="server" Text='<%= GetLabel("title") %>' CssClass="wizard-header" />
    </div>

    <asp:ValidationSummary ID="ValidationSummary1" runat="server"
        HeaderText='<%= GetLabel("validation_header") %>' CssClass="text-danger" />

    <asp:Label ID="lblMsg" runat="server" ForeColor="Red" /><br /><br />

    <asp:Label ID="lblUserName" runat="server"
        Text='<%= GetLabel("username") %>' AssociatedControlID="txtUserName" /><br />
    <asp:TextBox ID="txtUserName" runat="server" CssClass="form-control" />
    <asp:RequiredFieldValidator ID="rfvUserName" runat="server"
        ControlToValidate="txtUserName"
        ErrorMessage='<%= GetLabel("username_required") %>'
        Display="Dynamic" CssClass="text-danger" /><br /><br />

    <asp:Label ID="lblRole" runat="server"
        Text='<%= GetLabel("role") %>' AssociatedControlID="ddlRole" /><br />
    <asp:DropDownList ID="ddlRole" runat="server"
        CssClass="form-control" AutoPostBack="true"
        OnSelectedIndexChanged="ddlRole_SelectedIndexChanged">
        <asp:ListItem Value="normal" />
        <asp:ListItem Value="IT" />
    </asp:DropDownList><br /><br />

    <asp:Panel ID="pnlEnv" runat="server" Visible="false">
        <asp:Label ID="lblEnv" runat="server"
            Text='<%= GetLabel("environment") %>' AssociatedControlID="ddlEnv" /><br />
        <asp:DropDownList ID="ddlEnv" runat="server" CssClass="form-control">
            <asp:ListItem Value="prod" />
            <asp:ListItem Value="test" />
        </asp:DropDownList><br /><br />
    </asp:Panel>

    <asp:CheckBox ID="cbAddDba" runat="server"
        Text='<%= GetLabel("add_dba_group") %>' /><br /><br />

    <asp:Button ID="btnSubmit" runat="server"
        Text='<%= GetLabel("create_account") %>' CssClass="btn btn-primary"
        OnClick="btnSubmit_Click" /><br /><br />
</asp:Content>
