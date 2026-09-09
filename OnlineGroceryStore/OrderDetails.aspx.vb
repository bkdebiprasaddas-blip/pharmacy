Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Public Class OrderDetailsPage
    Inherits CustomerPage
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then Run(Sub() BindData())
    End Sub
    Private Sub BindData()
        Dim id = CommonHelper.PositiveId(Request.QueryString("id"))
        Dim row = StoreService.Order(UserId, id)
        OrderInfo.DataSource = row.Table
        OrderInfo.DataBind()
        ItemsGrid.DataSource = StoreService.OrderItems(UserId, id)
        ItemsGrid.DataBind()
        If Request.QueryString("placed") = "1" Then Notice = "Order placed successfully. Thank you for shopping with FreshBasket!"
    End Sub

End Class
