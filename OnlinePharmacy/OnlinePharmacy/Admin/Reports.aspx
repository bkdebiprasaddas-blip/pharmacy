<%@ Page Language="VB" AutoEventWireup="false" CodeBehind="Reports.aspx.vb" Inherits="OnlinePharmacy.Admin_Reports" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Reports - Admin</title>
    <style>
        body { font-family: Arial; }
        .sidebar { width: 250px; background: #2c3e50; color: white; height: 100vh; position: fixed; padding: 20px; }
        .sidebar a { display: block; color: white; padding: 10px; text-decoration: none; }
        .main-content { margin-left: 270px; padding: 20px; }
        .report-section { background: #f9f9f9; padding: 20px; margin-bottom: 20px; border-radius: 5px; }
        table { width: 100%; border-collapse: collapse; margin-top: 10px; }
        th, td { padding: 10px; border: 1px solid #ddd; }
        th { background: #34495e; color: white; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="sidebar">
            <h2>Admin Panel</h2>
            <a href="Dashboard.aspx">Dashboard</a>
            <a href="Reports.aspx">Reports</a>
        </div>
        <div class="main-content">
            <h2>Sales Reports</h2>
            
            <div class="report-section">
                <h3>Recent Orders</h3>
                <asp:GridView ID="gvRecentOrders" runat="server" AutoGenerateColumns="False">
                    <Columns>
                        <asp:BoundField DataField="OrderID" HeaderText="Order ID" />
                        <asp:BoundField DataField="CustomerName" HeaderText="Customer" />
                        <asp:BoundField DataField="OrderDate" HeaderText="Date" DataFormatString="{0:dd-MMM-yyyy}" />
                        <asp:BoundField DataField="TotalAmount" HeaderText="Amount" DataFormatString="Rs. {0}" />
                        <asp:BoundField DataField="Status" HeaderText="Status" />
                    </Columns>
                </asp:GridView>
            </div>

            <div class="report-section">
                <h3>Low Stock Products</h3>
                <asp:GridView ID="gvLowStock" runat="server" AutoGenerateColumns="False">
                    <Columns>
                        <asp:BoundField DataField="ProductName" HeaderText="Product" />
                        <asp:BoundField DataField="Brand" HeaderText="Brand" />
                        <asp:BoundField DataField="StockQuantity" HeaderText="Current Stock" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </form>
</body>
</html>