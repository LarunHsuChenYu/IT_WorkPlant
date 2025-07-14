<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="IT_ComputerListView.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_ComputerListView" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        .card-box {
            border: 1px solid #ccc;
            border-radius: 12px;
            padding: 15px;
            margin-bottom: 20px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.05);
        }
        .filter-btn {



            margin: 3px;
        }
    </style>

    <div class="container my-4">
        <h2>🖥️ Computer & Notebook List</h2>

        <!-- Filter Bar -->
        <div class="row my-3">
            <div class="col-md-6">
                <input type="text" id="searchBox" class="form-control" placeholder="🔍 Search anything..." />
            </div>
            <div class="col-md-6 text-end">
                <button type="button" class="btn btn-secondary filter-btn" onclick="filterDept('All')">All Dept</button>
                <button type="button" class="btn btn-outline-primary filter-btn" onclick="filterDept('IT')">IT</button>
                <button type="button" class="btn btn-outline-primary filter-btn" onclick="filterDept('HR')">HR</button>
                <button type="button" class="btn btn-outline-primary filter-btn" onclick="filterDept('MFG')">MFG</button>
                <button type="button" class="btn btn-outline-primary filter-btn" onclick="filterDept('PU')">PU</button>
            </div>
        </div>

        <!-- Type Toggle -->
        <div class="mb-3">
            <button type="button" class="btn btn-warning filter-btn" onclick="filterType('All')">All Types</button>
            <button type="button" class="btn btn-outline-dark filter-btn" onclick="filterType('PC')">PC</button>
            <button type="button" class="btn btn-outline-dark filter-btn" onclick="filterType('NB')">NB</button>
        </div>

        <!-- Cards Display -->
        <div class="row" id="cardContainer">
            <!-- JavaScript will inject cards here -->
        </div>
    </div>

    <asp:HiddenField ID="hiddenJson" runat="server" />

    <script>
        let allData = [];
        let currentDept = 'All';
        let currentType = 'All';

        window.onload = function () {
            allData = JSON.parse(document.getElementById('<%= hiddenJson.ClientID %>').value || "[]");
            renderCards(allData);

            document.getElementById("searchBox").addEventListener("input", function () {
                renderCards(allData);
            });
        };

        function renderCards(data) {
            const keyword = document.getElementById("searchBox").value.toLowerCase();
            const container = document.getElementById("cardContainer");
            container.innerHTML = "";

            data.filter(d => {
                const matchDept = currentDept === "All" || d.DeptName === currentDept;
                const matchType = currentType === "All" || d.Tyb === currentType;
                const matchSearch = Object.values(d).some(v => (v + '').toLowerCase().includes(keyword));
                return matchDept && matchType && matchSearch;
            }).forEach(item => {
                const card = `
                    <div class="col-md-4">
                        <div class="card-box">
                            <h5>${item.NamePC}</h5>
                            <p><b>User:</b> ${item.UserName || '-'} (${item.UserID || '-'})</p>
                            <p><b>Dept:</b> ${item.DeptName}</p>
                            <p><b>Brand:</b> ${item.Brand} | <b>Model:</b> ${item.Model}</p>
                            <p><b>Serial:</b> ${item.SerialNumber} | <b>Type:</b> ${item.Tyb}</p>
                            <p><b>Warranty:</b> ${item.Warranty} | <b>System:</b> ${item.System}</p>
                            <p><b>Status:</b> ${item.Status} | <b>Price:</b> ${item.Price}</p>
                        </div>
                    </div>
                `;
                container.innerHTML += card;
            });
        }

        function filterDept(dept) {
            currentDept = dept;
            renderCards(allData);
        }

        function filterType(type) {
            currentType = type;
            renderCards(allData);
        }
    </script>

</asp:Content>
