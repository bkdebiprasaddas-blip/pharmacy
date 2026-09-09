<%@ Page Language="VB" AutoEventWireup="false" CodeBehind="Orders.aspx.vb" Inherits="OnlinePharmacy.Orders" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>My Orders - Online Pharmacy</title>
    <style>
        body { font-family: Arial; }
        .container { max-width: 1200px; margin: 0 auto; padding: 20px; }
        table { width: 100%; border-collapse: collapse; margin-top: 20px; }
        th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }
        th { background: #34495e; color: white; }
        .status-pending { color: #f39c12; font-weight: bold; }
        .status-confirmed { color: #3498db; font-weight: bold; }
        .status-delivered { color: #27ae60; font-weight: bold; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="container">
            <h2>My Orders</h2>
            <asp:GridView ID="gvOrders" runat="server" AutoGenerateColumns="False">
                <Columns>
                    <asp:BoundField DataField="OrderID" HeaderText="Order ID" />
                    <asp:BoundField DataField="OrderDate" HeaderText="Order Date" DataFormatString="{0:dd-MMM-yyyy HH:mm}" />
                    <asp:BoundField DataField="TotalAmount" HeaderText="Total Amount" DataFormatString="Rs. {0}" />
                    <asp:BoundField DataField="Status" HeaderText="Status" />
                    <asp:BoundField DataField="DeliveryAddress" HeaderText="Delivery Address" />
                    <asp:BoundField DataField="PaymentMethod" HeaderText="Payment" />
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>