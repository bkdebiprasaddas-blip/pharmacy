<%@ Page Language="VB" AutoEventWireup="false" CodeBehind="Login.aspx.vb" Inherits="OnlinePharmacy.Login" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Login - Online Pharmacy</title>
    <style>
        body { font-family: Arial; background: #f4f4f4; }
        .login-container { max-width: 400px; margin: 100px auto; background: white; padding: 30px; border-radius: 5px; box-shadow: 0 2px 5px rgba(0,0,0,0.1); }
        .form-group { margin-bottom: 15px; }
        label { display: block; margin-bottom: 5px; font-weight: bold; }
        input[type="email"], input[type="password"] { width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 3px; }
        .btn { background: #3498db; color: white; padding: 12px 30px; border: none; cursor: pointer; border-radius: 3px; width: 100%; }
        .btn:hover { background: #2980b9; }
        .error { color: red; margin-top: 10px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="login-container">
            <h2>User Login</h2>
            <div class="form-group">
                <label>Email:</label>
                <asp:TextBox ID="txtEmail" runat="server" TextMode="Email"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Password:</label>
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password"></asp:TextBox>
            </div>
            <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn" />
            <asp:Label ID="lblMessage" runat="server" CssClass="error"></asp:Label>
            <p>Don't have an account? <a href="Register.aspx">Register here</a></p>
        </div>
    </form>
</body>
</html>