Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Public Class Admin_ManageProductsPage
    Inherits AdminPage
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then Run(Sub() BindData())
    End Sub
    Private Sub BindData()
        ProductsGrid.DataSource = StoreService.Products(Search.Text, 0, True)
        ProductsGrid.DataBind()
    End Sub
    Protected Sub SearchClick(sender As Object, e As EventArgs)
        Run(Sub() BindData())
    End Sub
    Protected Sub ProductCommand(sender As Object, e As GridViewCommandEventArgs)
        If e.CommandName <> "RemoveProduct" Then Return
        Run(Sub()
                AdminService.DeleteProduct(CommonHelper.PositiveId(CStr(e.CommandArgument)))
                BindData()
                Notice = "Product deleted."
            End Sub)
    End Sub

End Class
