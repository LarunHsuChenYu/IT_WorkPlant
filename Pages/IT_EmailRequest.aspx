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
        <div class="wizard-header">Email Account Request</div>
        <div class="wizard-subtitle">Submit a request for new email accounts. You can request up to 5 accounts at once.</div>
<!-- Input Fields -->
<div class="row mb-4">
    <div class="col-md-4">
        <label class="form-label">
            <asp:Image ID="imgUserIcon" runat="server" ImageUrl="~/Image/user1.png" CssClass="me-2" AlternateText="User" />
            Requester Name
        </label>
        <asp:TextBox ID="requesterName" runat="server" CssClass="form-control" ReadOnly="true" />
    </div>
    <div class="col-md-4">
        <label class="form-label">
            <asp:Image ID="imgDeptIcon" runat="server" ImageUrl="~/Image/Department.png" CssClass="me-2" AlternateText="Dept" />
            Department
        </label>
        <asp:TextBox ID="department" runat="server" CssClass="form-control" ReadOnly="true" />
    </div>
    <div class="col-md-4">
        <label class="form-label">
            <asp:Image ID="imgDateIcon" runat="server" ImageUrl="~/Image/date.png" CssClass="me-2" AlternateText="Date" />
            Request Date
        </label>
        <asp:TextBox ID="requestDate" runat="server" CssClass="form-control" ReadOnly="true" />
    </div>
</div>



        <!-- Section Title + Add Row -->
        <div class="d-flex justify-content-between align-items-center mt-3 mb-2">
            <h5 class="mb-0">Email Account Requests</h5>
            <asp:Button ID="btnRequestEmailAddRow" runat="server" Text="➕ Add Row" CssClass="btn btn-outline-primary btn-sm" OnClick="AddRow_Click" />
        </div>

        <!-- Table -->
        <table id="RequestEmailTable" runat="server" class="table table-bordered table-sm">
            <tr class="table-light">
                <th style="width: 5%;">No.</th>
                <th>First Name</th>
                <th>Last Name</th>
                <th>Employee ID</th>
                <th>Department</th>
            </tr>
        </table>

        <!-- Submit Button -->
        <div class="text-end mt-3">
            <asp:Button ID="btnRequestEmailSubmit" runat="server" Text="Submit Request" CssClass="btn btn-primary" OnClick="btnRequestEmailSubmit_Click" />
        </div>

        <small class="text-muted mt-2 d-block">* After submitting the request, the Word document will download automatically.</small>
    </div>
</asp:Content>
