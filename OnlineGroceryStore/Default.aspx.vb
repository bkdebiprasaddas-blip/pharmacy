Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Public Class DefaultPage
    Inherits GroceryPage
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then Run(Sub() BindData())
    End Sub
    Private Sub BindData()
        CategoryList.DataSource = StoreService.Categories()
        CategoryList.DataBind()
        Dim rows = StoreService.Products("", 0)
        Dim featured = rows.Clone()
        For index As Integer = 0 To Math.Min(7, rows.Rows.Count - 1)
            featured.ImportRow(rows.Rows(index))
        Next
        ProductList.DataSource = featured
        ProductList.DataBind()
        If rows.Rows.Count = 0 Then EmptyLabel.Text = "The shelves are being stocked. Check back soon."
    End Sub

End Class
