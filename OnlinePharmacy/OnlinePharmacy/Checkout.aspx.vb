Imports System.Data

Public Class Checkout
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then
            Response.Redirect("Login.aspx")
            Return
        End If

        If Not IsPostBack Then
            LoadOrderSummary()
        End If
    End Sub

    Private Sub LoadOrderSummary()
        Dim userID As Integer = Convert.ToInt32(Session("UserID"))
        Dim sql As String = "SELECT p.ProductName, c.Quantity, p.Price, (p.Price * c.Quantity) AS Subtotal " &
                           "FROM Cart c INNER JOIN Products p ON c.ProductID = p.ProductID " &
                           "WHERE c.UserID = ?"
        Dim dt As DataTable = DatabaseHelper.FillDataTable(sql, DatabaseHelper.P(userID))

        Dim summary As String = "<table style='width:100%; border-collapse: collapse;'>"
        summary &= "<tr><th>Product</th><th>Qty</th><th>Price</th><th>Subtotal</th></tr>"

        Dim total As Decimal = 0
        For Each row As DataRow In dt.Rows
            summary &= "<tr>"
            summary &= "<td>" & row("ProductName") & "</td>"
            summary &= "<td>" & row("Quantity") & "</td>"
            summary &= "<td>Rs. " & row("Price") & "</td>"
            summary &= "<td>Rs. " & row("Subtotal") & "</td>"
            summary &= "</tr>"
            total += Convert.ToDecimal(row("Subtotal"))
        Next

        summary &= "<tr><td colspan='3' style='text-align:right; font-weight:bold;'>Total:</td>"
        summary &= "<td style='font-weight:bold;'>Rs. " & total.ToString("0.00") & "</td></tr>"
        summary &= "</table>"

        lblOrderSummary.Text = summary
    End Sub

    Protected Sub btnPlaceOrder_Click(sender As Object, e As EventArgs)
        Dim userID As Integer = Convert.ToInt32(Session("UserID"))
        Dim address As String = txtAddress.Text.Trim()
        Dim paymentMethod As String = ddlPayment.SelectedValue

        If String.IsNullOrEmpty(address) Then
            lblMessage.Text = "Please enter delivery address."
            Return
        End If

        ' Get cart items
        Dim cartSql As String = "SELECT c.CartID, c.ProductID, c.Quantity, p.Price " &
                               "FROM Cart c INNER JOIN Products p ON c.ProductID = p.ProductID " &
                               "WHERE c.UserID = ?"
        Dim cartDt As DataTable = DatabaseHelper.FillDataTable(cartSql, DatabaseHelper.P(userID))

        If cartDt.Rows.Count = 0 Then
            lblMessage.Text = "Your cart is empty!"
            Return
        End If

        ' Calculate total
        Dim totalAmount As Decimal = 0
        For Each row As DataRow In cartDt.Rows
            totalAmount += Convert.ToDecimal(row("Price")) * Convert.ToInt32(row("Quantity"))
        Next

        ' Insert order
        Dim orderSql As String = "INSERT INTO Orders (UserID, OrderDate, TotalAmount, Status, DeliveryAddress, PaymentMethod) " &
                                "VALUES (?, Now(), ?, 'Pending', ?, ?)"
        DatabaseHelper.ExecuteNonQuery(orderSql,
            DatabaseHelper.P(userID),
            DatabaseHelper.P(totalAmount),
            DatabaseHelper.P(address),
            DatabaseHelper.P(paymentMethod))

        ' Get new order ID
        Dim orderID As Integer = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT @@IDENTITY"))

        ' Insert order items
        For Each row As DataRow In cartDt.Rows
            Dim productID As Integer = Convert.ToInt32(row("ProductID"))
            Dim quantity As Integer = Convert.ToInt32(row("Quantity"))
            Dim price As Decimal = Convert.ToDecimal(row("Price"))
            Dim subtotal As Decimal = price * quantity

            Dim itemSql As String = "INSERT INTO OrderItems (OrderID, ProductID, Quantity, Price, Subtotal) " &
                                   "VALUES (?, ?, ?, ?, ?)"
            DatabaseHelper.ExecuteNonQuery(itemSql,
                DatabaseHelper.P(orderID),
                DatabaseHelper.P(productID),
                DatabaseHelper.P(quantity),
                DatabaseHelper.P(price),
                DatabaseHelper.P(subtotal))

            ' Update product stock
            Dim stockSql As String = "UPDATE Products SET StockQuantity = StockQuantity - ? WHERE ProductID = ?"
            DatabaseHelper.ExecuteNonQuery(stockSql, DatabaseHelper.P(quantity), DatabaseHelper.P(productID))
        Next

        ' Clear cart
        Dim clearSql As String = "DELETE FROM Cart WHERE UserID = ?"
        DatabaseHelper.ExecuteNonQuery(clearSql, DatabaseHelper.P(userID))

        lblMessage.ForeColor = System.Drawing.Color.Green
        lblMessage.Text = "Order placed successfully! Order ID: " & orderID
        Response.Redirect("Orders.aspx")
    End Sub
End Class