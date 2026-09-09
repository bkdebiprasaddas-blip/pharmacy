<%@ Page Language="VB" AutoEventWireup="false" CodeBehind="Cart.aspx.vb" Inherits="OnlinePharmacy.Cart" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>Shopping Cart - Online Pharmacy</title>
    <style>
        body { font-family: Arial; margin: 0; padding: 0; }
        .header { background: #2c3e50; color: white; padding: 20px; text-align: center; }
        .container { padding: 20px; max-width: 1000px; margin: 0 auto; }
        table { width: 100%; border-collapse: collapse; margin-top: 20px; }
        th, td { padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }
        th { background: #34495e; color: white; }
        .btn { background: #27ae60; color: white; padding: 12px 30px; border: none; cursor: pointer; border-radius: 3px; }
        .btn-remove { background: #e74c3c; color: white; padding: 5px 10px; border: none; cursor: pointer; border-radius: 3px; }
        .total { font-size: 20px; font-weight: bold; text-align: right; margin-top: 20px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <div class="header">
            <h1>Shopping Cart</h1>
        </div>
        <div class="container">
            <asp:GridView ID="gvCart" runat="server" AutoGenerateColumns="False" DataKeyNames="CartID" OnRowCommand="gvCart_RowCommand">
                <Columns>
                    <asp:BoundField DataField="ProductName" HeaderText="Product" />
                    <asp:BoundField DataField="Brand" HeaderText="Brand" />
                    <asp:BoundField DataField="Price" HeaderText="Price" DataFormatString="Rs. {0}" />
                    <asp:BoundField DataField="Quantity" HeaderText="Quantity" />
                    <asp:BoundField DataField="Subtotal" HeaderText="Subtotal" DataFormatString="Rs. {0}" />
                    <asp:TemplateField HeaderText="Action">
                        <ItemTemplate>
                            <asp:Button ID="btnRemove" runat="server" Text="Remove" CommandName="Remove" 
                                        CommandArgument='<%# Eval("CartID") %>' CssClass="btn-remove" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
            <div class="total">
                Total Amount: Rs. <asp:Label ID="lblTotal" runat="server" Text="0"></asp:Label>
            </div>
            <br />
            <asp:Button ID="btnCheckout" runat="server" Text="Proceed to Checkout" CssClass="btn" OnClick="btnCheckout_Click" />
            <asp:Label ID="lblMessage" runat="server" ForeColor="Red"></asp:Label>
        </div>
    </form>
</body>
</html>