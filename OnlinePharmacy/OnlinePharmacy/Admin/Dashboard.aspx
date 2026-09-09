<%@ Page Language="VB" AutoEventWireup="false" CodeBehind="Dashboard.aspx.vb" Inherits="OnlinePharmacy.Admin_Dashboard" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Admin Dashboard - Online Pharmacy</title>
    <style>
        body { font-family: Arial; margin: 0; padding: 0; }
        .sidebar { width: 250px; background: #2c3e50; color: white; height: 100vh; position: fixed; padding: 20px; }
        .sidebar h2 { margin-top: 0; }
        .sidebar a { display: block; color: white; padding: 10px; text-decoration: none; margin: 5px 0; }
        .sidebar a:hover { background: #34495e; }
        .main-content { margin-left: 250px; padding: 20px; }
        .stats { display: grid; grid-template-columns: repeat(4, 1fr); gap: 20px; margin-bottom: 30px; }
        .stat-card { background: #3498db; color: white; padding: 20px; border-radius: 5px; }
        .stat-card h3 { margin: 0; font-size: 36px; }
        .stat-card p { margin: 5px 0 0 0; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="sidebar">
            <h2>Admin Panel</h2>
            <a href="Dashboard.aspx">Dashboard</a>
            <a href="ManageUsers.aspx">Manage Users</a>
            <a href="ManageProducts.aspx">Manage Products</a>
            <a href="ManageCategories.aspx">Manage Categories</a>
            <a href="ManageOrders.aspx">Manage Orders</a>
            <a href="Reports.aspx">Reports</a>
            <a href="../Default.aspx">Logout</a>
        </div>
        <div class="main-content">
            <h2>Dashboard Overview</h2>
            <div class="stats">
                <div class="stat-card">
                    <h3><asp:Label ID="lblTotalUsers" runat="server" Text="0"></asp:Label></h3>
                    <p>Total Users</p>
                </div>
                <div class="stat-card">
                    <h3><asp:Label ID="lblTotalProducts" runat="server" Text="0"></asp:Label></h3>
                    <p>Total Products</p>
                </div>
                <div class="stat-card">
                    <h3><asp:Label ID="lblTotalOrders" runat="server" Text="0"></asp:Label></h3>
                    <p>Total Orders</p>
                </div>
                <div class="stat-card">
                    <h3>Rs. <asp:Label ID="lblTotalRevenue" runat="server" Text="0"></asp:Label></h3>
                    <p>Total Revenue</p>
                </div>
            </div>
        </div>
    </form>
</body>
</html>