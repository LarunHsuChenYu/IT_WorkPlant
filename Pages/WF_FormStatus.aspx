<%@ Page Title="Workflow Status" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="WF_FormStatus.aspx.cs" Inherits="IT_WorkPlant.Pages.WF_FormStatus" %>

<asp:Content ID="HeaderContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        body {
            background-color: #f8f9fa;
        }
        .status-card {
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
        .timeline {
            position: relative;
            padding: 20px 0;
        }
        .timeline-item {
            position: relative;
            padding-left: 50px;
            margin-bottom: 20px;
        }
        .timeline-item:before {
            content: '';
            position: absolute;
            left: 20px;
            top: 0;
            bottom: -20px;
            width: 2px;
            background-color: #dee2e6;
        }
        .timeline-item:last-child:before {
            display: none;
        }
        .timeline-dot {
            position: absolute;
            left: 15px;
            top: 5px;
            width: 12px;
            height: 12px;
            border-radius: 50%;
            background-color: #007bff;
            border: 2px solid white;
        }
        .timeline-dot.completed {
            background-color: #28a745;
        }
        .timeline-dot.rejected {
            background-color: #dc3545;
        }
        .timeline-content {
            background-color: #f8f9fa;
            padding: 15px;
            border-radius: 5px;
        }
        .status-badge {
            display: inline-block;
            padding: 5px 10px;
            border-radius: 15px;
            font-size: 12px;
            font-weight: bold;
            margin-left: 10px;
        }
        .status-pending {
            background-color: #ffc107;
            color: #000;
        }
        .status-completed {
            background-color: #28a745;
            color: white;
        }
        .status-rejected {
            background-color: #dc3545;
            color: white;
        }
        .workflow-grid {
            margin-bottom: 20px;
        }
        .workflow-grid th {
            background-color: #e4efff;
            color: #1a3c6c;
            font-weight: 600;
        }
        .workflow-grid tr:hover {
            background-color: #f8f9fa;
            cursor: pointer;
        }
        .selected-row {
            background-color: #e4efff !important;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-4">
        <h3>我的工作流程</h3>
        
        <asp:GridView ID="gvWorkflows" runat="server" 
            CssClass="table table-bordered workflow-grid"
            AutoGenerateColumns="False"
            OnRowDataBound="gvWorkflows_RowDataBound"
            OnSelectedIndexChanged="gvWorkflows_SelectedIndexChanged"
            DataKeyNames="FormID">
            <Columns>
                <asp:BoundField DataField="FormID" HeaderText="表單編號" />
                <asp:BoundField DataField="FlowName" HeaderText="流程名稱" />
                <asp:BoundField DataField="RequestDate" HeaderText="申請日期" DataFormatString="{0:yyyy/MM/dd HH:mm}" />
                <asp:BoundField DataField="CurrentStep" HeaderText="目前步驟" />
                <asp:BoundField DataField="Status" HeaderText="狀態" />
                <asp:BoundField DataField="LastUpdateDate" HeaderText="最後更新" DataFormatString="{0:yyyy/MM/dd HH:mm}" />
            </Columns>
            <SelectedRowStyle CssClass="selected-row" />
        </asp:GridView>

        <div id="divFormDetail" runat="server" class="status-card" visible="false">
            <div class="form-header">
                <h4>工單狀態</h4>
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
                <div class="row mt-2">
                    <div class="col-md-4">
                        <label>目前狀態：</label>
                        <asp:Label ID="lblCurrentStatus" runat="server" CssClass="status-badge" />
                    </div>
                    <div class="col-md-4">
                        <label>完成日期：</label>
                        <asp:Label ID="lblCompletionDate" runat="server" />
                    </div>
                </div>
            </div>

            <div class="form-content">
                <asp:Label ID="lblFormContent" runat="server" />
            </div>

            <div class="timeline">
                <asp:Repeater ID="rptTimeline" runat="server">
                    <ItemTemplate>
                        <div class="timeline-item">
                            <div class="timeline-dot <%# GetStatusClass(Eval("Status")) %>"></div>
                            <div class="timeline-content">
                                <div class="d-flex justify-content-between align-items-center">
                                    <div>
                                        <strong><%# Eval("PositionName_EN") %></strong>
                                        <span class="status-badge <%# GetStatusClass(Eval("Status")) %>">
                                            <%# Eval("Status") %>
                                        </span>
                                    </div>
                                    <small><%# FormatDate(Eval("ApprovalDate")) %></small>
                                </div>
                                <%# Eval("Comment") %>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>
    </div>
</asp:Content> 