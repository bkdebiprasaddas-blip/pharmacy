<%@ Page Language="VB" AutoEventWireup="false" CodeBehind="Checkout.aspx.vb" Inherits="OnlinePharmacy.Checkout" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Checkout - Online Pharmacy</title>
    <style>
        body { font-family: Arial; }
        .container { max-width: 800px; margin: 50px auto; padding: 20px; }
        .form-group { margin-bottom: 15px; }
        label { display: block; margin-bottom: 5px; font-weight: bold; }
        input, textarea, select { width: 100%; padding: 10px; border: 1px solid #ddd; border-radius: 3px; }
        .btn { background: #27ae60; color: white; padding: 12px 30px; border: none; cursor: pointer; border-radius: 3px; }
        .order-summary { background: #f9f9f9; padding: 20px; border-radius: 5px; margin-bottom: 20px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <h2>Checkout</h2>
            <div class="order-summary">
                <h3>Order Summary</h3>
                <asp:Label ID="lblOrderSummary" runat="server"></asp:Label>
            </div>
            <div class="form-group">
                <label>Delivery Address:</label>
                <asp:TextBox ID="txtAddress" runat="server" TextMode="MultiLine" Rows="3"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Payment Method:</label>
                <asp:DropDownList ID="ddlPayment" runat="server">
                    <asp:ListItem>Cash on Delivery</asp:ListItem>
                    <asp:ListItem>Online Payment</asp:ListItem>
                </asp:DropDownList>
            </div>
            <asp:Button ID="btnPlaceOrder" runat="server" Text="Place Order" CssClass="btn" OnClick="btnPlaceOrder_Click" />
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>
        </div>
    </form>
</body>
</html>