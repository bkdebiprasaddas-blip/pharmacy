Option Strict On
Option Explicit On
Option Infer On

Imports System
Imports System.Data
Imports System.Data.OleDb
Imports System.Globalization
Imports System.Web.UI
Imports System.Web.UI.WebControls

Partial Public Class Admin_DashboardPage
    Inherits AdminPage
    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then Run(Sub() BindData())
    End Sub
    Private Sub BindData()
        Stats.DataSource = AdminService.Statistics()
        Stats.DataBind()
        Dim orders = AdminService.AllOrders()
        Dim recent = orders.Clone()
        For index As Integer = 0 To Math.Min(9, orders.Rows.Count - 1)
            recent.ImportRow(orders.Rows(index))
        Next
        RecentOrders.DataSource = recent
        RecentOrders.DataBind()
        LowStockGrid.DataSource = AdminService.LowStock(5)
        LowStockGrid.DataBind()
        TopProducts.DataSource = AdminService.TopSelling()
        TopProducts.DataBind()
    End Sub

End Class
