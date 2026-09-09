<%@ Page Language="VB" AutoEventWireup="false" CodeBehind="ManageOrders.aspx.vb" Inherits="OnlinePharmacy.Admin_ManageOrders" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Manage Orders - Admin</title>
    <style>
        body { font-family: Arial; }
        .sidebar { width: 250px; background: #2c3e50; color: white; height: 100vh; position: fixed; padding: 20px; }
        .sidebar a { display: block; color: white; padding: 10px; text-decoration: none; }
        .main-content { margin-left: 270px; padding: 20px; }
        table { width: 100%; border-collapse: collapse; }
        th, td { padding: 10px; border: 1px solid #ddd; }
        th { background: #34495e; color: white; }
        .btn { padding: 5px 15px; border: none; cursor: pointer; border-radius: 3px; }
        .btn-update { background: #3498db; color: white; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="sidebar">
            <h2>Admin Panel</h2>
            <a href="Dashboard.aspx">Dashboard</a>
            <a href="ManageOrders.aspx">Manage Orders</a>
            <a href="Reports.aspx">Reports</a>
        </div>
        <div class="main-content">
            <h2>Manage Orders</h2>
            <asp:GridView ID="gvOrders" runat="server" AutoGenerateColumns="False" DataKeyNames="OrderID" OnRowCommand="gvOrders_RowCommand">
                <Columns>
                    <asp:BoundField DataField="OrderID" HeaderText="Order ID" />
                    <asp:BoundField DataField="UserName" HeaderText="Customer" />
                    <asp:BoundField DataField="OrderDate" HeaderText="Order Date" DataFormatString="{0:dd-MMM-yyyy}" />
                    <asp:BoundField DataField="TotalAmount" HeaderText="Amount" DataFormatString="Rs. {0}" />
                    <asp:BoundField DataField="Status" HeaderText="Status" />
                    <asp:BoundField DataField="PaymentMethod" HeaderText="Payment" />
                    <asp:TemplateField HeaderText="Update Status">
                        <ItemTemplate>
                            <asp:DropDownList ID="ddlStatus" runat="server">
                                <asp:ListItem>Pending</asp:ListItem>
                                <asp:ListItem>Confirmed</asp:ListItem>
                                <asp:ListItem>Delivered</asp:ListItem>
                            </asp:DropDownList>
                            <asp:Button ID="btnUpdate" runat="server" Text="Update" CommandName="UpdateStatus" 
                                        CommandArgument='<%# Eval("OrderID") %>' CssClass="btn btn-update" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>