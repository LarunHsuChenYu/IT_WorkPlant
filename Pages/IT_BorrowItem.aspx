<%@ Page Title="Borrow Request Form" Language="C#" MasterPageFile="~/Site.Master"
    AutoEventWireup="true" CodeBehind="IT_BorrowItem.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_BorrowItem" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="p-4" style="background: linear-gradient(to right, #dbeafe, #fdf2f8); min-height: 100vh;">
        <h3 class="text-primary fw-bold mb-4"><%= GetLabel("BorrowRequestForm") %></h3>

        <div class="row mb-3">
            <div class="col-md-3">
                <label><%= GetLabel("User") %></label>
                <asp:TextBox ID="txtUser" runat="server" CssClass="form-control" ReadOnly="true" />
            </div>
            <div class="col-md-3">
                <label><%= GetLabel("Department") %></label>
                <asp:TextBox ID="txtDept" runat="server" CssClass="form-control" ReadOnly="true" />
            </div>
            <div class="col-md-3">
                <label><%= GetLabel("Date") %></label>
                <asp:TextBox ID="txtToday" runat="server" CssClass="form-control" TextMode="Date"
                    AutoPostBack="true" OnTextChanged="txtToday_TextChanged" />
            </div>
        </div>

        <asp:GridView ID="gvBorrowItems" runat="server" AutoGenerateColumns="False"
            CssClass="table table-bordered text-center"
            OnRowDataBound="gvBorrowItems_RowDataBound">
            <Columns>
                <asp:TemplateField HeaderText="#">
                    <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="">
                    <ItemTemplate>
                        <asp:DropDownList ID="ddlItem" runat="server" CssClass="form-select" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlItem_SelectedIndexChanged" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="Serial No.">
    <ItemTemplate>
        <div style="max-height: 200px; overflow-y: auto;">
            <asp:CheckBoxList ID="chkSerials" runat="server"
                CssClass="form-check d-block"
                RepeatDirection="Vertical"
                RepeatLayout="Table" />
        </div>
    </ItemTemplate>
</asp:TemplateField>

                <asp:TemplateField HeaderText="">
                    <ItemTemplate>
                        <asp:DropDownList ID="ddlUseTime" runat="server" CssClass="form-select" AutoPostBack="true"
                            OnSelectedIndexChanged="ddlUseTime_SelectedIndexChanged" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="">
                    <ItemTemplate>
                        <asp:DropDownList ID="ddlReturnTime" runat="server" CssClass="form-select" />
                    </ItemTemplate>
                </asp:TemplateField>

                <asp:TemplateField HeaderText="">
                    <ItemTemplate>
                        <asp:TextBox ID="txtRemark" runat="server" CssClass="form-control" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
        
        <asp:Button ID="btnAddRow" runat="server" Text="+ Add Row"
    CssClass="btn btn-outline-primary my-2"
    OnClick="btnAddRow_Click" />


        <div class="d-flex justify-content-between align-items-center mt-3">
            <asp:Button ID="btnSubmitMemo" runat="server"
                CssClass="btn btn-success fw-bold me-2 w-100"
                OnClick="btnSubmitMemo_Click" />

            <button type="button" class="btn btn-outline-dark fw-bold" onclick="scrollToHistory()">📜 <%= GetLabel("View History") %></button>
        </div>

        <div class="mt-5">
            <h4 class="fw-bold text-dark mb-3"><%= GetLabel("DeviceStatusOverview") %></h4>
            <div class="row row-cols-1 row-cols-sm-2 row-cols-md-3 g-4">
                <asp:Repeater ID="rptDeviceStatus" runat="server" OnItemDataBound="rptDeviceStatus_ItemDataBound">
                    <ItemTemplate>
                        <div class="col">
                            <div class="card shadow-sm border-0 h-100">
                                <div class="card-body">
                                    <h6 class="card-title fw-bold mb-1"><%# Eval("ItemName") %></h6>
                                    <p class="mb-1 text-muted small">Serial: <%# Eval("SerialNumber") %></p>
                                    <p class="mb-1 text-muted small">
                                        <strong><%= GetLabel("QtyLeft") %>:</strong> <%# Eval("AvailableQty") %><br />
                                        <strong><%= GetLabel("Status") %>:</strong>
                                        <span class='<%# Convert.ToInt32(Eval("AvailableQty")) == 0 ? "text-danger fw-bold" : "text-success fw-bold" %>'>
                                            <%# Convert.ToInt32(Eval("AvailableQty")) == 0 ? GetLabel("Unavailable") : GetLabel("Available") %>
                                        </span>
                                    </p>
                                    <img src='<%# GetItemImageUrl(Eval("ItemName").ToString()) %>' alt="item" class="position-absolute top-0 end-0 m-2" style="width: 100px;" />

                                    <asp:HyperLink ID="lnkViewBorrowers" runat="server"
                                        NavigateUrl='<%# "IT_BorrowStatus.aspx?item=" + Eval("ItemID") %>'
                                        CssClass="btn btn-outline-primary w-100 mt-2">
                                        <%= GetLabel("ViewBorrowers") %>
                                    </asp:HyperLink>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <h4 class="mt-5 fw-bold text-dark"><%= GetLabel("YourBorrowHistory") %></h4>
        <div id="historySection" style="display: none;">
            <asp:GridView ID="gvBorrowHistory" runat="server" CssClass="table table-bordered table-striped table-hover"
                AutoGenerateColumns="False" ShowHeaderWhenEmpty="True">
                <Columns>
                    <asp:TemplateField HeaderText="#">
                        <ItemTemplate><%# Container.DataItemIndex + 1 %></ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="ItemName" HeaderText="" />
                    <asp:BoundField DataField="Quantity" HeaderText="" />
                    <asp:BoundField DataField="BorrowDate" HeaderText="" DataFormatString="{0:yyyy-MM-dd}" />
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <%# ((TimeSpan)Eval("BorrowStartTime")).ToString(@"hh\:mm") + " - " + ((TimeSpan)Eval("BorrowEndTime")).ToString(@"hh\:mm") %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <%# Eval("Status").ToString() == "PASS" ? "<span class='text-success fw-bold'>✅ " + GetLabel("Approved") + "</span>" :
                                Eval("Status").ToString() == "REJECT" ? "<span class='text-danger fw-bold'>❌ " + GetLabel("Rejected") + "</span>" :
                                "<span class='text-warning fw-bold'>⌛ " + GetLabel("Pending") + "</span>" %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <%# (bool)Eval("Returned") ? "<span class='text-success fw-bold'>✅</span>" :
                                "<span class='text-danger fw-bold bg-danger bg-opacity-10 px-2 py-1 rounded'>❌ " + GetLabel("NotReturned") + "</span>" %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="">
                        <ItemTemplate>
                            <%# Eval("BorrowEndTime") != DBNull.Value ? ((TimeSpan)Eval("BorrowEndTime")).ToString(@"hh\:mm") : "-" %>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>

        <script>
            function scrollToHistory() {
                const section = document.getElementById("historySection");
                if (section) {
                    section.style.display = "block";
                    section.scrollIntoView({ behavior: "smooth", block: "start" });
                }
            }
        </script>
    </div>
</asp:Content>
