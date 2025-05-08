<%@ Page 
    Language="C#" 
    MasterPageFile="~/Site.Master" 
    AutoEventWireup="true" 
    EnableEventValidation="false" 
    CodeBehind="MeetingRoomBooking.aspx.cs" 
    Inherits="IT_WorkPlant.Pages.MeetingRoomBooking" %>

<%@ Register Assembly="System.Web.Extensions" Namespace="System.Web.UI" TagPrefix="asp" %>


<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
<title>Meeting Room Booking</title>
    <link href="https://cdn.jsdelivr.net/npm/sweetalert2@11/dist/sweetalert2.min.css" rel="stylesheet" />
  <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
    body {
        background: #f6f9fc;
        font-family: 'Segoe UI', sans-serif;
    }
    .bg-light-green {
    background-color: #e0f7ea;
    border-left: 5px solid #198754;
    border-radius: 0.75rem;
}

    .gradient-header {
    background-color: #ffffff; 
    padding: 1rem 2rem;
    border-top-left-radius: 1rem;
    border-top-right-radius: 1rem;
    display: flex;
    align-items: center;
    justify-content: space-between;
}
    .btn-blue-soft {
    background-color: #cfe2ff;  
    color: #084298;             
    font-weight: bold;
    border: none;
    border-radius: 6px;
    transition: background-color 0.2s ease-in-out;
}

.btn-blue-soft:hover {
    background-color: #b6d4fe;
}


    .avatar {
        display: flex;
        align-items: center;
        gap: 10px;
        font-weight: bold;
    }

    .avatar img {
        width: 40px;
        height: 40px;
        border-radius: 50%;
        object-fit: cover;
    }
    .equal-width-btn {
    min-width: 130px;  
}
    .card {
    box-shadow: none !important;
}
   .custom-calendar {
    border-radius: 6px;
    border: 1px solid #dee2e6;
    padding: 0.3rem;
    background-color: rgba(0,0,0,0.05);
    max-width: 100%;
    margin-bottom: 0.5rem;
    font-size: 0.75rem; 
}
    .custom-calendar table {
    width: 100%;
    border-collapse: collapse;
    font-family: 'Segoe UI', sans-serif;
    font-size: 0.75rem;
    border-radius: 6px;
    overflow: hidden;
    box-shadow: 0 1px 3px rgba(0,0,0,0.05);
}
.custom-calendar th {
    background-color: #f8f9fa;
    font-weight: bold;
    padding: 0.3rem;
    border: 1px solid #dee2e6;
}

.custom-calendar td {
    padding: 0.3rem;
    height: 1.6rem;
    text-align: center;
    border: 1px solid #dee2e6;
    transition: background-color 0.2s ease-in-out;
}


.custom-calendar .TodayDay {
    background-color: #0d6efd !important;
    color: white !important;
    font-weight: bold;
    border-radius: 4px;
}

.custom-calendar .SelectedDay {
    background-color: #6610f2 !important;
    color: #fff !important;
}
.custom-calendar .OtherMonthDay {
    color: #ccc;
}
.table-header-softblue thead th {
    background-color: #607d8b;  /* 🔵 ฟ้าเทาเข้ม */
    color: white !important;
    font-weight: bold;
    text-align: center;
    border: 1px solid #ffffff;
}
.table td, .table th {
    border: 1px solid #a2d8a2 !important;  
    vertical-align: middle;
    text-align: center;
    font-size: 1.15rem !important;
    color: #256029 !important;  
    font-weight: 600;
}
.free-style {
    display: flex;
    align-items: center;
    justify-content: center;
    width: 100%;
    height: 100%;
    font-weight: 600;
    font-size: 1.1rem;
    background-color: #e6f9e6;
    color: #000 !important; /* ✅ เปลี่ยนให้เป็นดำทั้งหมด */
}

.my-green-table tbody tr:nth-child(odd) td {
    background-color: #ffffff !important; 
}

.my-green-table tbody tr:nth-child(even) td {
    background-color: #f0f4f8 !important; /* ฟ้าเทาอ่อน */
}

    .meeting-title {
        color: #3399ff !important;
    }
    .btn-primary-soft {
    background-color: #3399ff;
    color: white;
    font-weight: bold;
    border: none;
    border-radius: 6px;
    transition: background-color 0.2s ease-in-out;
}
.btn-primary-soft:hover {
    background-color: #1f7edb;
}
label.form-label {
    font-size: 1.1rem !important;  
    font-weight: 600;
}

.form-select, .btn, .form-control {
    font-size: 1.05rem !important;
}

.custom-calendar th,
.custom-calendar td {
    font-size: 0.95rem !important; 
}

.table th,
.table td {
    font-size: 1.05rem !important;  
}

.table td, .table th {
    border: 1px solid #dee2e6 !important;  
    padding: 12px !important;
    vertical-align: middle;
    text-align: center;
    font-size: 1.15rem !important;
    color: #000 !important;  /* ✅ ตรงนี้ด้วย */
}
.booked-style {
    background-color: #ffe5e5 !important;       
    color: #cc0000 !important;                  
    font-size: 1.1rem;
    font-weight: 800;
    padding: 6px;
    border-radius: 6px;
    text-align: center;
    letter-spacing: 0.5px;
    text-shadow: 0 0 1px rgba(255, 0, 0, 0.15);
}



</style>



</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-4">
       
        <div class="card shadow rounded-4">
            
            <div class="gradient-header d-flex justify-content-between align-items-center rounded-top px-4 py-3">
                <h4 class="fw-bold mb-0 text-dark d-flex align-items-center" style="font-size: 1.8rem;">
                    <img src="/Image/calendar.gif" alt="Calendar" style="height: 48px; width: 48px; margin-right: 10px;" />
                    <span class="meeting-title">Meeting Room Booking</span>
                </h4>
                <div class="d-flex align-items-center gap-2">
                    <div class="avatar-circle bg-white text-dark p-0" style="width: 48px; height: 48px; overflow: hidden;">
                        <img src="/Image/profile1.gif" alt="avatar" style="width: 100%; height: auto;" />
                    </div>
                    <div class="text-end">
                        <asp:Label ID="lblUser" runat="server" CssClass="fw-bold text-dark d-block" />
                        <asp:Label ID="lblDept" runat="server" CssClass="text-muted" />
                    </div>
                </div>
            </div>
           
            <!-- Body -->
            <div class="card-body p-4">
                <div class="row g-4 mb-4">
                    <!-- Form -->
                    <div class="col-md-12">
                        <div class="row g-4">
                            <div class="col-md-3">
                                <label class="form-label">Date</label>
                                <div class="input-group mb-2">
                                    <asp:TextBox ID="txtBookingDate" runat="server" CssClass="form-control" ReadOnly="true" />
                                    <button type="button" class="btn btn-outline-secondary" onclick="showCalendar()">📅</button>
                                </div>
                                <asp:Calendar ID="Calendar1" runat="server"
                                    CssClass="custom-calendar"
                                    OnSelectionChanged="Calendar1_SelectionChanged"
                                    OnDayRender="Calendar1_DayRender"
                                    Style="display:none;" />
                            </div>
                            <div class="col-md-3">
                                <label class="form-label">Room</label>
                                <asp:DropDownList ID="roomList" runat="server" CssClass="form-select">
                                    <asp:ListItem Value="" Text="---Select---"></asp:ListItem>
                                    <asp:ListItem Value="101">Room 101</asp:ListItem>
                                    <asp:ListItem Value="102">Room 102</asp:ListItem>
                                    <asp:ListItem Value="103">Room 103</asp:ListItem>
                                    <asp:ListItem Value="201">Room 201</asp:ListItem>
                                    <asp:ListItem Value="202">Room 202</asp:ListItem>
                                    <asp:ListItem Value="203">Room 203</asp:ListItem>
                                </asp:DropDownList>
                            </div>
                            <div class="col-md-3">
                                <label class="form-label">Start Time</label>
                                <asp:DropDownList ID="startTimeList" runat="server" CssClass="form-select" />
                            </div>
                            <div class="col-md-3">
                                <label class="form-label">End Time</label>
                                <asp:DropDownList ID="endTimeList" runat="server" CssClass="form-select" />
                            </div>
                        </div>
                    </div><!-- Actions -->
                    <div class="col-md-12">
                        <label class="form-label invisible">Actions</label>
                        <div class="d-flex justify-content-center gap-3 flex-wrap">
                            <asp:Button ID="submitButton" runat="server" Text="Book Room"
                                OnClick="submitButton_Click"
                                CssClass="btn btn-primary px-4 py-2 fw-bold equal-width-btn" />
                            <div style="width: 150px;">
                                <asp:Button ID="btnEditMode" runat="server" Text="Edit"
                                    OnClick="btnEditMode_Click"
                                    CssClass="btn btn-outline-primary w-100 fw-bold" />
                                <asp:Button ID="btnExitEditMode" runat="server" Text="Finish Editing"
                                    OnClick="btnExitEditMode_Click"
                                    CssClass="btn btn-outline-secondary w-100 fw-bold d-none" />
                            </div>
                            <asp:DropDownList ID="departmentList" runat="server" CssClass="form-select w-auto d-none" />
                        </div>
                    </div>
                </div>

   <!-- ✅ เริ่มจากตรงนี้แทนของเดิม -->
<div id="export-zone">
    <div class="row">
        <!-- 🔹 ฝั่งซ้าย: GridView -->
        <div class="col-md-9">
            <div class="table-responsive">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <asp:GridView ID="GridView1" runat="server"
                        AutoGenerateColumns="false"
                        CssClass="table table-bordered text-center align-middle table-header-softblue my-green-table"
                        UseAccessibleHeader="true"
                        HeaderStyle-BackColor="Transparent"
                        DataKeyNames="TimeSlot"
                        OnRowCommand="GridView1_RowCommand"
                        OnRowDataBound="GridView1_RowDataBound">

                        <RowStyle CssClass="table-light" />
                        <AlternatingRowStyle CssClass="table-white" />
                        <SelectedRowStyle CssClass="table-secondary fw-bold" />


             

                <Columns>
                    <asp:BoundField DataField="TimeSlot" HeaderText="TimeSlot">
                        <ItemStyle Width="15%" />
                    </asp:BoundField>
<asp:TemplateField HeaderText="Room 101">
    <ItemTemplate>
        <div class='<%# (Eval("101")?.ToString().Contains("Booked") ?? false) ? "booked-style" : "free-style" %>'>
            <%# (Eval("101")?.ToString().Contains("Booked") ?? false)
                ? "Booked<br/>by " + Eval("101_Department")
                : "Free" %>
        </div>
        <asp:LinkButton ID="btnCancel101" runat="server"
            Text="cancel" CommandName="CancelBooking"
            CommandArgument='<%# Eval("TimeSlot") + "|101" %>'
            CssClass="btn btn-sm btn-danger mt-1"
            Visible='<%# Eval("101_ReservedBy")?.ToString() == Session["username"]?.ToString() %>' />
    </ItemTemplate>
</asp:TemplateField>

<asp:TemplateField HeaderText="Room 102">
    <ItemTemplate>
        <div class='<%# (Eval("102")?.ToString().Contains("Booked") ?? false) ? "booked-style" : "free-style" %>'>
            <%# (Eval("102")?.ToString().Contains("Booked") ?? false)
                ? "Booked<br/>by " + Eval("102_Department")
                : "Free" %>
        </div>
        <asp:LinkButton ID="btnCancel102" runat="server"
            Text="cancel" CommandName="CancelBooking"
            CommandArgument='<%# Eval("TimeSlot") + "|102" %>'
            CssClass="btn btn-sm btn-danger mt-1"
            Visible='<%# Eval("102_ReservedBy")?.ToString() == Session["username"]?.ToString() %>' />
    </ItemTemplate>
</asp:TemplateField>

<asp:TemplateField HeaderText="Room 103">
    <ItemTemplate>
        <div class='<%# (Eval("103")?.ToString().Contains("Booked") ?? false) ? "booked-style" : "free-style" %>'>
            <%# (Eval("103")?.ToString().Contains("Booked") ?? false)
                ? "Booked<br/>by " + Eval("103_Department")
                : "Free" %>
        </div>
        <asp:LinkButton ID="btnCancel103" runat="server"
            Text="cancel" CommandName="CancelBooking"
            CommandArgument='<%# Eval("TimeSlot") + "|103" %>'
            CssClass="btn btn-sm btn-danger mt-1"
            Visible='<%# Eval("103_ReservedBy")?.ToString() == Session["username"]?.ToString() %>' />
    </ItemTemplate>
</asp:TemplateField>

<asp:TemplateField HeaderText="Room 201">
    <ItemTemplate>
        <div class='<%# (Eval("201")?.ToString().Contains("Booked") ?? false) ? "booked-style" : "free-style" %>'>
            <%# (Eval("201")?.ToString().Contains("Booked") ?? false)
                ? "Booked<br/>by " + Eval("201_Department")
                : "Free" %>
        </div>
        <asp:LinkButton ID="btnCancel201" runat="server"
            Text="cancel" CommandName="CancelBooking"
            CommandArgument='<%# Eval("TimeSlot") + "|201" %>'
            CssClass="btn btn-sm btn-danger mt-1"
            Visible='<%# Eval("201_ReservedBy")?.ToString() == Session["username"]?.ToString() %>' />
    </ItemTemplate>
</asp:TemplateField>

<asp:TemplateField HeaderText="Room 202">
    <ItemTemplate>
        <div class='<%# (Eval("202")?.ToString().Contains("Booked") ?? false) ? "booked-style" : "free-style" %>'>
            <%# (Eval("202")?.ToString().Contains("Booked") ?? false)
                ? "Booked<br/>by " + Eval("202_Department")
                : "Free" %>
        </div>
        <asp:LinkButton ID="btnCancel202" runat="server"
            Text="cancel" CommandName="CancelBooking"
            CommandArgument='<%# Eval("TimeSlot") + "|202" %>'
            CssClass="btn btn-sm btn-danger mt-1"
            Visible='<%# Eval("202_ReservedBy")?.ToString() == Session["username"]?.ToString() %>' />
    </ItemTemplate>
</asp:TemplateField>

<asp:TemplateField HeaderText="Room 203">
    <ItemTemplate>
        <div class='<%# (Eval("203")?.ToString().Contains("Booked") ?? false) ? "booked-style" : "free-style" %>'>
            <%# (Eval("203")?.ToString().Contains("Booked") ?? false)
                ? "Booked<br/>by " + Eval("203_Department")
                : "Free" %>
        </div>
        <asp:LinkButton ID="btnCancel203" runat="server"
            Text="cancel" CommandName="CancelBooking"
            CommandArgument='<%# Eval("TimeSlot") + "|203" %>'
            CssClass="btn btn-sm btn-danger mt-1"
            Visible='<%# Eval("203_ReservedBy")?.ToString() == Session["username"]?.ToString() %>' />
    </ItemTemplate>
</asp:TemplateField>
                    </Columns>
                            </asp:GridView>
                        </ContentTemplate>
                    </asp:UpdatePanel>
            </div>
    </div>
                  
    <div class="col-md-3">
        <asp:Panel ID="pnlBookingResult" runat="server" CssClass="card shadow-sm p-3 bg-light-green h-100" Visible="true">
            <div class="fw-bold text-dark mb-2">✨ Displayable results:</div>

            <div class="mb-2">
                <div class="fw-semibold text-muted">Date</div>
                <asp:Label ID="lblDate" runat="server" Text="-" CssClass="form-control text-center bg-white fw-bold" />
            </div>

            <div class="mb-2">
                <div class="fw-semibold text-muted">Department</div>
                <asp:Label ID="lblDeptBooked" runat="server" Text="-" CssClass="form-control text-center bg-white fw-bold" />
            </div>

            <div class="mb-2">
                <div class="fw-semibold text-muted">Room</div>
                <asp:Label ID="lblRoom" runat="server" Text="-" CssClass="form-control text-center bg-white fw-bold" />
            </div>

            <div class="mb-2">
                <div class="fw-semibold text-muted">Start Time</div>
                <asp:Label ID="lblStart" runat="server" Text="-" CssClass="form-control text-center bg-white fw-bold" />
            </div>

            <div class="mb-2">
                <div class="fw-semibold text-muted">End Time</div>
                <asp:Label ID="lblEnd" runat="server" Text="-" CssClass="form-control text-center bg-white fw-bold" />
            </div>

            <button type="button" class="btn btn-outline-primary mt-3 w-100" onclick="captureResult()">📥 Export booking as image</button>
            <hr />
<div class="text-center mt-3">
    <img src="/Image/room.gif" alt="room icon" class="img-fluid rounded shadow-sm" style="max-width: 160px;" />
</div>
        </asp:Panel>
    </div>
</div>
<script src="https://cdnjs.cloudflare.com/ajax/libs/html2canvas/1.4.1/html2canvas.min.js"></script>
<script type="text/javascript">
    function captureResult() {
        const elementToCapture = document.querySelector("#export-zone");

        html2canvas(elementToCapture, {
            scale: 1,
            useCORS: true,
            backgroundColor: "#ffffff",
            scrollX: 0,
            scrollY: 0
        }).then(function (canvas) {
            var link = document.createElement("a");
            link.download = "booking_summary.png";
            link.href = canvas.toDataURL();
            link.click();
        });
    } 
    function showCalendar() {
        var calendar = document.getElementById("<%= Calendar1.ClientID %>");
        calendar.style.display = (calendar.style.display === "none") ? "block" : "none";
    }
</script>
                </div>
            </div>
        </div>
        </div>

                </asp:Content>
