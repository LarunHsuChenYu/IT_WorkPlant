<%@ Page Title="Workflow Approval" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="WF_Approve.aspx.cs" Inherits="IT_WorkPlant.Pages.WF_Approve" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        body {
            background-color: #f8f9fa;
        }
        .approval-card {
            background-color: white;
            border-radius: 10px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            padding: 20px;
            margin-bottom: 20px;
        }
        .form-header {
            background-color: #f8f9fa;
            padding: 15px;
            border-radius: 5px;
            margin-bottom: 20px;
        }
        .form-content {
            background-color: white;
            padding: 20px;
            border: 1px solid #dee2e6;
            border-radius: 5px;
            margin-bottom: 20px;
        }
        .approval-actions {
            display: flex;
            gap: 10px;
            justify-content: flex-end;
            margin-top: 20px;
        }
        .btn-approve {
            background-color: #28a745;
            color: white;
        }
        .btn-reject {
            background-color: #dc3545;
            color: white;
        }
        .step-indicator {
            display: flex;
            justify-content: space-between;
            margin-bottom: 20px;
            padding: 10px;
            background-color: #f8f9fa;
            border-radius: 5px;
        }
        .step {
            text-align: center;
            flex: 1;
        }
        .step.active {
            color: #007bff;
            font-weight: bold;   
        }
        .step.completed {
            color: #28a745;
        }
        .step.rejected {
            color: #dc3545;
        }
        .step.pending {
            color: #6c757d;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-4">
        <div class="approval-card">
            <div class="form-header">
                <h4>工單簽核</h4>
                <div class="row">
                    <div class="col-md-4">
                        <label>申請人：</label>
                        <asp:Label ID="lblRequester" runat="server" />
                    </div>
                    <div class="col-md-4">
                        <label>部門：</label>
                        <asp:Label ID="lblDepartment" runat="server" />
                    </div>
                    <div class="col-md-4">
                        <label>申請日期：</label>
                        <asp:Label ID="lblRequestDate" runat="server" />
                    </div>
                </div>
            </div>

            <div class="step-indicator">
                <asp:Repeater ID="rptTimeline" runat="server">
                    <ItemTemplate>
                        <div class="step <%# GetStepClass(Container.DataItem) %>">
                            <div><%# Eval("StepOrder") %></div>
                            <div><%# Eval("Role") %></div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>

            <div class="form-content">
                <asp:Label ID="lblFormContent" runat="server" />
            </div>

            <div class="approval-actions">
                <asp:TextBox ID="txtComment" runat="server" CssClass="form-control" placeholder="請輸入意見..." TextMode="MultiLine" Rows="3" />
                <asp:Button ID="btnApprove" runat="server" Text="同意" CssClass="btn btn-approve" OnClick="btnApprove_Click" />
                <asp:Button ID="btnReject" runat="server" Text="拒絕" CssClass="btn btn-reject" OnClick="btnReject_Click" />
            </div>
        </div>
    </div>

    <asp:HiddenField ID="hfFormID" runat="server" />
    <asp:HiddenField ID="hdnToken" runat="server" />
</asp:Content>
