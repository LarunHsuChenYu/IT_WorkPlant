<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true"
    CodeBehind="IT_WifiRequest.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_WifiRequest" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Wi-Fi Usage Request Form</title>
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script>
        $(document).ready(function () {
            updateTable();
        });
        function updateTable() {
            $.ajax({
                type: "POST",
                url: "YourPage.aspx/GetWiFiRequests",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: function (response) {
                    const tablesConfig = [
                        { tableId: "#tbVisitor", dataKey: "visitor", columns: ["No", "FullName", "Company", "StartDate", "EndDate", "Description"] },
                        { tableId: "#tbBizTrip", dataKey: "bizTrip", columns: ["No", "FullName", "EmployeeID", "Department", "DeviceType", "DeviceMACAddress", "StartDate", "EndDate"] },
                        { tableId: "#tbOnboard", dataKey: "onboard", columns: ["No", "FullName", "EmployeeID", "Department", "Email", "DeviceType", "MACAddress", "Description"] }
                    ];

                    
                    tablesConfig.forEach(config => {
                        const tableBody = $(config.tableId);
                        tableBody.empty();

                        const rows = response.d[config.dataKey];
                        rows.forEach(rowData => {
                            const row = $("<tr></tr>");
                            config.columns.forEach(column => {
                                row.append($("<td></td>").text(rowData[column] || ""));
                            });
                            tableBody.append(row);
                        });
                    });
                },
                error: function (xhr, status, error) {
                    console.error("Error loading table data:", error);
                }
            });
        }

        function showTab(tabId) {
            
            document.querySelectorAll('.tab-content').forEach(tab => {
                tab.classList.remove('active');
            });
            
            document.getElementById(tabId).classList.add('active');
           
            document.getElementById('<%= hfActiveTab.ClientID %>').value = tabId;
        }
    </script>
    <style>
    body {
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        font-size: 14px;
        background-color: #ffffff;
    }

    .form-control {
        font-size: 14px;
        padding: 8px 12px;
        height: 38px;
        border-radius: 6px;
        border: 1px solid #ced4da;
        background-color: #f0f4ff;
    }

    .form-icon {
        width: 32px;
        height: 32px;
        object-fit: contain;
        margin-bottom: 4px;
    }

    .form-label {
        font-size: 18px;
        font-weight: 600;
        color: #1a3c6c;
        margin: 6px 0;
    }

    .title-header {
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 12px;
        margin-bottom: 24px;
    }

    .wizard-header {
        font-size: 40px;
        font-weight: bold;
        color: #0d6efd;
    }

    .form-row {
        display: flex;
        justify-content: space-between;
        gap: 30px;
        margin-bottom: 30px;
        flex-wrap: wrap;
    }

    .form-group {
        flex: 1 1 30%;
        display: flex;
        flex-direction: column;
        align-items: center;
        text-align: center;
        gap: 4px;
    }

    .info-box {
        background-color: #f0f4ff !important;
        border: 1px solid #cfdcec;
        color: #333;
    }

    .form-box {
        background-color: #f9fcff;
        padding: 20px;
        border-radius: 10px;
        margin-top: 20px;
        box-shadow: 0 2px 6px rgba(0, 0, 0, 0.04);
    }

    .tabs {
        display: flex;
        border: 1px solid #cfdcec;
        border-radius: 10px;
        overflow: hidden;
        background-color: #e9f2ff;
    }

    .tab {
        flex-grow: 1;
        text-align: center;
        font-size: 17px;
        font-weight: 500;
        color: #1a3c6c;
        padding: 12px 0;
        height: 40px;
        cursor: pointer;
        border: none;
        border-right: 1px solid #cfdcec;
        transition: background-color 0.2s ease;
    }

    .tab:last-child {
        border-right: none;
    }

    .tab:hover {
        background-color: #d0e5ff;
        color: #0d6efd;
    }

    .tab.active {
        background-color: #d0e5ff;
        font-weight: 600;
    }

    .tab-content {
        display: none;
        padding: 20px;
        border: 1px solid #ccc;
        margin-top: 10px;
        border-radius: 5px;
    }

    .active {
        display: block;
    }

    .table-bordered {
        border: 1px solid #d1e0f5;
        border-radius: 8px;
        background-color: #f9fcff;
    }

    .table-bordered thead th {
        background-color: #e4efff;
        color: #1a3c6c;
        font-weight: 600;
    }

    .btn-primary {
        background-color: #0d6efd;
        border: none;
        color: white;
    }

    .btn-primary:hover {
        background-color: #0b5ed7;
    }

    .btn-success {
        background-color: #198754;
        border: none;
        color: white;
    }

    .btn-success:hover {
        background-color: #157347;
    }

    .container-center {
        background-color: #ffffff;
        padding: 20px;
        border-radius: 12px;
        box-shadow: 0 0 10px rgba(0, 0, 0, 0.05);
        margin-bottom: 40px;
    }
    .button-group {
    display: flex;
    justify-content: center;
    gap: 16px;
    margin-top: 20px;
}

.btn-add {
    background-color: #adb5bd; 
    border: none;
    color: white;
    padding: 8px 16px;
    border-radius: 6px;
    font-size: 16px;
    transition: background-color 0.2s ease;
}

.btn-add:hover {
    background-color: #868e96;
}

.btn-submit {
    background-color: #9ec5fe; 
    border: none;
    color: white;
    padding: 8px 16px;
    border-radius: 6px;
    font-size: 16px;
    transition: background-color 0.2s ease;
}

.btn-submit:hover {
    background-color: #74b0f4;
}

</style>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

        <div class="title-header">
            <img src="/Image/wifi.gif" alt="form Icon" style="width: 90px; height: 90px;" />
            <asp:Label ID="lblTitle" runat="server" Text='<%# GetLabel("title") %>' CssClass="wizard-header" />
        </div>

        <div class="form-row centered-row">
            <div class="form-group">
                <img src="/Image/user1.png" alt="User Icon" class="form-icon" />
                <asp:Label ID="lblName" runat="server" Text='<%# GetLabel("requester") %>' CssClass="form-label" AssociatedControlID="requesterName" />
                <asp:TextBox ID="requesterName" runat="server" CssClass="form-control info-box" ReadOnly="true" />
            </div>

            <div class="form-group">
                <img src="/Image/Department.png" alt="Department Icon" class="form-icon" />
               <asp:Label ID="lblDept" runat="server" Text='<%# GetLabel("department") %>' CssClass="form-label" AssociatedControlID="department" />
                <asp:TextBox ID="department" runat="server" CssClass="form-control info-box" ReadOnly="true" />
            </div>

            <div class="form-group">
                <img src="/Image/date.png" alt="Date Icon" class="form-icon" />
                <asp:Label ID="lblDate" runat="server" Text='<%# GetLabel("issuedate") %>' CssClass="form-label" AssociatedControlID="requestDate" />
                <asp:TextBox ID="requestDate" runat="server" CssClass="form-control info-box" ReadOnly="true" />
            </div>
           

        </div>
        <div class="form-box">
    <div class="tabs">
        <div class="tab" onclick="showTab('VisitorDiv')"><asp:Literal ID="litVisitor" runat="server" /></div>
<div class="tab" onclick="showTab('BizTripDiv')"><asp:Literal ID="litBizTrip" runat="server" /></div>
<div class="tab" onclick="showTab('OnboardDiv')"><asp:Literal ID="litOnboard" runat="server" /></div>
    </div>

    <asp:HiddenField ID="hfActiveTab" runat="server" />

    
    <div id="VisitorDiv" class="tab-content active">
        <h4><%= GetLabel("visitorrequest") %></h4>
        <table id="VisitorTable" class="table table-bordered" runat="server">
            <thead>
                <tr>
                    <th><%= GetLabel("no") %></th>
                    <th><%= GetLabel("fullname") %></th>
                    <th><%= GetLabel("company") %></th>
                    <th><%= GetLabel("startdate") %></th>
                    <th><%= GetLabel("enddate") %></th>
                    <th><%= GetLabel("description") %></th>
                </tr>
            </thead>
        </table>
        <div class="button-group">
            <asp:Button ID="btnVisitorAddRow" runat="server" Text="➕ Add Row" CssClass="btn-add" OnClick="AddRow_Click" />
            <asp:Button ID="btnVisitorSubmit" runat="server" Text="Submit Request" CssClass="btn-submit" OnClick="SubmitForm_Click" />
        </div>
    </div>

    
    <div id="BizTripDiv" class="tab-content">
        <h4><%= GetLabel("biztriprequest") %></h4>
        <table id="BizTripTable" class="table table-bordered" runat="server">
            <thead>
                <tr>
                    <th><%= GetLabel("no") %></th>
                    <th><%= GetLabel("fullname") %></th>
                    <th><%= GetLabel("empid") %></th>
                    <th><%= GetLabel("department") %></th>
                    <th><%= GetLabel("devicetype") %></th>
                    <th><%= GetLabel("mac") %></th>
                    <th><%= GetLabel("startdate") %></th>
                    <th><%= GetLabel("enddate") %></th>
                </tr>
            </thead>
        </table>
        <div class="button-group">
            <asp:Button ID="btnBizTripAddRow" runat="server" Text="➕ Add Row" CssClass="btn-add" OnClick="btnBizTripAddRow_Click" />
            <asp:Button ID="btnBizTripSubmit" runat="server" Text="Submit Request" CssClass="btn-submit" OnClick="btnBizTripSubmit_Click" />
        </div>
    </div>

   
    <div id="OnboardDiv" class="tab-content">
        <h4><%= GetLabel("onboardrequest") %></h4>
        <table id="OnboardTable" class="table table-bordered" runat="server">
            <thead>
                <tr>
                    <th><%= GetLabel("no") %></th>
                    <th><%= GetLabel("fullname") %></th>
                    <th><%= GetLabel("empid") %></th>
                    <th><%= GetLabel("department") %></th>
                    <th><%= GetLabel("email") %></th>
                    <th><%= GetLabel("devicetype") %></th>
                    <th><%= GetLabel("mac") %></th>
                    <th><%= GetLabel("description") %></th>
                </tr>
            </thead>
        </table>
        <div class="button-group">
            <asp:Button ID="btnOnboardAddRow" runat="server" Text="➕ Add Row" CssClass="btn-add" OnClick="btnOnboardAddRow_Click" />
            <asp:Button ID="btnOnboardSubmit" runat="server" Text="Submit Request" CssClass="btn-submit" OnClick="btnOnboardSubmit_Click" />
        </div>
    </div>


            <script type="text/javascript">
                document.addEventListener("DOMContentLoaded", function () {
                    var activeTab = document.getElementById('<%= hfActiveTab.ClientID %>').value;
        if (activeTab) {
            document.querySelectorAll('.tab-content').forEach(tab => tab.classList.remove('active'));
            document.getElementById(activeTab).classList.add('active');

            document.querySelectorAll('.tab').forEach(tab => tab.classList.remove('active'));
            if (activeTab === "VisitorDiv") document.querySelector('.tab:nth-child(1)').classList.add('active');
            if (activeTab === "BizTripDiv") document.querySelector('.tab:nth-child(2)').classList.add('active');
            if (activeTab === "OnboardDiv") document.querySelector('.tab:nth-child(3)').classList.add('active');
        }
    });
            </script>
    
            </div>
</asp:Content>
