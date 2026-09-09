Imports System.Data
Imports System.Data.OleDb

Public Class Products
    Inherits System.Web.UI.Page

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            LoadCategories()
            LoadProducts()
        End If
    End Sub

    Private Sub LoadCategories()
        Dim sql As String = "SELECT CategoryID, CategoryName FROM Categories WHERE Status='ACTIVE' ORDER BY CategoryName"
        Dim dt As DataTable = DatabaseHelper.FillDataTable(sql)
        ddlCategory.DataSource = dt
        ddlCategory.DataTextField = "CategoryName"
        ddlCategory.DataValueField = "CategoryID"
        ddlCategory.DataBind()
        ddlCategory.Items.Insert(0, New ListItem("All Categories", ""))
    End Sub

    Private Sub LoadProducts()
        Dim sql As String = "SELECT p.ProductID, p.ProductName, p.Brand, p.Price, p.StockQuantity, " &
                           "p.Description, p.ExpiryDate, c.CategoryName " &
                           "FROM Products p INNER JOIN Categories c ON p.CategoryID = c.CategoryID " &
                           "WHERE p.Status='ACTIVE' AND p.StockQuantity > 0"

        Dim parameters As New List(Of OleDbParameter)

        If Not String.IsNullOrEmpty(txtSearch.Text) Then
            sql &= " AND (p.ProductName LIKE ? OR p.Brand LIKE ? OR p.Description LIKE ?)"
            Dim searchTerm As String = "%" & txtSearch.Text & "%"
            parameters.Add(DatabaseHelper.P(searchTerm))
            parameters.Add(DatabaseHelper.P(searchTerm))
            parameters.Add(DatabaseHelper.P(searchTerm))
        End If

        If Not String.IsNullOrEmpty(ddlCategory.SelectedValue) Then
            sql &= " AND p.CategoryID = ?"
            parameters.Add(DatabaseHelper.P(Convert.ToInt32(ddlCategory.SelectedValue)))
        End If

        sql &= " ORDER BY p.ProductName"

        Dim dt As DataTable = DatabaseHelper.FillDataTable(sql, parameters.ToArray())
        rptProducts.DataSource = dt
        rptProducts.DataBind()
    End Sub

    Protected Sub btnSearch_Click(sender As Object, e As EventArgs) Handles btnSearch.Click
        LoadProducts()
    End Sub

    Protected Sub ddlCategory_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddlCategory.SelectedIndexChanged
        LoadProducts()
    End Sub

    Protected Sub AddToCart_Click(sender As Object, e As CommandEventArgs)
        If Session("UserID") Is Nothing Then
            Response.Redirect("Login.aspx")
            Return
        End If

        Dim productID As Integer = Convert.ToInt32(e.CommandArgument)
        Dim userID As Integer = Convert.ToInt32(Session("UserID"))

        Dim checkSql As String = "SELECT CartID FROM Cart WHERE UserID=? AND ProductID=?"
        Dim existingCart As Object = DatabaseHelper.ExecuteScalar(checkSql,
            DatabaseHelper.P(userID), DatabaseHelper.P(productID))

        If existingCart IsNot Nothing AndAlso Not IsDBNull(existingCart) Then
            Dim updateSql As String = "UPDATE Cart SET Quantity = Quantity + 1 WHERE CartID=?"
            DatabaseHelper.ExecuteNonQuery(updateSql, DatabaseHelper.P(Convert.ToInt32(existingCart)))
        Else
            Dim insertSql As String = "INSERT INTO Cart (UserID, ProductID, Quantity, AddedDate) VALUES (?, ?, 1, Now())"
            DatabaseHelper.ExecuteNonQuery(insertSql, DatabaseHelper.P(userID), DatabaseHelper.P(productID))
        End If

        Response.Redirect("Cart.aspx")
    End Sub
End Class