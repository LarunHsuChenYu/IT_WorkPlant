<%@ Page Language="C#"  MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MeetingRoomBooking.aspx.cs" 
    Inherits="IT_WorkPlant.Pages.MeetingRoomBooking" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title></title>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <section class="row" aria-labelledby="aspnetTitle">
        <h1 id="aspnetTitle">會議室預約 Meeting Room Booking</h1>
        <%--<p class="lead">ASP.NET is a free web framework for building great Web sites and Web applications using HTML, CSS, and JavaScript.</p>--%>
        <%--<p><a href="http://www.asp.net" class="btn btn-primary btn-md">Learn more &raquo;</a></p>--%>
    </section>
    <div class="row">
        <section class="col-md-8">
            <style>
                .myGridView td, .myGridView th {
                border: 1px solid #000;
                }
            </style>

            <p>
                <label for="GridView1">Schedule of Date:</label>
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" 
                        CssClass="myGridView" 
                    OnRowDataBound="GridView1_RowDataBound"
                        GridLines="Both" Width="100%" CellPadding="4" ForeColor="#333333" BorderStyle="Solid" BorderWidth="1px">
                    <HeaderStyle BackColor="#6B8E23" Font-Bold="True" ForeColor="White" Height="40px" />
                    <RowStyle BackColor="#F7F7DE" Height="35px" />
                    <AlternatingRowStyle BackColor="White" />
                    <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" />
                    <Columns>
                        <asp:BoundField DataField="TimeSlot" HeaderText="時段">
                            <ItemStyle Width="15%" HorizontalAlign="Center" VerticalAlign="Middle" />
                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" />
                        </asp:BoundField>

                        <asp:BoundField DataField="101" HeaderText="Room 101">
                            <ItemStyle Width="14%" HorizontalAlign="Center" VerticalAlign="Middle" />
                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" />
                        </asp:BoundField>

                        <asp:BoundField DataField="102" HeaderText="Room 102">
                            <ItemStyle Width="14%" HorizontalAlign="Center" VerticalAlign="Middle" />
                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" />
                        </asp:BoundField>

                        <asp:BoundField DataField="103" HeaderText="Room 103">
                            <ItemStyle Width="14%" HorizontalAlign="Center" VerticalAlign="Middle" />
                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" />
                        </asp:BoundField>

                        <asp:BoundField DataField="201" HeaderText="Room 201">
                            <ItemStyle Width="14%" HorizontalAlign="Center" VerticalAlign="Middle" />
                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" />
                        </asp:BoundField>

                        <asp:BoundField DataField="202" HeaderText="Room 202">
                            <ItemStyle Width="14%" HorizontalAlign="Center" VerticalAlign="Middle" />
                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" />
                        </asp:BoundField>

                        <asp:BoundField DataField="203" HeaderText="Room 203">
                            <ItemStyle Width="14%" HorizontalAlign="Center" VerticalAlign="Middle" />
                            <HeaderStyle HorizontalAlign="Center" VerticalAlign="Middle" />
                        </asp:BoundField>

                    </Columns>
                </asp:GridView>

            </p>
            
        </section>
        <section class="col-md-4" aria-labelledby="selectInfo">
            <p>
                <!-- Date Selection -->
                <label for="Calendar1">Select Date:</label>
                <asp:Calendar ID="Calendar1" runat="server" OnSelectionChanged="Calendar1_SelectionChanged"></asp:Calendar>
            </p>
            <p>
                <!-- Select Room -->
                <label for="roomList">Select Room:</label>
                <asp:DropDownList ID="roomList" runat="server">
                    <asp:ListItem Value="" Text="---Select---"></asp:ListItem>
                    <asp:ListItem Value="101">Room 101</asp:ListItem>
                    <asp:ListItem Value="102">Room 102</asp:ListItem>
                    <asp:ListItem Value="103">Room 103</asp:ListItem>
                    <asp:ListItem Value="201">Room 201</asp:ListItem>
                    <asp:ListItem Value="202">Room 202</asp:ListItem>
                    <asp:ListItem Value="203">Room 203</asp:ListItem>
                </asp:DropDownList>
            </p>
            <p>
                <!-- Select Department -->
                <label for="departmentList">Select Department:</label>
                <asp:DropDownList ID="departmentList" runat="server">
                </asp:DropDownList>
            </p>
            <p>
                <!-- Time Slot Selection -->
                <label for="startTimeList">Select Start Time:</label>
                <asp:DropDownList ID="startTimeList" runat="server">
                </asp:DropDownList>
            </p>
            <p>
                <label for="durationList">Select Duration (Hours):</label>
                <asp:DropDownList ID="durationList" runat="server">
                </asp:DropDownList>
            </p>
            <p>
                <label for="UserTextbox">Booking By:</label>
                <asp:TextBox ID="tbUser" runat="server">
                </asp:TextBox>
            </p>
            <p>
                <!-- Submit Button -->
                <asp:Button ID="submitButton" runat="server" Text="Book Room" OnClick="submitButton_Click" />
            </p>
        </section>
        
    </div>
    
</asp:Content>