Public Class Admin_Reports
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Session("Role") Is Nothing OrElse Session("Role").ToString() <> "ADMIN" Then
            Response.Redirect("../Login.aspx")
            Return
        End If

        If Not IsPostBack Then
            LoadReports()
        End If
    End Sub

    Private Sub LoadReports()
        ' Recent Orders
        Dim orderSql As String = "SELECT TOP 10 o.OrderID, u.FullName AS CustomerName, o.OrderDate, " &
                                "o.TotalAmount, o.Status " &
                                "FROM Orders o INNER JOIN Users u ON o.UserID = u.UserID " &
                                "ORDER BY o.OrderDate DESC"
        Dim orderDt As DataTable = DatabaseHelper.FillDataTable(orderSql)
        gvRecentOrders.DataSource = orderDt
        gvRecentOrders.DataBind()

        ' Low Stock Products
        Dim stockSql As String = "SELECT ProductName, Brand, StockQuantity " &
                                "FROM Products WHERE StockQuantity < 10 AND Status='ACTIVE' " &
                                "ORDER BY StockQuantity ASC"
        Dim stockDt As DataTable = DatabaseHelper.FillDataTable(stockSql)
        gvLowStock.DataSource = stockDt
        gvLowStock.DataBind()
    End Sub
End Class