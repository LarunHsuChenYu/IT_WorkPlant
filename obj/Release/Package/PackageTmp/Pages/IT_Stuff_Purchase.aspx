<%@ Page Title="IT Service Purchase" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" 
    CodeBehind="~/Pages/IT_Stuff_Purchase.aspx.cs"
    Inherits="IT_WorkPlant.Pages.IT_Stuff_Purchase" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h2>IT Service Purchase Form</h2>

    <!-- Form Details Section -->
    <div class="form-section">
         

        <label for="txtRequestDate">Request Date:</label>
        <asp:TextBox ID="txtRequestDate" runat="server" TextMode="Date"></asp:TextBox>

        <label for="txtonboardID">onboard ID:</label>
        <asp:TextBox ID="txtonboardID" runat="server"></asp:TextBox>

        <label for="txtEmail">Email:</label>
        <asp:TextBox ID="txtEmail" runat="server"></asp:TextBox>

        <label for="txtRequesterName">Requester Name:</label>
        <asp:TextBox ID="txtRequesterName" runat="server"></asp:TextBox>

        <label for="txtTelephone">Telephone:</label>
        <asp:TextBox ID="txtTelephone" runat="server"></asp:TextBox>

        <label for="ddlDepartment">Department:</label>
        <asp:DropDownList ID="ddlDepartment" runat="server">
            <asp:ListItem Text="IT Dept." Value="IT Dept." />
            <asp:ListItem Text="HR Dept." Value="HR Dept." />
            <asp:ListItem Text="Finance Dept." Value="Finance Dept." />
        </asp:DropDownList>

        <label for="txtDescription">Please specify details:</label>
        <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="3" Columns="50"></asp:TextBox>
    </div>

    <!-- Tabs for Product Selection -->
    <div class="tabs">
        <div class="tab" onclick="showTab('tabComputer')">Computer</div>
        <div class="tab" onclick="showTab('tabLaptop')">Laptop</div>
        <div class="tab" onclick="showTab('tabMonitor')">Monitor</div>
        <div class="tab" onclick="showTab('tabSoftware')">Software</div>
        <div class="tab" onclick="showTab('tabOther')">Other</div>
    </div>

    <style>
        .form-section { margin-bottom: 20px; }
        .tabs { display: flex; justify-content: space-around; }
        .tab { padding: 10px; background-color: #ccc; cursor: pointer; }
        .tab-content { display: none; }
        .active { display: block; }
        .product { border: 1px solid #000; padding: 10px; margin: 10px; text-align: center; }
        .product img { width: 100px; }
        .submit-popup { display: none; }
    </style>
    <!-- Tab Content: Computer Products -->
    <div id="tabComputer" class="tab-content active">
        <h3>Computer Selection</h3>
        <section class="col-md-3" aria-labelledby="Content1">
            <img src="acer.png" alt="Acer PC" />
            <p>Acer Aspire TC</p>
            <p>i3-13100, 8GB RAM, 512GB SSD, Windows 11 Pro</p>
            <p>Price: 23,230 THB</p>
            <asp:Button ID="Button1" runat="server" Text="Pick" OnClick="btnPick_Click" />
        </section>
        <section class="col-md-3" aria-labelledby="Content1">
            <img src="asus.png" alt="Asus PC" />
            <p>Asus S500TE</p>
            <p>i3-13100, 8GB RAM, 512GB SSD, Windows 11 Pro</p>
            <p>Price: 24,230 THB</p>
            <asp:Button ID="Button2" runat="server" Text="Pick" OnClick="btnPick_Click" />
        </section>
        <section class="col-md-3" aria-labelledby="Content1">
            <img src="dell.png" alt="Dell PC" />
            <p>Dell Optiplex 7010</p>
            <p>i5-12500, 8GB RAM, 512GB SSD, Windows 11 Pro</p>
            <p>Price: 24,730 THB</p>
            <asp:Button ID="btnPickDell" runat="server" Text="Pick" OnClick="btnPick_Click" />
        </section>
    </div>

    <!-- Other Tabs Content -->
    <div id="tabLaptop" class="tab-content">
        <h3>Laptop Selection (To be implemented)</h3>
    </div>
    <div id="tabMonitor" class="tab-content">
        <h3>Monitor Selection (To be implemented)</h3>
    </div>
    <div id="tabSoftware" class="tab-content">
        <h3>Software Selection (To be implemented)</h3>
    </div>
    <div id="tabOther" class="tab-content">
        <h3>Other Items (To be implemented)</h3>
    </div>

    <!-- Submit Button -->
    <asp:Button ID="btnSubmit" runat="server" Text="Submit" OnClick="btnSubmit_Click" />

    <!-- Popup Notification -->
    <div id="submitPopup" class="submit-popup">
        <p>Submit Completed</p>
    </div>

    <script type="text/javascript">
        function showTab(tabId) {
            var tabs = document.getElementsByClassName('tab-content');
            for (var i = 0; i < tabs.length; i++) {
                tabs[i].style.display = 'none';
            }
            document.getElementById(tabId).style.display = 'block';
        }

        function showPopup() {
            document.getElementById('submitPopup').style.display = 'block';
        }
    </script>

</asp:Content>
