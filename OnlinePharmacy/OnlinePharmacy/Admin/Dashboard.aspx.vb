Public Class Admin_Dashboard
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Session("Role") Is Nothing OrElse Session("Role").ToString() <> "ADMIN" Then
            Response.Redirect("../Login.aspx")
            Return
        End If

        If Not IsPostBack Then
            LoadStats()
        End If
    End Sub

    Private Sub LoadStats()
        ' Total Users
        Dim userCount As Object = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Users WHERE Role='USER'")
        lblTotalUsers.Text = If(userCount IsNot Nothing, userCount.ToString(), "0")

        ' Total Products
        Dim productCount As Object = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Products WHERE Status='ACTIVE'")
        lblTotalProducts.Text = If(productCount IsNot Nothing, productCount.ToString(), "0")

        ' Total Orders
        Dim orderCount As Object = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Orders")
        lblTotalOrders.Text = If(orderCount IsNot Nothing, orderCount.ToString(), "0")

        ' Total Revenue
        Dim revenue As Object = DatabaseHelper.ExecuteScalar("SELECT SUM(TotalAmount) FROM Orders WHERE Status='Delivered'")
        lblTotalRevenue.Text = If(revenue IsNot Nothing AndAlso Not IsDBNull(revenue), Convert.ToDecimal(revenue).ToString("0.00"), "0.00")
    End Sub
End Class