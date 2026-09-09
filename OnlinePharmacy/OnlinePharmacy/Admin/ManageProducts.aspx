<%@ Page Language="VB" AutoEventWireup="false" CodeBehind="ManageProducts.aspx.vb" Inherits="OnlinePharmacy.Admin_ManageProducts" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Manage Products - Admin</title>
    <style>
        body { font-family: Arial; }
        .sidebar { width: 250px; background: #2c3e50; color: white; height: 100vh; position: fixed; padding: 20px; }
        .sidebar a { display: block; color: white; padding: 10px; text-decoration: none; margin: 5px 0; }
        .main-content { margin-left: 270px; padding: 20px; }
        table { width: 100%; border-collapse: collapse; }
        th, td { padding: 10px; border: 1px solid #ddd; }
        th { background: #34495e; color: white; }
        .btn { padding: 5px 15px; margin: 2px; border: none; cursor: pointer; border-radius: 3px; }
        .btn-edit { background: #3498db; color: white; }
        .btn-delete { background: #e74c3c; color: white; }
        .btn-add { background: #27ae60; color: white; padding: 10px 20px; }
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
        </div>
        <div class="main-content">
            <h2>Manage Products</h2>
            <asp:Button ID="btnAddNew" runat="server" Text="Add New Product" CssClass="btn btn-add" OnClick="btnAddNew_Click" />
            <br /><br />
            <asp:GridView ID="gvProducts" runat="server" AutoGenerateColumns="False" DataKeyNames="ProductID" OnRowCommand="gvProducts_RowCommand">
                <Columns>
                    <asp:BoundField DataField="ProductID" HeaderText="ID" />
                    <asp:BoundField DataField="ProductName" HeaderText="Product Name" />
                    <asp:BoundField DataField="CategoryName" HeaderText="Category" />
                    <asp:BoundField DataField="Brand" HeaderText="Brand" />
                    <asp:BoundField DataField="Price" HeaderText="Price" DataFormatString="Rs. {0}" />
                    <asp:BoundField DataField="StockQuantity" HeaderText="Stock" />
                    <asp:BoundField DataField="Status" HeaderText="Status" />
                    <asp:TemplateField HeaderText="Actions">
                        <ItemTemplate>
                            <asp:Button ID="btnEdit" runat="server" Text="Edit" CommandName="EditProduct" 
                                        CommandArgument='<%# Eval("ProductID") %>' CssClass="btn btn-edit" />
                            <asp:Button ID="btnDelete" runat="server" Text="Delete" CommandName="DeleteProduct" 
                                        CommandArgument='<%# Eval("ProductID") %>' CssClass="btn btn-delete" 
                                        OnClientClick="return confirm('Are you sure?');" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>