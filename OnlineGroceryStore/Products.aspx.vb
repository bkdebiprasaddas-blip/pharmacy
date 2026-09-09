Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Public Class ProductsPage
    Inherits GroceryPage
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            Run(Sub()
                    Category.DataSource = StoreService.Categories()
                    Category.DataTextField = "CategoryName"
                    Category.DataValueField = "CategoryID"
                    Category.DataBind()
                    Category.Items.Insert(0, New ListItem("All categories", "0"))
                    Dim selected = Category.Items.FindByValue(Request.QueryString("category"))
                    If selected IsNot Nothing Then Category.SelectedValue = selected.Value
                    BindData()
                End Sub)
        End If
    End Sub
    Protected Sub SearchClick(sender As Object, e As EventArgs)
        Run(Sub() BindData())
    End Sub
    Private Sub BindData()
        Dim rows = StoreService.Products(Search.Text, Convert.ToInt32(Category.SelectedValue))
        ProductList.DataSource = rows
        ProductList.DataBind()
        EmptyLabel.Text = If(rows.Rows.Count = 0, "No products match your search. Try another name or category.", "")
    End Sub

End Class
