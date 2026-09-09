<%@ Page Language="VB" AutoEventWireup="false" CodeBehind="AddProduct.aspx.vb" Inherits="OnlinePharmacy.Admin_AddProduct" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Add Product - Admin</title>
    <style>
        body { font-family: Arial; }
        .sidebar { width: 250px; background: #2c3e50; color: white; height: 100vh; position: fixed; padding: 20px; }
        .sidebar a { display: block; color: white; padding: 10px; text-decoration: none; }
        .main-content { margin-left: 270px; padding: 20px; max-width: 800px; }
        .form-group { margin-bottom: 15px; }
        label { display: block; margin-bottom: 5px; font-weight: bold; }
        input, select, textarea { width: 100%; padding: 8px; border: 1px solid #ddd; border-radius: 3px; }
        .btn { background: #27ae60; color: white; padding: 10px 30px; border: none; cursor: pointer; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="sidebar">
            <h2>Admin Panel</h2>
            <a href="Dashboard.aspx">Dashboard</a>
            <a href="ManageProducts.aspx">Manage Products</a>
        </div>
        <div class="main-content">
            <h2>Add New Product</h2>
            <div class="form-group">
                <label>Product Name:</label>
                <asp:TextBox ID="txtProductName" runat="server"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Category:</label>
                <asp:DropDownList ID="ddlCategory" runat="server"></asp:DropDownList>
            </div>
            <div class="form-group">
                <label>Brand:</label>
                <asp:TextBox ID="txtBrand" runat="server"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Price:</label>
                <asp:TextBox ID="txtPrice" runat="server" TextMode="Number"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Stock Quantity:</label>
                <asp:TextBox ID="txtStock" runat="server" TextMode="Number"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Expiry Date:</label>
                <asp:TextBox ID="txtExpiry" runat="server" TextMode="Date"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Manufacturer:</label>
                <asp:TextBox ID="txtManufacturer" runat="server"></asp:TextBox>
            </div>
            <div class="form-group">
                <label>Description:</label>
                <asp:TextBox ID="txtDescription" runat="server" TextMode="MultiLine" Rows="4"></asp:TextBox>
            </div>
            <asp:Button ID="btnSave" runat="server" Text="Save Product" CssClass="btn" OnClick="btnSave_Click" />
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>
        </div>
    </form>
</body>
</html>