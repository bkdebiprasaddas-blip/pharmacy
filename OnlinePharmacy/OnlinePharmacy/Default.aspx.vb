Imports System.Data

Public Class _Default
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            LoadFeaturedProducts()
        End If
    End Sub

    Private Sub LoadFeaturedProducts()
        Dim sql As String = "SELECT TOP 8 ProductID, ProductName, Brand, Price, StockQuantity, Description " &
                           "FROM Products WHERE Status='ACTIVE' AND StockQuantity > 0 " &
                           "ORDER BY CreatedDate DESC"
        Dim dt As DataTable = DatabaseHelper.FillDataTable(sql)
        rptProducts.DataSource = dt
        rptProducts.DataBind()
    End Sub

    Protected Sub AddToCart_Click(sender As Object, e As CommandEventArgs)
        If Session("UserID") Is Nothing Then
            Response.Redirect("Login.aspx")
            Return
        End If

        Dim productID As Integer = Convert.ToInt32(e.CommandArgument)
        Dim userID As Integer = Convert.ToInt32(Session("UserID"))

        ' Check if product already in cart
        Dim checkSql As String = "SELECT CartID FROM Cart WHERE UserID=? AND ProductID=?"
        Dim existingCart As Object = DatabaseHelper.ExecuteScalar(checkSql,
            DatabaseHelper.P(userID), DatabaseHelper.P(productID))

        If existingCart IsNot Nothing AndAlso Not IsDBNull(existingCart) Then
            ' Update quantity
            Dim updateSql As String = "UPDATE Cart SET Quantity = Quantity + 1 WHERE CartID=?"
            DatabaseHelper.ExecuteNonQuery(updateSql, DatabaseHelper.P(Convert.ToInt32(existingCart)))
        Else
            ' Add new cart item
            Dim insertSql As String = "INSERT INTO Cart (UserID, ProductID, Quantity, AddedDate) VALUES (?, ?, 1, Now())"
            DatabaseHelper.ExecuteNonQuery(insertSql, DatabaseHelper.P(userID), DatabaseHelper.P(productID))
        End If

        Response.Redirect("Cart.aspx")
    End Sub
End Class