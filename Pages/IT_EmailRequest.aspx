<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true"
    CodeBehind="IT_EmailRequest.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_EmailRequest" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
<title>EMail Account Request Form</title>
<script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>

    <style>
        .tabs {
            display: flex;
            border: 1px solid #ccc; /* 加上外框 */
            border-radius: 10px; /* 外框也加上圓角 */
            overflow: hidden; /* 避免 border-radius 造成邊框溢出 */
        }
        .tab {
            flex-grow: 1;
            text-align: center;
            padding: 10px;
            background-color: #aaa2a2e7;
            cursor: pointer;
            border: none; /* 移除個別 tab 的邊框 */
            box-sizing: border-box;
            border-right: 1px solid #ccc; /* 除了最後一個 tab，其他加上右邊框 */
        }
        .tab:last-child {
            border-right: none;
        }
        .tab:hover { /* 滑鼠移上去的效果 */
            background-color: #808080;
            color: white;
        }
        .tab.active { /* 目前選中的 tab */
            background-color: #888282e7;
        }
        .tab-content { display: none; padding: 20px; border: 1px solid #ccc; margin-top: 10px; border-radius: 5px;}
        .active { display: block; }
    </style>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>EMAIL Account Request Form</h2>
    
    <table>
        <tr>
            <td>Requester Name:</td>
            <td><asp:TextBox ID="requesterName" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox></td>
        </tr>
        <tr>
            <td>Department:</td>
            <td><asp:TextBox ID="department" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox></td>
        </tr>
        <tr>
            <td>Date:</td>
            <td><asp:TextBox ID="requestDate" runat="server" CssClass="form-control" ReadOnly="true"></asp:TextBox></td>
        </tr>
    </table>
        
    <div id="div" class="tab-content active">
            
        <table id="RequestEmailTable" class="table table-bordered" runat="server">
            <thead>
                <tr>
                    <th>No.</th>
                    <th>FirstName</th>
                    <th>LastName</th>
                    <th>EmployeeID</th>
                    <th>Department</th>                    
                </tr>
            </thead>
            <tbody id="tbVisitor">
            </tbody>
        </table>

        <asp:Button ID="btnRequestEmailAddRow" runat="server" Text="Add Row" CssClass="btn btn-primary" OnClick="AddRow_Click" />
        <asp:Button ID="btnRequestEmailSubmit" runat="server" Text="Submit" CssClass="btn btn-success" OnClick="btnRequestEmailSubmit_Click" />
        
    </div>
    
</asp:Content>
