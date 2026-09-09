<%@ Page Language="VB" AutoEventWireup="false" CodeBehind="Register.aspx.vb" Inherits="OnlinePharmacy.Register" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Register - Online Pharmacy</title>
    <style>
        body { font-family: Arial; background: #f4f4f4; }
        .form-container { max-width: 500px; margin: 50px auto; background: white; padding: 30px; border-radius: 5px; box-shadow: 0 2px 5px rgba(0,0,0,0.1); }
        .form-group { margin-bottom: 15px; }
        label { display: block; margin-bottom: 5px; font-weight: bold; }
        input[type="text"], input[type="email"], input[type="password"], textarea { width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 3px; }
        .btn { background: #27ae60; color: white; padding: 12px 30px; border: none; cursor: pointer; border-radius: 3px; width: 100%; }
        .btn:hover { background: #229954; }
        .error { color: red; margin-top: 10px; }
        .success { color: green; margin-top: 10px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="form-container">
            <h2>User Registration</h2>
            <div class="form-group">
                <label>Full Name:</label>
                <asp:TextBox ID="txtFullName" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtFullName" ErrorMessage="*Required" ForeColor="Red"></asp:RequiredFieldValidator>
            </div>
            <div class="form-group">
                <label>Email:</label>
                <asp:TextBox ID="txtEmail" runat="server" TextMode="Email"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" ErrorMessage="*Required" ForeColor="Red"></asp:RequiredFieldValidator>
            </div>
            <div class="form-group">
                <label>Password:</label>
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" ErrorMessage="*Required" ForeColor="Red"></asp:RequiredFieldValidator>
            </div>
            <div class="form-group">
                <label>Phone:</label>
                <asp:TextBox ID="txtPhone" runat="server"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Address:</label>
                <asp:TextBox ID="txtAddress" runat="server" TextMode="MultiLine" Rows="3"></asp:TextBox>
            </div>
            <asp:Button ID="btnRegister" runat="server" Text="Register" CssClass="btn" />
            <asp:Label ID="lblMessage" runat="server" CssClass="error"></asp:Label>
            <p>Already have an account? <a href="Login.aspx">Login here</a></p>
        </div>
    </form>
</body>
</html>