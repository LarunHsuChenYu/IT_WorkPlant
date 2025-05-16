<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true"
    CodeBehind="IT_EmailRequest.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_EmailRequest" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Email Account Request</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.10.5/font/bootstrap-icons.css" />
    <style>
        .form-label { font-weight: 600; }
        .wizard-header {
            font-size: 1.8rem;
            font-weight: bold;
            color: #0d6efd;
            margin-bottom: 0.25rem;
        }
        .wizard-subtitle {
            color: #6c757d;
            margin-bottom: 1.5rem;
        }
        .table th, .table td {
            vertical-align: middle !important;
            text-align: center;
        }
        .form-label img {
            height: 32px;
            vertical-align: middle;
            margin-right: 7px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="card shadow-sm p-4">
        <!-- Header -->
        <div class="wizard-header"><%= GetLabel("Title") %></div>
        <div class="wizard-subtitle"><%= GetLabel("Subtitle") %></div>
<!-- Input Fields -->
<div class="row mb-4">
    <div class="col-md-4">
        <label class="form-label">
    <asp:Image ID="imgUserIcon" runat="server" ImageUrl="~/Image/user1.png" AlternateText="User" CssClass="me-2" />
    <%= GetLabel("RequesterName") %>
</label>
        <asp:TextBox ID="requesterName" runat="server" CssClass="form-control" ReadOnly="true" />
    </div>
    <div class="col-md-4">
        <label class="form-label">
    <asp:Image ID="Image1" runat="server" ImageUrl="~/Image/Department.png" AlternateText="User" CssClass="me-2" />
    <%= GetLabel("Department") %>
</label>

        <asp:TextBox ID="department" runat="server" CssClass="form-control" ReadOnly="true" />
    </div>
    <div class="col-md-4">
        <label class="form-label">
    <asp:Image ID="Image2" runat="server" ImageUrl="~/Image/date.png" AlternateText="User" CssClass="me-2" />
    <%= GetLabel("RequestDate") %>
</label>

        <asp:TextBox ID="requestDate" runat="server" CssClass="form-control" ReadOnly="true" />
    </div>
</div>
        <!-- Section Title + Add Row -->
        <div class="d-flex justify-content-between align-items-center mt-3 mb-2">
            <h5 class="mb-0"><%= GetLabel("EmailRequestList") %></h5>
            <asp:Button ID="btnRequestEmailAddRow" runat="server" Text='<%# GetLabel("➕AddRow") %>' CssClass="btn btn-outline-primary btn-sm" OnClick="AddRow_Click" />
        </div>

        <!-- Table -->
        <table id="RequestEmailTable" runat="server" class="table table-bordered table-sm">
            <tr class="table-light">
    <th><%= GetLabel("No") %></th>
    <th><%= GetLabel("FirstName") %></th>
    <th><%= GetLabel("LastName") %></th>
    <th><%= GetLabel("EmployeeID") %></th>
    <th><%= GetLabel("Dept") %></th>
</tr>

        </table>

        <!-- Submit Button -->
        <div class="text-end mt-3">
            <asp:Button ID="btnRequestEmailSubmit" runat="server" Text='<%# GetLabel("Submit") %>' CssClass="btn btn-primary" OnClick="btnRequestEmailSubmit_Click" />
        </div>

        <small class="text-muted mt-2 d-block"><%= GetLabel("Note") %></small>
    </div>
</asp:Content>
