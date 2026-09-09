Public Class Orders
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Session("UserID") Is Nothing Then
            Response.Redirect("Login.aspx")
            Return
        End If

        If Not IsPostBack Then
            LoadOrders()
        End If
    End Sub

    Private Sub LoadOrders()
        Dim userID As Integer = Convert.ToInt32(Session("UserID"))
        Dim sql As String = "SELECT OrderID, OrderDate, TotalAmount, Status, DeliveryAddress, PaymentMethod " &
                           "FROM Orders WHERE UserID = ? ORDER BY OrderDate DESC"
        Dim dt As DataTable = DatabaseHelper.FillDataTable(sql, DatabaseHelper.P(userID))
        gvOrders.DataSource = dt
        gvOrders.DataBind()
    End Sub
End Class