<%@ Page Title="Borrow Approval" Language="C#" MasterPageFile="~/Site.Master" 
    AutoEventWireup="true" CodeBehind="IT_BorrowApproval.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_BorrowApproval" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container py-4">
        <asp:Label ID="lblTitle" runat="server" CssClass="fw-bold text-primary mb-4" />

        <div class="card shadow-sm p-4">
            <asp:GridView ID="gvBorrowRequests" runat="server" AutoGenerateColumns="False"
                CssClass="table table-bordered table-hover"
                OnRowCommand="gvBorrowRequests_RowCommand"
                OnRowDataBound="gvBorrowRequests_RowDataBound"
                EnableViewState="true">
                <Columns>

                    <asp:TemplateField HeaderText="Borrow ID">
                        <ItemTemplate><%# Eval("BorrowID") %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Item">
                        <ItemTemplate><%# Eval("ItemName") %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="User">
                        <ItemTemplate><%# Eval("UserName") %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Department">
                        <ItemTemplate><%# Eval("DeptName") %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Date">
                        <ItemTemplate><%# Eval("BorrowDate", "{0:yyyy-MM-dd}") %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Start Time">
                        <ItemTemplate><%# Eval("BorrowStartTime", "{0:hh\\:mm}") %></ItemTemplate>
                        <ItemStyle Width="80px" HorizontalAlign="Center" />
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="End Time">
                        <ItemTemplate><%# Eval("BorrowEndTime", "{0:hh\\:mm}") %></ItemTemplate>
                        <ItemStyle Width="80px" HorizontalAlign="Center" />
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Qty">
                        <ItemTemplate><%# Eval("Quantity") %></ItemTemplate>
                        <ItemStyle Width="50px" HorizontalAlign="Center" />
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Purpose">
                        <ItemTemplate><%# Eval("Purpose") %></ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Serial No.">
                        <ItemTemplate>
                            <asp:Label ID="lblSerial" runat="server" Text='<%# GetSerialNumber(Eval("SerialItemID")) %>' />
                        </ItemTemplate>
                        <ItemStyle Width="120px" />
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Status">
                        <ItemTemplate>
                            <%# Eval("Status") == null ? "" : GetStatusBadge(Eval("Status").ToString()) %>
                        </ItemTemplate>
                    </asp:TemplateField>

                    <asp:TemplateField HeaderText="Returned">
    <ItemTemplate>
        <%# Convert.ToBoolean(Eval("Returned")) ?
    "<span class='badge bg-success'>✅ " + GetLabel("DONE") + "</span>" :
    "<span class='badge bg-warning text-dark'>🔄 " + GetLabel("WIP") + "</span>" %>
    </ItemTemplate>
    <ItemStyle HorizontalAlign="Center" />
</asp:TemplateField>

                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:Button ID="btnApprove" runat="server" CssClass="btn btn-success btn-sm mb-1"
                                Text='<%# "✅ " + GetLabel("Approve") %>' CommandName="Approve"
                                CommandArgument='<%# Eval("BorrowID") %>' UseSubmitBehavior="false" CausesValidation="false" />

                            <asp:Button ID="btnReject" runat="server" CssClass="btn btn-danger btn-sm mb-1"
                                Text='<%# "❌ " + GetLabel("Reject") %>' CommandName="Reject"
                                CommandArgument='<%# Eval("BorrowID") %>' UseSubmitBehavior="false" CausesValidation="false" />

                            <asp:Button ID="btnReturn" runat="server" CssClass="btn btn-warning btn-sm"
                                Text='<%# "📦 " + GetLabel("Return") %>' CommandName="Return"
                                CommandArgument='<%# Eval("BorrowID") %>' UseSubmitBehavior="false" CausesValidation="false" />
                        </ItemTemplate>
                        <ItemStyle Width="160px" />
                    </asp:TemplateField>

                </Columns>

                <EmptyDataTemplate>
                    <div class="alert alert-info text-center">
                        <asp:Literal runat="server" Text='<%# GetLabel("NoData") %>' />
                    </div>
                </EmptyDataTemplate>

            </asp:GridView>
        </div>
    </div>
</asp:Content>
