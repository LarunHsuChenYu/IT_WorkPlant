<%@ Page Title="IT Request Form" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true"
    CodeBehind="IT_RequestForm.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_RequestForm" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <!-- SweetAlert2 -->
<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
<link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />

   <style>
    body {
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        background-color: #ffffff;
    }

    .form-container {
        max-width: 1000px;
        margin: 40px auto;
        padding: 30px;
        background: #ffffff;
        border-radius: 12px;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
        border-left: 6px solid #70aee4;
    }

    .form-container h2 {
        text-align: center;
        color: #3b5f7c;
        font-size: 28px;
        margin-bottom: 30px;
    }

    .form-group {
        margin-bottom: 20px;
    }

    .form-group label {
        display: block;
        font-weight: 600;
        color: #456b87;
        margin-bottom: 6px;
    }

    .form-row {
        display: flex;
        flex-wrap: wrap;
        gap: 20px;
    }

    .form-row .form-group {
        flex: 1;
    }

    .form-control {
        width: 100%;
        padding: 8px 12px;
        border-radius: 6px;
        border: 1px solid #ccd6dd;
        background-color: #f2f7fb;
        font-size: 15px;
    }

    .form-control:focus {
        outline: none;
        border-color: #70aee4;
        background-color: #eaf3fb;
    }

    .table {
        width: 100%;
        border-collapse: collapse;
        background-color: #ffffff;
        margin-top: 25px;
        border-radius: 8px;
        overflow: hidden;
    }

    .table th {
        background-color: #d9e9f5;
        color: #2f506b;
        font-weight: 600;
        padding: 10px;
        border: 1px solid #c4d5e0;
    }

    .table td {
        border: 1px solid #e0e9f1;
        padding: 10px;
        background-color: #f9fcfe;
    }

    .btn-container {
        text-align: center;
        margin-top: 30px;
    }

    .btn {
        padding: 10px 24px;
        font-size: 16px;
        border-radius: 6px;
        cursor: pointer;
        transition: background-color 0.2s ease;
    }

    .btn-primary {
        background-color: #5fa8e5;
        color: white;
        border: none;
    }

    .btn-primary:hover {
        background-color: #4896d8;
    }

    .btn-secondary {
        background-color: #a0aec0;
        color: white;
        border: none;
    }

    .btn-secondary:hover {
        background-color: #8899b1;
    }
    .form-icon {
    width: 32px;
    height: 32px;
    margin-bottom: 6px;
    object-fit: contain;
    display: block;
    margin-left: auto;
    margin-right: auto;
}
    .form-row {
    display: flex;
    flex-wrap: wrap;
    gap: 24px;
    margin-bottom: 20px;
}
.centered-row {
    display: flex;
    justify-content: center;
    gap: 24px;
}
.form-group {
    display: flex;
    flex-direction: column;
    align-items: center;
    flex: 0 0 250px;
}
.title-header {
    display: flex;
    flex-direction: row;
    align-items: center;
    justify-content: center;
    gap: 16px;
    margin-bottom: 32px;
    text-align: center;
}

.title-header img {
    height: 72px;
    width: 72px;
    object-fit: contain;
}

.title-header span {
    font-size: 40px;
    font-weight: 900;
    color: #0d6efd;
}

</style>
    <div class="title-header">
    <img src="/Image/from.gif" alt="form Icon" />
    <asp:Label ID="lblTitle" runat="server" CssClass="wizard-header"></asp:Label>
</div>
        <!-- User Info -->
       <div class="form-row centered-row">
    <div class="form-group">
        <img src="/Image/user1.png" alt="User Icon" class="form-icon" />
        <asp:Label ID="lblName" runat="server" CssClass="form-label" AssociatedControlID="txtName"></asp:Label>
        <asp:TextBox ID="txtName" runat="server" CssClass="form-control" ReadOnly="true" />
    </div>
    <div class="form-group">
        <img src="/Image/Department.png" alt="Department Icon" class="form-icon" />
        <asp:Label ID="lblDept" runat="server" CssClass="form-label" AssociatedControlID="txtDept"></asp:Label>
        <asp:TextBox ID="txtDept" runat="server" CssClass="form-control" ReadOnly="true" />
    </div>
    <div class="form-group">
        <img src="/Image/date.png" alt="Date Icon" class="form-icon" />
        <asp:Label ID="lblDate" runat="server" CssClass="form-label" AssociatedControlID="txtDate"></asp:Label>
        <asp:TextBox ID="txtDate" runat="server" CssClass="form-control" ReadOnly="true" />
    </div>
</div>


        <!-- Request Items Table -->
        <asp:Repeater ID="rptRequestItems" runat="server" OnItemDataBound="rptRequestItems_ItemDataBound">
            <HeaderTemplate>
                <table class="table">
                    <thead>
                        <tr>
                            <th><%= GetLabel("No.") %></th>
                            <th><%= GetLabel("issuetype") %></th>
                            <th><%= GetLabel("description") %></th>
                            <th><%= GetLabel("attachment") %></th>
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
            <asp:Button ID="btnSubmit" runat="server" CssClass="btn btn-primary" OnClick="SubmitForm" />
            <asp:Button ID="btnCancel" runat="server" CssClass="btn btn-secondary" OnClick="CancelForm" />

            </div>


        <!-- Hidden for compatibility with legacy method -->
        <asp:DropDownList ID="ddlCategory" runat="server" Visible="false" />
    
</asp:Content>