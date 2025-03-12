<%@ Page Title="Login" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="IT_WorkPlant.Login" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main>
        <h2 style="text-align: center;">
        <img src="Image/login.gif" alt="Logo" style="width:150px; height: 150px;"/>
        </h2>
        <div style="text-align: center;">
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>
            <br />
            <img src="Image/profile.gif" alt="Logo" style="width:50px; height: 50px;"/> Employ ID:
            <br />
            <asp:TextBox ID="txtUserID" runat="server"></asp:TextBox>
            <br />
            <img src="Image/password.gif" alt="Logo" style="width:50px; height: 50px;"/> Password:
            <br />
            <asp:TextBox ID="txtPassword" runat="server" TextMode="Password"></asp:TextBox>
            <br />
            <br />
            <asp:Button ID="btnLogin" runat="server" Text="OK" OnClick="btnLogin_Click" style="font-size: 20px; padding: 10px 20px;"/>
        </div>
    </main>
</asp:Content>
