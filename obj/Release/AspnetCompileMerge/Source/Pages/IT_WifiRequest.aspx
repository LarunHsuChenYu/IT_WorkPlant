<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true"
    CodeBehind="IT_WifiRequest.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_WifiRequest" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <title>Wi-Fi Usage Request Form</title>
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script>
        $(document).ready(function () {
            updateTable();
        });

        function showTab(tabId) {
            var tabs = document.getElementsByClassName('tab-content');
            for (var i = 0; i < tabs.length; i++) {
                tabs[i].style.display = 'none';
            }
            document.getElementById(tabId).style.display = 'block';
        }

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

                    // 通用表格更新邏輯
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
            // 隱藏所有 .tab-content
            document.querySelectorAll('.tab-content').forEach(tab => {
                tab.classList.remove('active');
            });

            // 顯示選中的 tab-content
            document.getElementById(tabId).classList.add('active');

            // 更新 HiddenField 的值
            document.getElementById('<%= hfActiveTab.ClientID %>').value = tabId;
        }
    </script>
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
    <h2>Wi-Fi Usage Request Form</h2>

    <asp:Panel ID="wifiRequestPanel" runat="server">
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
        <div class="tabs">
            <div class="tab" onclick="showTab('VisitorDiv')">Visitor</div>
            <div class="tab" onclick="showTab('BizTripDiv')">Buiness Trip</div>
            <div class="tab" onclick="showTab('OnboardDiv')">New Employee</div>
        </div>
        <asp:HiddenField ID="hfActiveTab" runat="server" />
        
        <div id="VisitorDiv" class="tab-content active">
            <h4>Visitor Request</h4>
            <table id="VisitorTable" class="table table-bordered" runat="server">
                <thead>
                    <tr>
                        <th>No.</th>
                        <th>Full Name</th>
                        <th>Company</th>
                        <th>Start Date</th>
                        <th>End Date</th>
                        <th>Description</th>
                    </tr>
                </thead>
                <tbody id="tbVisitor">
                </tbody>
            </table>

            <asp:Button ID="btnVisitorAddRow" runat="server" Text="Add Row" CssClass="btn btn-primary" OnClick="AddRow_Click" />
            <asp:Button ID="btnVisitorSubmit" runat="server" Text="Submit" CssClass="btn btn-success" OnClick="SubmitForm_Click" />
        </div>
        
        <div id="BizTripDiv" class="tab-content">
            <h4>BizTrip Request</h4>
            <table id="BizTripTable" class="table table-bordered" runat="server">
                <thead>
                    <tr>
                        <th>No.</th>
                        <th>Full Name</th>
                        <th>Employee ID</th>
                        <th>Department</th>
                        <th>Device Type</th>
                        <th>Device MAC Address</th>
                        <th>Start Date</th>
                        <th>End Date</th>
                    </tr>
                </thead>
                <tbody id="tbBizTrip">
                </tbody>
            </table>

            <asp:Button ID="btnBizTripAddRow" runat="server" Text="Add Row" CssClass="btn btn-primary" OnClick="btnBizTripAddRow_Click" />
            <asp:Button ID="btnBizTripSubmit" runat="server" Text="Submit" CssClass="btn btn-success" OnClick="btnBizTripSubmit_Click" />
        </div>

        <div id="OnboardDiv" class="tab-content">
            <h4>New Emplyee Request</h4>
            <table id="OnboardTable" class="table table-bordered" runat="server">
                <thead>
                    <tr>
                        <th>No.</th>
                        <th>Full Name</th>
                        <th>Employee ID</th>
                        <th>Department</th>
                        <th>Email</th>
                        <th>Device Type</th>
                        <th>MAC Address</th>
                        <th>Description</th>
                    </tr>
                </thead>
                <tbody id="tbOnboard">
                </tbody>
            </table>

            <asp:Button ID="btnOnboardAddRow" runat="server" Text="Add Row" CssClass="btn btn-primary" OnClick="btnOnboardAddRow_Click" />
            <asp:Button ID="btnOnboardSubmit" runat="server" Text="Submit" CssClass="btn btn-success" OnClick="btnOnboardSubmit_Click" />
        </div>
    </asp:Panel>
</asp:Content>