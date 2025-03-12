<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="IT_WebPortalList.aspx.cs" Inherits="IT_WorkPlant.Pages.IT_WebPortalList" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>IT 入口網頁</title>

    <!-- Bootstrap -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
    <!-- DataTables -->
    <link href="https://cdn.datatables.net/1.13.6/css/jquery.dataTables.min.css" rel="stylesheet">

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-4">
        <h1 class="text-center">🔗 IT Web Protal</h1>

        <div class="card mt-4">
            <div class="card-body">
                <table id="linkTable" class="table table-striped">
                    <thead>
                        <tr>
                            <th>No.</th>
                            <th>System</th>
                            <th>Link</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptLinks" runat="server">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("No") %></td>
                                    <td><%# Eval("System") %></td>
                                    <td>
                                        <a href="<%# Eval("URL") %>" target="_blank" class="btn btn-primary btn-sm">GO...</a>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <!-- jQuery & Bootstrap -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
    <!-- DataTables -->
    <script src="https://cdn.datatables.net/1.13.6/js/jquery.dataTables.min.js"></script>
    <script>
        $(document).ready(function () {
            $('#linkTable').DataTable({
                "paging": true,
                "searching": true,
                "ordering": true,
                "info": false,
                "language": {
                    "search": "🔍 Search："
                }
            });
        });
    </script>


</asp:Content>