<%@ Page Language="VB" AutoEventWireup="false" CodeBehind="Default.aspx.vb" Inherits="OnlinePharmacy._Default" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Online Pharmacy - Home</title>
    <style>
        body { font-family: Arial; margin: 0; padding: 0; }
        .header { background: #2c3e50; color: white; padding: 20px; text-align: center; }
        .nav { background: #34495e; padding: 10px; text-align: center; }
        .nav a { color: white; margin: 0 15px; text-decoration: none; }
        .container { padding: 20px; max-width: 1200px; margin: 0 auto; }
        .product-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); gap: 20px; }
        .product-card { border: 1px solid #ddd; padding: 15px; border-radius: 5px; }
        .btn { background: #3498db; color: white; padding: 10px 20px; border: none; cursor: pointer; border-radius: 3px; }
        .btn:hover { background: #2980b9; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="header">
            <h1>Online Pharmacy Management System</h1>
            <p>Your trusted healthcare partner</p>
        </div>
        <div class="nav">
            <a href="Default.aspx">Home</a>
            <a href="Products.aspx">Products</a>
            <asp:LoginName ID="LoginName1" runat="server" FormatString="Welcome, {0}!" />
            <asp:LoginStatus ID="LoginStatus1" runat="server" LogoutText="Logout" />
            <a href="Cart.aspx">Cart</a>
            <a href="Orders.aspx">My Orders</a>
            <a href="About.aspx">About Us</a>
        </div>
        <div class="container">
            <h2>Featured Products</h2>
            <asp:Repeater ID="rptProducts" runat="server">
                <HeaderTemplate><div class="product-grid"></HeaderTemplate>
                <ItemTemplate>
                    <div class="product-card">
                        <h3><%# Eval("ProductName") %></h3>
                        <p><strong>Brand:</strong> <%# Eval("Brand") %></p>
                        <p><strong>Price:</strong> Rs. <%# Eval("Price") %></p>
                        <p><strong>Stock:</strong> <%# Eval("StockQuantity") %></p>
                        <p><%# Eval("Description") %></p>
                        <asp:Button ID="btnAddToCart" runat="server" Text="Add to Cart" 
                                    CommandName="AddToCart" CommandArgument='<%# Eval("ProductID") %>'
                                    CssClass="btn" OnCommand="AddToCart_Click" />
                    </div>
                </ItemTemplate>
                <FooterTemplate></div></FooterTemplate>
            </asp:Repeater>
        </div>
    </form>
</body>
</html>