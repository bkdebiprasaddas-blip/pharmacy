<%@ Page Language="VB" AutoEventWireup="false" CodeBehind="Products.aspx.vb" Inherits="OnlinePharmacy.Products" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Products - Online Pharmacy</title>
    <style>
        body { font-family: Arial; margin: 0; padding: 0; }
        .header { background: #2c3e50; color: white; padding: 20px; text-align: center; }
        .nav { background: #34495e; padding: 10px; text-align: center; }
        .nav a { color: white; margin: 0 15px; text-decoration: none; }
        .container { padding: 20px; max-width: 1200px; margin: 0 auto; }
        .search-box { margin-bottom: 20px; }
        .search-box input { padding: 10px; width: 300px; border: 1px solid #ddd; }
        .search-box button { padding: 10px 20px; background: #3498db; color: white; border: none; cursor: pointer; }
        .product-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); gap: 20px; }
        .product-card { border: 1px solid #ddd; padding: 15px; border-radius: 5px; }
        .btn { background: #3498db; color: white; padding: 10px 20px; border: none; cursor: pointer; border-radius: 3px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="header">
            <h1>Our Products</h1>
        </div>
        <div class="nav">
            <a href="Default.aspx">Home</a>
            <a href="Products.aspx">Products</a>
            <a href="Cart.aspx">Cart</a>
            <a href="Orders.aspx">My Orders</a>
            <a href="About.aspx">About Us</a>
        </div>
        <div class="container">
            <div class="search-box">
                <asp:TextBox ID="txtSearch" runat="server" placeholder="Search products..."></asp:TextBox>
                <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn" />
                <asp:DropDownList ID="ddlCategory" runat="server" AutoPostBack="true">
                    <asp:ListItem Value="">All Categories</asp:ListItem>
                </asp:DropDownList>
            </div>
            <asp:Repeater ID="rptProducts" runat="server">
                <HeaderTemplate><div class="product-grid"></HeaderTemplate>
                <ItemTemplate>
                    <div class="product-card">
                        <h3><%# Eval("ProductName") %></h3>
                        <p><strong>Brand:</strong> <%# Eval("Brand") %></p>
                        <p><strong>Category:</strong> <%# Eval("CategoryName") %></p>
                        <p><strong>Price:</strong> Rs. <%# Eval("Price") %></p>
                        <p><strong>Stock:</strong> <%# Eval("StockQuantity") %></p>
                        <p><strong>Expiry:</strong> <%# Eval("ExpiryDate", "{0:dd-MMM-yyyy}") %></p>
                        <p><%# Eval("Description") %></p>
                        <asp:Button ID="btnAddToCart" runat="server" Text="Add to Cart" 
                                    CommandArgument='<%# Eval("ProductID") %>'
                                    CssClass="btn" OnCommand="AddToCart_Click" />
                    </div>
                </ItemTemplate>
                <FooterTemplate></div></FooterTemplate>
            </asp:Repeater>
        </div>
    </form>
</body>
</html>