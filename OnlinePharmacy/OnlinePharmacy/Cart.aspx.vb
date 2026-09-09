Imports System.Data

Public Class Cart
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then
            Response.Redirect("Login.aspx")
            Return
        End If

        If Not IsPostBack Then
            LoadCart()
        End If
    End Sub

    Private Sub LoadCart()
        Dim userID As Integer = Convert.ToInt32(Session("UserID"))
        Dim sql As String = "SELECT c.CartID, p.ProductName, p.Brand, p.Price, c.Quantity, " &
                           "(p.Price * c.Quantity) AS Subtotal " &
                           "FROM Cart c INNER JOIN Products p ON c.ProductID = p.ProductID " &
                           "WHERE c.UserID = ? ORDER BY c.AddedDate DESC"
        Dim dt As DataTable = DatabaseHelper.FillDataTable(sql, DatabaseHelper.P(userID))
        gvCart.DataSource = dt
        gvCart.DataBind()

        ' Calculate total
        Dim total As Decimal = 0
        For Each row As DataRow In dt.Rows
            total += Convert.ToDecimal(row("Subtotal"))
        Next
        lblTotal.Text = total.ToString("0.00")
    End Sub

    Protected Sub gvCart_RowCommand(sender As Object, e As GridViewCommandEventArgs)
        If e.CommandName = "Remove" Then
            Dim cartID As Integer = Convert.ToInt32(e.CommandArgument)
            Dim sql As String = "DELETE FROM Cart WHERE CartID = ?"
            DatabaseHelper.ExecuteNonQuery(sql, DatabaseHelper.P(cartID))
            LoadCart()
        End If
    End Sub

    Protected Sub btnCheckout_Click(sender As Object, e As EventArgs)
        If gvCart.Rows.Count = 0 Then
            lblMessage.Text = "Your cart is empty!"
            Return
        End If

        Response.Redirect("Checkout.aspx")
    End Sub
End Class