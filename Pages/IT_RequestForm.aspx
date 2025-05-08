<%@ Page Title="IT Request Form" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true"
    CodeBehind="IT_RequestForm.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_RequestForm" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .form-container {
            max-width: 1000px;
            margin: auto;
            padding: 20px;
            border: 1px solid #ddd;
            border-radius: 8px;
            background-color: #f9f9f9;
            box-shadow: 0px 6px 10px rgba(0, 0, 0, 0.1);
        }

        .form-container h2 {
            text-align: center;
            color: #333;
            margin-bottom: 20px;
        }

        .form-group {
            margin-bottom: 15px;
            width: 100%;
        }

        .form-group label {
            display: block;
            font-weight: bold;
            margin-bottom: 5px;
        }

        .form-row {
            display: flex;
            gap: 20px;
            width: 100%;
        }

        .form-row .form-group {
            flex: 1;
        }

        .table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }

        .table th, .table td {
            border: 1px solid #ccc;
            padding: 8px;
            text-align: left;
        }

        .btn-container {
            text-align: center;
            margin-top: 20px;
        }

        .btn-container .btn {
            padding: 10px 20px;
            font-size: 16px;
            border: none;
            border-radius: 5px;
            cursor: pointer;
        }

        .btn-primary {
            background-color: #007bff;
            color: white;
        }

        .btn-primary:hover {
            background-color: #0056b3;
        }

        .btn-secondary {
            background-color: #6c757d;
            color: white;
        }

        .btn-secondary:hover {
            background-color: #5a6268;
        }
    </style>

    <div class="form-container">
        <h2>IT Request Form</h2>

        <!-- User Info -->
        <div class="form-row">
            <div class="form-group">
                <label for="txtName">Requestor Name:</label>
                <asp:TextBox ID="txtName" runat="server" CssClass="form-control" ReadOnly="true" />
            </div>
            <div class="form-group">
                <label for="txtDept">Department:</label>
                <asp:TextBox ID="txtDept" runat="server" CssClass="form-control" ReadOnly="true" />
            </div>
            <div class="form-group">
                <label for="txtDate">Issue Date:</label>
                <asp:TextBox ID="txtDate" runat="server" CssClass="form-control" ReadOnly="true" />
            </div>
        </div>

        <!-- Request Items Table -->
        <asp:Repeater ID="rptRequestItems" runat="server" OnItemDataBound="rptRequestItems_ItemDataBound">
            <HeaderTemplate>
                <table class="table">
                    <thead>
                        <tr>
                            <th>No.</th>
                            <th>Issue Type</th>
                            <th>Issue Description</th>
                            <th>Attachment (Image)</th>
                        </tr>
                    </thead>
                    <tbody>
            </HeaderTemplate>
            <ItemTemplate>
                <tr>
                    <td><%# Container.ItemIndex + 1 %></td>
                    <td>
                        <asp:DropDownList ID="ddlIssueType" runat="server" CssClass="form-control" />
                    </td>
                    <td>
                        <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" />
                    </td>
                    <td>
                        <asp:FileUpload ID="fileUploadImage" runat="server" CssClass="form-control" />
                    </td>
                </tr>
            </ItemTemplate>
            <FooterTemplate>
                    </tbody>
                </table>
            </FooterTemplate>
        </asp:Repeater>

        <!-- Buttons -->
        <div class="btn-container">
            <asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" Text="Submit Request" OnClick="SubmitForm" />
            <asp:Button ID="btnCancel" runat="server" CssClass="btn btn-secondary" Text="Cancel" OnClick="CancelForm" />
            </div>


        <!-- Hidden for compatibility with legacy method -->
        <asp:DropDownList ID="ddlCategory" runat="server" Visible="false" />
    </div>
</asp:Content>