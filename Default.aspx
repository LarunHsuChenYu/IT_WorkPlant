<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IT_WorkPlant._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .section-title {
            font-weight: bold;
            font-size: 20px;
            text-align: left;
            margin-top: 40px;
            margin-bottom: 5px;
            padding-left: 10px;
        }

        .divider-line {
            border-top: 2px solid #000;
            margin-bottom: 20px;
        }

        .card {
            height: 200px;
            display: flex;
            flex-direction: column;
            justify-content: center;
            align-items: center;
            transition: all 0.3s ease;
        }

        .card:hover {
            transform: translateY(-5px);
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.15);
        }

        .card img {
            width: 100px;
            height: 100px;
        }

        .card h5 {
            margin-top: 10px;
            font-size: 25px;
        }
    </style>

    <main class="text-center">
        <section class="mb-4">
            <h1 class="display-5 fw-bold">IT Work Plate</h1>
            <asp:Label ID="lblWelcome" runat="server" CssClass="lead"></asp:Label>
            <div class="my-3">
                <img src="Image/Enrich_Logo.jpg" alt="Logo" class="img-fluid" style="max-width: 700px; height: auto;" />
            </div>
        </section>

        <div class="container">

        <!-- Public -->
        <div class="section-title">Public</div>
        <div class="divider-line"></div>
        <div class="row row-cols-1 row-cols-sm-2 row-cols-md-4 g-4 justify-content-start">
            <div class="col">
                <a href="Pages/MeetingRoomBooking" class="card shadow-sm text-decoration-none rounded-4 p-3">
                    <img src="Image/appointment.gif" />
                    <h5><%= GetLabel("MeetingRoom") %></h5>
                </a>
            </div>
            <div class="col">
                <a href="Pages/IT_BorrowItem.aspx" class="card shadow-sm text-decoration-none rounded-4 p-3">
                    <img src="Image/borrow.gif" />
                    <h5><%= GetLabel("BorrowRequest") %></h5>
                </a>
            </div>
            <div class="col">
    <a href="Pages/WarRoom.aspx" class="card shadow-sm text-decoration-none rounded-4 p-3">
        <img src="Image/warroom.gif" />
        <h5><%= GetLabel("WarRoom") %></h5>
    </a>
</div>
        </div>

        <!-- Apply -->
        <div class="section-title">Apply</div>
        <div class="divider-line"></div>
        <div class="row row-cols-1 row-cols-sm-2 row-cols-md-4 g-4 justify-content-start">
            <div class="col">
                <a href="Pages/IT_WifiRequest" class="card shadow-sm text-decoration-none rounded-4 p-3">
                    <img src="Image/online.gif" />
                    <h5><%= GetLabel("WifiRequest") %></h5>
                </a>
            </div>
            <div class="col">
                <a href="Pages/IT_EmailRequest.aspx" class="card shadow-sm text-decoration-none rounded-4 p-3">
                    <img src="Image/email.gif" />
                    <h5><%= GetLabel("EmailRequest") %></h5>
                </a>
            </div>
            <div class="col">
                <a href="#" class="card shadow-sm text-decoration-none rounded-4 p-3">
                    <img src="Image/registered.gif" />
                    <h5><%= GetLabel("ERPRequest") %></h5>
                </a>
            </div>
            <div class="col">
                <a href="Pages/IT_Stuff_Purchase" class="card shadow-sm text-decoration-none rounded-4 p-3">
                    <img src="Image/shopping-cart.gif" />
                    <h5><%= GetLabel("ITStuff") %></h5>
                </a>
            </div>
        </div>

        <!-- Help Desk -->
        <div class="section-title">Help Desk</div>
        <div class="divider-line"></div>
        <div class="row row-cols-1 row-cols-sm-2 row-cols-md-4 g-4 justify-content-start">
            <div class="col">
                <a href="Pages/IT_RequestForm" class="card shadow-sm text-decoration-none rounded-4 p-3">
                    <img src="Image/repair-tools.gif" />
                    <h5><%= GetLabel("HelpDesk") %></h5>
                </a>
            </div>
            <div class="col">
                <a href="Pages/IT_RequestsList" class="card shadow-sm text-decoration-none rounded-4 p-3">
                    <img src="Image/checklist.gif" />
                    <h5><%= GetLabel("ITMission") %></h5>
                </a>
            </div>
            <div class="col">
                <a href="Pages/IT_RequestsDashboard.aspx" class="card shadow-sm text-decoration-none rounded-4 p-3">
                    <img src="Image/graph.gif" />
                    <h5><%= GetLabel("ITDashboard") %></h5>
                </a>
            </div>
        </div>

    </div>
</main>
</asp:Content>