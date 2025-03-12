<%@ Page Title="IT Request Form" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true"
    CodeBehind="IT_RequestForm.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_RequestForm" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .form-container {
            max-width: 1000px; /* 統一容器的寬度 */
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

        .form-group input,
        .form-group select,
        .form-group textarea {
            padding: 10px;
            border: 1px solid #ccc;
            border-radius: 5px;
            box-sizing: border-box; /* 將 padding 和 border 包含在寬度中 */
        }

        .form-group textarea {
            resize: none;
        }

        .form-row {
            display: flex;
            gap: 20px;
            width: 100%; /* 與 .form-group 保持一致 */
        }

        .form-row .form-group {
            flex: 1;
        }

        .full-width-textarea {
            width: 100%;
            box-sizing: border-box; 
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

        <!-- User Name and Department -->
        <div class="form-row">
            <div class="form-group">
                <label for="txtName">Requestor Name:</label>
                <asp:TextBox ID="txtName" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
            </div>
            <div class="form-group">
                <label for="txtDept">Department:</label>
                <asp:TextBox ID="txtDept" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
            </div>
            <div class="form-group">
                <label for="txtDate">Issue Date:</label>
                <asp:TextBox ID="txtDate" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox>
            </div>
        </div>

        <!-- Issue Type -->
        <div class="form-group">
            <label for="ddlCategory">Issue Type:</label>
            <asp:DropDownList ID="ddlCategory" runat="server" CssClass="form-control">
                <asp:ListItem Text="Select Issue Type" Value="" />
            </asp:DropDownList>
        </div>
        
        <!-- Issue Details -->
        <div class="form-group">
            <label for="txtDescription">Issue Description:</label>
            <asp:TextBox ID="txtDescription" runat="server" CssClass="form-control full-width-textarea"
                TextMode="MultiLine" placeholder="Enter details about the issue"></asp:TextBox>
        </div>

        <!-- File Upload -->
        <div class="form-group">
            <label for="fileUploadImage">Upload Attachment:</label>
            <asp:FileUpload ID="fileUploadImage" runat="server" CssClass="form-control" />
        </div>

        <!-- Buttons -->
        <div class="btn-container">
            <asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" Text="Submit Request"
                OnClick="SubmitForm" />
            <asp:Button ID="btnCancel" runat="server" CssClass="btn btn-secondary" Text="Cancel"
                OnClick="CancelForm" />
        </div>
    </div>
    
</asp:Content>
