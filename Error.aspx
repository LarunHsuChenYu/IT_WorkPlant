<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Error.aspx.cs" Inherits="IT_WorkPlant.Error" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Error - Something went wrong</title>
    <style>
        body {
            font-family: 'Segoe UI', sans-serif;
            background-color: #fff0f0;
            color: #333;
            padding: 50px;
        }
        .box {
            background: white;
            padding: 40px;
            border-radius: 10px;
            max-width: 600px;
            margin: auto;
            box-shadow: 0 0 10px rgba(255,0,0,0.3);
        }
        h1 {
            color: #dc3545;
        }
        .back-btn {
            display: inline-block;
            margin-top: 20px;
            padding: 10px 20px;
            background: #dc3545;
            color: white;
            border-radius: 5px;
            text-decoration: none;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="box">
            <h1>Oops! Something went wrong 😵</h1>
            <p><strong>Error Message:</strong> <%= Server.UrlDecode(Request.QueryString["message"] ?? "Unknown error.") %></p>
            <a class="back-btn" href="javascript:history.back()">← Go Back</a>
        </div>
    </form>
</body>
</html>
