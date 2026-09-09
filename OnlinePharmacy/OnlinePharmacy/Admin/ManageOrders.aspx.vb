Imports System.Data
Imports System.Web.UI.WebControls

Public Class Admin_ManageOrders
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Session("Role") Is Nothing OrElse Session("Role").ToString() <> "ADMIN" Then
            Response.Redirect("../Login.aspx")
            Return
        End If

        If Not IsPostBack Then
            LoadOrders()
        End If
    End Sub

    Private Sub LoadOrders()
        Dim sql As String = "SELECT o.OrderID, u.FullName AS UserName, o.OrderDate, o.TotalAmount, " &
                           "o.Status, o.PaymentMethod " &
                           "FROM Orders o INNER JOIN Users u ON o.UserID = u.UserID " &
                           "ORDER BY o.OrderDate DESC"
        Dim dt As DataTable = DatabaseHelper.FillDataTable(sql)
        gvOrders.DataSource = dt
        gvOrders.DataBind()
    End Sub

    Protected Sub gvOrders_RowCommand(sender As Object, e As GridViewCommandEventArgs)
        If e.CommandName = "UpdateStatus" Then
            Dim orderID As Integer = Convert.ToInt32(e.CommandArgument)
            Dim row As GridViewRow = CType(CType(sender, Button).NamingContainer, GridViewRow)
            Dim ddlStatus As DropDownList = CType(row.FindControl("ddlStatus"), DropDownList)
            Dim newStatus As String = ddlStatus.SelectedValue

            Dim sql As String = "UPDATE Orders SET Status=? WHERE OrderID=?"
            DatabaseHelper.ExecuteNonQuery(sql, DatabaseHelper.P(newStatus), DatabaseHelper.P(orderID))
            LoadOrders()
        End If
    End Sub
End Class