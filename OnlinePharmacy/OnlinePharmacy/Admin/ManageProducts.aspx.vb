Imports System.Data

Public Class Admin_ManageProducts
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Session("Role") Is Nothing OrElse Session("Role").ToString() <> "ADMIN" Then
            Response.Redirect("../Login.aspx")
            Return
        End If

        If Not IsPostBack Then
            LoadProducts()
        End If
    End Sub

    Private Sub LoadProducts()
        Dim sql As String = "SELECT p.ProductID, p.ProductName, c.CategoryName, p.Brand, p.Price, " &
                           "p.StockQuantity, p.Status " &
                           "FROM Products p INNER JOIN Categories c ON p.CategoryID = c.CategoryID " &
                           "ORDER BY p.ProductID DESC"
        Dim dt As DataTable = DatabaseHelper.FillDataTable(sql)
        gvProducts.DataSource = dt
        gvProducts.DataBind()
    End Sub

    Protected Sub gvProducts_RowCommand(sender As Object, e As GridViewCommandEventArgs)
        If e.CommandName = "DeleteProduct" Then
            Dim productID As Integer = Convert.ToInt32(e.CommandArgument)
            Dim sql As String = "UPDATE Products SET Status='INACTIVE' WHERE ProductID=?"
            DatabaseHelper.ExecuteNonQuery(sql, DatabaseHelper.P(productID))
            LoadProducts()
        ElseIf e.CommandName = "EditProduct" Then
            Dim productID As Integer = Convert.ToInt32(e.CommandArgument)
            Response.Redirect("EditProduct.aspx?id=" & productID)
        End If
    End Sub

    Protected Sub btnAddNew_Click(sender As Object, e As EventArgs)
        Response.Redirect("AddProduct.aspx")
    End Sub
End Class