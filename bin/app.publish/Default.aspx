<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="IT_WorkPlant._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <main>
        <section class="row" aria-labelledby="aspnetTitle">
            <h1 id="aspnetTitle">IT Work Plate</h1>
            <asp:Label ID="lblWelcome" runat="server"></asp:Label>
            <p class="lead">
                <img src="Image/Enrich_Logo.jpg" alt="Logo" style="width:1000px; height: 250px;"/>
            </p>
            <%--<p><a href="http://www.asp.net" class="btn btn-primary btn-md">Learn more &raquo;</a></p>--%>
        </section>
        
            <div class="row">
                <section class="col-md-3" aria-labelledby="gettingStartedTitle">
                    <h2 id="MeetingRoomBooking">Meeting Room Booking</h2>
                    <a href="Pages/MeetingRoomBooking">
                        <img src="Image/appointment.gif" alt="Logo" style="width:150px; height: 150px;" />
                    </a>
                </section>
                
                <section class="col-md-3" aria-labelledby="gettingStartedTitle">
                    <h2 id="itHelpDesk">IT HelpDesk</h2>
                    <a href="Pages/IT_RequestForm">
                        <img src="Image/repair-tools.gif" alt="Logo" style="width:150px; height: 150px;"/>
                    </a>
                </section>
                <section class="col-md-3" aria-labelledby="gettingStartedTitle">
                    <h2 id="WifiRequest">WIFI Request</h2>
                    <a href="Pages/IT_WifiRequest">
                        <img src="Image/online.gif" alt="Logo" style="width:150px; height: 150px;"/>
                    </a>
                </section>
                
                <section class="col-md-3" aria-labelledby="gettingStartedTitle">
                    <h2 id="EMailRequest">EMAIL Request</h2>
                    <a href="Pages/IT_EmailRequest.aspx">
                        <img src="Image/email.gif" alt="Logo" style="width:150px; height: 150px;"/>
                    </a>
                </section>
                
                <section class="col-md-3" aria-labelledby="gettingStartedTitle">
                    <h2 id="itPurchase">IT Stuff Purchase</h2>
                    <a href="Pages/IT_Stuff_Purchase">
                        <img src="Image/shopping-cart.gif" alt="Logo" style="width:150px; height: 150px;"/>        
                    </a>
                </section>
                

                <section class="col-md-3" aria-labelledby="gettingStartedTitle">
                    <h2 id="ERP_New_Account">ERP Register Request</h2>
                    <p>
                        <img src="Image/registered.gif" alt="Logo" style="width:150px; height: 150px;"/>
                    </p>
                </section>
                
                <section class="col-md-3" aria-labelledby="gettingStartedTitle">
                    <h2 id="itJobList">IT Mission Check</h2>
                    <a href="Pages/IT_RequestsList">
                        <img src="Image/checklist.gif" alt="Logo" style="width:150px; height: 150px;"/>
                    </a>
                </section>
                
                <%--<section class="col-md-4" aria-labelledby="librariesTitle">
                    <h2 id="librariesTitle">Get more libraries</h2>
                    <p>
                        &nbsp;</p>
                    <p>
                        <a class="btn btn-default" href="https://go.microsoft.com/fwlink/?LinkId=301949">Learn more &raquo;</a>
                    </p>
                </section>--%>
            </div>
        

        
    </main>

</asp:Content>
